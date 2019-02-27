using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Dimer : MonoBehaviour
{

	public GameObject monomerPrefab;
	public bool shouldBreak = true;
	private VelocityEstimator velEst;
	private bool dimerAttached = false;
	private GameObject monomer1;
	private GameObject monomer2;
	private Transform monomerPos;
	private Transform partnerPos;
	private Transform fishtank;

	public Fishtank fishtankScript;
	public GameObject fishtankGO;
	public ParticleSystem psPartyTrail;

	void Awake()
	{
		velEst = GetComponent<VelocityEstimator>();
		fishtank = transform.parent;
		GameObject fishtankGO = GameObject.Find("fishtank");
		fishtankScript = fishtankGO.GetComponent<Fishtank>();
	}

	void Update()
	{
		if (fishtankScript.partyMode == true)
		{
			if (psPartyTrail.isStopped)
			{
				psPartyTrail.Play();
			}
		}
		else
		{
			if (psPartyTrail.isPlaying)
			{
				psPartyTrail.Stop();
			}
		}
	}

	void BreakDimer(Hand hand)
	{
		// Drop whatever you're holding
		hand.otherHand.DetachObject(hand.otherHand.currentAttachedObject);
		hand.DetachObject(hand.currentAttachedObject);
		//Debug.Log(hand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + hand.name);

		BreakApartDimer();

		if ((monomer1 != null) && (monomer2 != null))
		{
			var distanceToM1 = Vector3.Distance(monomer1.transform.position, hand.otherHand.hoverSphereTransform.position);
			var distanceToM2 = Vector3.Distance(monomer2.transform.position, hand.otherHand.hoverSphereTransform.position);
			var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachOthers;
			if (distanceToM1 < distanceToM2)
			{
				hand.otherHand.AttachObject(monomer1, attachmentFlags);
			}
			else
			{
				hand.otherHand.AttachObject(monomer2, attachmentFlags);
			}
		}

	}

	public void BreakApartDimer()
	{
		// Make a monomer and attach it to the hand. This replaces the dimer you were just holding.
		monomerPos = transform.Find("monomer1");
		monomer1 = Instantiate(monomerPrefab, monomerPos.position, monomerPos.rotation, fishtank);
		//monomer1.GetComponent<Rigidbody>().AddForce(-monomer1.transform.forward * Random.RandomRange(0.01f,0.02f), ForceMode.Impulse);
		monomer1.name = "Mo_" + monomer1.GetInstanceID();
		fishtankScript.SetCartoonRendering(monomer1);
		partnerPos = monomer1.transform.Find("partnerPos");
		monomer2 = Instantiate(monomerPrefab, partnerPos.position, partnerPos.rotation, fishtank);
		//monomer2.GetComponent<Rigidbody>().AddForce(-monomer2.transform.forward * Random.RandomRange(0.01f,0.02f), ForceMode.Impulse);
		monomer2.name = "Mo_" + monomer2.GetInstanceID();
		fishtankScript.SetCartoonRendering(monomer2);
		//Debug.Log("Destroying " + gameObject.name);
		Destroy(gameObject);
	}

	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldBreak)
		{
			BreakDimer(hand);
		}
	}

	void HandAttachedUpdate(Hand hand)
	{
		if (gameObject == hand.currentAttachedObject && shouldBreak)
		{
			Vector3 velocity = velEst.GetVelocityEstimate();
			//Debug.Log("Velocity: " + velocity + velocity.magnitude);

			if (velocity.magnitude > 3.0 && dimerAttached)
			{
				BreakDimer(hand);
				dimerAttached = false;
			}
		}
	}

	void OnAttachedToHand()
	{
		dimerAttached = true;
		velEst.BeginEstimatingVelocity();
	}

	void OnDetachedFromHand()
	{
		dimerAttached = false;
		velEst.FinishEstimatingVelocity();
	}

}
