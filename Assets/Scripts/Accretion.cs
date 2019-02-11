using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accretion : MonoBehaviour {

	private ParticleSystem particleSystem;

	void Awake()
	{
		particleSystem = GetComponent<ParticleSystem>();
	}
	
	void Update () {
		// self-destructs when particle ends.
		if (!particleSystem.IsAlive()) {
			Destroy(this.gameObject);
		}
	}

}
