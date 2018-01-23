using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;

public class Fishtank : MonoBehaviour {

	public GameObject monomerPrefab;
	public GameObject dimerPrefab;
	public GameObject ringPrefab;
	public int numMonomers = 50;
	private Dictionary<GameObject, GameObject> pairs;
	public float pairingVelocity = .05f;
	public int rotationVelocity = 50;
	private Bounds bounds;
	public bool shouldDimerise = true;
	private string[] tags;
	public float pairingInterval = .1f;
	private List<GameObject> masterDimers;

	void FindPairs()
	{
		pairs = new Dictionary<GameObject, GameObject>();
		masterDimers = new List<GameObject>();
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
				if (tag == "ring")
				{
					foreach (var b in gos)
					{
						if (a != b)
						{
							var partnerPos = b.transform.Find("partnerPos").gameObject;
							float dist = Vector3.Distance(a.transform.position, partnerPos.transform.position);
							if (dist < minDistance && !pairs.ContainsKey(partnerPos))
							{
								var isCyclic = false;
								var next = b;
								while (pairs.ContainsKey(next))
								{
									next = pairs[next];
									if (next == a)
									{
										isCyclic = true;
										Debug.Log(a.name + " was interested in " + b.name + " but they have a pointer to me somewhere in their chain");
										break;
									}
								}
								if (!isCyclic)
								{
									minDistance = dist;
									match = b;
								}
							}
						}
					}
					if (minDistance == float.PositiveInfinity)
					{
						Debug.LogError("Unable to find a partner for " + a.name + "!");
					}
					else
					{
						pairs[a] = match;
						var partnerPos = match.transform.Find("partnerPos").gameObject;
						pairs[partnerPos] = a;
						Debug.Log(a.name + " is choosing " + match.name + " as target");
					}
				}
				else if (tag == "dimer")
				{
					bool hasAll = true;
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
								//Debug.LogError("Unable to find a dimer for " + child.name + " in " + a.name + "!");
								hasAll = false;
							}
							else
							{
								pairs[child.gameObject] = match;
								pairs[match] = child.gameObject;
								//Debug.Log(a.name + " has chosen " + match.name + " to fit into " + child.name + " with dist " + minDistance);
							}
						}
					}
					pairs[a] = a;
					if (hasAll)
					{
						masterDimers.Add(a);
					}
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
				if (!partner) continue;

				var targetPos = partner.transform.position;
				var targetRotation = partner.transform.rotation;
				if (tag == "monomer" || tag == "ring")
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

				if (masterDimers.Contains(go)) {
					try
					{
						float totalDist = 0;
						foreach (Transform child in go.transform)
						{
							if (child.name.StartsWith("ring"))
							{
								if (!pairs.ContainsKey(child.gameObject))
								{
									totalDist += float.PositiveInfinity;
									break;
								}
								var childTarget = pairs[child.gameObject];
								if (!childTarget)
								{
									totalDist += float.PositiveInfinity;
									break;
								}
								var childDist = Vector3.Distance(child.position, childTarget.transform.position);
								totalDist += childDist;
								var childTargetHeld = lh && lh.currentAttachedObject == childTarget || rh && rh.currentAttachedObject == childTarget;
								if (childTargetHeld)
								{
									totalDist += float.PositiveInfinity;
								}
							}
						}
						//Debug.Log(go.name + " is a master dimer, and the sum of it's child ring targets is " + totalDist);
						if (totalDist < .01f)
						{
							var ring = Instantiate(ringPrefab, go.transform.position, go.transform.rotation, transform);
							ring.name = "ring from " + go.name;
							Debug.Log(ring.name);
							masterDimers.Remove(go);
							Destroy(go);
							foreach (Transform child in go.transform)
							{
								if (child.name.StartsWith("ring"))
								{
									var childTarget = pairs[child.gameObject];
									Destroy(childTarget);
								}
							}
						}
					}
					catch (KeyNotFoundException e) {
						// Incomplete ring, not interested here
					}
					catch (MissingReferenceException e)
					{
						// Something was destroyed
					}
				}
				
				go.transform.position = Vector3.MoveTowards(go.transform.position, targetPos, Time.deltaTime * pairingVelocity);
				go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * rotationVelocity);

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
		tags = new string[] { "monomer", "dimer", "ring" };

		phSlider.onValueChanged.AddListener (delegate {
			PHValueChanged ();
		});
		
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

	private void FixHoverlock()
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


	
	// Update is called once per frame
	void Update () {
		PushTogether();
		FixHoverlock();
	}
}
