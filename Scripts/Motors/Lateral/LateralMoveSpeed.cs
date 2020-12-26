using UnityEngine;
using Unity.Mathematics;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Hidden/Platformer/Character/Lateral Movement/Lateral Move Speed")]
	public class LateralMoveSpeed : MonoBehaviour {
		[SerializeField] float speed = 5f; //To be hidden when an overriding component exists!

		float speedMultiply_L = -1;
		float speedMultiply_I = 0;
		float speedMultiply_R = 1;

		float addedValue_L = 0.0f;
		float addedValue_I = 0.0f;
		float addedValue_R = 0.0f;

		/// <summary>
		/// Sets the base movement speed, in units per second.
		/// </summary>
		/// <param name="newSpeed">The speed to switch to.</param>
		public void SetSpeed (float newSpeed) {
			speed = newSpeed;
		}

		/// <summary>
		/// Sets the values to multiply movement by in each direction.
		/// These absolutely don't need to be limited to (-1, 1), or even to within that range.
		/// Go crazy, man.
		/// </summary>
		/// <param name="goingL">What to multiply the base speed by when going left. Typically this would be a negative value.</param>
		/// <param name="goingR">What to multiply the base speed by when going right. Typically this would be a positive value.</param>
		/// <param name="idle"> What to multiply the base speed by when standing idle. Typically this would be near zero.</param>
		public void SetMultipliers (float goingL = -1, float goingR = 1, float idle = 0) {
			speedMultiply_L = goingL;
			speedMultiply_I = idle;
			speedMultiply_R = goingR;
		}
		/// <summary>
		/// Sets the values to multiply movement by in each direction.
		/// These absolutely don't need to be limited to (-1, 1), or even to within that range.
		/// Go crazy, man.
		/// </summary>
		/// <param name="multipliers">What to multiply the base speed by in each direction.
		/// X is left, and would usually be negative. Y is idle, and is usually close to zero.
		/// Z is right, which typically should be positive.</param>
		public void SetMultipliers (float3 multipliers) {
			SetMultipliers(multipliers.x, multipliers.z, multipliers.y);
		}

		/// <summary>
		/// Sets the constant value to be added to each frame's movement, regardless of direction.
		/// Intended to implement sliding down slopes, but it certainly doesn't _have_ to be used for that.
		/// </summary>
		/// <param name="valToAdd">The value which will be added each frame.</param>
		public void SetAddedValue (float valToAdd) { 
			addedValue_L = valToAdd;
			addedValue_I = valToAdd;
			addedValue_R = valToAdd;
		}
		/// <summary>
		/// Sets the constant value to be added to each frame's movement, dependent on direction.
		/// Intended to implement sliding down slopes, but it certainly doesn't _have_ to be used for that.
		/// An anticipated use case is to have slope climbing/descending, but not standing, affected by angle.
		/// </summary>
		/// <param name="leftVal">The value which will be added each frame the character is moving left.</param>
		/// <param name="rightVal">The value which will be added each frame the character is moving right.</param>
		/// <param name="idleVal">The value which will be added each frame the character is standing idle.</param>
		public void SetAddedValue (float leftVal, float rightVal, float idleVal = 0) { 
			addedValue_L = leftVal;
			addedValue_I = idleVal;
			addedValue_R = rightVal;
		}
		/// <summary>
		/// Sets the constant value to be added to each frame's movement, dependent on direction.
		/// Intended to implement sliding down slopes, but it certainly doesn't _have_ to be used for that.
		/// </summary>
		/// <param name="vals">The value which will be added in each direction.
		/// X is left; Y is idle; Z is right.</param>
		public void SetAddedValue (float3 vals) { 
			addedValue_L = vals.x;
			addedValue_I = vals.y;
			addedValue_R = vals.z;
		}

		/// <summary>
		/// Gets the speeds of each direction that can be gone, with all multipliers factored and constants added.
		/// X is left; Y is idle; Z is right.
		/// Please note: this value has not been multiplied by deltaTime, meaning this is your responsibility.
		/// </summary>
		/// <returns>A 3-wide float vector containing the move speed for each direction.</returns>
		public float3 GetSpeedsAllSigned () {
			return speed
				* new float3(speedMultiply_L, 	speedMultiply_I,	speedMultiply_R)
				+ new float3(addedValue_L, 		addedValue_I, 		addedValue_R)
			;
		}
		/// <summary>
		/// Gets the speed of a single direction the player can go, with all multipliers factored and constants added.
		/// When all speeds are going to be needed, fetching them all using <c>GetSpeedsAllSigned()</c> will be more
		/// efficient (for this juncture--your own code may include the same branching that is present in this method).
		/// Please note: the return value has not been multiplied by deltaTime, meaning this is your responsibility.
		/// </summary>
		/// <param name="sign">The direction to get the speed of going.</param>
		/// <returns>The move speed for the direction which matches the passed value's sign.</returns>
		public float GetSpeedBySign (int sign) {
			return sign == 0 ? 
				speed * speedMultiply_I + addedValue_I
			: 
				math.select (
					speed * speedMultiply_L + addedValue_L,
					speed * speedMultiply_R + addedValue_R,
					sign < 0
				)
			;
		}
		
	}
}