using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class BreakRing : MonoBehaviour {
	
	public GameObject dimerPrefab;
	public bool shouldBreak = true;
	private VelocityEstimator velEst;
	private bool ringAttached = false;

	void Awake()
	{
		velEst = GetComponent<VelocityEstimator>();
	}

	void breakRing(Hand hand)
	{
		// Drop whatever you're holding
		hand.otherHand.DetachObject(hand.otherHand.currentAttachedObject);
		hand.DetachObject(hand.currentAttachedObject);
		Debug.Log(hand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + hand.name);

        // Make a dimer and attach it to the hand. This replaces the ring you were just holding.
        var dimer = Instantiate(dimerPrefab, transform.position, transform.rotation, transform.parent);
        dimer.name = "dimer_" + dimer.GetInstanceID();
        float minDist = Vector3.Distance(dimer.transform.position, hand.otherHand.hoverSphereTransform.position);
        var match = dimer;
        foreach (Transform child in dimer.transform)
        {
            if (child.name.StartsWith("ring"))
            {
                var childDimer = Instantiate(dimerPrefab, child.transform.position, child.transform.rotation, transform.parent);
                childDimer.name = "dimer_" + dimer.GetInstanceID();
                var dist = Vector3.Distance(child.transform.position, hand.otherHand.hoverSphereTransform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    match = childDimer;
                }
            }
        }
        Destroy(gameObject);

        var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachOthers;
		hand.otherHand.AttachObject(match, attachmentFlags);
	}

    public void breakApartRing() {
        // Make a dimer and attach it to the hand. This replaces the ring you were just holding.
        var dimer = Instantiate(dimerPrefab, transform.position, transform.rotation, transform.parent);
        dimer.name = "dimer_" + dimer.GetInstanceID();
        var match = dimer;
        foreach (Transform child in dimer.transform)
        {
            if (child.name.StartsWith("ring"))
            {
                var childDimer = Instantiate(dimerPrefab, child.transform.position, child.transform.rotation, transform.parent);
                childDimer.name = "dimer_" + dimer.GetInstanceID();
            }
        }
        Destroy(gameObject);
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
