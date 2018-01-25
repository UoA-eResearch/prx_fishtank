using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;

public class Fishtank : MonoBehaviour
{

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
	private PHSlider phSlider;
	private int phValue;
    public float phMonomer2Dimer;
    public float phDimer2Ring;
    public float phRing2Stack;
	private int probability;

	void FindPairs()
	{
		assignProbability ();
		phValue = phSlider.GetPhValue();
		//Debug.Log("PH value " + phValue);
		pairs = new Dictionary<GameObject, GameObject>();
		masterDimers = new List<GameObject>();
		foreach (var tag in tags)
		{
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
				bool isDonor = true;
				if (tag == "ring")
                {
					if (phValue >= phDimer2Ring && Random.Range(1,101) <= probability)
                    {
                        a.GetComponent<BreakRing>().breakRing(null);
                    }
					if (phValue <= phRing2Stack && Random.Range(1,101) <= probability) {
						foreach (var b in gos) {
							if (a != b) {
								var partnerPos = b.transform.Find ("partnerPos").gameObject;
								float dist = Vector3.Distance (a.transform.position, partnerPos.transform.position);
								if (dist < minDistance && !pairs.ContainsKey (partnerPos)) {
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
										isDonor = true;
									}
								}
								var myPartnerPos = a.transform.Find("partnerPos").gameObject;
								dist = Vector3.Distance(b.transform.position, myPartnerPos.transform.position);
								if (dist < minDistance && !pairs.ContainsKey(myPartnerPos))
								{
									minDistance = dist;
									match = b;
									isDonor = false;
								}
							}
						}
						if (minDistance == float.PositiveInfinity)
						{
							Debug.LogError("Unable to find a partner for " + a.name + "!");
						}
						else
						{
							if (isDonor)
							{
								pairs[a] = match;
								var partnerPos = match.transform.Find("partnerPos").gameObject;
								pairs[partnerPos] = a;
							}
							else
							{
								pairs[match] = a;
								var myPartnerPos = a.transform.Find("partnerPos").gameObject;
								pairs[myPartnerPos] = match;
							}
							Debug.Log(a.name + " is choosing " + match.name + " as target");
						}
					}
				}
				else if (tag == "dimer")
                {
					
                    if (phValue > phMonomer2Dimer)
                    {
                        a.GetComponent<BreakDimer>().breakApartDimer();
                    }
					if (phValue <= phDimer2Ring && Random.Range(1,101) <= probability) {
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
				}
				else //if monomer
				{
					if (phValue <= phMonomer2Dimer)
					{
						minDistance = float.PositiveInfinity;
						match = a;
						foreach (var b in gos)
						{
							if (a != b && !pairs.ContainsKey(b))
							{ // Prevent love triangles
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
				if (thisGoAttached || !go)
				{
					continue;
				}
				if (!pairs.ContainsKey(go))
				{
					var randomPos = go.transform.position + new Vector3(Random.value, Random.value, Random.value) - Vector3.one / 2;
					var randomRot = Random.rotation;
					go.transform.position = Vector3.MoveTowards(go.transform.position, randomPos, Time.deltaTime * pairingVelocity);
					go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, randomRot, Time.deltaTime * rotationVelocity);
					continue;
				}
				var partner = pairs[go];
				if (!partner)
				{
					continue;
				}

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

				if (masterDimers.Contains(go))
				{
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
					catch (KeyNotFoundException e)
					{
						// Incomplete ring, not interested here
					}
					catch (MissingReferenceException e)
					{
						// Something was destroyed
					}
				}

				if (distanceFromTarget > .001f)
				{
					go.transform.position = Vector3.MoveTowards(go.transform.position, targetPos, Time.deltaTime * pairingVelocity);
				}
				else if (!bounds.Contains(go.transform.position))
				{
					// Wayward monomer
					go.transform.position = Vector3.MoveTowards(go.transform.position, bounds.center, Time.deltaTime * pairingVelocity);
				}
				go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * rotationVelocity);
				
			}
		}
	}

	// Use this for initialization
	void Start()
	{
		phSlider = gameObject.GetComponent<PHSlider>();

		tags = new string[] { "monomer", "dimer", "ring" };

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

	void assignProbability(){

		switch (phSlider.GetPhValue()) {
			
		case 9:
			probability = 50;
			break;
		case 8:
			probability = 95;
			break;
		case 7:
			probability = 25;
			break;
		case 6:
			probability = 50;
			break;
		case 5:
			probability = 75;
			break;
		case 4:
			probability = 95;
			break;
		case 3:
			probability = 40;
			break;
		}

	}



	// Update is called once per frame
	void Update()
	{
		PushTogether();
		FixHoverlock();
	}
}
