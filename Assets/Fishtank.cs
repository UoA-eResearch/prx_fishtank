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
	private Dictionary<GameObject, GameObject> pairsDonor;
	public float pairingVelocity = .05f;
	public int rotationVelocity = 50;
	private Bounds bounds;
	private string[] tags;
	public float pairingInterval = 0.1f;
	private List<GameObject> masterDimers;
	private PHSlider phSlider;
	private int phValue;
	public int phMonomer2Dimer;
	public int phDimer2Ring;
	public int phRing2Stack;
	private int probabilityDimerMake;
	private int probabilityDimerBreak;
	private int probabilityRingMake;
	private int probabilityRingBreak;
	private int probabilityStackMake;
	private float fishtankAlpha = 0.01f;
   


	void FindPairs()
	{
		//print(Time.realtimeSinceStartup);
		assignProbability ();
		//Debug.Log ("Prob for" + phValue + "is " + probabilityBind + probabilityBreak);
		phValue = phSlider.GetPhValue();
		//Debug.Log("PH value " + phValue);
		pairs = new Dictionary<GameObject, GameObject>();
		pairsDonor = new Dictionary<GameObject, GameObject>();
		masterDimers = new List<GameObject>();
		foreach (var tag in tags)
		{
			var gos = GameObject.FindGameObjectsWithTag(tag);
			//Debug.Log("There are " + gos.Length + " " + tag + " around");
			foreach (var a in gos)
			{
				
				if (tag == "dimer" && pairs.ContainsKey(a))
				{
					// Already know the pair for this
					continue;
				}
				
				float minDistance = float.PositiveInfinity;
				var match = a;
				bool isDonor = false; //default behaviour is !isDonor - i.e. my go (acceptor) moves to other go's partnerPos (donor)
				bool foundMatchDonor = false;
				bool foundMatchAcceptor = false;
				if (tag == "ring")
				{
					if (Random.Range(1,100) <= probabilityRingBreak)
					{
						a.GetComponent<BreakRing>().breakRing(null);
					}
					else if (Random.Range(1,100) <= probabilityStackMake)
					{
						//var partnerPosA = a.transform.Find("partnerPos").gameObject;
						//Debug.DrawLine(a.transform, a.transform.Find("partnerPos").gameObject.position);
						foreach (var b in gos)
						{
							if (a != b)
							{
								GameObject partnerPos;
								Vector3 b2a;
								float testPairAlignDot;
								float testPairRelateDot;
								float dist;

								if (!pairs.ContainsKey(a))
								{
									// look in acceptor(a)<-donor(b) direction
									partnerPos = b.transform.Find("partnerPos").gameObject;
									//Debug.Log(b.name + " is b " + b.transform.Find("partnerPos").gameObject.name + " is partnerPos ");
									dist = Vector3.Distance(a.transform.position, partnerPos.transform.position);
									b2a = (a.transform.position - b.transform.position).normalized;
									testPairAlignDot = Vector3.Dot(a.transform.up, b.transform.up);
									testPairRelateDot = Vector3.Dot(a.transform.up, b2a);
									Debug.Log("a = " + a.name + " b = " + b.name + " testPairAlignDot =  " + testPairAlignDot + " testPairRelateDot =  " + testPairRelateDot);
									if (dist < minDistance && (testPairAlignDot > 0.0f) && (testPairRelateDot > 0.0f) && !pairsDonor.ContainsKey(b))
									{
										var isCyclic = false;
										var next = b;
										while (pairs.ContainsKey(next))
										{
											next = pairs[next];
											if (next == a)
											{
												isCyclic = true;
												Debug.Log(a.name + " was interested in being ACCEPTOR from " + b.name + " but they have a pointer to me somewhere in their chain");
												break;
											}
										}
										if (!isCyclic)
										{
											minDistance = dist;
											match = b;
											foundMatchDonor = true;
											isDonor = false;
											//Debug.DrawLine(a.transform.position, b.transform.position, Color.cyan, 0.2f);
										}
									}
									if (foundMatchDonor)
									{
										pairs[a] = match;
										pairsDonor[match] = a;
										partnerPos = match.transform.Find("partnerPos").gameObject;
										//pairs[partnerPos] = a;
										Vector3 pairTransform = (partnerPos.transform.position - a.transform.position);
										Debug.DrawLine((a.transform.position + (0.75f * pairTransform)), partnerPos.transform.position, Color.blue, 0.2f);
										Debug.DrawLine(a.transform.position, (a.transform.position + (0.75f * pairTransform)), Color.cyan, 0.2f);
										Debug.Log(a.name + " as ACCEPTOR is choosing " + match.name + " as donor");
									}
								}
								
								if (!pairsDonor.ContainsKey(a))
								{
									// look in donor(a)->acceptor(b) direction
									var myPartnerPos = a.transform.Find("partnerPos").gameObject;
									dist = Vector3.Distance(b.transform.position, myPartnerPos.transform.position);
									b2a = (a.transform.position - b.transform.position).normalized;
									testPairAlignDot = Vector3.Dot(a.transform.up, b.transform.up);
									testPairRelateDot = Vector3.Dot(a.transform.up, b2a);

									if (dist < minDistance && (testPairAlignDot > 0.0f) && (testPairRelateDot < 0.0f) && !pairs.ContainsKey(b))
									{
										var isCyclic = false;
										var next = b;
										while (pairsDonor.ContainsKey(next))
										{
											next = pairsDonor[next];
											if (next == a)
											{
												isCyclic = true;
												Debug.Log(a.name + " was interested in being DONOR to " + b.name + " but they have a pointer to them somewhere in their chain");
												break;
											}
										}
										if (!isCyclic)
										{
											minDistance = dist;
											match = b;
											foundMatchAcceptor = true;
											isDonor = true;
											//Debug.DrawLine(a.transform.position, b.transform.position, Color.magenta, 0.2f);
										}
									}
									if (foundMatchAcceptor)
									{
										pairs[match] = a;
										pairsDonor[a] = match;
										//var myPartnerPos = a.transform.Find("partnerPos").gameObject;
										//pairs[myPartnerPos] = match;
										Vector3 pairTransform = (match.transform.position - myPartnerPos.transform.position);
										//Debug.DrawLine(match.transform.position, myPartnerPos.transform.position, Color.magenta, 0.2f);
										Debug.DrawLine(myPartnerPos.transform.position, myPartnerPos.transform.position + (0.25f * pairTransform), Color.red, 0.2f);
										Debug.DrawLine(myPartnerPos.transform.position + (0.25f * pairTransform), match.transform.position, Color.magenta, 0.2f);
										Debug.Log(a.name + " as DONOR is choosing " + match.name + " as acceptor");
									}
									
								}
								
							}
						}
						if (minDistance == float.PositiveInfinity)
						{
							//Debug.LogError("Unable to find a partner for " + a.name + "!");
						}
						else
						{
							if (!isDonor)
							{

							}
							else // isDonor
							{

							}
							//Debug.Log(a.name + " is choosing " + match.name + " as target");
						}
					}
				}
				else if (tag == "dimer")
				{
					
					if (Random.Range (1, 100) <= probabilityDimerBreak)
					{
						a.GetComponent<BreakDimer>().breakApartDimer();
					}
					else if (Random.Range (1, 100) <= probabilityRingMake) {
						bool hasAll = true;
						foreach (Transform child in a.transform) {
							if (child.name.StartsWith ("ring")) {
								minDistance = float.PositiveInfinity;
								match = child.gameObject;
								foreach (var b in gos) {
									if (a != b && !pairs.ContainsKey (b)) {
										float dist = Vector3.Distance (child.transform.position, b.transform.position);
										if (dist < minDistance) {
											minDistance = dist;
											match = b;
										}
									}
								}
								if (minDistance == float.PositiveInfinity) {
									//Debug.LogError("Unable to find a dimer for " + child.name + " in " + a.name + "!");
									hasAll = false;
								} else {
									pairs [child.gameObject] = match;
									pairs [match] = child.gameObject;
									//Debug.Log(a.name + " has chosen " + match.name + " to fit into " + child.name + " with dist " + minDistance);
								}
							}
						}
						pairs [a] = a;
						if (hasAll) {
							masterDimers.Add (a);
						}
					}
				}
				else if (tag == "monomer")
				{
					if (Random.Range(1, 100) <= probabilityDimerMake)
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
							//Debug.LogError("Unable to find a partner for " + a.name + "!");
						}
						else
						{
							 //Debug.Log(a.name + "'s closest pair is " + match.name + " with distance " + minDistance);
							pairs[a] = match;
							pairs[match] = a;
						}
					}
				}
				else
				{
					Debug.LogError("Untagged GameObject!");
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
					//go is being held by player
					continue;
				}
				// check for unpaired gos which should drift
				if (!pairs.ContainsKey(go))
				{
					if (tag == "monomer" || tag == "dimer")
					{
						// unpaired monomer or dimer (no pair) => should drift
						AddRandomMotion(go);
						continue;
					}
					else if (!pairsDonor.ContainsKey(go))
					{
						// unpaired ring (no pair or pairDonor) => should drift
						AddRandomMotion(go);
						continue;
					}
				}
				//var partner = pairs[go];
				GameObject partner;
				if (pairs.TryGetValue(go, out partner))
				{
					//partner = pairs[go];
					var targetPos = partner.transform.position;
					var targetRotation = partner.transform.rotation;

					if (tag == "monomer")
					{
						var partnerPos = partner.transform.Find("partnerPos");
						targetPos = partnerPos.position;
						targetRotation = partnerPos.rotation;
					}

					if (tag == "ring")
					{
						/*
						var partnerPos = partner.transform.Find("partnerPos");
						targetPos = partnerPos.position;
						targetRotation = partnerPos.rotation;
						*/

						var partnerPos = partner.transform.Find("partnerPos").gameObject;
						targetPos = partnerPos.transform.position;
						targetRotation = partnerPos.transform.rotation;
						if (pairs.ContainsKey(go) && !pairsDonor.ContainsKey(go))
						{
							//Debug.Log("----->" + go.name + " is a RING on the ACCEPTOR end of a stack");
						}
						if (pairs.ContainsKey(go) && pairsDonor.ContainsKey(go))
						{
							//Debug.Log("----->" + go.name + " is a RING in the MIDDLE of a stack");
						}
						if (!pairs.ContainsKey(go) && pairsDonor.ContainsKey(go))
						{
							//never get here because inside - if (pairs.TryGetValue(go, out partner))
							Debug.Log("----->" + go.name + " is a RING on the DONOR end of a stack");
						}
					}

					var distanceFromTarget = Vector3.Distance(go.transform.position, targetPos);

					var partnerAttached = lh && lh.currentAttachedObject == partner || rh && rh.currentAttachedObject == partner;
					if (distanceFromTarget < .01f && !partnerAttached && partner && go.GetInstanceID() > partner.GetInstanceID() && tag == "monomer")
					{
						var dimerPos = go.transform.Find("dimerPos");
						var dimer = Instantiate(dimerPrefab, dimerPos.position, dimerPos.rotation, transform);
						dimer.name = "dimer from " + go.name + " and " + partner.name;
						
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
							if (totalDist < 0.01f)
							{
								var ring = Instantiate(ringPrefab, go.transform.position, go.transform.rotation, transform);
								ring.name = "ring from " + go.name;
								//Debug.Log(ring.name);
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
							//master dimers have pairs but do not try to move towards them
							//but making master dimers wander seems to compromise ring formation
							//AddRandomMotion(go);
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

					if (!bounds.Contains(go.transform.position))
					{
						// monomer/dimer/ring is outside tank bounds
						go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(bounds.center - go.transform.position) * Time.deltaTime * Random.Range(0.1f, 0.5f), ForceMode.Impulse);
					}
					else if (distanceFromTarget > 0.02f) // use rigidbody forces to push paired game objects together
					{
						float maxPush = Mathf.Min(distanceFromTarget * 5.0f, 0.5f);
						go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize((targetPos - go.transform.position) + (Random.onUnitSphere * 0.01f)) * Time.deltaTime * Random.RandomRange(0.0f, maxPush), ForceMode.Impulse);
						go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.RandomRange(0.1f, 0.5f) * rotationVelocity);
					}
					else // close to target transform - manipulate my transform directly (not through rigidbody)
					{
						//Debug.Log(dimer.name);
						go.transform.position = Vector3.MoveTowards(go.transform.position, targetPos, Time.deltaTime * pairingVelocity);
						go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.RandomRange(0.1f, 0.1f) * rotationVelocity);
					}

					//go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.RandomRange(0.1f, 0.5f) * rotationVelocity);

					/*
					{
					targetPos = transform.InverseTransformPoint(targetPos);
					var requiredTorqueX = (targetPos.x / targetPos.magnitude);
					var requiredTorqueY = (targetPos.y / targetPos.magnitude);
					float rotationTorque = 0.01f;
					go.GetComponent<Rigidbody>().AddRelativeTorque(((rotationTorque) * requiredTorqueY), ((rotationTorque) * requiredTorqueX) * -1, 0f, ForceMode.Impulse);
					}
					*/
				} else
				{
					{
						//Debug.Log("----->" + go.name + " has no partner");
						if (tag != "ring")
						{
							continue;
						}
						else
						{
							//Debug.Log("-----> RING " + go.name + " has no pairs partner");
							if (false)//pairsDonor.ContainsKey(go))
							{
								// ring with donor only (end of chain)
								//Debug.Log("----->" + go.name + " has a pairsDonor and should move to them? ");
								//Debug.Log("----->" + go.name + " is a RING on the DONOR end of a stack");

								{ // duplicated movement code - added for movement of end DONOR ring - needs refactoring

									var partnerAcceptor = pairsDonor[go];
									var partnerPos = partnerAcceptor.transform.Find("partnerPos").gameObject;
									var targetPos = partnerPos.transform.position;
									var targetRotation = partnerPos.transform.rotation;
									var distanceFromTarget = Vector3.Distance(go.transform.position, targetPos);

									if (!bounds.Contains(go.transform.position))
									{
										// monomer/dimer/ring is outside tank bounds
										go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(bounds.center - go.transform.position) * Time.deltaTime * Random.Range(0.1f, 0.5f), ForceMode.Impulse);
									}
									else if (distanceFromTarget > 0.02f) // use rigidbody forces to push paired game objects together
									{
										float maxPush = Mathf.Min(distanceFromTarget * 5.0f, 0.5f);
										go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize((targetPos - go.transform.position) + (Random.onUnitSphere * 0.01f)) * Time.deltaTime * Random.RandomRange(0.0f, maxPush), ForceMode.Impulse);
									}
									else // close to target transform - manipulate my transform directly (not through rigidbody)
									{
										go.transform.position = Vector3.MoveTowards(go.transform.position, targetPos, Time.deltaTime * pairingVelocity);
									}

									go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.RandomRange(0.1f, 0.5f) * rotationVelocity);
								}
								continue;
							}
						}

					}
				}

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

	private void AddRandomMotion(GameObject go)
	{
		
		if (!bounds.Contains(go.transform.position)) // duplicate code needs tidying
		{
			// Wayward go monomer/dimer/ring
			//go.transform.position = Vector3.MoveTowards(go.transform.position, bounds.center, Time.deltaTime * pairingVelocity);
			go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(bounds.center - go.transform.position) * Time.deltaTime * Random.RandomRange(0.1f, 0.5f), ForceMode.Impulse);
		}
		else
		{
			go.GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * Time.deltaTime * Random.RandomRange(1.0f, 2.0f), ForceMode.Impulse);
		}
		
		go.GetComponent<Rigidbody>().AddRelativeTorque(0.0001f * Random.onUnitSphere, ForceMode.Impulse);
	}

	void assignProbability(){
		Color col;
		col.a = fishtankAlpha;
		col = Color.cyan;

		switch (phSlider.GetPhValue()) {
			
		case 9:
			probabilityDimerMake = 1;
			probabilityDimerBreak = 50;
			probabilityRingMake = 0;
			probabilityRingBreak = 100;
			probabilityStackMake = 0;
			col = Color.cyan;
			break;

		case 8:
			probabilityDimerMake = 10;
			probabilityDimerBreak = 1;
			probabilityRingMake = 0;
			probabilityRingBreak = 100;
			probabilityStackMake = 0;
			col = Color.blue;
			break;

		case 7:
			probabilityDimerMake = 100;
			probabilityDimerBreak = 0;
			probabilityRingMake = 10;
			probabilityRingBreak = 5;
			probabilityStackMake = 1;
			col = Color.gray;
			break;

		case 6:
			probabilityDimerMake = 100;
			probabilityDimerBreak = 0;
			probabilityRingMake = 50;
			probabilityRingBreak = 2;
			probabilityStackMake = 1;
			col = Color.green;
			break;

		case 5:
			probabilityDimerMake = 100;
			probabilityDimerBreak = 0;
			probabilityRingMake = 50;
			probabilityRingBreak = 1;
			probabilityStackMake = 1;
			col = Color.magenta;
			break;

		case 4:
			probabilityDimerMake = 100;
			probabilityDimerBreak = 0;
			probabilityRingMake = 100;
			probabilityRingBreak = 1;
			probabilityStackMake = 50;
			col = Color.red;
			break;

		case 3:
			probabilityDimerMake = 100;
			probabilityDimerBreak = 0;
			probabilityRingMake = 100;
			probabilityRingBreak = 0;
			probabilityStackMake = 100;
			col = Color.yellow;
			break;
		}
		col.a = fishtankAlpha;
		gameObject.GetComponent<Renderer> ().material.color = col;

	}

	void ClampRigidBodyDynamics()
	{
		// hack to calm down physics explosions
		foreach (var tag in tags)
		{
			var gos = GameObject.FindGameObjectsWithTag(tag);
			//Debug.Log("There are " + gos.Length + " " + tag + " around");
			foreach (var go in gos)
			{
				go.GetComponent<Rigidbody>().velocity = Vector3.ClampMagnitude(go.GetComponent<Rigidbody>().velocity, 2.0f);
				go.GetComponent<Rigidbody>().angularVelocity = Vector3.ClampMagnitude(go.GetComponent<Rigidbody>().angularVelocity, 6.0f);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		PushTogether();
		FixHoverlock();
		ClampRigidBodyDynamics();
	}
}
