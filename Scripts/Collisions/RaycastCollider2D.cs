using UnityEngine;
using Unity.Mathematics;

using Clouds.Movement2D;
using Clouds.Raycasting;

namespace Clouds.Platformer.Character {
	[RequireComponent(typeof(CharaCasterDimensions))]
	[RequireComponent(typeof(PlatformerCollisions), typeof(Clouds.Collision2D.CollisionRecoil))]
	[AddComponentMenu("Platformer/Collision/Character Collider 2D")]
	public class RaycastCollider2D : MonoBehaviour, Clouds.Collision2D.ICollisionHandler {
		[SerializeField] Collider2D myCollider;

		//[Header("Cast Dimensions Settings")]
		CharaCasterDimensions dimensions;
		[Header("Cast Dimensions Settings")]
		[SerializeField] float skinWidth = 0.015f;
		//[SerializeField] float maxStairStepHeight = 0.1f;

		[Header("Collision Output")]
		PlatformerCollisions collisions;
		Clouds.Collision2D.CollisionRecoil collisionRecoil;

		[Header("Layer Masks")]
		[SerializeField] LayerMask solid;
		[SerializeField] LayerMask semiSolid;

		[Header("Optional Components")]
		[SerializeField] RaycastSlopeCollider2D slopeHandler;
		[SerializeField] FallThroughSemiSolid fallThroughHandler;
		[SerializeField] MovingPlatformHandler movingPlatformHandler;

		bool hasSlopeHandler = false;
		bool hasFallThroughHandler = false;
		bool hasMovingPlatformHandler = false;
		int hCastDirection = 0;
		bool _overrideIsGrounded;

		void Awake () {
			//Configure Physics2D to work right!
			Physics2D.queriesStartInColliders = true;
			Physics2D.queriesHitTriggers = false;

			//IIRC, Unity's null checks are slower than one would expect? Thus, cache it.
			hasSlopeHandler = slopeHandler != null;
			hasFallThroughHandler = fallThroughHandler != null;
			hasMovingPlatformHandler = movingPlatformHandler != null;

			dimensions = GetComponent<CharaCasterDimensions>();

			collisions = GetComponent<PlatformerCollisions>();
			collisionRecoil = GetComponent<Clouds.Collision2D.CollisionRecoil>();
		}

		public void ForceGrounded () {
			_overrideIsGrounded |= true;
		}


