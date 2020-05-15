using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class ResetButton : MonoBehaviour {

	public PHSlider pHSlider;
	public PartyModeSwitch partyModeSwitch;
	public Fishtank fishTank;
	// Use this for initialization
	void Start () {
		
	}
	
	void OnTriggerEnter()
	{
		ResetPartyMode();
	}

	/// <summary>
	/// resets party mode by physically moving the linear drive position to serious mode then back to party mode.
	/// </summary>
	public void ResetPartyMode()
	{
		fishTank.StopPartyMode();
		fishTank.StartPartyMode();
	}
}
