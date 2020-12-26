using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

using UnityEngine;

namespace Clouds.Platformer.Character {
	[System.Serializable]
	public struct AccelerationSpeeds {
		public float startSpeed;
		public float stopSpeed;
		public float turnBeginSpeed;
		public float turnFinishSpeed;
		/*public float dashStartSpeed;
		public float dashStartSpeedFast;
		public float dashToWalkingSpeed;
		public float dashStopSpeed;
		public float dashTurnBeginSpeed;
		public float dashTurnFinishSpeed;*/

		public AccelerationSpeeds(float StartSpeed, float StopSpeed) {
			startSpeed = StartSpeed;
			stopSpeed = StopSpeed;
			turnBeginSpeed = StartSpeed;
			turnFinishSpeed = StopSpeed;
		}

		public AccelerationSpeeds(
			float StartSpeed, float StopSpeed,
			float TurnBeginSpeed, float TurnFinishSpeed
		) {
			startSpeed = StartSpeed;
			stopSpeed = StopSpeed;
			turnBeginSpeed = TurnBeginSpeed;
			turnFinishSpeed = TurnFinishSpeed;
		}

		//@TODO: Definitely Burstable. That ref's not on here, though.
		public float SelectByState (AccelerationState accelState, float deltaTime) {
			float returner;
			switch (accelState) {
				case AccelerationState.Start:
					returner = startSpeed;
					break;
				case AccelerationState.Stop:
					returner = stopSpeed;
					break;
				case AccelerationState.TurningStart:
					returner = turnBeginSpeed;
					break;
				case AccelerationState.TurningFinish:
					returner = turnFinishSpeed;
					break;
				default:
					returner = float.Epsilon;
					break;
			}
			//Can't have a zero value--we divide by this number!
			//Thus, give us the lowest nonzero value.
			return math.select(returner, deltaTime, returner == 0);
		}
	}
}