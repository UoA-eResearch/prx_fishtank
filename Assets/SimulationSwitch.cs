using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class SimulationSwitch : MonoBehaviour {

	public LinearMapping simulationLM;
	public Text simulationText;
	public bool simulationUsesSprings = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (simulationLM.value < .5)
		{
			simulationText.text = "spring constraints";
			simulationUsesSprings = true;
		}
		else
		{
			simulationText.text = "transforms";
			simulationUsesSprings = false;
		}
	}

	public bool GetSimulationMode()
	{
		return simulationUsesSprings;
	}
}
