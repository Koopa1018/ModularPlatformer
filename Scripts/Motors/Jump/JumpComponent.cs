using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Unity.Mathematics;

using Clouds.PlayerInput;
using Clouds.MovementBase;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Platformer/Character/Gravity/Can Jump")]
	public class JumpComponent : MonoBehaviour {
		[Header("Components To Read")]
		[SerializeField] PlatformerCollisions collisions;
		[SerializeField] ButtonInputField jumpButton;
		[Header("Components To Write")]
		[SerializeField] GravityComponent gravityComponent;
		[SerializeField] GravityStrength gravityStrength;
		[SerializeField] ScalarVelocity storedVelocity;
		[Header("Optional Components")]
		[SerializeField] PlatformerSlopeCollisions slopeCollisions;
		bool hasSlopeCollisions = false;
		
		[Header("Function Parameters")]
		[Tooltip("How high the jump can rise, in units.")]
		[SerializeField] float jumpHeight = 3.2f; //((timeToJumpApex^2) * gravity.floatValue) / -2;
		[Tooltip("How long until the highest point, in seconds.")]
		[SerializeField] float timeToPeak = 0.45f;
		[Tooltip("When falling, gravity will be this many times the norm.")]
		[SerializeField] float fallGravityMultiplier = 1.5f;
		[Tooltip("When the player lets go of a jump, up velocity drops to this many times its former self.")]
		[SerializeField] float jumpEndReduceSpeedTo = 0.4f;
		[Tooltip("When the player walks off a cliff, they will have this much extra time able to jump.")]
		[SerializeField] [Min(0)] float coyoteTime = 0.2f;

		[Header("Events")]
		[SerializeField] public UnityEvent onJumped;
		[SerializeField] public UnityEvent onJumpedIntentionally;
		[SerializeField] public UnityEvent onJumpedInvoluntarily;
		[SerializeField] public UnityEvent onReleaseJump;

		(bool has, ForcedJump value) _forcedJump;
		/// <summary>
		/// Returns the current jump that is being forced here, or <c>null</c> if none is being forced.
		/// </summary>
		public ForcedJump? GetForcedJump () {
			return _forcedJump.has? (ForcedJump?)_forcedJump.value : null;
		}
		/// <summary>
		/// Forces a jump to occur. The jump can be of arbitrary height and duration, and will always be
		/// executed to its full extent before any further jumps can be added (it cannot be cancelled).
		/// If two jumps are forced on the same frame, the resulting jump will be as high as the higher
		/// and as long as the longer of the two.
		/// 
		/// If the character is airborne, this method does nothing.
		/// </summary>
		/// <param name="newForcedJump">The jump to force.</param>
		public void SetForcedJump (ForcedJump newForcedJump) {
			if (!collisions.Below) {
				return;
			}

			//Flag that we have a forced jump now!
			_forcedJump.has = true;

			_forcedJump.value.CombineWith(newForcedJump);
		}
		/// <summary>
		/// If a forced jump has been begun, abort it.
		/// </summary>
		public void EndAllForcedJumps () {
			if (_forcedJump.has) {
				clearForcedJump();
				clearJumpTimer();
			}
		}
		void clearForcedJump () {
			_forcedJump = (false, default(ForcedJump));
		}

		/// <summary>
		/// Should jumping with the jump button be prevented?
		/// (This will not affect forced jumps.)
		/// </summary>
		public bool blockManualJumps {get; set;}
		/// <summary>
		/// Should ending a jump early by releasing the jump button be prevented?
		/// </summary>
		public bool blockJumpRelease {get; set;}

		void Awake () {
			//Initialize jump blocks, since there's no other way to customize properties' initial values.
			blockManualJumps = blockJumpRelease = false;

			//Cache result of slopeCollisions != null, since Unity's null comparison operator may be slower than other null checks.
			hasSlopeCollisions = slopeCollisions != null;

			//Calculate rising gravity.
			gravityRising = 2 * jumpHeight;
			gravityRising = -gravityRising / math.pow(timeToPeak,2);
			//Calculate falling gravity.
			gravityFalling = gravityRising * fallGravityMultiplier;

			//Calculate jumping power.
			jumpPower = math.abs(gravityRising) * timeToPeak;
		}

		void clearJumpTimer () => jumpTimer = 0;

		void OnEnable () {
			//Register me for running after Gravity Component collides.
			gravityComponent.AfterCollisionBeforeApply.AddListener(doJump);
			
			//Clear forced jumps on landing.
			gravityComponent.OnHitGround.AddListener(clearForcedJump);

			//Reset jump timer when we hit our heads.
			gravityComponent.OnHitCeiling.AddListener(clearJumpTimer);
		}

		void OnDisable () {
			//Revert gravity's value to its default since we're not modifying it anymore.
			gravityStrength.RevertStrength();

			//Remove our registered callbacks.
			gravityComponent.AfterCollisionBeforeApply.RemoveListener(doJump);
			gravityComponent.OnHitGround.RemoveListener(clearForcedJump);
			gravityComponent.OnHitCeiling.RemoveListener(clearJumpTimer);
		}

		float gravityRising;
		float gravityFalling;
		float jumpPower;

		float jumpTimer;
		float coyoteTimer = 0;
		bool lastJumpButton = false;

		/// <summary>
		/// Checks if this character is on the ground or under effects of coyote time.
		/// </summary>
		/// <returns></returns>
		bool isAbleToJump () {
			return !blockManualJumps && (collisions.Below || coyoteTimer > 0);
		}
		
		//This is FixedUpdate, except it's called from GravityComponent in the middle of _its_ FixedUpdate function.
		void doJump () {
			//Update coyote time.
			coyoteTimer = collisions.Below ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;				

			//Jump if grounded and pressed the button (and not being blocked from jumps).
			if (jumpButton.Value) {
				//Debug.Log ("Trying to jump.");
				//Debug.Log ($"Last button: {lastJumpButton}; is on ground: {collisions.Below}");
				if (!lastJumpButton && isAbleToJump()) {
					//Debug.Log("Jumping newly.");
					storedVelocity.Value = registerNewJump(
						jumpPower, timeToPeak
					);

					//Call the on-jumped event, and events to do with specifically intentional jumps.
					onJumped?.Invoke();
					onJumpedIntentionally?.Invoke();
				}
			}
			//Jump if a jump was forced--unless in the middle of an existing one!
			if (_forcedJump.has && !lastJumpButton) {
				storedVelocity.Value = registerNewJump(
					//Jump power - compute from gravity.
					math.abs(storedVelocity.Value) * _forcedJump.value.duration,
					//Jump time.
					_forcedJump.value.duration
				);

				//Ensure we can't let go of this jump.
				blockJumpRelease = true;

				//Call the on-jumped event, and involuntary-only jump events!
				onJumped?.Invoke();
				onJumpedInvoluntarily?.Invoke();
			}

			//Release jump if released the button (and jumps can be let go of).
			if ((!jumpButton.Value && lastJumpButton) && !(blockJumpRelease | _forcedJump.has)) {
				storedVelocity.Value = releaseJump(storedVelocity.Value, jumpEndReduceSpeedTo);
			}

			//Set gravity to use.
			gravityStrength.Value = math.select(
				//Has hit the peak.
				//Hitting the peak of the jump will begin adding extra gravity.
				gravityFalling,
				//Hasn't hit the peak yet.
				//Use normal gravity.
				gravityRising,
				jumpTimer > 0
			);
				
			//Tick jump timer, until such a tick would drop below zero.
			jumpTimer -= math.min(jumpTimer, Time.fixedDeltaTime);

			//Cache current jump-button state.
			lastJumpButton = jumpButton.Value;
		}

		/// <summary>
		/// Begins a new jump with the given power and duration.
		/// </summary>
		/// <param name="jumpPower">The force to be returned.</param>
		/// <param name="jumpTime">The desired duration of the jump timer.</param>
		/// <param name="jumpTimer">The jump timer in question.</param>
		/// <param name="movement">This value may be modified if sliding down a max slope.</param>
		/// <returns>The new velocity to write.</returns>
		float registerNewJump (
			float jumpPower,
			float jumpTime
		) {//vloat
			float returner = jumpPower;

			//Start the jump timer.
			jumpTimer = jumpTime;

			return returner;
		}

		/// <summary>
		/// Hits a jump's apex prematurely, ending the jump timer and reducing velocity by a given multiplier.
		/// An event will be called.
		/// If the jump timer's already rung, does nothing.
		/// </summary>
		/// <param name="velocity">The current value of the expected velocity.</param>
		/// <param name="jumpEndReduceSpeedTo">The multiplier with which to reduce the velocity's speed.
		/// (Or increase, should you see fit. It's a number; do what you want with it.)</param>
		/// <returns>The velocity multiplied by jumpEndReduceSpeedTo.</returns>
		float releaseJump (float velocity, float jumpEndReduceSpeedTo) {
			//If the timer has already run out, abort.
			if (jumpTimer <= 0) {
				return velocity;
			}

			//Drop any positive velocity to whatever fraction of itself is requested.
			if (velocity > 0) {
				velocity *= jumpEndReduceSpeedTo;
			}

			//Stop the jump timer.
			jumpTimer = 0;

			//Call the on-release-jump event.
			onReleaseJump?.Invoke();

			return velocity;
		}

	}
}