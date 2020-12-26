using UnityEngine;
using Unity.Mathematics;
using Clouds.Movement2D;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Platformer/Character/Lateral Movement/Simple Acceleration Signal")]
	public class SimpleAccelerationSignal : MonoBehaviour, ILateralMoveSignal {
		[SerializeField] AnimationCurve accelCurve = AnimationCurve.Linear(0,0,1,1);
		[SerializeField] float startTime = 0.1f;

		float accelTimer;
		float lastInput = 0;
		float lastVelocity = 0;

		public float GetSignal (
			float velocity,
			float timestep,
			float rawInput,
			int facingDirection
		) {
			float accelMul; //To avoid GC allocs.
			float sampledCurve; //To avoid GC.

			//If the input changed, set things up for turning around.
			if (rawInput != lastInput) {
				lastInput = rawInput; //Don't need to update this every frame, do we?
				lastVelocity = velocity;
				accelTimer = 1;
			}

			//Here's where we set animation parameter "MoveSpeed"
			//to math.abs(rawInput)

			rawInput *= timestep;

			//accelMul = walkState.isRunning ? 1/accelTimeWalk : 1/accelTimeRun;
			accelMul = startTime;//For simplicity.

			accelTimer -= timestep * accelMul;
			accelTimer = math.max(0, accelTimer);

			if (accelTimer > 0) {
				//NOTE: I tested this b4, and it's apparently more stable to use
				//animation curves rather than hardcoded exponential?
				sampledCurve = accelCurve.Evaluate(1 - accelTimer);
				
				velocity = math.lerp(lastVelocity, rawInput, sampledCurve);
				//accelTimer is clamped to 0, so using == is not a problem.
				if (accelTimer == 0) {
					lastVelocity = rawInput;
				}
			}

			return velocity;
		}
	}
}