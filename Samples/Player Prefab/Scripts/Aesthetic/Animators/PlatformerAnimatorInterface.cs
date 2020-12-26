using UnityEngine;
using Unity.Mathematics;

using Clouds.MovementBase;
using Clouds.Platformer.Character;
using Clouds.Facing2D;

namespace Clouds.Platformer.Animation {
	public class PlatformerAnimatorInterface : MonoBehaviour {
		[SerializeField] Animator animator;

		[Header("Data To Read")]
		[SerializeField] FacingDirection_8Way facing;
		[SerializeField] string facingXProperty = "Facing Direction";
		[SerializeField] string facingYProperty = "Look Up/Down";

		[Space]
		
		[SerializeField] ScalarVelocity lateralMovement;
		[SerializeField] string lateralMovementProperty = "Walk Speed";
		[SerializeField] ScalarVelocity verticalMovement;
		[SerializeField] string verticalMovementProperty = "Vertical Speed";
		
		[Space]
		
		[SerializeField] PlatformerCollisions collisions;
		[SerializeField] string floorCollisionProperty = "Grounded";
		[SerializeField] string ceilingCollisionProperty = "Ceilinged";
		[SerializeField] string frontWallCollisionProperty = "Wall Front";
		[SerializeField] string backWallCollisionProperty = "Wall Back";

		[Space]
		

		[Header("Events To Subscribe")]
		[SerializeField] JumpComponent jumpComponent;
		[SerializeField] string jumpTrigger = "Jumped";

		bool _didJumpOnPurpose;
		void DidJump () {
			_didJumpOnPurpose = true;
		}


		void OnEnable () {
			jumpComponent?.onJumpedIntentionally.AddListener(DidJump);
		}
		void OnDisable () {
			jumpComponent?.onJumpedIntentionally.RemoveListener(DidJump);
			//Clear this flag so it's not around next time.
			_didJumpOnPurpose = false;
		}

		bool stringNotEmpty (string str) {
			return str != string.Empty && str != "";
		}

		void Update () {
			//Set the facing directions first of all.
			if (stringNotEmpty(facingXProperty) ) {
				animator.SetInteger(facingXProperty, facing.x);
			}
			if (stringNotEmpty(facingYProperty) ) {
				animator.SetInteger(facingYProperty, facing.y);
			}

			//Set the movement speeds second (not gameplay information, largely aesthetic).
			if (stringNotEmpty(lateralMovementProperty)) {
				animator.SetFloat(lateralMovementProperty, math.abs(lateralMovement.Value));
			}
			if (stringNotEmpty(verticalMovementProperty)) {
				animator.SetFloat(verticalMovementProperty, verticalMovement.Value);
			}

			//Set vertical collision flags (not facing dependent).
			if (stringNotEmpty(floorCollisionProperty)) {
				animator.SetBool(floorCollisionProperty, collisions.Below);
			}
			if (stringNotEmpty(ceilingCollisionProperty)) {
				animator.SetBool(ceilingCollisionProperty, collisions.Above);
			}
			//Set horizontal collision flags (facing dependent).
			if (stringNotEmpty(frontWallCollisionProperty)) {
				bool frontWallHit = facing.x > 0 ? collisions.Right : collisions.Left;
				animator.SetBool(frontWallCollisionProperty, frontWallHit);
			}
			if (stringNotEmpty(backWallCollisionProperty)) {
				bool backWallHit = facing.x > 0 ? collisions.Left : collisions.Right;
				animator.SetBool(backWallCollisionProperty, backWallHit);
			}

			//Set jump trigger.
			if (stringNotEmpty(jumpTrigger)) {
				animator.SetTrigger(jumpTrigger);
			}
		}

	}
}