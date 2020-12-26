using UnityEngine;
using Unity.Mathematics;
using System.Collections;

namespace Clouds.Platformer.Character {
	[DisallowMultipleComponent]
	[AddComponentMenu("Platformer/Collision/Behaviors/Set Character Collider Dimensions")]
	public class SetCasterDimensions : MonoBehaviour {
		[SerializeField] internal CharaCasterDimensions target;
		[SerializeField] internal CasterDimensions newDimensions = new CasterDimensions(new float2(0.75f, 1.5f));

		public void SetDimensions () {
			//VALIDATE: Is the target field set? If not, abort.
			if (target == null) {
				Debug.LogError("No destination raycaster is set for writing dimensions to.", this);
				return;
			}

			target.SetDimensions(newDimensions);
		}

		/// <summary>
		/// Begins gradually changing the dimensions of the raycaster. Time will step in fixedDeltaTime-sized increments.
		/// NOTICE: Box origin will not change until the process ends.
		/// At the moment, neither will the midpoint position, though this is subject to change.
		/// For best results, the original and new dimensions should share an origin setting.
		/// </summary>
		/// <param name="timeToComplete">The time it will take to complete the transition.</param>
		public IEnumerator SetDimensionsGradual (float timeToComplete) {
			return target.SetDimensionsGradual(newDimensions, timeToComplete);
		}

	}
}