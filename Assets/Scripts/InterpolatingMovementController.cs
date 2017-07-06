using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolatingMovementController : HeadMovementController {

	public void TakeNewValue (Quaternion rotation) {
		headMovement.SetRotation (rotation);
	}

}
