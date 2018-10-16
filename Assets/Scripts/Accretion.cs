using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accretion : MonoBehaviour {

	private ParticleSystem particleSystem;

	// Use this for initialization
	void Start () {
		particleSystem = this.GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
		// self-destructs when particle ends.
		if (!particleSystem.IsAlive()) {
			Destroy(this.gameObject);
		}
	}
}
