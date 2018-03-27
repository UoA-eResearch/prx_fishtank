﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;

public class Fishtank : MonoBehaviour
{

	public GameObject monomerPrefab;
	public GameObject dimerPrefab;
	public GameObject ringPrefab;
	public Text timer;
	public Text timer2;
	public Text monomerCount;
	public Text dimerCount;
	public Text ringCount;
	public Text ringCount2;
	public Text stackLongestTxt;
	public Text stackNumberTxt;
	private bool hasWon = false;
	private bool confettiDone = false;

	private int stackLongest = 0;
	private int stacks = 0;

	public int numMonomers = 180;
	private Dictionary<GameObject, GameObject> pairs;

	private Dictionary<GameObject, GameObject> pairsMyAcceptor;
	private Dictionary<GameObject, GameObject> pairsMyAcceptorPrev;
	private Dictionary<GameObject, GameObject> pairsMyDonor;
	private Dictionary<GameObject, GameObject> pairsMyDonorPrev;

	public GameObject ringPS;
	public GameObject confettiPS;

	public AudioSource fishtankAudioSource;

	public PartyModeSwitch partyModeSwitch;
	
	public GameObject solventH;
	public GameObject solventOH;
	public GameObject solventH2O;

	private ParticleSystem psSolventH;
	private ParticleSystem psSolventOH;
	private ParticleSystem psSolventH2O;

	private Bounds bounds;
	private string[] tags;

	private List<GameObject> masterDimers;
	private PHSlider phSlider;
	private int phValue;
	//public int phMonomer2Dimer;
	//public int phDimer2Ring;
	//public int phRing2Stack;
	private int probabilityDimerMake;
	private int probabilityDimerBreak;
	private int probabilityRingMake;
	private int probabilityRingBreak;
	private int probabilityStackMake;
	private float fishtankAlpha = 0.01f;

	public float alignLimitDot = 0.5f;
	public float relateLimitDot = 0.5f;

	// magic numbers for pushing gos around - entirely empirical !

	private float forceTankMin = 0.1f;                  // impulse force range for keeping go in tank
	private float forceTankMax = 0.5f;

	private float torqueDiffuse = 0.0001f;              // torque for random motion (monomer / dimer) - ring is higher in code
	private float forceDiffuseMin = 1.0f;               // impulse forces for random motion
	private float forceDiffuseMax = 2.0f;

	public float pairingInterval = 0.1f;

	public float ringAntiparallelCheckInterval = 1f;

	public float pairingVelocity = 0.05f;               // translation rate for pairing using positional transform lerp
	public int pairingRotationVelocity = 40;            // rotation rate for pairing using quaternion slerp

	public float minDistApplyRBForces = 0.02f;          // lower distance limit for using forces on RBs to push monomer / dimer go together
	public float minDistApplyRBForcesRing = 0.08f;      // lower distance limit for using forces on RBs to push ring go together

	public float stackForceDistance = 0.01f;            // distance threshold for forcing ring stack - hack to allow some stack manipulation with motion controller

	public float ringRepelDistance = 0.2f;
	public float ringAngleDiff = 10;

	public float pairingForcingVelocity = 20.0f;        // translation rate for pairing using positional transform lerp - maintains forced ring stacking for manipulation
	public int pairingForcingRotationVelocity = 50;     // rotation rate for pairing using quaternion slerp

	public int ringRotSymmetry = 6;						// number of equivalent docking positions around ring

	public bool cheat = false;

