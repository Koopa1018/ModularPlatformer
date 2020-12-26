using UnityEngine;
using Unity.Mathematics;
using System.Collections;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Hidden/Platformer/Collision/Slope Collision State 2D")]
	public class PlatformerSlopeCollisions : MonoBehaviour {
		public float angle {get; set;}
		public float lastAngle {get; set;}
		public float2 normal {get; set;}

		public SlopeDirection directionOnSlope {get; set;}

		public bool slidingDownMax () {
			return directionOnSlope == SlopeDirection.SlidingDown;
		}

		public void Clear () {
			angle = lastAngle = 0;
			normal = 0;
			directionOnSlope = SlopeDirection.None;
		}
	}
}