using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class CartoonModeSwitch : MonoBehaviour
{
	public LinearMapping linearMapping;
	public Text text;
	public bool renderCartoon = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (linearMapping.value < .5)
		{
			text.text = "cartoon";
			renderCartoon = true;
			//scoreboard.SetActive(true);
		}
		else
		{
			text.text = "surface";
			renderCartoon = false;
			//scoreboard.SetActive(false);
		}
	}

	public bool GetRenderCartoon()
	{
		return renderCartoon;
	}
}
