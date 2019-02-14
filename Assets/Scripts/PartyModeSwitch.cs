using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class PartyModeSwitch : MonoBehaviour, IMenu
{

	public LinearMapping linearMapping;
	public Text text;
	private bool partyMode = false;

	public GameObject scoreboard;
	public GameObject scoreboardbest;
	public GameObject chartStats;
	public GameObject heldScoreboard;
	public Transform start;
	public Transform end;
	public GameObject myHandle;
	
	// Update is called once per frame
	void Update ()
	{
		Debug.Log("party mode switch update playing");
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

	public void IncrementValue()
	{
		linearMapping.value = 1f;
		SynchronizeHandleToValue();
	}

	public void DecrementValue()
	{
		linearMapping.value = 0f;
		SynchronizeHandleToValue();
	}

	public void SynchronizeHandleToValue()
	{
		myHandle.transform.position = Vector3.Lerp(start.position, end.position, linearMapping.value);
	}

	public bool GetPartyMode()
	{
		return partyMode;
	}
}
