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
		linearMapping.value = myHandle.transform.localPosition.x;
		if (linearMapping.value < -.2) {
			linearMapping.value = 0;
			SynchronizeHandleToValue();
		} else if (linearMapping.value > .2) {
			linearMapping.value = 1;
			SynchronizeHandleToValue();
		}
		if (linearMapping.value <= 0)
		{
			EnablePartyMode();
		}
		else
		{
			DisablePartyMode();
		}
	}

    /// <summary>
    /// sets appropriate booleans and changes active states game scoreboards
    /// </summary>
	public void EnablePartyMode()
	{
		isPartyModeOn = true;
		text.text = "Party!";
		scoreboard.SetActive(true);
		scoreboardbest.SetActive(true);
		chartStats.SetActive(false);
		heldScoreboard.SetActive(true);
	}

    /// <summary>
    /// sets appropriate booleans and changes active states game scoreboards
    /// </summary>
	public void DisablePartyMode()
	{
		isPartyModeOn = false;
		text.text = "Serious";
		scoreboard.SetActive(false);
		scoreboardbest.SetActive(false);
		chartStats.SetActive(true);
		heldScoreboard.SetActive(false);
	}

    /// <summary>
    /// sets value to 1 as it is a switch
    /// </summary>
	public void IncrementValue()
	{
		linearMapping.value = 1f;
		SynchronizeHandleToValue();
	}

    /// <summary>
    /// sets value to 0 as it is a switch
    /// </summary>
	public void DecrementValue()
	{
		linearMapping.value = 0f;
		SynchronizeHandleToValue();
	}

    /// <summary>
    /// enables party mode then sets the linear driver value to match
    /// </summary>
	public void TogglePartyMode()
	{
		// currently 1 = serious mode, 0 = party mode
		if (linearMapping.value >= 0.5)
		{
			DecrementValue();
			EnablePartyMode();
		}
		else
		{
			IncrementValue();
			DisablePartyMode();
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
