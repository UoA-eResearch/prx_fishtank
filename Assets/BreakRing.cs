using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class BreakRing : MonoBehaviour {
	
	public GameObject dimerPrefab;
	public bool shouldBreak = true;

	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldBreak)
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
	}
}
