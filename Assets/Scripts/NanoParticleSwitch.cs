using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class NanoParticleSwitch : MonoBehaviour, IMenu {

	public LinearMapping nanoLM;
	public Text nanoText;
	public bool useNanoParticles = true;
	public Transform start;
	public Transform end;
	public GameObject myHandle;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update()
	{
		nanoLM.value = myHandle.transform.localPosition.x;
		if (nanoLM.value < -.2) {
			nanoLM.value = 0;
			SynchronizeHandleToValue();
		} else if (nanoLM.value > .2) {
			nanoLM.value = 1;
			SynchronizeHandleToValue();
		}
		if (nanoLM.value <= 0)
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

	public void IncrementValue(){
		nanoLM.value = 1f;
		SynchronizeHandleToValue();
	}

	public void DecrementValue() {
		nanoLM.value = 0f;
		SynchronizeHandleToValue();
	}

	public void SynchronizeHandleToValue(){
		myHandle.transform.position = Vector3.Lerp(start.position, end.position, nanoLM.value);
	}

	public bool GetNanoParticleMode()
	{
		return useNanoParticles;
	}
}
