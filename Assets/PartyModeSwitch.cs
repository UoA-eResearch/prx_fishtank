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
	public GameObject scoreboardbest;
	public GameObject chartStats;
	public GameObject heldScoreboard;
	
	// Update is called once per frame
	void Update ()
	{
		if (linearMapping.value < .5)
		{
			text.text = "Party!";
			partyMode = true;
			scoreboard.SetActive(true);
			scoreboardbest.SetActive(true);
			chartStats.SetActive(false);
			heldScoreboard.SetActive(true);
		}
		else
		{
			text.text = "Serious";
			partyMode = false;
			scoreboard.SetActive(false);
			scoreboardbest.SetActive(false);
			chartStats.SetActive(true);
			heldScoreboard.SetActive(false);
		}
	}

	public bool GetPartyMode()
	{
		return partyMode;
	}
}
