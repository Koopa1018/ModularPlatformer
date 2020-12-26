using UnityEngine;
using Unity.Mathematics;
using Clouds.Movement2D;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Platformer/Character/Lateral Movement/Fancy Acceleration Signal")]
	public class FancyAccelerationSignal : MonoBehaviour, ILateralMoveSignal {
		[SerializeField] AnimationCurve accelCurve = AnimationCurve.Linear(0,0,1,1);
		[SerializeField] AccelerationSpeeds accelerationSpeeds = new AccelerationSpeeds(5f, 100);

		float lastSpeed;
		float lastVelocity;

		float accelTimer;
		AccelerationState accelState;
		float turnAroundTimer;
		float turnAroundSign; //Both 32-bit, no reason to use int.

		public float GetSignal (
			float velocity,
			float timestep,
			float rawSpeed,
			int facingDirection
		) {
			float sampledCurve; //To avoid GC.
			//TODO: Add a SetFacingDirectionFromInput system.
			//Would run after this one.

			rawSpeed *= timestep;

			//If the input changed, set things up for turning around.
			if (rawSpeed != lastSpeed) {
				if (lastSpeed == 0) {
					accelState = AccelerationState.Start;
				} else {
					accelState = AccelerationState.Stop;
//					Debug.Log("Stopping - input: " + rawSpeed + " lastInput: " + lastSpeed);
					turnAroundSign = math.sign(lastSpeed);
				}
				accelTimer = 0;

				lastSpeed = rawSpeed; //Don't need to update this every frame, do we?
				lastVelocity = velocity;
			}

			//Here's where we set animation parameter "parms.moveSpeed"
			//to math.abs(rawSpeed)

			float accelTime = accelerationSpeeds.SelectByState(accelState, timestep);
			//NOTE: if math.rcp doesn't return the reciprocal, just use 1/value.
			float accelMul = math.rcp(accelerationSpeeds.SelectByState(accelState, timestep));
			//Debug.Log("Accel mul (math.rcp(info...)) is " + accelMul + ". As 1/info..., it's " +
			//	1 / info.accelSpeeds.GetAccelerationTime(accelState, timestep) + "."
			//);

			TickTimers(timestep, accelTime);

			if (accelState != AccelerationState.None) {
				//NOTE: I tested this b4, and it's apparently more stable to use
				//animation curves rather than hardcoded exponential?
				sampledCurve = accelCurve.Evaluate(accelTimer / accelTime);
				//Unfortunately, the Job System demands that we not use animation curves.

				velocity = math.lerp(lastVelocity, rawSpeed, sampledCurve);
				//accelTimer is clamped to 0, so using == is not a problem.
				if (accelTimer == 0 || sampledCurve == 1) {
					lastVelocity = rawSpeed;
					
					//Check for a turn around.
					//WARNING: can't check negativity due to possible 0.
					//Instead check non-positivity.
					if (math.sign(rawSpeed) == -turnAroundSign) {
						//Debug.Log("Finishing turn; last input was " + lastSpeed + ", current is " + rawSpeed);
						accelState = AccelerationState.TurningFinish;
						accelTimer = accelerationSpeeds.turnFinishSpeed;
					}
					//Check if coming out of a dash start.
					/*else if (accelState == AccelerationState.Start && isDashing) {
						accelState = AccelerationState.DashStart;
						accelTimer = info.accelSpeeds.dashStartSpeed;
					}*/
					else {
						accelState = AccelerationState.None;
					}
				}
			}

			return velocity;
		}

		void TickTimers (float timestep, float accelTime) {
			//Count up the acceleration timer.
			accelTimer += timestep;
			accelTimer = math.min(accelTime, accelTimer);

			//Count down the turnaround timer.
			turnAroundTimer -= timestep;
			turnAroundTimer = math.max(turnAroundTimer, 0);
			turnAroundSign = math.select(turnAroundSign, 0, turnAroundTimer <= 0);
		}

	}
}