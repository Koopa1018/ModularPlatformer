using UnityEngine;
using Unity.Mathematics;
using System.Collections;

using Clouds.Movement2D;

namespace Clouds.Platformer.Character {
	[RequireComponent(typeof(PlatformerSlopeCollisions))]
	[AddComponentMenu("Platformer/Collision/Character Slope Collider 2D")]
	public class RaycastSlopeCollider2D : MonoBehaviour {
		[SerializeField] float hSlopeCastHeight = 0.15f;
		PlatformerSlopeCollisions collisions;
		[Range(0,90)]
		[SerializeField] float maxSlopeAngle = 45f;

		void Awake () {
			collisions = GetComponent<PlatformerSlopeCollisions>();
		}

		public bool isClimbing () {
			return collisions.directionOnSlope == SlopeDirection.Climbing;
		}
		public bool canWalkOnSlope (float slopeAngle) {
			return slopeAngle <= maxSlopeAngle;
		}

		public void ClearCollisions() {
			collisions.Clear();
		}

		float2 HandleSlope (float yVelocity, float slopeAngle, float flatMoveAmount, bool descending, float flatAngle = 0) {
			float2 returner = 0;
			//Convert the slope angle to the flat-surface angle's space.
			slopeAngle -= flatAngle;

			//Calculate the Y displacement to apply.
			float yDisplacement = math.sin(math.radians(slopeAngle) ) * math.abs(flatMoveAmount);

			//If descending, subtract; otherwise, assign.
			returner.y = (descending? yVelocity : 0) + (yDisplacement * (descending ? -1 : 1));
			returner.x = math.cos(math.radians(slopeAngle)) * flatMoveAmount;

			return returner;
		}

#region Climbing
		public bool HandleClimbing (
			ref Velocity velocity,
			float2 velocityLastTime,
			float2 hitNormal,
			float hitDistance,
			float slopeAngle,
			float skinWidth,
			int moveDirX
		) {			
			//To not slow down in v-shaped valleys.
			if (collisions.directionOnSlope == SlopeDirection.Descending) {
				velocity.Value = velocityLastTime;
			}
			//To stick to the slope rather than float above slightly.
			float distanceToSlopeStart = 0;
			if (slopeAngle != collisions.lastAngle) {
				distanceToSlopeStart = hitDistance - skinWidth;
				velocity.x -= distanceToSlopeStart * moveDirX;
			}

			bool shouldForceGrounded = DoClimb(ref velocity, slopeAngle, hitNormal);

			//To ensure that the stick-to-slope thing doesn't affect us past climbing.
			velocity.x += distanceToSlopeStart * moveDirX;

			return shouldForceGrounded;
		}
		bool DoClimb (
			ref Velocity moveAmount,
			float slopeAngle,
			float2 slopeNormal
		) {
			//moveAmount X is the distance we want to go up the slope in total.
			//Our goal is to treat the surface as flat regardless of angle.
			//That way, we can apply speed scaling afterwards.

			float climbmoveAmountY = math.sin(math.radians(slopeAngle) ) * math.abs(moveAmount.x);

			//If movement is greater than climbed distance, we're jumping; abort.
			if (moveAmount.y > climbmoveAmountY) {
				return false;
			}

			//NOTE: this bit of assigning is duplicated in the static Slopes class.
			moveAmount.y = climbmoveAmountY;
			moveAmount.x = math.cos(math.radians(slopeAngle)) * moveAmount.x;

			//collisions.below = true;
			//Because since we're climbing, moveAmount.y is positive, so it only checks upwards!
			collisions.directionOnSlope = SlopeDirection.Climbing;
			collisions.angle = slopeAngle;
			collisions.normal = slopeNormal;
			
			return true;
		}

