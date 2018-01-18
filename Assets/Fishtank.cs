using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Fishtank : MonoBehaviour {

	public GameObject monomerPrefab;
	public GameObject dimerPrefab;
	public int numMonomers = 50;
	private Dictionary<GameObject, GameObject> pairs;
	public float pairingVelocity = .05f;
	public int rotationVelocity = 50;
	private Bounds bounds;
	public bool shouldDimerise = true;

	void FindPairs()
	{
		pairs = new Dictionary<GameObject, GameObject>();
		var monomers = GameObject.FindGameObjectsWithTag("monomer");
		//Debug.Log("There are " + monomers.Length + " monomers around");
		foreach (var a in monomers)
		{
			if (pairs.ContainsKey(a))
			{
				// Already know the pair for this
				continue;
			}
			var minDistance = float.PositiveInfinity;
			var match = a;
			foreach (var b in monomers)
			{
				if (a != b && !pairs.ContainsKey(b)) // Prevent love triangles
				{
					float dist = Vector3.Distance(a.transform.position, b.transform.position);
					if (dist < minDistance)
					{
						minDistance = dist;
						match = b;
					}
				}
			}
			//Debug.Log(a.name + "'s closest pair is " + match.name + " with distance " + minDistance);
			pairs[a] = match;
			pairs[match] = a;
		}
	}

	void PushMonomersTogether()
	{
		var monomers = GameObject.FindGameObjectsWithTag("monomer");
		var lh = Player.instance.leftHand;
		var rh = Player.instance.rightHand;
		foreach (var monomer in monomers)
		{
			var thisMonomerAttached = lh && lh.currentAttachedObject == monomer || rh && rh.currentAttachedObject == monomer;
			if (thisMonomerAttached || !monomer)
			{
				continue;
			}
			var partner = pairs[monomer];
			var dimerPos = partner.transform.Find("dimerPos");
			var targetPos = dimerPos.position;
			var targetRotation = dimerPos.rotation;

			var distanceFromTarget = Vector3.Distance(monomer.transform.position, targetPos);

			var partnerAttached = lh && lh.currentAttachedObject == partner || rh && rh.currentAttachedObject == partner;
			if (distanceFromTarget < .01f && !partnerAttached && partner && shouldDimerise && monomer.GetInstanceID() > partner.GetInstanceID())
			{
				var dimer = Instantiate(dimerPrefab, monomer.transform.position, monomer.transform.rotation, transform);
				dimer.name = "dimer from " + monomer.name + " and " + partner.name;
				Debug.Log(dimer.name);
				Destroy(monomer);
				Destroy(partner);
			}
			
			monomer.transform.position = Vector3.MoveTowards(monomer.transform.position, targetPos, Time.deltaTime * pairingVelocity);
			monomer.transform.rotation = Quaternion.RotateTowards(monomer.transform.rotation, targetRotation, Time.deltaTime * rotationVelocity);

			if (!bounds.Contains(monomer.transform.position))
			{
				// Wayward monomer
				monomer.transform.position = Vector3.MoveTowards(monomer.transform.position, bounds.center, Time.deltaTime * pairingVelocity);
			}
		}
	}

	// Use this for initialization
	void Start () {
		bounds = gameObject.GetComponent<Collider>().bounds;
		var b = bounds.extents;
		for (int i = 0; i < numMonomers; i++)
		{
			var monomer = Instantiate(monomerPrefab, transform);
			var pos = transform.position + new Vector3(Random.Range(-b.x, b.x), Random.Range(-b.y, b.y), Random.Range(-b.z, b.z));
			monomer.transform.position = pos;
			monomer.transform.rotation = Random.rotation;
			monomer.name = "monomer_" + i;	
		}
		InvokeRepeating("FindPairs", 0, .1f);
	}
	
	// Update is called once per frame
	void Update () {
		PushMonomersTogether();
	}
}
