// Deciding which movements should prevail? It actually depends on whether the _platform_ is parallel or perpendicular to the wall.
		
// Say we've got a platform moving under a moving (or, heck, static) wall:
//    [[[
//   o[[[
//  >>>
// The wall's movement (or lack thereof) must prevail, and the player must be forced off the platform.
// Thusly: we let the X movement from horizontal casts _replace_ X movement from vertical casts.

// Say the player is wallhanging on a vertically-moving wall and hits the same situation:
// [[[[^
//    o^
//     ^
// This could be a situation with a number of different outcomes. In Sonic & Knuckles-style
// wallhanging, the player would be forced to drop off; in Mega Man X-style hanging, the
// player would be forced gradually downward as in the horizontal example above.

// =====tl;dr Perpendicular platforms invariably lose to the wall.

//Parallel platforms, on the other hand, will crush the player, so they just always callback out to some other system.

using UnityEngine;
using Unity.Mathematics;

using Clouds.Movement2D;
using Clouds.Raycasting;

namespace Clouds.Platformer.Character {
	
	
	public class MovingPlatformHandler : MonoBehaviour {
		[SerializeField] bool _supportMovingFloors = true;
		internal bool SupportMovingFloors => _supportMovingFloors;
		
		[SerializeField] bool _supportMovingWalls = false;
		internal bool SupportMovingWalls => _supportMovingWalls;

		[Space]

		[SerializeField] bool breakWallClimbOnWallEmergence = true;
		[SerializeField] bool breakWallClimbOnHitFloor = true;
		[SerializeField] bool breakWallClimbOnHitCeiling = true;


		public float2 speed => math.select(weakSpeed, strongSpeed, strongSpeedUsed);
		
		/// <summary>
		/// Accumulated movement that, in the event of conflict, replaces weak movement.
		/// </summary>
		float2 strongSpeed = 0;
		/// <summary>
		/// Accumulated movement that, in the event of conflict, will be replaced by strong movement.
		/// </summary>
		float2 weakSpeed = 0;
		/// <summary>
		/// Which axes strong movement is active on, if any.
		/// </summary>
		bool2 strongSpeedUsed = false;


		Collider2D strongCollider, weakCollider;
		Velocity strongVelocity, weakVelocity;

		/// <summary>
		/// Find the displacement that ought to have occurred from moving platforms.
		/// </summary>
		/// <param name="velocity"></param>
		/// <returns></returns>
		internal float2 HandleMovingPlatforms(CharaCasterDimensions dimensions, float skinWidth, Collider2D myCollider, float2 velocity, int facingX, LayerMask hMask, LayerMask vMask) {
			//We want to displace the origin of the collision cast by our currently-ridden platform's velocity,
			//assuming we have one, so we need to read that value.
			float2 castCtr = valueOf(strongVelocity);
			castCtr += dimensions.BoxEpicenter();


			//To store raycasts in.
			RaycastHit2D[] veloCheck = new RaycastHit2D[2];

			//Should cast as far as is needed to find the correct platform.
			//This means if the previously-used collider has a velocity value, we should
			//use _its velocity_ (unless input velocity is longer).

			if (SupportMovingWalls) {
				//Check for moving platforms forwards.
				Physics2D.BoxCastNonAlloc(
					castCtr, //Center
					dimensions.Size, //Size
					0, //Rotation/angle
					Vector2.right * facingX, //diracion
					veloCheck,
					math.abs(velocity.x) + skinWidth, //distance
					hMask //layer mask
				);
				veloCheck[0] = veloCheck.FirstNonSelfHit(myCollider);

				if (veloCheck[0]) {
					TakeSpeedX(veloCheck[0].collider, /*isWallSliding*/false);
				}

				veloCheck.Clear();
			}

			if (SupportMovingFloors) {
				//Cast a thin (skinWidth)-sized box to find a floor.
				Physics2D.BoxCastNonAlloc(
					castCtr, //dimensions.BottomCenter(skinWidth) + new float2(0, skinWidth/2), //Center
					dimensions.Size, //new float2(dimensions.Size.x, skinWidth), //Size*/
					0, //Rotation/angle
					Vector2.up * math.sign(velocity.y), //diracion
					veloCheck,
					math.abs(velocity.y) + skinWidth, //distance
					vMask //layer mask
				);
				veloCheck[0] = veloCheck.FirstNonSelfHit(myCollider);

				if (veloCheck[0]) {
					TakeSpeedY(
						veloCheck[0].collider,
						veloCheck[0].point.y <= dimensions.BottomPoint(0, skinWidth).y
					);
				}
			}

			return FinalizePlatformMovement();
		}

