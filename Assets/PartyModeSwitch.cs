using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class PartyModeSwitch : MonoBehaviour {

	public LinearMapping linearMapping;
	public Text text;
	private bool partyMode = false;
	public GameObject scoreboard;
	public GameObject chartStats;
	
	// Update is called once per frame
	void Update () {
		if (linearMapping.value < .5)
		{
			text.text = "Party!";
			partyMode = true;
			scoreboard.SetActive(true);
			chartStats.SetActive(false);
		}
		else
		{
			text.text = "Serious";
			partyMode = false;
			scoreboard.SetActive(false);
			chartStats.SetActive(true);
		}
	}
	public bool GetPartyMode()
	{
		return partyMode;
	}
}
