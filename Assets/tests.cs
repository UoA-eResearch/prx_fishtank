using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class tests : MonoBehaviour {

	public Fishtank ft;
	public PHSlider pHSlider;

	// Use this for initialization
	void Start () {
		minPh();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void minPh() {
		for (int i = 0; i < 8; i++)
		{
			pHSlider.DecrementValue();
		}
	}
}
