using Unity.Mathematics;

namespace Clouds.Platformer.Character {
	public struct ForcedJump {
		/// <summary>
		/// The height the jump will attain by the apex.
		/// </summary>
		public float height;
		/// <summary>
		/// The time the jump will take to attain its apex height.
		/// Some systems (case in point: jumping) change gravity on the peak being attained;
		/// as such, the time post-apex should not be assumed equal to this value.
		/// </summary>
		public float duration;

		/// <summary>
		/// Constructs a new Forced Jump with a given height and duration.
		/// </summary>
		/// <param name="height">How high will the jump get? Will always be clamped to lowest value of 0.</param>
		/// <param name="duration">How long until the apex of the jump? Will always be clamped to lowest value of 0.</param>
		public ForcedJump (float height, float duration) {
			this.height = math.max(height, 0);
			this.duration = math.max(duration, 0);
		}

		/// <summary>
		/// Combines this forced jump with another, taking the larger height and duration from each.
		/// </summary>
		/// <param name="other">The forced-jump to combine with.</param>
		public void CombineWith (ForcedJump other) {
			height = math.max(this.height, other.height);
			duration = math.max(this.duration, other.duration);

			//Combining a ForcedJump with another will never, ever
			//create a value which is negative!
		}

		public static explicit operator ForcedJump (float me) {
			ForcedJump returner = new ForcedJump();
			//@TODO: This is converting from Power, not Height.
			returner.height = me;
			returner.duration = math.abs(me);
			return returner;
		}
	}

	public static class ForcedJumpUtility {
		public static void PrepareForcedJump(ref ForcedJump tJump, out float gravity) {
			gravity = -(2*tJump.height) / math.pow(tJump.duration, 2);
		}
	}
	
}