	void FindPairs()
	{
		//print(Time.realtimeSinceStartup);
		assignProbability();
		//Debug.Log ("Prob for" + phValue + "is " + probabilityBind + probabilityBreak);
		phValue = phSlider.GetPhValue();
		//Debug.Log("PH value " + phValue);
		pairs = new Dictionary<GameObject, GameObject>();
		pairsMyAcceptor = new Dictionary<GameObject, GameObject>();
		pairsMyDonor = new Dictionary<GameObject, GameObject>();

		masterDimers = new List<GameObject>();
		foreach (var a in GameObject.FindGameObjectsWithTag("ring"))
		{
			var ring = a.GetComponent<Ring>();
			ring.partnerDonor = null;
			ring.partnerAcceptor = null;
		}

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
				if (tag == "ring")
				{
					GameObject bestDonor = null;
					GameObject bestAcceptor = null;
					var bestDonorScore = float.PositiveInfinity;
					var bestAcceptorScore = float.PositiveInfinity;
					if (Random.Range(1, 100) <= probabilityRingBreak)
					{
						a.GetComponent<Ring>().breakRing(null);
					}
					else if (Random.Range(1, 100) <= probabilityStackMake)
					{
						//var partnerPosA = a.transform.Find("partnerPos").gameObject;
						//Debug.DrawLine(a.transform, a.transform.Find("partnerPos").gameObject.position);
						foreach (var b in gos)
						{
							if (a != b)
							{
								Vector3 b2a;
								float testPairAlignDot;
								float testPairRelateDot;

								if (a.GetComponent<Ring>().partnerDonor == null && b.GetComponent<Ring>().partnerAcceptor == null) ;
								// I don't have a donor, and b isn't already donating so far in this findPairs() call 
								{
									// look in acceptor(a)<-donor(b) direction
									//Debug.Log(a.name + " as ACC is testing " + b.name + "as poss DONOR: align = " + testPairAlignDot + " relate = " + testPairRelateDot);
									var partnerPos = b.transform.Find("acceptorPos").gameObject; // a is accepting so we are looking to b's acceptorPos
									var dist = Vector3.Distance(a.transform.position, partnerPos.transform.position);
									//var angleDiff = Quaternion.Angle(a.transform.rotation, partnerPos.transform.rotation);
									var bHasBetterAcceptor = false;
									var score = float.PositiveInfinity;
									//var score = dist * angleDiff;

									b2a = (a.transform.position - b.transform.position).normalized;
									testPairAlignDot = Vector3.Dot(a.transform.up, b.transform.up);
									testPairRelateDot = Vector3.Dot(a.transform.up, b2a);



									if ((testPairAlignDot > alignLimitDot) && (testPairRelateDot > relateLimitDot))
									{
										score = dist;

										if (pairsMyAcceptorPrev.ContainsKey(b))
										{
											// b was a donor last time and c was the acceptor (i.e. a is now competing with c)

											var c = pairsMyAcceptorPrev[b];
											//Debug.Log(b.name + " was a DONOR LAST time " + c.name + " was the ACC");
											var cdist = Vector3.Distance(c.transform.position, partnerPos.transform.position);
											//var cangleDiff = Quaternion.Angle(c.transform.rotation, partnerPos.transform.rotation);

											var cscore = cdist;// * cangleDiff;

											if (cscore < score)
											{
												bHasBetterAcceptor = true;
												//Debug.Log(a.name + " as ACC is testing " + b.name + " as possible DONOR but " + c.name + " is a better ACC");
											}
										}
									}


									if (!bHasBetterAcceptor)
									{
										var next = b;
										while (next.GetComponent<Ring>().partnerDonor != null)
										{
											next = next.GetComponent<Ring>().partnerDonor;
											if (next == a) // cyclic
											{
												score = float.PositiveInfinity;
												break;
											}
										}
										if (score < bestDonorScore)
										{
											bestDonorScore = score;
											bestDonor = b;
										}
									}

								}

								if (a.GetComponent<Ring>().partnerAcceptor == null && b.GetComponent<Ring>().partnerDonor == null)
								// I'm not yet a donor, and b has an acceptor slot I could fill - so far in this findPairs() call 
								{
									// look in donor(a)->acceptor(b) direction
									var myPartnerPos = a.transform.Find("acceptorPos").gameObject;
									var dist = Vector3.Distance(b.transform.position, myPartnerPos.transform.position);
									//var angleDiff = Quaternion.Angle(b.transform.rotation, myPartnerPos.transform.rotation);
									var bHasBetterDonor = false;

									var score = float.PositiveInfinity;
									//var score = dist * angleDiff;

									b2a = (a.transform.position - b.transform.position).normalized;
									testPairAlignDot = Vector3.Dot(a.transform.up, b.transform.up);
									testPairRelateDot = Vector3.Dot(a.transform.up, b2a);

									if ((testPairAlignDot > alignLimitDot) && (testPairRelateDot < -1.0f * relateLimitDot))
									{
										score = dist;

										if (pairsMyDonorPrev.ContainsKey(b))
										{
											// b was an acceptor last time and c was the donor (i.e. a is now competing with c)
											var c = pairsMyDonorPrev[b];
											var cPartnerPos = c.transform.Find("acceptorPos").gameObject;
											var cdist = Vector3.Distance(b.transform.position, cPartnerPos.transform.position);
											//var cangleDiff = Quaternion.Angle(c.transform.rotation, cPartnerPos.transform.rotation);

											var cscore = cdist;// * cangleDiff;

											if (cscore < score)
											{
												bHasBetterDonor = true;
												//Debug.Log(a.name + " as DONOR is testing " + b.name + " as ACC but " + c.name + " is better DONOR");
											}
										}
									}

									if (!bHasBetterDonor)
									{
										var next = b;
										while (next.GetComponent<Ring>().partnerAcceptor != null)
										{
											next = next.GetComponent<Ring>().partnerAcceptor;
											if (next == a) // cyclic
											{
												score = float.PositiveInfinity;
												break;
											}
										}

										//Debug.Log(a.name + " acting as acceptor for " + b.name + " score=" + score);

										if (score < bestAcceptorScore)
										{
											bestAcceptorScore = score;
											bestAcceptor = b;
										}
									}

								}

							}
						}
						if (bestDonor != null) //(bestDonorScore < bestAcceptorScore && bestDonor != null)
						{
							a.GetComponent<Ring>().partnerDonor = bestDonor;
							bestDonor.GetComponent<Ring>().partnerAcceptor = a;
							var partnerPos = bestDonor.transform.Find("acceptorPos").position; // I want to go to my donor's acceptorPos

							Vector3 pairTransform = (partnerPos - a.transform.position);
							Debug.DrawLine((a.transform.position + (0.75f * pairTransform)), partnerPos, Color.blue, 0.2f);
							Debug.DrawLine(a.transform.position, (a.transform.position + (0.75f * pairTransform)), Color.cyan, 0.2f);
							//Debug.Log(a.name + " as ACCEPTOR is choosing " + bestDonor.name + " as donor");

							pairsMyDonor[a] = bestDonor;
							pairsMyAcceptor[bestDonor] = a;

						}
						if (bestAcceptor != null) //(bestAcceptorScore < bestDonorScore && bestAcceptor != null && bestDonor != bestAcceptor)
						{
							a.GetComponent<Ring>().partnerAcceptor = bestAcceptor;
							bestAcceptor.GetComponent<Ring>().partnerDonor = a;
							var partnerPos = bestAcceptor.transform.Find("donorPos").position; // I want to go to my acceptor's donorPos

							Vector3 pairTransform = (partnerPos - a.transform.position);
							Debug.DrawLine((a.transform.position + (0.75f * pairTransform)), partnerPos, Color.red, 0.2f);
							Debug.DrawLine(a.transform.position, (a.transform.position + (0.75f * pairTransform)), Color.magenta, 0.2f);
							//Debug.Log(a.name + " as DONOR is choosing " + bestAcceptor.name + " as acceptor");

							pairsMyAcceptor[a] = bestAcceptor;
							pairsMyDonor[bestAcceptor] = a;

						}
						if (bestAcceptor == null && bestDonor == null)
						{
							//Debug.LogError("Unable to find a donor or acceptor ring for " + a.name + "!");
						}
					}
				}
				else if (tag == "dimer")
				{

					if (Random.Range(1, 100) <= probabilityDimerBreak)
					{
						a.GetComponent<BreakDimer>().breakApartDimer();
					}
					else if (Random.Range(1, 100) <= probabilityRingMake)
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
		//pairsMyAcceptorPrev = new Dictionary<GameObject, GameObject>();
		//pairsMyDonorPrev = new Dictionary<GameObject, GameObject>();
		pairsMyAcceptorPrev = pairsMyAcceptor;
		pairsMyDonorPrev = pairsMyDonor;
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
				if (tag == "monomer" || tag == "dimer")
				{
					if (!pairs.ContainsKey(go))
					{
						// unpaired monomer or dimer (no pair) => should drift
						AddRandomMotion(go);
						continue;
					}
				}
				if (tag == "ring")
				{
					if (go.GetComponent<Ring>().partnerAcceptor == null && go.GetComponent<Ring>().partnerDonor == null)
					{
						// unpaired ring (no donor or acceptor) => should drift
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

					var distanceFromTarget = Vector3.Distance(go.transform.position, targetPos);

					var partnerAttached = lh && lh.currentAttachedObject == partner || rh && rh.currentAttachedObject == partner;
					if (distanceFromTarget < .01f && !partnerAttached && partner && go.GetInstanceID() > partner.GetInstanceID() && tag == "monomer")
					{
						var dimerPos = go.transform.Find("dimerPos");
						var dimer = Instantiate(dimerPrefab, dimerPos.position, dimerPos.rotation, transform);
						dimer.name = "dimer (" + go.name + " + " + partner.name + ")";

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

								if (partyModeSwitch.partying)
								{
									var ringVfx = Instantiate(ringPS, go.transform.position, Quaternion.identity);
									Destroy(ringVfx, 4.0f);
								}

								ring.name = "ring [ " + go.name + "]";
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

					/*
					if (tag == "ring")
					{

					}
					*/

					// 
					if (tag == "monomer" || tag == "dimer")
					{
						if (distanceFromTarget > minDistApplyRBForces) // use rigidbody forces to push paired game objects together
						{
							float maxPush = Mathf.Min(distanceFromTarget * 5.0f, 0.5f); //
							go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize((targetPos - go.transform.position) + (Random.onUnitSphere * 0.01f)) * Time.deltaTime * Random.Range(0.0f, maxPush), ForceMode.Impulse);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 0.5f) * pairingRotationVelocity);
						}
						else
						{
							//Debug.Log(dimer.name);
							go.transform.position = Vector3.MoveTowards(go.transform.position, targetPos, Time.deltaTime * pairingVelocity);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 0.1f) * pairingRotationVelocity);
						}
					}
				}
				else if (tag == "ring")
				{
					var ring = go.GetComponent<Ring>();
					float bestRotationOffsetAngle = 0.0f;

					ring.dockedToDonor = false;
					ring.dockedToAcceptor = false;

					if (ring.partnerDonor != null)
					{
						var donor = ring.partnerDonor.transform.Find("acceptorPos");
						var donorPos = donor.position;
						var targetRotation = donor.rotation;
						var distanceFromDonorPos = Vector3.Distance(go.transform.position, donorPos);

						//var angle = Quaternion.Angle(go.transform.rotation, targetRotation);
						//Debug.Log(go.name + " is " + angle + " from donor target rotation ");

						{
							bestRotationOffsetAngle = GetBestRotationOffsetAngle(donor.rotation, go);
							//Debug.Log("bestRotationOffsetAngle (donor) is " + bestRotationOffsetAngle);
							targetRotation = donor.rotation * Quaternion.Euler(new Vector3(0, bestRotationOffsetAngle, 0));
						}

						if (distanceFromDonorPos > minDistApplyRBForcesRing) // use rigidbody forces to push paired game objects together
						{
							float maxPush = Mathf.Min(distanceFromDonorPos * 5.0f, 0.5f); //
							go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize((donorPos - go.transform.position) + (Random.onUnitSphere * 0.01f)) * Time.deltaTime * Random.Range(0.0f, maxPush), ForceMode.Impulse);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 0.5f) * pairingRotationVelocity);
						}
						else if (distanceFromDonorPos > stackForceDistance)
						{
							go.transform.position = Vector3.MoveTowards(go.transform.position, donorPos, Time.deltaTime * pairingVelocity);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 0.5f) * pairingRotationVelocity);
						}
						else
						{
							// update docked flags - not currently used except for debug in inspector
							ring.dockedToDonor = true;

							go.transform.position = Vector3.MoveTowards(go.transform.position, donorPos, Time.deltaTime * pairingForcingVelocity);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * pairingForcingRotationVelocity);

						}
						if (cheat)
						{
							go.transform.position = donorPos;
							go.transform.rotation = targetRotation;
						}
					}
					if (ring.partnerAcceptor != null)
					{
						var acceptor = ring.partnerAcceptor.transform.Find("donorPos");
						var acceptorPos = acceptor.position;
						var targetRotation = acceptor.rotation;
						var distanceFromAcceptorPos = Vector3.Distance(go.transform.position, acceptorPos);

						{
							bestRotationOffsetAngle = GetBestRotationOffsetAngle(acceptor.rotation, go);
							//Debug.Log("bestRotationOffsetAngle (acceptor) is " + bestRotationOffsetAngle);
							targetRotation = acceptor.rotation * Quaternion.Euler(new Vector3(0, bestRotationOffsetAngle, 0));
						}
			
						/*
						{
							// possible optimisation as alternative to code block above
							bestRotationOffsetAngle = 360.0f - bestRotationOffsetAngle;
							targetRotation = acceptor.rotation * Quaternion.Euler(new Vector3(0, bestRotationOffsetAngle, 0));
							Debug.Log("bestRotationOffsetAngle (acceptor) is " + bestRotationOffsetAngle);
						}
						*/

						if (distanceFromAcceptorPos > minDistApplyRBForcesRing) // use rigidbody forces to push paired game objects together
						{
							float maxPush = Mathf.Min(distanceFromAcceptorPos * 5.0f, 0.5f); //
							go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize((acceptorPos - go.transform.position) + (Random.onUnitSphere * 0.01f)) * Time.deltaTime * Random.Range(0.0f, maxPush), ForceMode.Impulse);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 0.5f) * pairingRotationVelocity);
						}
						else if (distanceFromAcceptorPos > stackForceDistance)
						{
							go.transform.position = Vector3.MoveTowards(go.transform.position, acceptorPos, Time.deltaTime * pairingVelocity);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 0.5f) * pairingRotationVelocity);
						}
						else
						{
							// update docked flags - not currently used except for debug in inspector
							ring.dockedToAcceptor = true;

							//go.transform.position = Vector3.MoveTowards(go.transform.position, acceptorPos, Time.deltaTime * pairingForcingVelocity);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * pairingForcingRotationVelocity);

						}

						if (cheat)
						{
							go.transform.position = acceptorPos;
							go.transform.rotation = targetRotation;
						}
					}
				}



				if (!bounds.Contains(go.transform.position) && tag != "dimer")
				{
					go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(bounds.center - go.transform.position) * Time.deltaTime * Random.Range(forceTankMin, forceTankMax), ForceMode.Impulse);
				}
			}
		}
	}

	// Use this for initialization
	void Start()
	{

		phSlider = gameObject.GetComponent<PHSlider>();

		// initialise solvent particle systems
		var mysolventH = Instantiate(solventH,  gameObject.transform.position, Quaternion.identity);
		var mysolventOH = Instantiate(solventOH, gameObject.transform.position, Quaternion.identity);
		var mysolventH2O = Instantiate(solventH2O, gameObject.transform.position, Quaternion.identity);

		psSolventH = mysolventH.GetComponentInChildren<ParticleSystem>();
		psSolventOH = mysolventOH.GetComponentInChildren<ParticleSystem>();
		psSolventH2O = mysolventH2O.GetComponentInChildren<ParticleSystem>();


		tags = new string[] { "monomer", "dimer", "ring" };

		bounds = gameObject.GetComponent<Collider>().bounds;
		var b = bounds.extents;
		for (int i = 0; i < numMonomers; i++)
		{
			var monomer = Instantiate(monomerPrefab, transform);
			var pos = transform.position + new Vector3(Random.Range(-b.x, b.x), Random.Range(-b.y, b.y), Random.Range(-b.z, b.z));
			monomer.transform.position = pos;
			monomer.transform.rotation = Random.rotation;
			monomer.name = "monomer" + i;
		}
		InvokeRepeating("FindPairs", 0, pairingInterval);
		InvokeRepeating("DetectAntiparallel", 0, ringAntiparallelCheckInterval);
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
		float torque = torqueDiffuse;
		if (go.tag == "ring")
		{
			//make rings tumble more
			torque = 0.001f;
		}

		if (!bounds.Contains(go.transform.position)) // duplicate code needs tidying
		{
			// Wayward go monomer/dimer/ring
			go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(bounds.center - go.transform.position) * Time.deltaTime * Random.Range(forceTankMin, forceTankMax), ForceMode.Impulse);
		}
		else
		{
			go.GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * Time.deltaTime * Random.Range(forceDiffuseMin, forceDiffuseMax), ForceMode.Impulse);
		}

		go.GetComponent<Rigidbody>().AddRelativeTorque(torque * Random.onUnitSphere, ForceMode.Impulse);
	}

	void assignProbability()
	{
		Color col;
		Color particleCol;
		col.a = fishtankAlpha;
		col = Color.cyan;

		switch (phSlider.GetPhValue())
		{

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
		gameObject.GetComponent<Renderer>().material.color = col;

		{
			// visualise pH in solvent particle systems
			ParticleSystem.MainModule psHMain = psSolventH.main;
			ParticleSystem.MainModule psOHMain = psSolventOH.main;
			ParticleSystem.EmissionModule psHEmission = psSolventH.emission;
			ParticleSystem.EmissionModule psOHEmission = psSolventOH.emission;
			ParticleSystem.EmissionModule psH2OEmission = psSolventH2O.emission;

			if (true)
			{ 
				// test A - linking solvent particle (H) colours to pH

				particleCol = col;
				particleCol.a = 1.0f;
				psHMain.startColor = particleCol;
				psHMain.startSize = 0.012f; // slightly larger than prefab to make it more visible

				psHEmission.rateOverTime = ((9 - (phSlider.GetPhValue() - 3))^2) * 50;
				psOHEmission.rateOverTime = 0.0f;
				psH2OEmission.rateOverTime = 100.0f;

			}

			if (false)
			{
				// test B - linking solvent particle (H and OH) numbers to pH

				psH2OEmission.rateOverTime = 100.0f;
				psHEmission.rateOverTime = (6 - (phSlider.GetPhValue() - 3)) * 30;
				psOHEmission.rateOverTime = (phSlider.GetPhValue() - 3) * 30;
			}
		}

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

	void RingRepel()
	{
		foreach (var a in GameObject.FindGameObjectsWithTag("ring"))
		{
			var ring = a.GetComponent<Ring>();
			foreach (var b in GameObject.FindGameObjectsWithTag("ring"))
			{
				var dist = Vector3.Distance(a.transform.position, b.transform.position);
				var angleDiff = Quaternion.Angle(a.transform.rotation, b.transform.rotation);
				if (a != b && ring.partnerDonor != b && ring.partnerAcceptor != b && dist < ringRepelDistance && angleDiff > ringAngleDiff)
				{
					var oppositeForce = b.transform.position - a.transform.position;
					b.GetComponent<Rigidbody>().AddForce(oppositeForce);
					Debug.DrawLine(a.transform.position, a.transform.position + oppositeForce, Color.green, 2f);
				}
			}
		}
	}

	void UpdateTimer()
	{
		var monomers = GameObject.FindGameObjectsWithTag("monomer");
		var dimers = GameObject.FindGameObjectsWithTag("dimer");
		var rings = GameObject.FindGameObjectsWithTag("ring");
		monomerCount.text = monomers.Length.ToString();
		dimerCount.text = dimers.Length.ToString();
		ringCount.text = rings.Length.ToString();
		ringCount2.text = ringCount.text;
		int stackLength = 0;

		stacks = 0;
		stackLongest = 0;

		if (rings.Length > 0) // we have rings
		{
			foreach (var ring in rings)
			{
				var r = ring.GetComponent<Ring>();

				//find donor end of a stack
				if (r.dockedToAcceptor && !r.dockedToDonor)
				{
					stackLength = 2;
					stacks++;
					//docks++;
					var next = r.GetComponent<Ring>().partnerAcceptor;
					var nextR = next.GetComponent<Ring>();
					while (nextR.dockedToAcceptor)
					{
						next = nextR.GetComponent<Ring>().partnerAcceptor;
						nextR = next.GetComponent<Ring>();
						stackLength++;					
					}
				}
				if (stackLength > stackLongest)
				{
					stackLongest = stackLength;
				}

			}
			if (stackLongest == numMonomers / 2 / 6)
			{
				hasWon = true;	
			}
		}
		stackLongestTxt.text = stackLongest.ToString();
		stackNumberTxt.text = stacks.ToString();

		if (!hasWon)
		{
			// update timer if we have not yet won
			double timeD = System.Math.Round(Time.timeSinceLevelLoad, 1);
			timer.text = timeD.ToString() + "s";
			timer2.text = timeD.ToString();
		}
		if (hasWon && !confettiDone && partyModeSwitch.partying)
		{
			fishtankAudioSource.Play();
			Vector3 confettiOffset = new Vector3(0f, 2.5f, 0f);
			Instantiate(confettiPS, (gameObject.transform.position + confettiOffset), Quaternion.identity);
			confettiDone = true;
		}


	}

	void DetectAntiparallel()
	{
		float minDist = float.PositiveInfinity;
		GameObject flipper = null;
		foreach (var a in GameObject.FindGameObjectsWithTag("ring"))
		{
			foreach (var b in GameObject.FindGameObjectsWithTag("ring"))
			{
				if (a != b && a.GetComponent<Ring>().partnerAcceptor == null && b.GetComponent<Ring>().partnerAcceptor == null)
				{
					var testPairAlignDot = Vector3.Dot(a.transform.up, b.transform.up);
					var dist = Vector3.Distance(a.transform.position, b.transform.position);
					if (dist < minDist && testPairAlignDot < -relateLimitDot)
					{
						// Both a and b are missing acceptors, they're the closest unpaired acceptors in the fishtank, and they're aligned in antiparallel. Designate b to flip
						minDist = dist;
						Debug.Log(a.name + " and " + b.name + " are both missing acceptors - relate=" + testPairAlignDot + " = suitable for a flip");
						flipper = b;
					}
				}
			}
		}
		if (flipper != null)
		{
			// Flip the designated ring around, along with it's entire stack
			flipper.transform.Rotate(0, 0, 180, Space.Self);
			var donor = flipper.GetComponent<Ring>().partnerDonor;
			while (donor != null)
			{
				donor.transform.transform.Rotate(0, 0, 180, Space.Self);
				donor = donor.GetComponent<Ring>().partnerDonor;
			}
		}
	}

	float GetBestRotationOffsetAngle (Quaternion partnerRotation, GameObject ring)
	{
		Quaternion testRotationQuat;
		float testRotationDiff;
		float bestRotationDiff = 360.0f;
		float bestRotationOffsetAngle = 0.0f;
		for (int i = 0; i < ringRotSymmetry; i++)
		{
			testRotationQuat = partnerRotation * Quaternion.Euler(new Vector3(0, (i * (360.0f / ringRotSymmetry)), 0));
			testRotationDiff = Quaternion.Angle(ring.transform.rotation, testRotationQuat);
			if (testRotationDiff < bestRotationDiff)
			{
				bestRotationDiff = testRotationDiff;
				bestRotationOffsetAngle = (i * (360.0f / ringRotSymmetry));
			}

		}
		//Debug.Log("bestRotationOffsetAngle (donor) is " + bestRotationOffsetAngle);
		return bestRotationOffsetAngle;
	}

	// Update is called once per frame
	void Update()
	{
		PushTogether();
		FixHoverlock();
		//RingRepel();
		ClampRigidBodyDynamics();
		UpdateTimer();
	}
}
