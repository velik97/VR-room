using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadMovement : MonoBehaviour {

	public void SetRotation (Quaternion rotation) {
		transform.localRotation = rotation;
	}

}
