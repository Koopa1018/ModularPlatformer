using UnityEngine;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Hidden/Platformer/Collision/Character Collider Dimensions")]
	public class CharaCasterDimensions : MonoBehaviour {
		static readonly CasterDimensions DEFAULT_DIMENSIONS = new CasterDimensions(new float2(0.75f, 1.5f));
		
		[SerializeField] CasterDimensions dimensions;
		internal CasterDimensions initialDimensions = DEFAULT_DIMENSIONS;

		void Awake () {
			dimensions = initialDimensions;
		}
		
		/// <summary>
		/// Set raycaster dimensions.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDimensions (CasterDimensions newDimensions) {
			dimensions = newDimensions;
		}
		/// <summary>
		/// Begins gradually changing the dimensions of the raycaster. Time will step in fixedDeltaTime-sized increments.
		/// NOTICE: Box origin will not change until the process ends.
		/// At the moment, neither will the midpoint position, though this is subject to change.
		/// For best results, the original and new dimensions should share an origin setting.
		/// </summary>
		/// <param name="timeToComplete">The time it will take to complete the transition.</param>
		public System.Collections.IEnumerator SetDimensionsGradual (CasterDimensions newDimensions, float timeToComplete) {
			CasterDimensions originalDimensions = dimensions;

			for (float i = 0; i < timeToComplete; i += Time.fixedDeltaTime) {
				float2 extents = math.lerp(originalDimensions.extents, newDimensions.extents, math.clamp(i,0,1));

				dimensions.extents = extents;
				yield return new WaitForFixedUpdate();
			}
			
			//The gradual change has finished; carry over all remaining changes which cannot be lerped effectively.
			dimensions = newDimensions;
		}
		/// <summary>
		/// Revert raycaster dimensions to the value set in the editor.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RevertDimensions () {
			dimensions = initialDimensions;
		}

		/// <summary>
		/// The width and height of the raycaster.
		/// </summary>
		public float2 Size { get => dimensions.Size; }
		/// <summary>
		/// Half the width and height of the raycaster.
		/// </summary>
		public float2 Extents {get => dimensions.extents;}

		[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float2 rightPoint(float2 input, float sw) => sidePoint(input, 1, sw);
		[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float2 leftPoint (float2 input, float sw) => sidePoint(input, -1,sw);
		private float2 sidePoint (float2 input, int side, float sw) {
			return input + (hSkinWidthed(Extents, sw) * new float2(math.sign(side), 0));
		}
		/// <summary>
		/// Factors in horizontal skin width.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="skinWidth"></param>
		/// <returns></returns>
		[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float2 hSkinWidthed (float2 input, float skinWidth) {
			return input - new float2(skinWidth * math.sign(input.x), 0);
		}

		/// <summary>
		/// Calculates, in world space, the point at the bottom and in the center of the raycaster.
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's bottom-center point.</returns>
		public float2 BottomCenter (float skinWidth) {
			float2 position = ((float3)transform.position).xy;

			switch (dimensions.dimensionsOrigin) {
				case VerticalOrigin.Bottom:
					break;
				case VerticalOrigin.Middle:
					position.y -= dimensions.extents.y;
					break;
				case VerticalOrigin.Top:
					position.y -= Size.y;
					break;
			};

			//UnityEngine.Debug.Log("Pre-skin, the Bottom Center point is " + position.y);
			position.y += skinWidth;
			//UnityEngine.Debug.Log("Post-skin, the Bottom Center point is " + position.y);

			return position;
		}
		private float2 _MidCenterUnaltered (float2 position) {
			switch (dimensions.dimensionsOrigin) {
				case VerticalOrigin.Bottom:
					position.y += dimensions.extents.y;
					break;
				case VerticalOrigin.Middle:
					break;
				case VerticalOrigin.Top:
					position.y -= dimensions.extents.y;
					break;
			};

			return position;
		}
		/// <summary>
		/// Calculates, in world space, the point in the center of the raycaster (with vertical offset applied).
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's middle-center point (with vertical offset applied).</returns>
		public float2 MidCenter (float skinWidth) {
			float2 position = ((float3)transform.position).xy;
			float2 center = new float2(0, dimensions.midpointOffset);

			switch (dimensions.midpointOffsetOrigin) {
				case VerticalOrigin.Bottom:
					center += BottomCenter(skinWidth);
					break;
				case VerticalOrigin.Middle:
					center += _MidCenterUnaltered(position);
					break;
				case VerticalOrigin.Top:
					center += TopCenter(skinWidth);
					break;
			};

			return center;
		}
		/// <summary>
		/// Calculates, in world space, the point at the top and in the center of the raycaster.
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's top-center point.</returns>
		public float2 TopCenter (float skinWidth) {
			float2 position = ((float3)transform.position).xy;

			switch (dimensions.dimensionsOrigin) {
				case VerticalOrigin.Bottom:
					position.y += Size.y;
					break;
				case VerticalOrigin.Middle:
					position.y += Extents.y;
					break;
				case VerticalOrigin.Top:
					break;
			};

			position.y -= skinWidth;

			return position;
		}
		/// <summary>
		/// Calculates, in world space, the center point of the actual collision box.
		/// </summary>
		public float2 BoxEpicenter () { 
			return math.lerp(BottomRight(0), TopLeft(0), 0.5f);
		}

		/// <summary>
		/// Calculates, in world space, the point at the bottom right of the raycaster.
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's bottom-right point.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 BottomRight 	(float skinWidth) => 
			rightPoint(BottomCenter(skinWidth), skinWidth);
		/// <summary>
		/// Calculates, in world space, the point at the bottom left of the raycaster.
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's bottom-left point.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 BottomLeft 	(float skinWidth) =>
			leftPoint (BottomCenter(skinWidth), skinWidth);
		/// <summary>
		/// Calculates, in world space, the point on the right and in the center of the raycaster (with vertical offset applied).
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's middle-right point (with vertical offset applied).</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 MidRight 		(float skinWidth) =>
			rightPoint(MidCenter(skinWidth), skinWidth);
		/// <summary>
		/// Calculates, in world space, the point on the left and in the center of the raycaster (with vertical offset applied).
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's middle-left point (with vertical offset applied).</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 MidLeft		(float skinWidth) =>
			leftPoint (MidCenter(skinWidth), skinWidth);
		/// <summary>
		/// Calculates, in world space, the point at the top right of the raycaster.
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's top-right point.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 TopRight 		(float skinWidth) => 
			rightPoint(TopCenter(skinWidth), skinWidth);
		/// <summary>
		/// Calculates, in world space, the point at the top left of the raycaster.
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <returns>The world space position of the raycaster's top-left point.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 TopLeft		(float skinWidth) => 
			leftPoint (TopCenter(skinWidth), skinWidth);

		/// <summary>
		/// Calculates, in world space, a point on the bottom of the raycaster.
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <param name="side">The side to sample the point from.
		/// Only the sign matters: + is right; 0 is center; - is left. </param>
		/// <returns>The world space position of the requested point on the bottom of the raycaster.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 BottomPoint (int side, float skinWidth) {
			return sidePoint(BottomCenter(skinWidth), side, skinWidth);
		}
		/// <summary>
		/// Calculates, in world space, a point on the horizontal midline of the raycaster (with vertical offset applied).
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <param name="side">The side to sample the point from.
		/// Only the sign matters: + is right; 0 is center; - is left. </param>
		/// <returns>The world space position of the requested point on the midline of the raycaster (with vertical offset applied).</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 MidPoint (int side, float skinWidth) {
			return sidePoint(MidCenter(skinWidth), side, skinWidth);
		}
		/// <summary>
		/// Calculates, in world space, a point on the top of the raycaster.
		/// </summary>
		/// <param name="position">The world space position of the caster's game object.</param>
		/// <param name="side">The side to sample the point from.
		/// Only the sign matters: + is right; 0 is center; - is left. </param>
		/// <returns>The world space position of the requested point on the top of the raycaster.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float2 TopPoint (int side, float skinWidth) {
			return sidePoint(TopCenter(skinWidth), side, skinWidth);
		}
	}
}