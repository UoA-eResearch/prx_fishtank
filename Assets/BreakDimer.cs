using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class BreakDimer : MonoBehaviour
{

	public GameObject monomerPrefab;
	public bool shouldBreak = true;
	private VelocityEstimator velEst;
	private bool dimerAttached = false;
	private GameObject monomer1;
	private GameObject monomer2;
	private Transform monomerPos;
	private Transform partnerPos;

	void Awake()
	{
		velEst = GetComponent<VelocityEstimator>();
	}

	void breakDimer(Hand hand)
	{
		// Drop whatever you're holding
		hand.otherHand.DetachObject(hand.otherHand.currentAttachedObject);
		hand.DetachObject(hand.currentAttachedObject);
		Debug.Log(hand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + hand.name);

		breakApartDimer();

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

	public void breakApartDimer()
	{
		// Make a monomer and attach it to the hand. This replaces the dimer you were just holding.
		monomerPos = transform.Find("monomer1");
		monomer1 = Instantiate(monomerPrefab, monomerPos.position, monomerPos.rotation, transform.parent);
		monomer1.GetComponent<Rigidbody>().AddForce(-monomer1.transform.forward, ForceMode.Impulse);
		monomer1.name = "monomer_" + monomer1.GetInstanceID();
		partnerPos = monomer1.transform.Find("partnerPos");
		monomer2 = Instantiate(monomerPrefab, partnerPos.position, partnerPos.rotation, transform.parent);
		monomer2.GetComponent<Rigidbody>().AddForce(-monomer2.transform.forward, ForceMode.Impulse);
		monomer2.name = "monomer_" + monomer2.GetInstanceID();
		Debug.Log("Destroying " + gameObject.name);
		Destroy(gameObject);
	}



	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldBreak)
		{
			breakDimer(hand);
		}
	}

	void HandAttachedUpdate(Hand hand)
	{
		if (gameObject == hand.currentAttachedObject && shouldBreak)
		{
			Vector3 velocity = velEst.GetVelocityEstimate();
			Debug.Log("Velocity: " + velocity + velocity.magnitude);

			if (velocity.magnitude > 2.0 && dimerAttached)
			{
				breakDimer(hand);
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
