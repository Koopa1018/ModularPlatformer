using UnityEngine;
using Unity.Mathematics;

using Clouds.Movement2D;
using Clouds.Raycasting;

namespace Clouds.Platformer.Character {
	public static class RaycastProducer2D {
		/// <summary>
		/// Cast rays in one direction from a series of points--one per given point.
		/// If no points are given, or the point array is uninitialized, no rays will be cast.
		/// </summary>
		/// <param name="castDirection">The direction all casts will go in.</param>
		/// <param name="castOrigins">The points from which the casts will originate.</param>
		/// <param name="rayLength">The length of each cast.</param>
		/// <param name="rayLayerMask">The mask the casts will check against.</param>
		/// <returns>An array of RaycastHit2Ds as long as <c>castOrigins</c>.
		/// If <c>castOrigins</c> is uninitialized or its length is zero,
		/// the length of the returned array will be zero.</returns>
		public static RaycastHit2D[] testPoints (
			Collider2D collider,
			float2 castDirection,
			float2[] castOrigins,
			float rayLength,
			int rayLayerMask,
			bool debugCastVaryLength = true
		) {
			if (castOrigins == null || castOrigins.Length == 0) {
				return new RaycastHit2D[0];
			}

			RaycastHit2D[] returner = new RaycastHit2D[castOrigins.Length];

			//Allocate two hits to store, in case the first is us.
			RaycastHit2D[] castScratchpad = new RaycastHit2D[2];

			for (int i = 0; i < castOrigins.Length; i++) {
				//Do a cast.
				Physics2D.RaycastNonAlloc(
					castOrigins[i],
					castDirection,
					castScratchpad,
					rayLength,
					rayLayerMask
				);
				//Filter out our collider.
				returner[i] = castScratchpad.FirstNonSelfHit(collider);
				//Wipe the cast scratchpad so the next iteration doesn't have residual collision hits.
				castScratchpad.Clear();

				//Debug.Log($"Collision cast {i} is {returner[i].collider}.");

				//Draw collision rays.
				Debug.DrawRay(
					(Vector2)castOrigins[i],
					(Vector2)castDirection * (debugCastVaryLength ? rayLength : 1),
					Color.red
				);
			}

			return returner;
		}
				

}	}