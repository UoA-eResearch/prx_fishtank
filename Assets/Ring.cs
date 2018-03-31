using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Ring: MonoBehaviour
{

	public GameObject dimerPrefab;
	public bool shouldBreak = true;
	private VelocityEstimator velEst;
	private bool ringAttached = false;
	private Transform fishtank;

	public GameObject partnerAcceptor;
	public GameObject partnerDonor;
	public bool dockedToAcceptor = false;
	public bool dockedToDonor = false;

	public Fishtank fishtankScript;
	public GameObject fishtankGO;

	void Awake()
	{
		velEst = GetComponent<VelocityEstimator>();
		fishtank = transform.parent;
		GameObject fishtankGO = GameObject.Find("fishtank");
		fishtankScript = fishtankGO.GetComponent<Fishtank>();
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
		var ring2DimerTransform = transform.Find("tf_ring2dimer");
		var dimer = Instantiate(dimerPrefab, ring2DimerTransform.position, transform.rotation, fishtank);
		//var dimer = Instantiate(dimerPrefab, transform.position, transform.rotation, fishtank);
		dimer.GetComponent<Rigidbody>().AddForce(-dimer.transform.forward * Random.RandomRange(0.01f, 0.02f), ForceMode.Impulse);
		dimer.name = "dimer_" + dimer.GetInstanceID();
		fishtankScript.SetCartoonRendering(dimer);

		float minDist = 0;
		if (currentHand != null) {
			minDist = Vector3.Distance (dimer.transform.position, currentHand.otherHand.hoverSphereTransform.position);
		}
		var match = dimer;
		foreach (Transform child in dimer.transform)
		{
			if (child.name.StartsWith("ring"))
			{
				var childDimer = Instantiate(dimerPrefab, child.transform.position, child.transform.rotation, fishtank);
				childDimer.GetComponent<Rigidbody>().AddForce(-childDimer.transform.forward * Random.RandomRange(0.01f, 0.02f), ForceMode.Impulse);
				childDimer.name = "dimer_" + dimer.GetInstanceID();
				fishtankScript.SetCartoonRendering(childDimer);

				if (currentHand != null)
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

		if (currentHand != null) {
			var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachOthers;
			currentHand.otherHand.AttachObject(match, attachmentFlags);
		}
	}


	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldBreak && hand.otherHand.AttachedObjects.Count <= 2)
		{
			breakRing(hand);
		}
	}

	void HandAttachedUpdate(Hand hand)
	{
		if (gameObject == hand.currentAttachedObject && shouldBreak)
		{
			Vector3 velocity = velEst.GetVelocityEstimate();
			//Debug.Log("Velocity: " + velocity + velocity.magnitude);

			if (velocity.magnitude > 10.0 && ringAttached)
			{
				breakRing(hand);
				ringAttached = false;
			}
		}
	}

	void OnAttachedToHand(Hand hand)
	{
		ringAttached = true;
		var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachFromOtherHand;
		if (dockedToAcceptor && !partnerAcceptor.GetComponent<Ring>().ringAttached)
		{
			hand.AttachObject(partnerAcceptor, attachmentFlags);
		}
		if (dockedToDonor && !partnerDonor.GetComponent<Ring>().ringAttached)
		{
			hand.AttachObject(partnerDonor, attachmentFlags);
		}
		foreach (var ao in hand.AttachedObjects)
		{
			ao.attachedObject.SetActive(true);
		}
		velEst.BeginEstimatingVelocity();
	}

	void OnDetachedFromHand()
	{
		ringAttached = false;
		velEst.FinishEstimatingVelocity();
	}

}
