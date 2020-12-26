using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;

using Clouds.MovementBase;
using Clouds.Movement2D;

namespace Clouds.Platformer.Character {
	public class SlideDownMaxSlopes : MonoBehaviour {
		[Header("Basic Components")]
		[Tooltip("We need to have information on what slope we're on if we want to know if we're on max!")]
		[SerializeField] PlatformerSlopeCollisions slopeCollisions;
		[Tooltip("How can you slide down a slope without gravity pulling you?")]
		[SerializeField] GravityComponent groundedResponseToModify;
		
		[Header("Jump Response")]
		[SerializeField] JumpComponent jumpComponentToModify;
		[SerializeField] GravityStrength gravityStrengthToModify;
		[SerializeField] Velocity velocityToModifyForJump;
		
		float slideDownGroundedResponse (float timestep, float storedVelocity, float currentGravity) {
			return storedVelocity + math.select(
				//Add -(self) to revert to zero.
				-storedVelocity,
				//This will counteract an outer add of addedGravity.
				//If this number gets used, it determines how far down you slide.
				//The horizontal component, I think, is from this value also.
				//@TODO: Refigure max-slope integration for custom X angles?
				slopeCollisions.normal.y * -(currentGravity * timestep),
				slopeCollisions.slidingDownMax()
			);
		}

		void OnEnable () {
			groundedResponseToModify.groundedResponse = slideDownGroundedResponse;

			jumpComponentToModify?.onJumped.AddListener(jumpOffSlidingDown);
		}

		//If was sliding down a max slope, do a special jump.
		void jumpOffSlidingDown () {
			if (slopeCollisions.slidingDownMax()) {
				//Rotate X velocity so that it points off of the slope.
				velocityToModifyForJump.x = velocityToModifyForJump.y * slopeCollisions.normal.x;

				//Gravity's going to modify the Y velocity after this is called.
				//So let's let it do its job--just reduce its effect some...:)
				gravityStrengthToModify.Value *= slopeCollisions.normal.y;
			}
		}
		
	}
}