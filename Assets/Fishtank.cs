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
	private string[] tags;
	public float pairingInterval = .1f;

	void FindPairs()
	{
		pairs = new Dictionary<GameObject, GameObject>();
		foreach (var tag in tags) {
			var gos = GameObject.FindGameObjectsWithTag(tag);
			//Debug.Log("There are " + gos.Length + " " + tag + " around");
			foreach (var a in gos)
			{
				if (pairs.ContainsKey(a))
				{
					// Already know the pair for this
					continue;
				}
				float minDistance = float.PositiveInfinity;
				var match = a;
				if (tag == "dimer")
				{
					foreach (Transform child in a.transform)
					{
						if (child.name.StartsWith("ring"))
						{
							minDistance = float.PositiveInfinity;
							match = child.gameObject;
							foreach (var b in gos)
							{
								if (a != b && !pairs.ContainsKey(b))
								{
									float dist = Vector3.Distance(child.transform.position, b.transform.position);
									if (dist < minDistance)
									{
										minDistance = dist;
										match = b;
									}
								}
							}
							if (minDistance == float.PositiveInfinity)
							{
								Debug.LogError("Unable to find a dimer for " + child.name + " in " + a.name + "!");
							}
							else
							{
								pairs[child.gameObject] = match;
								pairs[match] = child.gameObject;
								Debug.Log(a.name + " has chosen " + match.name + " to fit into " + child.name);
							}
						}
					}
					pairs[a] = a;
				}
				else
				{
					minDistance = float.PositiveInfinity;
					match = a;
					foreach (var b in gos)
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
					if (minDistance == float.PositiveInfinity)
					{
						Debug.LogError("Unable to find a partner for " + a.name + "!");
					}
					else
					{
						//Debug.Log(a.name + "'s closest pair is " + match.name + " with distance " + minDistance);
						pairs[a] = match;
						pairs[match] = a;
					}
				}
			}
		}
	}

	void PushTogether()
	{
		foreach (var tag in tags)
		{
			var gos = GameObject.FindGameObjectsWithTag(tag);
			var lh = Player.instance.leftHand;
			var rh = Player.instance.rightHand;
			foreach (var go in gos)
			{
				var thisGoAttached = lh && lh.currentAttachedObject == go || rh && rh.currentAttachedObject == go;
				if (thisGoAttached || !go || !pairs.ContainsKey(go))
				{
					continue;
				}
				var partner = pairs[go];

				var targetPos = partner.transform.position;
				var targetRotation = partner.transform.rotation;
				if (tag == "monomer")
				{
					var partnerPos = partner.transform.Find("partnerPos");
					targetPos = partnerPos.position;
					targetRotation = partnerPos.rotation;
				}

				var distanceFromTarget = Vector3.Distance(go.transform.position, targetPos);

				var partnerAttached = lh && lh.currentAttachedObject == partner || rh && rh.currentAttachedObject == partner;
				if (distanceFromTarget < .01f && !partnerAttached && partner && shouldDimerise && go.GetInstanceID() > partner.GetInstanceID() && tag == "monomer")
				{
					var dimerPos = go.transform.Find("dimerPos");
					var dimer = Instantiate(dimerPrefab, dimerPos.position, dimerPos.rotation, transform);
					dimer.name = "dimer from " + go.name + " and " + partner.name;
					Debug.Log(dimer.name);
					Destroy(go);
					Destroy(partner);
					continue;
				}

				if (distanceFromTarget > .01f)
				{
					go.transform.position = Vector3.MoveTowards(go.transform.position, targetPos, Time.deltaTime * pairingVelocity);
					go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * rotationVelocity);
				}

				if (!bounds.Contains(go.transform.position))
				{
					// Wayward monomer
					go.transform.position = Vector3.MoveTowards(go.transform.position, bounds.center, Time.deltaTime * pairingVelocity);
				}
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
		tags = new string[] { "monomer", "dimer" };
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
		InvokeRepeating("FindPairs", 0, pairingInterval);
	}
	
	// Update is called once per frame
	void Update () {
		PushTogether();
	}
}
