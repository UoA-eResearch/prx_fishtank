using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class BreakDimer : MonoBehaviour {

	public GameObject monomerPrefab;
	public bool shouldBreak = true;

	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldBreak)
		{
			// Drop whatever you're holding
			hand.otherHand.DetachObject(hand.otherHand.currentAttachedObject);
			hand.DetachObject(hand.currentAttachedObject);
			Debug.Log(hand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + hand.name);
			// Make a monomer and attach it to the hand. This replaces the dimer you were just holding.
			var monomerPos = transform.Find("monomer1");
			var monomer1 = Instantiate(monomerPrefab, monomerPos.position, monomerPos.rotation, transform.parent);
			monomer1.name = "monomer_" + monomer1.GetInstanceID();
			var partnerPos = monomer1.transform.Find("partnerPos");
			var monomer2 = Instantiate(monomerPrefab, partnerPos.position, partnerPos.rotation, transform.parent);
			monomer2.name = "monomer_" + monomer2.GetInstanceID();
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

}