		public void ApplyCollisions(ref Velocity velocity) {
			//Begin by CLEARING the collisions from last time!
			collisions.Clear();
			slopeHandler?.ClearCollisions();

			//For testing purposes: let's see where our centerpoint is.
			Debug.DrawLine(
				(Vector3)(Vector2)dimensions.MidCenter(skinWidth) - (Vector3.right * 0.1f),
				(Vector3)(Vector2)dimensions.MidCenter(skinWidth) + (Vector3.right * 0.1f),
				Color.blue
			);
			Debug.DrawLine(
				(Vector3)(Vector2)dimensions.MidCenter(skinWidth) - (Vector3.up * 0.1f),
				(Vector3)(Vector2)dimensions.MidCenter(skinWidth) + (Vector3.up * 0.1f),
				Color.blue
			);


			float2 unaltered = velocity.Value;
			bool didDescendSlope = false;
			bool didAscendSlope = false;

			//Descending slope check.
			//Needs to be before setting the H-cast direction, lest the X collision check
			//toward the slope rather than in the direction of any possible collisions (i.e. our move direction).
			if (velocity.Value.y <= 0 && hasSlopeHandler) {
				//@TODO: Can we cast early so this can share hit-data with VerticalCollisions?
				RaycastHit2D descenderSlope = slopeHandler.TestDescending(
					velocity,
					dimensions,
					solid | semiSolid,
					skinWidth
				);

				if (descenderSlope) {
					didDescendSlope = slopeHandler.DoDescendSlope(
						ref velocity,
						skinWidth,
						descenderSlope.distance,
						descenderSlope.normal
					);
				}				
			}

			//When idle, we want to keep casting the last X direction we moved.
			//So we store the sign of the X velocity ONLY if it's non-zero.
			if (velocity.Value.x != 0) {
				hCastDirection = (int)math.sign(velocity.Value.x);
			}

			//Moving platform processing, assuming we've got a moving-platform controller.
			if (hasMovingPlatformHandler) {
				velocity.Value += movingPlatformHandler.HandleMovingPlatforms(
					dimensions,
					skinWidth,
					myCollider,
					velocity.Value,
					hCastDirection,
					solid | semiSolid,
					LayerMaskWithSemiSolid(solid, semiSolid, (int)math.sign(velocity.y))
				);
			}

			//Collisions here
			//Debug.Log("Pre-X collision, velocity.y is " + velocity.y + " and position.y is " + transform.position.y);
			HorizontalCollisions(
				ref velocity,
				unaltered,
				hCastDirection,
				solid | semiSolid,
				out didAscendSlope
			);
			//Debug.Log("Pre-Y collision, velocity.y is " + velocity.y);
			if (velocity.y != 0) {
				VerticalCollisions(ref velocity);
			}

			//Debug.Log("Post-collision, velocity.y is " + velocity.y);


			//Force grounded if we're walking along a slope...
			collisions.Below |= didAscendSlope;
			collisions.Below |= didDescendSlope;
			//or if we're overridden.
			collisions.Below |= _overrideIsGrounded;
			//Don't force it again next frame.
			_overrideIsGrounded = false;

			//Output collision difference for the next frame to react with.
			collisionRecoil.Value = unaltered - velocity.Value;
				//(castArray[0].point.y - transform.position.y) * castArray[0].normal.y
		}

		

		void HorizontalCollisions (
			ref Velocity velocity,
			float2 velocityLastFrame,
			int directionX,
			int layerMask,
			out bool didAscendSlope
		) {
			//We store this so we can then reduce it and check against it.
			float rayLength = math.min(
				math.abs(velocity.x),
				skinWidth
			);
			rayLength += skinWidth;

			RaycastHit2D[] hits;
			{
				int side = directionX == 0 ? 1 : directionX;
				float2[] castPoints = new float2[] {
					dimensions.BottomPoint(side, skinWidth),
					dimensions.MidPoint(side, skinWidth),
					dimensions.TopPoint(side, skinWidth)
					};

				hits = RaycastProducer2D.testPoints(
					myCollider,
					Vector2.right * directionX, //dir
					castPoints,
					rayLength,
					layerMask,
					true
				);
			}
			
			//Initialize the did-ascend flag so we can return safely.
			didAscendSlope = false;

			for (int i = 0; i < hits.Length; i++) {
				//If we didn't hit anything, continue to next ray.
				if (hits[i].collider == null) {
					//Debug.Log($"Aborting hit {i} due to non-hit.");
					continue;
				}
				//If the ray ended up longer than an earlier one, continue to next ray.
				if (hits[i].distance > rayLength) {
					//Debug.Log($"Aborting hit {i} due to overextending.");
					continue;
				}
				//If we're in a wall, just ignore it...?
				//if (hits[i].distance == 0) {
				//	Debug.Log($"Aborting hit {i} due to distance = 0.");
				//	continue;
				//}

				if (hasSlopeHandler) {
					float slopeAngle = Vector2.Angle(hits[i].normal, Vector2.up);

					if (slopeHandler.canWalkOnSlope(slopeAngle)) {
						//Debug.Log("Handling slopes?");
						
						//Only check for slope ascension if this is the first cast (i.e. the floor cast).
						if (i == 0) {
							didAscendSlope = slopeHandler.HandleClimbing(
								ref velocity,
								velocityLastFrame,
								hits[i].normal, hits[i].distance,
								slopeAngle,
								skinWidth,
								directionX
							);
						}

						//The next code after this is for flat ground--works for descending or sliding (probably).
						//But if we're climbing, skip to the next ray.
						if (slopeHandler.isClimbing()) {
							continue;
						}
					}
				}

				//At this point, something's hit.

				//Write to left and right collisions.
				collisions.Left = directionX == -1;
				collisions.Right = directionX == 1;

				//NEW 1/17/2019: check if the change is less than current movement.
				//Should fix the problem of sporadically stopping when changing slopes.
				//Thanks Jose Manuel Larios Alonso (U2B comment, vid 4 "climbing slopes")!
				//Originals on the left, additions on the right.
				velocity.x = math.min(hits[i].distance - skinWidth, math.abs(velocity.x)) * directionX;

				//Debug.Log("Collision-disping Velocity X to: " + math.min(hits[i].distance - skinWidth, math.abs(velocity.x)) * directionX);

				//Reduce ray length so we don't check surfaces farther away than this one.
				rayLength = math.min(hits[i].distance, math.abs(velocity.x) + skinWidth);

				//To prevent the wallhug jitters~
				if (hasSlopeHandler && slopeHandler.isClimbing()) {
					slopeHandler.preventWallhugJitters(ref velocity);
				}
			}
		}

