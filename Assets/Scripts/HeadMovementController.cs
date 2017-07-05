using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadMovementController : MonoBehaviour {

#if !UNITY_EDITOR
	Quaternion startRotation;

	void Start () {
		Input.gyro.enabled = true;
		startRotation = transform.rotation * Quaternion.Euler (90, 0, 0);

	}
		
	void Update () {
		Vector3 gyroEuler = Input.gyro.attitude.eulerAngles;

		Quaternion gyroInput = Quaternion.Euler (new Vector3 (-gyroEuler.x, -gyroEuler.y, gyroEuler.z));

		transform.rotation = startRotation * gyroInput;
	}
#endif

}
