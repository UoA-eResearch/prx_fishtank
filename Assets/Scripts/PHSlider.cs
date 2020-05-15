using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;


public class PHSlider : MonoBehaviour, IMenu
{
    public LinearMapping linearMapping;
    public LinearDrive myLinearDrive;
    private float currentLinearMapping = float.NaN;
	public int phValue
	{
		get;
		private set;
	}
	private GameObject phCanvas;
	public Text phText
	{
		get;
		set;
	}
	public GameObject myHandle;

	//-------------------------------------------------
	void Awake()
    {
        if (linearMapping == null)
        {
            linearMapping = GetComponent<LinearMapping>();
        }

        if (!phCanvas)
        {
            phCanvas = GameObject.Find("pHCanvas");
        }

        if (phText == null)
        {
            phText = phCanvas?.GetComponentInChildren<Text>();
        }

        //myLinearDrive = myHandle.GetComponent<LinearDrive>();
		SynchronizeHandleToValue();
    }

    //-------------------------------------------------
    void Update()
    {
		SetPhFromLinearMapping();
    }

	/// <summary>
	/// Compares linear mapping value to last update.If the value has changed then convert the value to ph value and store in public variable.
	/// </summary>
	private void SetPhFromLinearMapping(){
		linearMapping.value = myHandle.transform.localPosition.y / 5;
        if (currentLinearMapping == linearMapping.value)
        {
			return;
		}
		currentLinearMapping = linearMapping.value;
		var mappedToPH = (currentLinearMapping - 0.0f) / (1.0f - 0.0f) * (9.0f - 3.0f) + 3.0f;
		phValue = Mathf.RoundToInt(mappedToPH);
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

	public string GetPhValueStr()
	{
		return "" + phValue;
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

	/// <summary>
	/// uses linear mapping value to set the value of the slider.
	/// </summary>
	public void SynchronizeHandleToValue(){
		// Debug.Log("handle position was: " + myHandle.transform.position);
		Vector3 pos = myHandle.transform.localPosition;
		pos.y = linearMapping.value * 5;
		myHandle.transform.localPosition = pos;
	}

	public void SetPhToMin()
	{
		linearMapping.value = 0;
		phValue = 3;
		SynchronizeHandleToValue();
	}

	public void SetPhToMax()
	{
		linearMapping.value = 1;
		phValue = 9;
		SynchronizeHandleToValue();
	}
}
