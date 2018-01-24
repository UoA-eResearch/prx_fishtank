using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;


public class PHSlider : MonoBehaviour
{

    public LinearMapping linearMapping;
    private float currentLinearMapping = float.NaN;
	private float phValue;
	private GameObject phCanvas;
	private Text phText;

    //-------------------------------------------------
    void Awake()
    {
        if (linearMapping == null)
        {
            linearMapping = GetComponent<LinearMapping>();
        }

		phCanvas = GameObject.Find("TitleCanvas");
		phText = phCanvas.GetComponentInChildren<Text>();
    }


    //-------------------------------------------------
    void Update()
    {
        if (currentLinearMapping != linearMapping.value)
        {
            currentLinearMapping = linearMapping.value;

            var mappedToPH = (currentLinearMapping - 0.0f) / (1.0f - 0.0f) * (14.0f - 3.0f) + 3.0f;
            mappedToPH = Mathf.Round(mappedToPH * 10f) / 10f;
            phText.text = "PH value: " + mappedToPH;
			phValue = mappedToPH;
        }
    }

	public float GetPhValue(){
		return phValue;
	}
}
