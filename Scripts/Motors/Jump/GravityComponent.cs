using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Unity.Mathematics;

using Clouds.Movement2D;
using Clouds.MovementBase;

namespace Clouds.Platformer.Character {
	[RequireComponent(typeof(GravityStrength), typeof(ScalarVelocity))]
	[AddComponentMenu("Platformer/Gravity/Affected By Gravity")]
	public class GravityComponent : MonoBehaviour {
		public delegate float GroundedResponse (float timestep, float storedVelocity, float currentGravity);

		[SerializeField] PlatformerCollisions collisions;
		[SerializeField] Velocity output;

		GravityStrength info;
		ScalarVelocity storedVelocity;

		[SerializeField] public UnityEvent AfterCollisionBeforeApply;

		[SerializeField] public UnityEvent OnHitGround = new UnityEvent();
		[SerializeField] public UnityEvent OnHitCeiling = new UnityEvent();
		bool lastGrounded = true; //Init w/grounded flag already set so if we're already grounded, no response occurs.
		bool lastCeilinged = false;

		public GroundedResponse groundedResponse = hitGround;
		private static float hitGround (float timestep, float storedVelocity, float currentGravity) {
			return storedVelocity - storedVelocity;
		}
		
		void Awake () {
			//if (info == null) {
				info = GetComponent<GravityStrength>();
			//}
			//if (storedVelocity == null) {
				storedVelocity = GetComponent<ScalarVelocity>();
			//}
		}

		void FixedUpdate () {
			bool debugLog = false;
			// if (storedVelocity.Value > 0) {
			// 	Debug.Log("Beginning gravity, stored velocity is " + storedVelocity.Value);
			// 	debugLog = true;
			// }

			//Do collision response.
			storedVelocity.Value = applyVCollisions(
				storedVelocity.Value,
				info.Value,
				Time.fixedDeltaTime
			);
			// if (debugLog) {
			// 	Debug.Log("After colliding gravity, stored velocity is " + storedVelocity.Value);
			// }

			//Add gravity to the stored velocity.
			storedVelocity.Value += info.Value * Time.fixedDeltaTime;
			//Yes it is after collision response--THIS IS CRUCIAL.
			//If you let the stored velocity hit zero while grounded, a terrible
			//jittering will occur and you'll end up unable to jump (possibly inconsistent).

			// if (debugLog) {
			// 	Debug.Log("After adding gravity, stored velocity is " + storedVelocity.Value);
			// }

			//Perform any inbetween actions, like jumping.
			AfterCollisionBeforeApply?.Invoke();

			//Apply vertical movement.
			output.y += storedVelocity.Value * Time.fixedDeltaTime;
		}

		
		float applyVCollisions(
			float storedVelocity,
			//ref float jumpTimer,
			float currentGravity,
			float deltaTime
		) {
			//If we're not colliding, return.
			//if (!collisions.Above && !collisions.Below) return storedVelocity; //Pointless--we're about to check both!

			//If hit from above while falling, flag this unusual occurrence (other obj. moving faster'n us).
			collisions.HitAbove = storedVelocity < 0;
			//Likewise if hit from below while rising.
			collisions.HitBelow = storedVelocity > 0;

			bool ceilinged = collisions.Above && !collisions.HitAbove;
			//If hit head against STATIC object, stop moving.
			if (ceilinged) {
				//Make sure we don't abruptly stop if hit by a moving platform.
				storedVelocity = math.min(0, storedVelocity);
				
				//Only do this if we didn't hit the ceiling last time!
				if (!lastCeilinged) {
					//Run hit-head-on-ceiling code that's tied here.
					OnHitCeiling.Invoke();
				}
				
				//Debug.Log("Stored velocity after head hit is: " + storedVelocity);
			}
			lastCeilinged = ceilinged;

			bool grounded = collisions.Below  /*&& !collisions.HitBelow*/;
			//If landed on STATIC object, stop moving likewise.
			if (grounded) {
				//Alter stored velocity 
				storedVelocity = groundedResponse(deltaTime, storedVelocity, currentGravity);

				//Only do this if we didn't hit the ground last time!
				if (!lastGrounded) {
					//Run landing code hooked to this spot.
					OnHitGround.Invoke();
				}

//				Debug.Log("Stored velocity after landing is: " + storedVelocity);
			}
			lastGrounded = grounded;

			return storedVelocity;
		}
		
	}
}