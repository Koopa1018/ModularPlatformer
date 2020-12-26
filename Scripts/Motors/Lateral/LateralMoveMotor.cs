using UnityEngine;
using Unity.Mathematics;

using Clouds.MovementBase;
using Clouds.Movement2D;
using Clouds.Facing2D;
using Clouds.PlayerInput;

namespace Clouds.Platformer.Character {
	[RequireComponent(typeof(LateralMoveSpeed), typeof(ScalarVelocity))]
	[AddComponentMenu("Platformer/Character/Lateral Movement/Lateral Movement")]
	public class LateralMoveMotor : MonoBehaviour {
		[Header("Components To Read")]
		[SerializeField] ScalarInputField input;
		[SerializeField] Velocity velocity;
		[SerializeField] FacingDirection_8Way facing;
		LateralMoveSpeed speed;

		[Header("Components To Write")]
		[SerializeField] ScalarVelocity lateralVelocity;

		[Header("Optional Components")]
		[SerializeReference] ILateralMoveSignal smoothedSignalGenerator;
		bool hasLateralSignal = false;

		void Awake () {
			smoothedSignalGenerator = GetComponent<ILateralMoveSignal>();

			speed = GetComponent<LateralMoveSpeed>();

			hasLateralSignal = smoothedSignalGenerator != null;
		}
		
		void FixedUpdate () {
			if (hasLateralSignal) {
				//Generate a smooth lateral movement signal.
				lateralVelocity.Value = smoothedSignalGenerator.GetSignal(
					lateralVelocity.Value,
					Time.fixedDeltaTime,
					speed.GetSpeedBySign((int)math.sign(input)),
					facing.x
				);
			} else {
				//Make the simplest movement signal possible.
				lateralVelocity.Value = speed.GetSpeedBySign((int)math.sign(input));
				lateralVelocity.Value *= Time.fixedDeltaTime;
			}

			velocity.x += lateralVelocity.Value;
		}
		
	}
}