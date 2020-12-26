using Unity.Mathematics;

namespace Clouds.Platformer.Character {
	public enum VerticalOrigin {Bottom, Middle, Top};

	[System.Serializable]
	public struct CasterDimensions {
		public float2 extents; //Should edit as Size.
		public VerticalOrigin dimensionsOrigin;

		public float midpointOffset;
		public VerticalOrigin midpointOffsetOrigin;

		public CasterDimensions (float2 size, VerticalOrigin boxOrigin = VerticalOrigin.Bottom) {
			extents = size/2;
			dimensionsOrigin = boxOrigin;

			midpointOffset = 0;
			midpointOffsetOrigin = VerticalOrigin.Middle;
		}
		public CasterDimensions (
			float2 size,
			float verticalMidpointOffset,
			VerticalOrigin verticalMidpointOrigin = VerticalOrigin.Middle,
			VerticalOrigin boxOrigin = VerticalOrigin.Bottom
		) {
			extents = size/2;
			dimensionsOrigin = boxOrigin;

			midpointOffset = verticalMidpointOffset;
			midpointOffsetOrigin = verticalMidpointOrigin;
		}

		/// <summary>
		/// The width and height of the raycaster.
		/// </summary>
		public float2 Size { get => extents * 2; }

		
	}
}