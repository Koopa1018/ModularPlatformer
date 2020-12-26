using UnityEngine;
using Unity.Mathematics;
using System.Collections;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Platformer/Character/Lateral Movement/Change Speed On Slopes")]
	public class ChangeSpeedOnSlopes : MonoBehaviour {
		public enum CurveResponse {
			AddForceAlways,
			AddForceWhenMoving,
			MultiplyWhenMoving
		};

		[SerializeField] LateralMoveSpeed target;
		[SerializeField] PlatformerCollisions collisions;

		[Header("Curve Response")]
		[SerializeField] AnimationCurve angleResponse = AnimationCurve.Linear(0,0,1,1);
		[SerializeField] CurveResponse interpretCurveAs = CurveResponse.AddForceWhenMoving;
		[SerializeField] float addValueMax = 2f;

		[Header("Tweaks")]
		[SerializeField] JumpResponse onBecameAirborne;

		/// <summary>
		/// A value representing the absolute angle of the slope. 0 should == flat; 1 should == perfect wall.
		/// </summary>
		float currentSlope = 0;

		/// <summary>
		/// Stores the signed angle of a slope, with 0* representing flat ground and +90* representing a normals-completely-left surface.
		/// </summary>
		/// <param name="anglePredivided">The signed angle to be stored. This ought to be given as (degrees/90).</param>
		public void SetCurrentSlope (float anglePredivided) {
			currentSlope = anglePredivided;
		}

		void FixedUpdate () {
			//If we're NOT on the ground--respond as requested.
			if (!collisions.Below) {
				if (onBecameAirborne == JumpResponse.ClearOnAirborne) {
					//If we're set to reset to normal on airborne, reset values!
					target.SetAddedValue(0);
					target.SetMultipliers();
				}
				
				//Abort.
				return;
			}

			//Our slope response can take multiple forms.
			switch (interpretCurveAs) {
				case CurveResponse.AddForceAlways:
					target.SetAddedValue(angleResponse.Evaluate(currentSlope));
					break;
				case CurveResponse.AddForceWhenMoving:
					float added = angleResponse.Evaluate(currentSlope);
					target.SetAddedValue(added, added, 0);
					break;
				case CurveResponse.MultiplyWhenMoving:
					float multiplierValueL = angleResponse.Evaluate(currentSlope);
					float multiplierValueR = multiplierValueL;
					//Reciprocate the downhill-facing value--we want to go downhill _faster._
					if (currentSlope < 0) {
						multiplierValueR = math.rcp(multiplierValueR);
					} else if (currentSlope > 0) {
						multiplierValueL = math.rcp(multiplierValueL);
					}

					target.SetMultipliers(multiplierValueL, multiplierValueR, 0);

					break;
			}
		}

	}
}