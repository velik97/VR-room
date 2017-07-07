using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour {

	public float touchLength = 10f;
	private LayerMask touchFieldsLayer;

	private bool isTouching;

	private Camera mainCamera;

	void Start () {
		Initialize ();
	}

	void Update () {
		if (Input.touches.Length > 0 || Input.GetMouseButton (0)) {
			Vector3 inputPosition;

			#if UNITY_EDITOR || UNITY_STANDALONE_OSX
			inputPosition = Input.mousePosition;
			#else
			inputPosition = (Vector3)Input.touches[0].position;
			#endif

			Ray camRay = mainCamera.ScreenPointToRay (inputPosition);
			RaycastHit hit;

			if (Physics.Raycast (camRay, out hit, touchLength, touchFieldsLayer)) {
				if (!isTouching) {
					GameObject hitObject = hit.collider.gameObject;
					if (hitObject) {
						LampButton lampButton = hitObject.GetComponentInParent <LampButton> ();
						if (lampButton) {
							lampButton.Press (hitObject);
						}
					}
				}
				isTouching = true;
			}
		} else {
			isTouching = false;
		}
	}

	void Initialize() {
		touchFieldsLayer = LayerMask.GetMask ("Touch Field");
		isTouching = false;
		mainCamera = Camera.main;
	}

}
