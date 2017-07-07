using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampButton : MonoBehaviour {

	public Animator animator;
	public AudioSource audioSource;

	public GameObject onButton;
	public GameObject offButton;

	public float onPitch;
	public float offPitch;


	public void Press (GameObject presedButton) {
		if (presedButton == onButton || presedButton ==  offButton) {

			bool _on = presedButton == onButton;
			MyNetworkManager.Instance.RequestChangeLampState (_on);
		}
	}

	public void Set (bool _on) {			
		animator.SetBool ("On", _on);
		audioSource.pitch = _on ? onPitch : offPitch;
		audioSource.Play ();
	}

	public void SetSilently (bool _on) {
		animator.SetBool ("On", _on);
	}
}
