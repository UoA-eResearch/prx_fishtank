using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class PartyModeSwitch : MonoBehaviour, IMenu
{

	public LinearMapping linearMapping;
	public Text text;
	private bool isPartyModeOn = false;

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
		if (linearMapping.value < .5)
		{
			EnablePartyMode();
		}
		else
		{
			DisablePartyMode();
		}
	}

	public void EnablePartyMode()
	{
		isPartyModeOn = true;
		text.text = "Party!";
		scoreboard.SetActive(true);
		scoreboardbest.SetActive(true);
		chartStats.SetActive(false);
		heldScoreboard.SetActive(true);
	}

	public void DisablePartyMode()
	{
		isPartyModeOn = false;
		text.text = "Serious";
		scoreboard.SetActive(false);
		scoreboardbest.SetActive(false);
		chartStats.SetActive(true);
		heldScoreboard.SetActive(false);
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

	public void TogglePartyMode()
	{
		// currently 1 = serious mode, 0 = party mode
		if (linearMapping.value >= 0.5)
		{
			EnablePartyMode();
			DecrementValue();
		}
		else
		{
			DisablePartyMode();
			IncrementValue();
		}
	}

	/// <summary>
	/// move the handle to the right position dictated by the menu value.
	/// </summary>
	public void SynchronizeHandleToValue()
	{
		myHandle.transform.position = Vector3.Lerp(start.position, end.position, linearMapping.value);
	}

	public bool GetPartyModeStatus()
	{
		return isPartyModeOn;
	}
}
