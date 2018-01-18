using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class BreakDimer : MonoBehaviour {

	public GameObject monomerPrefab;
	public bool shouldMonomerise = true;

	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldMonomerise)
		{
			hand.otherHand.DetachObject(hand.otherHand.currentAttachedObject);
			hand.DetachObject(hand.currentAttachedObject);
			Debug.Log(hand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + hand.name);
			var monomer1 = Instantiate(monomerPrefab, transform.position, transform.rotation, transform.parent);
			monomer1.name = "monomer1 from " + gameObject.name;
			hand.otherHand.AttachObject(monomer1, Hand.defaultAttachmentFlags);
			var child = monomer1.transform.Find("dimerPos");
			var monomer2 = Instantiate(monomerPrefab, child.position, child.rotation, transform.parent);
			monomer2.name = "monomer2 from " + gameObject.name;
			Debug.Log("Destroying " + gameObject.name);
			Destroy(gameObject);
		}
	}
}
