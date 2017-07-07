using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolatingMovementController : HeadMovementController {

	private Vector3 currentRotation;
	private Vector3[] rotationGraph;

	private float tickRate;

	void Start () {
		
		tickRate = (float)MyNetworkManager.Instance.tickRate;

		currentRotation = transform.rotation.eulerAngles;
		rotationGraph = new Vector3 [3];

		for (int i = 0; i < rotationGraph.Length; i++) {
			rotationGraph [i] = currentRotation;
		}

	}

	public void TakeNewValue (Vector3 newRotation) {

		StopAllCoroutines ();

		if (Mathf.Abs (rotationGraph [2].x - newRotation.x) > 180f) {
			
			if (Mathf.Abs (currentRotation.x) > 180f) {
				currentRotation.x -= Mathf.Sign (currentRotation.x) * 360f;
				rotationGraph [1].x -= Mathf.Sign (rotationGraph [1].x) * 360f;
				rotationGraph [2].x -= Mathf.Sign (rotationGraph [2].x) * 360f;
			} else
				newRotation.x -= Mathf.Sign (newRotation.x) * 360f;
		}

		if (Mathf.Abs (rotationGraph [2].y - newRotation.y) > 180f) {

			if (Mathf.Abs (currentRotation.y) > 180f) {
				currentRotation.y -= Mathf.Sign (currentRotation.y) * 360f;
				rotationGraph [1].y -= Mathf.Sign (rotationGraph [1].y) * 360f;
				rotationGraph [2].y -= Mathf.Sign (rotationGraph [2].y) * 360f;
			} else
				newRotation.y -= Mathf.Sign (newRotation.y) * 360f;
		}

		if (Mathf.Abs (rotationGraph [2].z - newRotation.z) > 180f) {

			if (Mathf.Abs (currentRotation.z) > 180f) {
				currentRotation.z -= Mathf.Sign (currentRotation.z) * 360f;
				rotationGraph [1].z -= Mathf.Sign (rotationGraph [1].z) * 360f;
				rotationGraph [2].z -= Mathf.Sign (rotationGraph [2].z) * 360f;
			} else
				newRotation.z -= Mathf.Sign (newRotation.z) * 360f;
		}

		rotationGraph [0] = currentRotation;
		rotationGraph [1] = (2f * rotationGraph [2]) - rotationGraph [1];
		rotationGraph [2] = (rotationGraph [1] + newRotation) * .5f;


		StartCoroutine (Interpolate ());
	}

	IEnumerator Interpolate () {

		float offset = Time.time;
		float t = 0f;

		Vector3 a = rotationGraph [0] + rotationGraph [2] - (2f * rotationGraph [1]);
		Vector3 b = 2f * (rotationGraph [1] - rotationGraph [0]);
		Vector3 c = rotationGraph [0];

		while (t < 1f) {
			currentRotation = a * (t*t) + b * t + c;
			headMovement.SetRotation (Quaternion.Euler (currentRotation));
			t = (Time.time - offset) * tickRate;
			yield return new WaitForEndOfFrame ();
		}
		TakeNewValue (currentRotation);
	}


}
