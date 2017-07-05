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

	bool buttonIsOn;

	void Start () {
		buttonIsOn = true;
	}

	public void Set (GameObject presedButton) {
		if (presedButton == onButton || presedButton ==  offButton) {
			
			bool _on = presedButton == onButton;

			animator.SetBool ("On", _on);
			Lamp.Instance.Set (_on);

			if (buttonIsOn != _on) {
				audioSource.pitch = _on ? onPitch : offPitch;
				audioSource.Play ();
			}

			buttonIsOn = _on;
		}

	}
}
