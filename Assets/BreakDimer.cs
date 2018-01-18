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
			// Drop whatever you're holding
			hand.otherHand.DetachObject(hand.otherHand.currentAttachedObject);
			hand.DetachObject(hand.currentAttachedObject);
			Debug.Log(hand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + hand.name);
			// Make a monomer and attach it to the hand. This replaces the dimer you were just holding.
			var monomer1 = Instantiate(monomerPrefab, transform.position, transform.rotation, transform.parent);
			hand.otherHand.AttachObject(monomer1, Hand.defaultAttachmentFlags);
			var child = monomer1.transform.Find("dimerPos");
			// Second monomer should go in dimerPos
			var monomer2 = Instantiate(monomerPrefab, child.position, child.rotation, transform.parent);
			var names = gameObject.name.Split();
			monomer1.name = names[2];
			monomer2.name = names[4];
			Debug.Log("Destroying " + gameObject.name);
			Destroy(gameObject);
		}
	}
}
