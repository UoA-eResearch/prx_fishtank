using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;


public class PHSlider : MonoBehaviour
{

    public LinearMapping linearMapping;
    private float currentLinearMapping = float.NaN;
	private int phValue;
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

            var mappedToPH = (currentLinearMapping - 0.0f) / (1.0f - 0.0f) * (9.0f - 3.0f) + 3.0f;
			phValue = Mathf.RoundToInt (mappedToPH);
            phText.text = "PH value: " + phValue;
        }
    }

	public int GetPhValue(){
		return phValue;
	}
}
