using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class PartyModeSwitch : MonoBehaviour {

	public LinearMapping linearMapping;
	public Text text;
	public bool partying = true;
	public GameObject scoreboard;
	
	// Update is called once per frame
	void Update () {
		if (linearMapping.value < .5)
		{
			text.text = "Party mode!";
			partying = true;
			scoreboard.SetActive(true);
		}
		else
		{
			text.text = "Serious mode.";
			partying = false;
			scoreboard.SetActive(false);
		}
	}
}
