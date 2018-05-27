using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class CartoonModeSwitch : MonoBehaviour
{
	public LinearMapping renderCartoonLM;
	public Text renderCartoonText;
	public bool renderCartoon = false;

	public LinearMapping fishtankScaleLM;
	public Text fishtankScaleText;
	public float fishtankScale = 1.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (renderCartoonLM.value < .5)
		{
			renderCartoonText.text = "cartoon";
			renderCartoon = true;
			//scoreboard.SetActive(true);
		}
		else
		{
			renderCartoonText.text = "surface";
			renderCartoon = false;
			//scoreboard.SetActive(false);
		}

		fishtankScale = 1.0f - ((1.0f - fishtankScaleLM.value) / 2.5f);
		//fishtankScale = 1.2f - ((1.0f - fishtankScaleLM.value) / 1.5f);
		double _scaleD = System.Math.Round(fishtankScale, 1);
		fishtankScaleText.text = _scaleD.ToString();
	}

	public bool GetRenderCartoon()
	{
		return renderCartoon;
	}

	public float GetFishtankScale()
	{
		return fishtankScale;
	}
}
