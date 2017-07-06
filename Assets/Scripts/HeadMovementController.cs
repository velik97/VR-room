using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HeadMovement))]
public class HeadMovementController : MonoBehaviour {

	protected HeadMovement headMovement;

	void Awake () {
		headMovement = GetComponent <HeadMovement> ();
	}
}
