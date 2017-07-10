using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity {

	public HeadMovementController headMovementController;
	public LampButton lampButton;

	public Entity (HeadMovementController headMovementController, LampButton lampButton) {
		this.headMovementController = headMovementController;
		this.lampButton = lampButton;
	}

	public void Destroy () {
		GameObject.Destroy (headMovementController.gameObject);
		GameObject.Destroy (lampButton.gameObject);
	}

}
