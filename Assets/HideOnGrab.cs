using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class HideOnGrab : MonoBehaviour {

	public Hand parentHand;

	public GameObject[] hidableUIElements;

	private void OnEnable() {

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// NOTE: Put in late update at the moment. Something is messing with a single teleport UI Hint. I suspect the fishtank UpdateViveControllers
	void LateUpdate()
	{
		if (parentHand.controller.GetHairTrigger()) {
			foreach (var uiElement in hidableUIElements) {
				Debug.Log(uiElement.name);
				uiElement.SetActive(false);
			}
		} else {
			foreach (var uiElement in hidableUIElements) {
				uiElement.SetActive(true);
			}
		}
	}
}
