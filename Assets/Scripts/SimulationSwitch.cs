using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class SimulationSwitch : MonoBehaviour, IMenu {

	public LinearMapping simulationLM;
	public Text simulationText;
	public bool simulationUsesSprings = false;
	public Transform start;
	public Transform end;
	public GameObject myHandle;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		simulationLM.value = myHandle.transform.localPosition.x;
		if (simulationLM.value < -0.2) {
			simulationLM.value = 0;
			SynchronizeHandleToValue();
		} else if (simulationLM.value > .2) {
			simulationLM.value = 1;
			SynchronizeHandleToValue();
		}
		if (simulationLM.value <= 0)
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

	public void IncrementValue(){
		simulationLM.value = 1f;
		SynchronizeHandleToValue();
		}

	public void DecrementValue() {
		simulationLM.value = 0f;
		SynchronizeHandleToValue();
	}

	public void SynchronizeHandleToValue(){
		myHandle.transform.position = Vector3.Lerp(start.position, end.position, simulationLM.value);
	}

	public bool GetSimulationMode()
	{
		return simulationUsesSprings;
	}
}
