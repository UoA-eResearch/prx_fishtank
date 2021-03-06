﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class tests : MonoBehaviour
{
	public Fishtank fishtank;
	public PHSlider pHSlider;
	public bool allowWasdMovement = true;
	public float fastForward = 1.0f;
	public bool handTest = true;
	public int amountToDecrementPh = 0;

	public ResetButton resetButton;

	[Header("single run tests")]
	[Tooltip("breaks all the rings in the scene and logs the before and after scores")]
	public bool breakAllRings = false;
	[Tooltip("Sums all molecules in the game and logs the score")]
	public bool countAllMolecules = false;
	public bool simulatePartyWin = false;
	public string simulatePartyWinResult = "";
	public bool simulatePartyModeReset = false;
	public bool testPartyModeScore = false;

	public bool runAllTests = false;

	public bool attachToHandTest;


#if UNITY_EDITOR
	private void Start () {
		if (fishtank != null)
		{
			fishtank = GameObject.Find("fishtank").GetComponent<Fishtank>();
		}
		DecrementPh(amountToDecrementPh);

		if (handTest)
		{
			pHSlider.SetPhToMin();
		}
	}

	private void SimulatePartyWin()
	{
	    Debug.Log("simulating party win");
		fishtank.PartyModeWin();
		simulatePartyWin = false;
	    if (fishtank.confettiOn)
	    {
	        simulatePartyWinResult = "true";
	    }
        else
        {
	        simulatePartyWinResult = "false";
        }
	}

	private void BreakAllRings()
	{
		CountAllMolecules();
		var rings = FindObjectsOfType<Ring>();
		foreach (var ring in rings)
		{
			ring.BreakRing(null);
		}
		breakAllRings = false;
		CountAllMolecules();
	}

	private void CountAllMolecules()
	{
		int moleculeScore = 0;
		var monomerScore = moleculeScore += FindObjectsOfType<Monomer>().Length;
		var dimerScore = moleculeScore += FindObjectsOfType<Dimer>().Length * 2;
		var ringScore = moleculeScore += FindObjectsOfType<Ring>().Length * 12;
		Debug.Log("monomer score score: " + monomerScore);
		Debug.Log("dimer score: " + dimerScore);
		Debug.Log("ring score: " + ringScore);
		Debug.Log(Time.time + ": " + moleculeScore);
		countAllMolecules = false;
	}

	// Update is called once per frame
	void Update () {
		if (Time.timeScale != fastForward)
		{
			Time.timeScale = fastForward;
		}
		if (allowWasdMovement)
		{
			PollWasdMovement();
		}
        RunTests();
	}

    private void RunTests()
    {
        if (runAllTests)
        {
            SimulatePartyWin();
            TestPartyModeScore();
            SimulatePartyModeReset();
            CountAllMolecules();
            BreakAllRings();
            CountAllMolecules();
            runAllTests = false;
        }
 
        if (handTest)
		{
			HandAttachmentTest();
		}
		if (breakAllRings)
		{
			BreakAllRings();
		}
		if (countAllMolecules)
		{
			CountAllMolecules();
		}
		if (simulatePartyWin)
		{
			SimulatePartyWin();
			// StartCoroutine(SimulatePartyWin);
		}
		if (simulatePartyModeReset)
		{
			SimulatePartyModeReset();
		}
		if (testPartyModeScore)
		{
			// TestPartyModeScore();
			StartCoroutine(TestPartyModeScore());
		}

		if (attachToHandTest)
		{
			var hand = Player.instance.hands[0];
			GameObject stackedRing = null;
			var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachFromOtherHand;
			// var attachmentFlags = Hand.AttachmentFlags.ParentToHand;
			GameObject nextRing = null;

			foreach (var ring in UnityEngine.Object.FindObjectsOfType<Ring>())
			{
				if (ring.dockedToAcceptor && ring.dockedToDonor)
				{
					// we got a stack
					stackedRing = ring.gameObject;
					nextRing = ring.partnerAcceptor;
					// break;
				}
			}
			hand.AttachObject(stackedRing, attachmentFlags);
			attachToHandTest = false;

			// attach next ring to other hand
			var nextHand = Player.instance.hands[1];
			nextHand.AttachObject(nextRing, attachmentFlags);
		}
    }

	private void SimulatePartyModeReset()
	{
		resetButton.ResetPartyMode();
        simulatePartyModeReset = false;
	}

	private void PollWasdMovement()
	{
		if (Input.GetKey(KeyCode.W))
		{
			Player.instance.transform.position += Time.deltaTime * Vector3.forward;
		}
		if (Input.GetKey(KeyCode.A))
		{
			Player.instance.transform.position -= Time.deltaTime * Vector3.right;
		}
		if (Input.GetKey(KeyCode.S))
		{
			Player.instance.transform.position -= Time.deltaTime * Vector3.forward;
		}
		if (Input.GetKey(KeyCode.D))
		{
			Player.instance.transform.position += Time.deltaTime * Vector3.right;
		}
    }

    private void DecrementPh(int amount) {
		for (int i = 0; i < amount; i++)
		{
			pHSlider.DecrementValue();
		}
	}

	/// <summary>
	/// tests attaching things to hands currently polls and detaches somewhere in the code so need to hold down the triggers to prevent detaching
	/// </summary>
	private void HandAttachmentTest()
	{
		if (Time.time >= 55)
		{
			// Debug.Log("start test");

			foreach (Ring ring in FindObjectsOfType<Ring>())
			{
				if (ring.dockedToAcceptor && ring.dockedToDonor)
				{
					// then it's connected to a stack and is at least 4 rings long
					Debug.Log("stack is at least 3 rings long");
					Player.instance.leftHand.AttachObject(ring.gameObject);
					if (ring.partnerAcceptor)
					{
						var rh = Player.instance.rightHand;
						rh.AttachObject(ring.partnerAcceptor);

						var lh = Player.instance.leftHand;
						Debug.Log(transform.parent.GetChild(transform.GetSiblingIndex() + 1).gameObject);
						lh.AttachObject(transform.parent.GetChild(transform.GetSiblingIndex() + 1).gameObject);
					}
					break;
				}
			}
			// Player.instance.leftHand.AttachObject(FindObjectOfType<Ring>().gameObject);
			// Player.instance.leftHand.AttachObject(FindObjectOfType<Ring>().gameObject);
		}
		else {
			// Debug.Log(Time.time);
		}

	}

	private IEnumerator TestPartyModeScore()
	{
		Debug.Log("testing party mode score");
		// fishtank.partyModeSwitch.IncrementValue();
		fishtank.partyModeSwitch.EnablePartyMode();
		fishtank.partyModeUi.SetActive(true);
		yield return null;
        Debug.Log("idk");
	}

#endif
}
