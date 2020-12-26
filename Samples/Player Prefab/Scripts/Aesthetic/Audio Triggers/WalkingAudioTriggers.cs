using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clouds.Platformer.Animation {
	[RequireComponent(typeof(AudioSource))]
	public class WalkingAudioTriggers : MonoBehaviour {
		//@TODO: Velocity layers!
		//@TODO: Encapsulate AudioPlaylist functionality into a struct for reuse anywhere.
		[SerializeField] AudioClip stepSoundL;
		[SerializeField] AudioClip stepSoundR;
		[SerializeField] [Range(0,1)] public float fader = 1;
		
		AudioSource src;
		void Awake () {
			src = GetComponent<AudioSource>();
		}

		public void StepLeft (/*float force*/) {
			//@TODO: Random playlists!~
			src.PlayOneShot(stepSoundL, fader);
		}
		public void StepRight () {
			src.PlayOneShot(stepSoundR, fader);
		}
	}
}