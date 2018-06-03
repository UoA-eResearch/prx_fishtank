using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class ScaleSlider : MonoBehaviour {

	public LinearMapping fishtankScaleLM;
	public Text fishtankScaleText;
	public float fishtankScale = 1.0f;

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

	public float GetFishtankScale()
	{
		return fishtankScale;
	}

}
