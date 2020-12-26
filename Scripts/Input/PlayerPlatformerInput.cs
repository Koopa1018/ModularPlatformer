using UnityEngine;
#if UNITY_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using Clouds.PlayerInput;
using Clouds.Platformer.Character;

namespace Clouds.Platformer.CharacterControl {
	public class PlayerPlatformerInput : MonoBehaviour, IGenerateInputSignals {
		#if UNITY_NEW_INPUT_SYSTEM
		[Header("Inputs")]
		[SerializeField] InputActionAsset inputMap;
		[SerializeField] InputActionReference walkAction;
		[SerializeField] InputActionReference jumpAction;
		#else
		[Header("Inputs")]
		[SerializeField] string walkAxis = "Horizontal";
		[SerializeField] string jumpAxis = "Jump";
		#endif

		[Header("Output Fields")]
		[SerializeField] WalkInputField walkOutput;
		[SerializeField] JumpButtonField jumpOutput;

		#if UNITY_NEW_INPUT_SYSTEM
		void Awake () {
			inputMap.FindActionMap("Platforming", true).Enable();
		}
		#endif

		/// <summary>
		/// Fetch input from component data.
		/// NOTE: Sourcing it from here allows for easy switching out of control code:
		/// should you want AI controlled characters, just write to these from an
		/// enemy-AI component instead of a player-input-fetching component.
		/// </summary>
		public void GenerateInputSignal () {
			#if UNITY_NEW_INPUT_SYSTEM
				walkOutput.Value = walkAction.action.ReadValue<float>();
				jumpOutput.Value = jumpAction.action.ReadValue<float>() > 0;
			#else
				walkOutput.Value = Input.GetAxisRaw(walkAxis);
				jumpOutput.Value = Input.GetButton (jumpAxis);
			#endif

			//Due to a quirk of Unity's 2D system, we need to invert the X axis of walking.
			walkOutput.Value *= -1;
		}

		public void ClearInputSignal () {
			walkOutput.Value = 0;
			jumpOutput.Value = false;
		}

	}
}