		public void preventWallhugJitters (ref Velocity velocity) {
			velocity.y = math.tan(math.radians(collisions.angle)) * math.abs(velocity.x);
			//Can't pass in slopeAngle, as that's out of date as of every raycast past 0.
		}
#endregion

#region Descending
		/// <summary>
		/// 
		/// </summary>
		/// <param name="moveAmount"></param>
		/// <param name="dimensions"></param>
		/// <param name="collisionMask"></param>
		/// <param name="skinWidth"></param>
		/// <returns><c>true</c> if we ought to force grounded; <c>false</c> otherwise.</returns>
		public RaycastHit2D TestDescending (
			Velocity moveAmount,
			in CharaCasterDimensions dimensions,
			int collisionMask,
			float skinWidth
		) {
			if (collisions.directionOnSlope == SlopeDirection.SlidingDown) {
				return default(RaycastHit2D);
			}

			int directionX = (int)math.sign(moveAmount.x);

			//Find the point on the character's back, since that's the part which is on the slope now.
			float2 rayOrigin = dimensions.BottomPoint(directionX == 0 ? 1 : directionX, skinWidth);
			//See, it's the back of the character, not the front....
			return Physics2D.Raycast(
				rayOrigin,
				Vector2.down,
				float.PositiveInfinity,
				collisionMask
			);
		}

		public bool DoDescendSlope(
			ref Velocity moveAmount,
			float skinWidth,
			float hitDistance,
			float2 hitNormal
		) {
			float slopeAngle = Vector2.Angle(hitNormal, Vector2.up);
			if (slopeAngle == 0 || slopeAngle > maxSlopeAngle) {
				return false;
			}
			if (math.sign(hitNormal.x) != (int)math.sign(moveAmount.x)) {
				return false;
			}

			float tangent = math.tan(math.radians(slopeAngle)) * math.abs(moveAmount.x);
			//Again, Sebastian's screen is big enough for him to just insert this tangent
			//stuff directly into the if condition, and mine isn't.
			//Also he doesn't have the explorer open in MonoDevelop. That probably helps.
			if (hitDistance - skinWidth > tangent) {
				return false;
			}

			//moveAmount += Slopes.Descend (moveAmount.x, slopeAngle);
			//NOTE TO SELF: DO NOT forget to update this using moveAmount.x's UNMODIFIED value.
			moveAmount.y -= math.sin(math.radians(slopeAngle)) * math.abs(moveAmount.x);
			moveAmount.x = math.cos(math.radians(slopeAngle)) * moveAmount.x;

			collisions.angle = slopeAngle;
			collisions.directionOnSlope = SlopeDirection.Descending;
			collisions.normal = hitNormal;
			
			return true;
		}

		public void PreventCeilingJitters (ref Velocity velocity) {
			if (collisions.directionOnSlope == SlopeDirection.Climbing) {
				//If angle is 0, tan(angle) == 0 which should not be divided by.
				//Thus, we abort.
				if (collisions.angle == 0) {
					return;
				}

				//Tangent is adjacent (Xcoord) divided by opposite (Ycoord).
				//Dividing Y by that, presumably, gives us our "ideal" X coordinate
				//and not just the one we expect.
				//Thus--it prevents jittering in the event that the Y coordinate is too small.
				velocity.x = velocity.y / math.tan(math.radians(collisions.angle)) *
								math.sign(velocity.x);
			}
		}
		public void CounteractJointCatching(
			ref Velocity moveAmount,
			float skinWidth,
			int collisionMask,
			in CharaCasterDimensions dimensions
		) {
			//To counteract "catching on" the join between two different angles of slope.
			if (collisions.directionOnSlope == SlopeDirection.Climbing) {
				int directionX = (int)math.sign(moveAmount.x);
				float rayLength = math.abs(moveAmount.x + skinWidth);

				float2 rayOrigin = dimensions.BottomPoint(directionX == 0 ? 1 : directionX, skinWidth);
				rayOrigin.y += moveAmount.y;

				RaycastHit2D hit = Physics2D.Raycast(
					rayOrigin,
					Vector2.right * directionX,
					rayLength,
					collisionMask
				);
				if (!hit) { return; }

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (slopeAngle != collisions.angle) {
					//Hit a new slope!
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					collisions.angle = slopeAngle;
					collisions.normal = hit.normal;
				}
			}
		}
#endregion
	}
}