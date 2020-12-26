using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

using Clouds.Movement2D;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Platformer/Collision/Behaviors/Can Fall Through Semi-Solid")]
	public class FallThroughSemiSolid : MonoBehaviour {
		/*[System.Flags]
		public enum IgnorePlatforms {
			UntilTimer = 1,
			UntilGrounded = 2,
			UntilTimerOrGrounded = 3
		};*/

		//[SerializeField] IgnorePlatforms ignoreThesePlatforms = IgnorePlatforms.UntilTimer;
		[SerializeField] float waitForThisLong = 0.1f;

		float timer = 0.0f;
		internal Collider2D ignoredCollider = null;

		/// <summary>
		/// Begins falling through platforms. If a collider is given, that collider will be fallen through; otherwise, all colliders will be fallen through.
		/// </summary>
		/// <param name="colToIgnore">The collider to fall through. <c>null</c> is acceptable; if given, all colliders will be fallen through.</param>
		public void BeginFallThrough(Collider2D colToIgnore = null) {
			timer = waitForThisLong; /*math.select(
				waitForThisLong,
				0,
				ignoreThesePlatforms == IgnorePlatforms.UntilGrounded
			);*/
			ignoredCollider = colToIgnore;
		}

		/// <summary>
		/// Advances the fall-through-semi-solids timer.
		/// </summary>
		/// <param name="timestep">The amount to advance the time by. Should match whatever delta-time you use.</param>
		/// <returns><c>TRUE</c> if the timer is expired; <c>FALSE</c> otherwise.</returns>
		public bool TickTimer (float timestep) {
			/*if (!ignoreThesePlatforms.HasFlag(IgnorePlatforms.UntilTimer)) {
				return false;
			}*/

			timer = math.max(0, timer - timestep);
			return timer <= 0;
		}

	}
}