		/// <summary>
		/// Check horizontal velocity of given colliders and store it for later use.
		/// </summary>
		/// <param name="candidate">Collider we'd like to check for velocity (if they have it, they're moving).</param>
		/// <param name="wallHanging">Are we hanging on a wall? (meaning: is the moving terrain's vertical movement important to check for?)</param>
		internal void TakeSpeedX (Collider2D candidate, bool wallHanging) {
			if (candidate == null) {
				return;
			}
			
			Velocity candidateVelocity = candidate.GetComponent<Velocity>();
			float2 wallVelocity = valueOf(candidateVelocity);
			
			//If this platform is coming towards us (from our perspective), it's meant to override our current speed!
			if (compareVelocities(speed.x, wallVelocity.x) == -1) {
				//Save this collider as the one we're under the effects of.
				strongCollider = candidate;
				strongVelocity = candidateVelocity;

				//Use its velocity.
				strongSpeedUsed.x = candidateVelocity != null;
				strongSpeed.x = wallVelocity.x;

				if (breakWallClimbOnWallEmergence) {
					//Force break wall climbing. Style of Megaman, it should automatically re-detect.
					wallHanging = false;
					//@TODO: Send the message out to other code.
				}
			}

			//If we're wall-hanging, factor in vertical movement also, though in a position to be overridden.
			if (wallHanging) {
				assignWeakCollider(candidate);

				//If this is the weak collider, accept its velocity as the weak velocity.
				if (candidate == weakCollider) {
					//Do something to stick to the current wall.
					weakSpeed = wallVelocity.y;
				}
			}
		}
		/// <summary>
		/// Check horizontal velocity of given colliders and store it for later use.
		/// </summary>
		/// <param name="candidate">Collider we'd like to check for velocity (if they have it, they're moving).</param>
		/// <param name="isGrounded">Are we standing on the ground? (meaning: is the moving terrain's horizontal movement important to check for?)</param>
		internal void TakeSpeedY (Collider2D candidate, bool isGrounded) {
			if (candidate == null) {
				return;
			}
			
			float2 wallVelocity = valueOf(candidate.GetComponent<Velocity>());
			
			//If this platform is coming towards us (from our perspective), it's meant to override our current speed!
			if (compareVelocities(speed.y, wallVelocity.y) == -1) {
				//Save this collider as the one we're under the effects of.
				strongCollider = candidate;

				//Use its velocity.
				strongSpeedUsed.y = true;
				strongSpeed.y = wallVelocity.y;

				if ((wallVelocity.y > 0 && breakWallClimbOnHitCeiling) 
				||	(wallVelocity.y < 0 && breakWallClimbOnHitFloor)
				) {
					//Force break wall climbing. Style of Megaman, it should automatically re-detect.
					//@TODO: Send the message out to other code.
				}
			}
			
			//If we're on the ground, factor in horizontal movement also, though in a position to be overridden.
			if (isGrounded) {
				assignWeakCollider(candidate);

				//If this is the weak collider, accept its velocity as the weak velocity.
				if (candidate == weakCollider) {
					//Do something to stick to the current platform.
					weakSpeed = wallVelocity.x;
				}
				//By filtering for this, two platforms under each other will not lead to odd overlaps.
			}
		}

		internal float2 FinalizePlatformMovement () {
			float2 returner = speed;
			resetState();
			return returner;
		}

		/// <summary>
		/// Is this character moving toward the wall?
		/// </summary>
		/// <param name="myCurrentSpeed">The speed I'm going. Should we factor the character's manual speed? I don't know.</param>
		/// <param name="wallSpeed">Speed the wall is going.</param>
		/// <returns><c>TRUE</c> if this character is moving towards the wall; <c>FALSE</c> otherwise.</returns>
		int compareVelocities (float myCurrentSpeed, float wallSpeed) {
			//So we can rotate everything so my movement is forwards.
			float mySpeedSign = math.sign(myCurrentSpeed);

			//Rotated so current movement is 1, is wall movement less than my current movement?
			return (wallSpeed * mySpeedSign).CompareTo(myCurrentSpeed * mySpeedSign);
		}

		/// <summary>
		/// Caches the collider used to provide weak movement.
		/// </summary>
		/// <param name="candidate">The current collider we're checking.</param>
		void assignWeakCollider (Collider2D candidate) {
			//If we've got no weak collider, assign this one, I guess.
			if (weakCollider == null) {
				weakCollider = candidate;
			}
			//If we've got one, re-evaluate if it's the right one.
			else {
				//@TODO: Select whether to favor same-direction or different-direction,
				//whether to favor higher speed or lower speed or both,
				//and of course don't even check for new ones if it's not desired.
			}
		}

		float2 valueOf (Velocity velocity) {
			if (velocity == null) {
				return 0;
			} else {
				return velocity.Value;
			}
		}

		void resetState () {
			strongCollider = weakCollider = null;
			strongSpeed = weakSpeed = 0;
			strongSpeedUsed = false;
		}


	}
}