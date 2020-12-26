using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clouds.Platformer.Animation {
	[RequireComponent(typeof(AudioSource))]
	public class JumpAudioTriggers : MonoBehaviour {
		[SerializeField] AudioClip jumpSound;
		[SerializeField] [Range(0,1)] public float jumpFader = 1;
		[SerializeField] AudioClip landed;
		[SerializeField] [Range(0,1)] public float landingFader = 1;

		AudioSource src;
		void Awake () {
			src = GetComponent<AudioSource>();
		}

		public void Jumped () {
			src.PlayOneShot(jumpSound, jumpFader);
		}
		public void Landed () {
			src.PlayOneShot(landed, landingFader);
		}
	}
}