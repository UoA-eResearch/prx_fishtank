using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class NanoParticleSwitch : MonoBehaviour {

	public LinearMapping nanoLM;
	public Text nanoText;
	public bool useNanoParticles = true;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update()
	{
		if (nanoLM.value < .5)
		{
			nanoText.color = Color.red;
			nanoText.text = "OFF";
			useNanoParticles = false;
		}
		else
		{
			nanoText.color = Color.green;
			nanoText.text = "ON";
			useNanoParticles = true;
		}
	}

	public bool GetNanoParticleMode()
	{
		return useNanoParticles;
	}
}
