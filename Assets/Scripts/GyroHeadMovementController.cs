using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HeadMovement))]
public class GyroHeadMovementController : HeadMovementController {

#if !UNITY_EDITOR && !UNITY_STANDALONE_OSX
	Quaternion startRotation;

	void Start () {

		Input.gyro.enabled = true;
		startRotation = transform.rotation * Quaternion.Euler (90, 0, 0);

	}

	void Update () {
		
		Vector3 gyroEuler = Input.gyro.attitude.eulerAngles;

		Quaternion gyroInput = Quaternion.Euler (new Vector3 (-gyroEuler.x, -gyroEuler.y, gyroEuler.z));

		headMovement.SetRotation (startRotation * gyroInput);
	}
#else

	[Range(0f, 10f)]
	public float speed = 3f;

	void Update () { 

		float v = Input.GetAxis ("Vertical");
		float h = Input.GetAxis ("Horizontal");

		if (v != 0f) {
			transform.rotation = Quaternion.LookRotation (Vector3.Lerp (transform.forward, transform.up * Mathf.Sign (v), Mathf.Abs (v) * speed * 0.01f));
		}

		if (Input.GetAxis ("Horizontal") != 0f) {
			transform.rotation = Quaternion.LookRotation (Vector3.Lerp (transform.forward, transform.right * Mathf.Sign (h), Mathf.Abs (h) * speed * 0.01f));
		}
	}


#endif

}
