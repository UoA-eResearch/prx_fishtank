using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class ScaleSlider : MonoBehaviour {

	public LinearMapping fishtankScaleLM;
	public Text fishtankScaleText;
	public float fishtankScale = 1.0f;
	public Transform start;
	public Transform end;
	public GameObject myHandle;
	

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		fishtankScale = 1.0f - ((1.0f - fishtankScaleLM.value) / 2.5f);
		//fishtankScale = 1.2f - ((1.0f - fishtankScaleLM.value) / 1.5f);
		double _scaleD = System.Math.Round(fishtankScale, 1);
		fishtankScaleText.text = _scaleD.ToString();
	}

	public void IncrementValue() {
		float newValue = fishtankScaleLM.value + 0.1f;
		if (newValue > 1) {
			newValue = 1;
		}
		fishtankScaleLM.value = newValue;
		SynchronizeHandleToValue();
	}

	public void DecrementValue() {
		float newValue = fishtankScaleLM.value - 0.1f;
		if (newValue < 0) {
			newValue = 0;
		}
		fishtankScaleLM.value = newValue;
		SynchronizeHandleToValue();
	}

	public void SynchronizeHandleToValue(){
		myHandle.transform.position = Vector3.Lerp(start.position, end.position, fishtankScaleLM.value);
	}

	public float GetFishtankScale()
	{
		return fishtankScale;
	}

}
