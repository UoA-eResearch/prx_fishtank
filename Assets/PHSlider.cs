using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;


public class PHSlider : MonoBehaviour
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
        // RepositionHandle();
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
		return phText.text;
	}

	public void ResetPhHigh()
	{
		linearMapping.value = 1;
        myHandle.transform.position = Vector3.Lerp(myLinearDrive.startPosition.position, myLinearDrive.endPosition.position, linearMapping.value);
    }
}
