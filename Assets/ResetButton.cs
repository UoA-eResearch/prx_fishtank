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
	
	void OnCollisionEnter(Collision other)
	{
		if (other.transform != transform && other.transform.GetComponent<Hand>())
		{
			Debug.Log("colliding with a hand" + other.transform.name);
		}
	}

	private void OnHandHoverEnd(Hand hand)
	{
		Debug.Log(hand.transform.name);
	}

	// private void OnHandHoverBegin(Hand hand)
	// {
	// 	Debug.Log(hand.transform.name);
	// 	Debug.Log(transform.name);
	// }

	// private IEnumerator HandHoverUpdate(Hand hand)
	// {
	// 	if (hand.GetThumbpadButtonDown() || ((hand.controller != null) && hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip)))
	// 	{
	// 		partyModeSwitch.IncrementValue();
	// 		yield return null;
	// 		partyModeSwitch.DecrementValue();
	// 	}
	// }

	private void HandHoverUpdate(Hand hand)
	{
		if (hand.GetThumbpadButtonDown() || ((hand.controller != null) && hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip)))
		{
			partyModeSwitch.IncrementValue();
			fishTank.StopPartyMode();
			// yield return null;
			partyModeSwitch.DecrementValue();
		}
	}
}