		int LayerMaskWithSemiSolid (LayerMask solid, LayerMask semiSolid, int verticalMovementSign) {
			//Save the collision mask we're using.
			int tempColMask = solid;
			//Conditionally include the semi-solids mask if we're going down.
			tempColMask |= math.select(
				0,
				semiSolid,
				verticalMovementSign == -1 && (hasFallThroughHandler? fallThroughHandler.TickTimer(Time.fixedDeltaTime) : true)
			);

			return tempColMask;
		}

		void VerticalCollisions (ref Velocity velocity) {
			//Get the direction we're currently moving vertically.
			float directionY = math.sign(velocity.y);
			//Get the distance of current move (plus skinWidth)--that's how far we'll cast!
			float rayLength = math.abs(velocity.y) + skinWidth;
//			Debug.Log("Ray length is " + rayLength);
			
			//Save the collision mask we're using.
			int tempColMask = LayerMaskWithSemiSolid(solid, semiSolid, (int)directionY);

			RaycastHit2D[] hits;
			{
				//Figure out where we're going to be casting from: top, or bottom?
				float2[] castPos = directionY == 1 ? 
					new float2[] {dimensions.TopLeft(skinWidth), dimensions.TopRight(skinWidth)}
					//new float2[] {dimensions.BottomCenter(skinWidth)}
					:
					new float2[] {dimensions.BottomLeft(skinWidth), dimensions.BottomRight(skinWidth)}
					//new float2[] {dimensions.TopCenter(skinWidth)}
				;
				//Displace it with movement amount, just to ensure it all holds together.
				castPos[0].x += velocity.x;
				castPos[1].x += velocity.x;

				hits = RaycastProducer2D.testPoints(
					myCollider,
					Vector2.up * directionY,
					castPos,
					rayLength,
					tempColMask,
					true
				);
			}


			//Iterate left to right(?) over each raycast, processing it if needed.
			for (int i = 0; i < hits.Length; i++) {
				//Now check: did we get a hit in this iteration?
				if (hits[i] && hits[i].distance <= rayLength) {
//					Debug.Log("Current collider hit: " + hits[i].collider);
					//If we did, reduce velocity.y to the distance from the hit.
					velocity.y = (hits[i].distance - skinWidth) * directionY;
					
					//Update ray distance for next round: if it's further than this one, we don't need it.
					rayLength = hits[i].distance;

					slopeHandler?.PreventCeilingJitters(ref velocity);

					//Since we hit something, set our collision flags.
					collisions.Below = directionY == -1;
					collisions.Above = directionY == 1;
				}
			}

			slopeHandler?.CounteractJointCatching(ref velocity, skinWidth, tempColMask, dimensions);
		}

	}
}