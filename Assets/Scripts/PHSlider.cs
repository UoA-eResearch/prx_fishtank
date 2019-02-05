﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;


public class PHSlider : MonoBehaviour, IMenu
{
    public LinearMapping linearMapping;
    public LinearDrive myLinearDrive;
    private float currentLinearMapping = float.NaN;
	private int phValue;
	private GameObject phCanvas;
	private Text phText;
	public GameObject myHandle;

	//-------------------------------------------------
	void Awake()
    {
        if (linearMapping == null)
        {
            linearMapping = GetComponent<LinearMapping>();
        }

		phCanvas = GameObject.Find("pHCanvas");
		phText = phCanvas.GetComponentInChildren<Text>();

		//myLinearDrive = myHandle.GetComponent<LinearDrive>();
    }


    //-------------------------------------------------
    void Update()
    {
        if (currentLinearMapping != linearMapping.value)
        {
            currentLinearMapping = linearMapping.value;

            var mappedToPH = (currentLinearMapping - 0.0f) / (1.0f - 0.0f) * (9.0f - 3.0f) + 3.0f;
			phValue = Mathf.RoundToInt (mappedToPH);
			switch (phValue)
			{

				case 9:
					phText.text = "pH high";
					break;

				case 8:
				case 7:
				case 6:
				case 5:
				case 4:
					phText.text = "pH = " + phValue;
					break;

				case 3:
					phText.text = "pH low";
					break;

			}
        }
    }

	public int GetPhValue(){
		return phValue;
	}
	public string GetPhValueStr()
	{
		return "" + phValue;
	}

	public void ResetPhValue()
	{
		Debug.Log("ph value was: " + linearMapping.value);
		linearMapping.value = 1;
		Debug.Log("ph value is now: " + linearMapping.value);
		SynchronizeHandleToValue();
    }

	public void IncrementValue() {
		if (linearMapping.value + 0.1f > 1) {
			linearMapping.value = 1;
		}
		else {
			linearMapping.value += 0.1f;
		}
		SynchronizeHandleToValue();
	}

	public void DecrementValue() {
		if (linearMapping.value - 0.1f < 0) {
			linearMapping.value = 0;
		}
		else {
			linearMapping.value -= 0.1f;
		}
		SynchronizeHandleToValue();
	}

	public void SynchronizeHandleToValue(){
		// Debug.Log("handle position was: " + myHandle.transform.position);
		Vector3 originalPos = myHandle.transform.position;
		myHandle.transform.position = Vector3.Lerp(myLinearDrive.startPosition.position, myLinearDrive.endPosition.position, linearMapping.value);
		Vector3 newPos = myHandle.transform.position;
		// Debug.Log("handle position is now: " + myHandle.transform.position);
		if (originalPos != newPos) {
			Debug.Log("position was changed");
		}
	}

	
}
