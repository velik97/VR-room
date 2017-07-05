using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoSingleton <Lamp> {

	public Light lightSource;
	public MeshRenderer[] lampBodyParts;

	public Material lampOnMaterial;
	public Material lampOffMaterial;

	public float intencity;

	void Start () {
		Set (true);
	}

	public void Set (bool _on) {
		lightSource.intensity = _on ? intencity : 0f;
		for (int i = 0; i < lampBodyParts.Length; i++) {
			lampBodyParts[i].material = _on ? lampOnMaterial : lampOffMaterial;
		}
	}
}
