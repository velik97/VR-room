using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearInterpolatingMovementController : HeadMovementController {

	private Vector3 newRotation;
	private Vector3 currentRotation;
	private Vector3 prevRotation;

	private float tickRate;
	private float timeOffset;

	void Start () {
		
		tickRate = (float)MyNetworkManager.Instance.tickRate;
		timeOffset = Time.time;

		newRotation = transform.rotation.eulerAngles;
		prevRotation = newRotation;
		currentRotation = newRotation;
	}

	public void TakeNewValue (Vector3 rotation) {
		newRotation = rotation;
		LeadAnglesToCommonView ();
	}

	void Update () {
		
		if (currentRotation != newRotation) {

			float t = Time.time - timeOffset;
			currentRotation = Vector3.Lerp (prevRotation, newRotation, t * tickRate);

			headMovement.SetRotation (Quaternion.Euler (currentRotation));
		} else {

			LeadAnglesToCommonView ();
			prevRotation = currentRotation;
			timeOffset = Time.time;
		}
	}

	void LeadAnglesToCommonView () {

		if (Mathf.Abs (prevRotation.x - newRotation.x) > 180f) {
			prevRotation.x = (prevRotation.x + 360f) % 360f;
			newRotation.x = (newRotation.x + 360f) % 360f;

			if (Mathf.Abs (prevRotation.x - newRotation.x) > 180f) {
				if (prevRotation.x > 180f)
					prevRotation.x -= 360f;
				else
					newRotation.x -= 360f;
			}
		}

		if (Mathf.Abs (prevRotation.y - newRotation.y) > 180f) {
			prevRotation.y = (prevRotation.y + 360f) % 360f;
			newRotation.y = (newRotation.y + 360f) % 360f;

			if (Mathf.Abs (prevRotation.y - newRotation.y) > 180f) {
				if (prevRotation.y > 180f)
					prevRotation.y -= 360f;
				else
					newRotation.y -= 360f;
			}
		}

		if (Mathf.Abs (prevRotation.z - newRotation.z) > 180f) {
			prevRotation.z = (prevRotation.z + 360f) % 360f;
			newRotation.z = (newRotation.z + 360f) % 360f;

			if (Mathf.Abs (prevRotation.z - newRotation.z) > 180f) {
				if (prevRotation.z > 180f)
					prevRotation.z -= 360f;
				else
					newRotation.z -= 360f;
			}
		}

	}

}
