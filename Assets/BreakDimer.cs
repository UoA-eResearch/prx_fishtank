using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class BreakDimer : MonoBehaviour {

	public GameObject monomerPrefab;
	public bool shouldMonomerise = true;
	private VelocityEstimator velEst;
	private bool dimerAttached = false;

	void Awake(){
		velEst = GetComponent<VelocityEstimator> ();
	}

	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldMonomerise)
		{
			// Drop whatever you're holding
			hand.otherHand.DetachObject(hand.otherHand.currentAttachedObject);
			hand.DetachObject(hand.currentAttachedObject);
			Debug.Log(hand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + hand.name);
			// Make a monomer and attach it to the hand. This replaces the dimer you were just holding.
			var monomerPos = transform.Find("monomerPos");
			var monomer1 = Instantiate(monomerPrefab, monomerPos.position, monomerPos.rotation, transform.parent);
			var partnerPos = monomer1.transform.Find("partnerPos");
			var monomer2 = Instantiate(monomerPrefab, partnerPos.position, partnerPos.rotation, transform.parent);
			var names = gameObject.name.Split();
			monomer1.name = names[2];
			monomer2.name = names[4];
			Debug.Log("Destroying " + gameObject.name);
			Destroy(gameObject);

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

	void HandAttachedUpdate(Hand hand){
		if(gameObject == hand.currentAttachedObject && shouldMonomerise){
			Vector3 velocity = velEst.GetVelocityEstimate ();
			Debug.Log ("Velocity: " + velocity + velocity.magnitude);

			if (velocity.magnitude > 1.0 && dimerAttached) {
				breakDimerOnShake (hand);
				dimerAttached = false;
			}
		}
	}

	void breakDimerOnShake(Hand hand){
		GameObject.Find ("fishtank").GetComponent<Fishtank> ().shouldDimerise = false;
		// Drop whatever you're holding
		hand.DetachObject(hand.currentAttachedObject);
		Debug.Log(gameObject.name + " is attached to " + hand.name);
		// Make a monomer and attach it to the hand. This replaces the dimer you were just holding.
		var monomerPos = transform.Find("monomerPos");
		var monomer1 = Instantiate(monomerPrefab, monomerPos.position, monomerPos.rotation, transform.parent);
		var partnerPos = monomer1.transform.Find("partnerPos");
		var monomer2 = Instantiate(monomerPrefab, partnerPos.position, partnerPos.rotation, transform.parent);
		var names = gameObject.name.Split();
		monomer1.name = names[2];
		monomer2.name = names[4];
		Debug.Log("Destroying " + gameObject.name);
		Destroy(gameObject);
	}

	void OnAttachedToHand(){
		dimerAttached = true;
		velEst.BeginEstimatingVelocity ();
	}

	void OnDetachedFromHand(){
		dimerAttached = false;
		velEst.FinishEstimatingVelocity ();
	}

	private void Update()
	{
		var lh = Player.instance.leftHand;
		var rh = Player.instance.rightHand;
		if (lh && lh.currentAttachedObject == null && lh.hoverLocked)
		{
			Debug.LogError("Left hand hoverlocked!!! Forcing off");
			lh.HoverUnlock(null);
		}
		if (rh && rh.currentAttachedObject == null && rh.hoverLocked)
		{
			Debug.LogError("Right hand hoverlocked!!! Forcing off");
			rh.HoverUnlock(null);
		}
	}
}
