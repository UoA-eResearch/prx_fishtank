﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class BreakRing : MonoBehaviour
{

	public GameObject dimerPrefab;
	public bool shouldBreak = true;
	private VelocityEstimator velEst;
	private bool ringAttached = false;

	void Awake()
	{
		velEst = GetComponent<VelocityEstimator>();
	}

	public void breakRing(Hand currentHand)
	{
		if (currentHand != null)
		{
			// Drop whatever you're holding
			currentHand.otherHand.DetachObject(currentHand.otherHand.currentAttachedObject);
			currentHand.DetachObject(currentHand.currentAttachedObject);
			Debug.Log(currentHand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + currentHand.name);
		}

		// Make a dimer and attach it to the hand. This replaces the ring you were just holding.
		var dimer = Instantiate(dimerPrefab, transform.position, transform.rotation, transform.parent);
		dimer.GetComponent<Rigidbody>().AddForce(-dimer.transform.forward, ForceMode.Impulse);
		dimer.name = "dimer_" + dimer.GetInstanceID();
		float minDist = 0;
		if (currentHand)
		{
			minDist = Vector3.Distance(dimer.transform.position, currentHand.otherHand.hoverSphereTransform.position);
		}
		var match = dimer;
		foreach (Transform child in dimer.transform)
		{
			if (child.name.StartsWith("ring"))
			{
				var childDimer = Instantiate(dimerPrefab, child.transform.position, child.transform.rotation, transform.parent);
				childDimer.GetComponent<Rigidbody>().AddForce(-childDimer.transform.forward, ForceMode.Impulse);
				childDimer.name = "dimer_" + dimer.GetInstanceID();
				if (currentHand)
				{
					var dist = Vector3.Distance(child.transform.position, currentHand.otherHand.hoverSphereTransform.position);

					if (dist < minDist)
					{
						minDist = dist;
						match = childDimer;
					}
				}
			}
		}
		Destroy(gameObject);

		if (currentHand)
		{
			var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachOthers;
			currentHand.otherHand.AttachObject(match, attachmentFlags);
		}
	}


	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldBreak)
		{
			breakRing(hand);
		}
	}

	void HandAttachedUpdate(Hand hand)
	{
		if (gameObject == hand.currentAttachedObject && shouldBreak)
		{
			Vector3 velocity = velEst.GetVelocityEstimate();
			Debug.Log("Velocity: " + velocity + velocity.magnitude);

			if (velocity.magnitude > 2.0 && ringAttached)
			{
				breakRing(hand);
				ringAttached = false;
			}
		}
	}

	void OnAttachedToHand()
	{
		ringAttached = true;
		velEst.BeginEstimatingVelocity();
	}

	void OnDetachedFromHand()
	{
		ringAttached = false;
		velEst.FinishEstimatingVelocity();
	}

}
