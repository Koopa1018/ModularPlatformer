#if UNITY_NEW_INPUT_SYSTEM && FORCE_UNITY_INPUTMANAGER
	#undef UNITY_NEW_INPUT_SYSTEM
#endif

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
		[SerializeField] string actionMapName = "Platforming";
		[SerializeField] InputActionReference walkAction;
		[SerializeField] InputActionReference jumpAction;
		#else
		[Header("Inputs")]
		[SerializeField] string walkAxis = "Horizontal";
		[SerializeField] string jumpAxis = "Jump";
		#endif

		[Header("Tweaks")]
		[SerializeField] bool invertXInput = true;

		[Header("Output Fields")]
		[SerializeField] WalkInputField walkOutput;
		[SerializeField] JumpButtonField jumpOutput;

		#if UNITY_NEW_INPUT_SYSTEM
		void Awake () {
			inputMap.FindActionMap(actionMapName, true).Enable();
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
				walkOutput.Value = walkAction != null ? walkAction.action.ReadValue<float>() : 0;
				jumpOutput.Value = jumpAction != null ? jumpAction.action.ReadValue<float>() > 0 : false;
			#else
				walkOutput.Value = Input.GetAxisRaw(walkAxis);
				jumpOutput.Value = Input.GetButton (jumpAxis);
			#endif

			//Due to a quirk of Unity's 2D system, we need to invert the X axis of walking.
			walkOutput.Value *= invertXInput ? -1 : 1;
		}

		public void ClearInputSignal () {
			walkOutput.Value = 0;
			jumpOutput.Value = false;
		}

	}
}