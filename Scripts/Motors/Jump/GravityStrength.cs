using UnityEngine;
using Unity.Mathematics;
using System.Collections;

namespace Clouds.Platformer.Character {
	[DisallowMultipleComponent]
	[AddComponentMenu("Hidden/Platformer/Gravity/Strength Of Gravity")]
	public class GravityStrength : MonoBehaviour {
		[SerializeField] float initialStrength = -50;
		public float Value {get; set;}

		public GravityStrength () {
			Value = initialStrength;
		}
	
		/// <summary>
		/// Revert gravity strength to the value set in the editor.
		/// </summary>
		public void RevertStrength () {
			Value = initialStrength;
		}

	}
}