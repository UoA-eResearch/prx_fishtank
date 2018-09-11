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
	public Text scoreboardTimerLabel;
	public Text scoreboardTimerValue;
	public Text scoreboardTimerSecs;
	public Text scoreboardBestTime;
	public Text handheldTimerValue;
	public Text monomerCount;
	public Text dimerCount;
	public Text ringCount;
	public Text ringCount2;
	public Text stackLongestTxt;
	public Text stackNumberTxt;
	public Text pHValueChartStatisticsTxt;
	public Text pHValueChartFishtankLabelTxt;

	private bool hasWon = false;
	private bool confettiDone = false;

	private GameObject myConfettiFern;
	private ParticleSystem.EmissionModule emitFern;
	private GameObject myConfettiDonut;
	private ParticleSystem.EmissionModule emitDonut;
	private GameObject myConfettiHeart;
	private ParticleSystem.EmissionModule emitHeart;
	private bool confettiOn = false;

	private int stackLongest = 0;
	private int stacks = 0;

	private bool partyModeLast = false;
	private bool partyIntro = false;

	private double partyStartTime;
	private double timePartyingD;
	private float timePartyingF = 0f;
	private float thisWinTime = float.PositiveInfinity;
	private float bestWinTime = float.PositiveInfinity;

	public int numMonomers = 180;
	private Dictionary<GameObject, GameObject> pairs;

	private Dictionary<GameObject, GameObject> pairsMyAcceptor;
	private Dictionary<GameObject, GameObject> pairsMyAcceptorPrev;
	private Dictionary<GameObject, GameObject> pairsMyDonor;
	private Dictionary<GameObject, GameObject> pairsMyDonorPrev;

	public AudioSource fishtankAudioSource;
	public AudioClip beepUpSound;
	public AudioClip beepDownSound;
	public AudioClip sfxElectricity01;
	public AudioClip sfxElectricity02;
	public AudioClip sfxCheer;

	public GameObject ringPS;
	public GameObject confettiFern;
	public GameObject confettiDonut;
	public GameObject confettiHeart;
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
	private float fishtankAlpha = 0.0004f;

	// dot product thresholds for ring pairing
	private float alignLimitDot = 0.5f;
	private float relateLimitDot = 0.5f;

	// magic numbers for pushing gos around - entirely empirical !
	private float forceTankMin = 0.1f;                  // impulse force range for keeping go in tank
	private float forceTankMax = 0.5f;

	private float torqueDiffuse = 0.0001f;              // torque for random motion (monomer / dimer) - ring is higher in code
	private float forceDiffuseMin = 1.0f;               // impulse forces for random motion
	private float forceDiffuseMax = 2.0f;

	private float pairingInterval = 0.1f;
	private float ringAntiparallelCheckInterval = 1f;

	private float pairingVelocity = 0.05f;               // translation rate for pairing using positional transform lerp
	private int pairingRotationVelocity = 40;            // rotation rate for pairing using quaternion slerp

	private float minDistApplyRBForces = 0.02f;          // lower distance limit for using forces on RBs to push monomer / dimer go together
	private float minDistApplyRBForcesRing = 0.08f;      // lower distance limit for using forces on RBs to push ring go together

	private float stackForceDistance = 0.02f;            // distance threshold for forcing ring stack - hack to allow some stack manipulation with motion controller

	private float ringRepelDistance = 0.25f;
	private float ringAngleDiff = 45;

	private float pairingForcingVelocity = 25.0f;        // translation rate for pairing using positional transform lerp - maintains forced ring stacking for manipulation
	private int pairingForcingRotationVelocity = 50;     // rotation rate for pairing using quaternion slerp

	private int ringRotSymmetry = 6;                     // number of equivalent docking positions around ring

	public bool cheat = false;
	public bool renderCartoon = false;
	private bool renderCartoonLast = false;
	public bool partyMode = false;

	public bool doNanoParticles = true;

	public AudioSource bgm_serious;
	public AudioSource bgm_party;

	public float fishtankScaleFactor = 1.0f;
	private Vector3 fishtankScaleInit = new Vector3(1f, 1f, 1f);
    private Vector3 handObjectsScaleInit = Vector3.one;
	private Vector3 fishtankPositionInit = new Vector3(0f, 0f, 0f);
	private Vector3 fishtankPositionCurrent = new Vector3(0f, 0f, 0f);

	public float nanowireFxScale = 0.05f;               //particle scale for nanowire electric fx

	public int modeUI = 0;

	public GameObject pHSliderUI;
	public GameObject cartoonRenderUI;
	public GameObject fishtankScaleUI;
	public GameObject partyModeUI;
	public GameObject simulationUI;
	public GameObject nanoUI;
	public GameObject menuHintUI;
	public GameObject teleportHintUI;

	public GameObject signSplash;
	public GameObject chartStatsGO;

	public PartyModeSwitch partyModeSwitch;
	public CartoonModeSwitch cartoonModeSwitch;
	public ScaleSlider scaleModeSlider;
	public SimulationSwitch simulationSwitch;
	public NanoParticleSwitch nanoSwitch;

	public ChartStats chartStats;

	public Hand myHand1;
	public Hand myHand2;

    public float tractorBeamAttractionFactor = 50;
    public float attractionParticleThreshold = 2.0f;

    private bool myHand1TouchPressedLastLastUpdate = false; // debouncing

	public bool ringsUseSpringConstraints = false; // if true, enables use of spring constraints for ring stacking
	//public float ringMinSpringStrength = 0f; 


	void DropObjectIfAttached(GameObject go)
	{
		var lh = Player.instance.leftHand;
		var rh = Player.instance.rightHand;

		if (lh.currentAttachedObject == go)
		{
			lh.DetachObject(lh.currentAttachedObject);
		}
		if (rh.currentAttachedObject == go)
		{
			rh.DetachObject(rh.currentAttachedObject);
		}
	}

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
					FindPairsRing(a, gos);
				}
				else if (tag == "dimer")
				{

					if (Random.Range(1, 100) <= probabilityDimerBreak)
					{

						// breaking this object will destroy it - so detach from hand (if attached)
						DropObjectIfAttached(a);
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
							//pairs[a] = match; Debug.DrawLine(a.transform.position, match.transform.position, Color.green, 0.1f);
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

	void FindPairsRing(GameObject a, GameObject[] gos)
	{
		// a is a Ring
		// gos is an array of all Ring GameOjects
		{
			GameObject bestDonor = null;
			GameObject bestAcceptor = null;
			var bestDonorScore = float.PositiveInfinity;
			var bestAcceptorScore = float.PositiveInfinity;
			if (Random.Range(1, 100) <= probabilityRingBreak)
			{
				// breaking this object will destroy it - so detach from hand (if attached)
				DropObjectIfAttached(a);
				a.GetComponent<Ring>().breakRing(null);
			}
			else if ((Random.Range(1, 100) <= probabilityStackMake) && (a.GetComponent<Ring>().ringCanStack))
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

						if (a.GetComponent<Ring>().partnerDonor == null && b.GetComponent<Ring>().partnerAcceptor == null && b.GetComponent<Ring>().ringCanStack)
						// I don't have a donor, and b isn't already donating so far in this findPairs() call 
						{
							// look in acceptor(a)<-donor(b) direction
							//Debug.Log(a.name + " as ACC is testing " + b.name + "as poss DONOR: align = " + testPairAlignDot + " relate = " + testPairRelateDot);
							var partnerPos = b.transform.Find("tf_stack/acceptorPos").gameObject; // a is accepting so we are looking to b's acceptorPos
							var dist = Vector3.Distance(a.transform.position, partnerPos.transform.position);
							//var angleDiff = Quaternion.Angle(a.transform.rotation, partnerPos.transform.rotation);
							var bHasBetterAcceptor = false;
							var score = float.PositiveInfinity;
							//var score = dist * angleDiff;

							b2a = (a.transform.position - b.transform.position).normalized;

							//testPairAlignDot = Vector3.Dot(a.transform.up, b.transform.up);
							//testPairRelateDot = Vector3.Dot(a.transform.up, b2a);
							testPairAlignDot = Vector3.Dot(a.transform.Find("tf_stack/acceptorPos").up, b.transform.Find("tf_stack/acceptorPos").up);
							testPairRelateDot = Vector3.Dot(a.transform.Find("tf_stack/acceptorPos").up, b2a);


							if ((testPairAlignDot > alignLimitDot) && (testPairRelateDot > relateLimitDot))
							{
								score = dist;

								if (pairsMyAcceptorPrev.ContainsKey(b))
								{
									// b was a donor last time and c was the acceptor (i.e. a is now competing with c)

									var c = pairsMyAcceptorPrev[b];
									if (c)
									{
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

						if (a.GetComponent<Ring>().partnerAcceptor == null && b.GetComponent<Ring>().partnerDonor == null && b.GetComponent<Ring>().ringCanStack)
						// I'm not yet a donor, and b has an acceptor slot I could fill - so far in this findPairs() call 
						{
							// look in donor(a)->acceptor(b) direction
							var myPartnerPos = a.transform.Find("tf_stack/acceptorPos").gameObject;
							var dist = Vector3.Distance(b.transform.position, myPartnerPos.transform.position);
							//var angleDiff = Quaternion.Angle(b.transform.rotation, myPartnerPos.transform.rotation);
							var bHasBetterDonor = false;

							var score = float.PositiveInfinity;
							//var score = dist * angleDiff;

							b2a = (a.transform.position - b.transform.position).normalized;
							//testPairAlignDot = Vector3.Dot(a.transform.up, b.transform.up);
							//testPairRelateDot = Vector3.Dot(a.transform.up, b2a);
							testPairAlignDot = Vector3.Dot(a.transform.Find("tf_stack/acceptorPos").up, b.transform.Find("tf_stack/acceptorPos").up);
							testPairRelateDot = Vector3.Dot(a.transform.Find("tf_stack/acceptorPos").up, b2a);

							if ((testPairAlignDot > alignLimitDot) && (testPairRelateDot < -1.0f * relateLimitDot))
							{
								score = dist;

								if (pairsMyDonorPrev.ContainsKey(b))
								{
									// b was an acceptor last time and c was the donor (i.e. a is now competing with c)
									var c = pairsMyDonorPrev[b];
									if (c)
									{
										var cPartnerPos = c.transform.Find("tf_stack/acceptorPos").gameObject;
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
					var partnerPos = bestDonor.transform.Find("tf_stack/acceptorPos").position; // I want to go to my donor's acceptorPos

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
					var partnerPos = bestAcceptor.transform.Find("tf_stack/donorPos").position; // I want to go to my acceptor's donorPos

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
	}

	void ClearRingDockedFlags()
	{
		var ringGos = GameObject.FindGameObjectsWithTag("ring");
		foreach (var ringGo in ringGos)
		{
			Ring ring = ringGo.GetComponent<Ring>();
			ring.dockedToDonor = false;
			ring.dockedToAcceptor = false;
		}
	}

	void SetRingDockedFlags()
	{
		var ringGos = GameObject.FindGameObjectsWithTag("ring");
		foreach (var ringGo in ringGos)
		{
			Ring ring = ringGo.GetComponent<Ring>();
			if (ring.partnerAcceptor)
			{
				var acceptor = ring.partnerAcceptor.transform.Find("tf_stack/donorPos");
				var acceptorPos = acceptor.position;
				var targetRotation = acceptor.rotation;
				var distanceFromAcceptorPos = Vector3.Distance(ringGo.transform.position, acceptorPos);
				if (distanceFromAcceptorPos < stackForceDistance)
				{
					// update docked flags - used for nanowires
					ring.dockedToAcceptor = true;
				}
			}
			if (ring.partnerDonor)
			{
				var donor = ring.partnerDonor.transform.Find("tf_stack/acceptorPos");
				var donorPos = donor.position;
				var targetRotation = donor.rotation;
				var distanceFromDonorPos = Vector3.Distance(ringGo.transform.position, donorPos);
				if (distanceFromDonorPos < stackForceDistance)
				{
					// update docked flags - used for nanowires
					ring.dockedToDonor = true;
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
				// check for gameobjects which are attached to player - we don't want to manipulate them
				var thisGoAttached = lh && lh.currentAttachedObject == go || rh && rh.currentAttachedObject == go;
				if (thisGoAttached || !go)
				{
					if (ringsUseSpringConstraints == true && tag == "ring")
					{
						// this doesn't apply for rings if using spring constraints
						// skipping the push update would be harmful as the spring constraint
						// scaling will get out of sync with the distances and cause physics issues
					}
					else
					{
                        // directly manipulating transforms for these objects will look strange
                        // so we skip the push together
                        continue;

					}

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
					Ring ring = go.GetComponent<Ring>();

					if ((ring.partnerAcceptor == null && ring.partnerDonor == null))
					{
						//I am an unpaired ring (no donor or acceptor) => should drift
						AddRandomMotion(go);
						continue;
					}
					{
						// intercept stacking behaviour here for nanowire formation
						if (!ring.ringCanStack)
						{
							// I can't stack
							AddRandomMotion(go);
							continue;
						}
						if (ring.partnerAcceptor)
						{
							Ring ringPartnerAcceptor = ring.partnerAcceptor.GetComponent<Ring>();
							if (!ringPartnerAcceptor.ringCanStack)
							{
								//Have an acceptor - but it can't stack
								if (ring.partnerDonor)
								{
									Ring ringPartnerDonor = ring.partnerDonor.GetComponent<Ring>();
									if (!ringPartnerDonor.ringCanStack)
									{
										//Have a donor but it can't stack
										AddRandomMotion(go);
										continue;
									}
								}
							}
						}
					}

				}


				//var partner = pairs[go];
				GameObject partner;
				if (pairs.TryGetValue(go, out partner) && partner)
				{
					//partner = pairs[go];
					var targetPos = partner.transform.position;
					var targetRotation = partner.transform.rotation;
                    Transform partnerPos = null;

					if (tag == "monomer")
					{
						partnerPos = partner.transform.Find("partnerPos");
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

						SetCartoonRendering(dimer);

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
								var dimer2RingTransform = go.transform.Find("tf_dimer2ring");
								//var ring = Instantiate(ringPrefab, go.transform.position, go.transform.rotation, transform);
								var ring = Instantiate(ringPrefab, dimer2RingTransform.position, go.transform.rotation, transform);
								SetCartoonRendering(ring);

								if (partyMode) //(partyModeSwitch.partying)
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
							AddRandomMotion(go);
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

					// do the actual pushing together
					if (tag == "monomer")
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
					if (tag == "dimer")
					{
						if (distanceFromTarget > minDistApplyRBForces) // use rigidbody forces to push paired game objects together
						{
							float maxPush = Mathf.Min(distanceFromTarget * 5.0f, 0.5f); //
							go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize((targetPos - go.transform.position) + (Random.onUnitSphere * 0.01f)) * Time.deltaTime * Random.Range(0.0f, maxPush), ForceMode.Impulse);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 1.0f) * pairingRotationVelocity);
						}
						else
						{
							//Debug.Log(dimer.name);
							go.transform.position = Vector3.MoveTowards(go.transform.position, targetPos, Time.deltaTime * 3.0f * pairingVelocity);
							go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(1.0f, 2.0f) * pairingRotationVelocity);
						}
					}
				}
				if (tag == "ring")
				{
					if (ringsUseSpringConstraints)
					{
						PushRingsWithSprings(go);
					}
					else
					{
						PushRingsDirectly(go);
					}
				}

				if (!bounds.Contains(go.transform.position) && tag != "dimer")
				{
					go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(bounds.center - go.transform.position) * Time.deltaTime * Random.Range(forceTankMin, forceTankMax), ForceMode.Impulse);
				}
			}
		}
	}

    void UpdateAttractionHaptics()
    {
        foreach (var hand in Player.instance.hands)
        {
            GameObject go;
            if (hand.currentAttachedObject != null)
            {
                go = hand.currentAttachedObject;
                if (go.tag == "monomer")
                {
                    // check for partner, if present, vibrate based on distance + acidity.
                    GameObject partner;
                    if (pairs.TryGetValue(go, out partner) && partner)
                    {
                        Transform goBondPos = go.transform.Find("partnerPos");
                        Transform partnerBondPos = partner.transform;
                        float pulseStrength = 50;
                        float distanceFactor = Vector3.Distance(goBondPos.position, partnerBondPos.position);
                        // clamp distance to .1 (i.e trigger mult will be 10 at most.
                        if (distanceFactor < .1)
                        {
                            distanceFactor = .1f;
                        }
                        // apply distance and pH factors
                        pulseStrength /= distanceFactor;
                        pulseStrength *= 1 + ((9 - phValue) * 0.1f);
                        Quaternion desiredAngle = Quaternion.LookRotation(go.transform.position, partnerBondPos.position);
                        float angleDiff = Quaternion.Angle(go.transform.rotation, desiredAngle);
                        if (angleDiff > 90)
                        {
                            float amplitude = pulseStrength * 1.0f;
                            float freq = 30;
                            float offset = pulseStrength;
                            pulseStrength = (amplitude * Mathf.Sin(Time.time * freq)) + offset;
                        }
                        hand.controller.TriggerHapticPulse(System.Convert.ToUInt16(pulseStrength));
                    }
                }
            }
        }
    }

    void UpdateMonomerAttractionParticle()
    {
       foreach (var go in GameObject.FindGameObjectsWithTag("monomer"))
        {
            Monomer goMonomer = go.GetComponent<Monomer>();
            GameObject partner;
            if (pairs.TryGetValue(go, out partner) && partner)
            {
                Monomer partnerMonomer = partner.GetComponent<Monomer>();
                // play attraction particle when within threshold proximity.
                Transform partnerPos = partner.transform.Find("partnerPos");
                var distanceFromTarget = Vector3.Distance(go.transform.position, partner.transform.position);
                if (distanceFromTarget < attractionParticleThreshold)
                {
                    // need to increase maximum particles if threshold increases too much
                    goMonomer.ActivateAttractionParticle(partnerPos, distanceFromTarget);
                    partnerMonomer.ActivateAttractionParticle(go.transform, distanceFromTarget);
                }
                else
                {
                    goMonomer.DeactivateAttractionParticle();
                    partnerMonomer.DeactivateAttractionParticle();
                }
            } else
            {
                goMonomer.DeactivateAttractionParticle();
            }
        }
    }

	void PushRingsWithSprings(GameObject go)
	{
		// investigating using spring constraints to move rings
		{

			float bestRotationOffsetAngle = 0.0f;
			var ring = go.GetComponent<Ring>();

			//ring.dockedToDonor = false;
			//ring.dockedToAcceptor = false;


			if (ring.partnerAcceptor != null)
			{
				{
					var acceptor = ring.partnerAcceptor.transform.Find("tf_stack/donorPos");
					var acceptorPos = acceptor.position;
					var targetRotation = acceptor.rotation;
					var distanceFromAcceptorPos = Vector3.Distance(go.transform.position, acceptorPos);

					var acceptorRing = ring.partnerAcceptor;

					bestRotationOffsetAngle = GetBestRotationOffsetAngle(acceptor.rotation, go);
					//Debug.Log("bestRotationOffsetAngle (acceptor) is " + bestRotationOffsetAngle);

					//SetAcceptorConstraints(ring, bestRotationOffsetAngle - ringStackRotation);
					ring.RingSetDonorToAcceptorConstraints(bestRotationOffsetAngle - ring.ringStackingAxialRotation);

					//targetRotation = acceptor.rotation * Quaternion.Euler(new Vector3(0, bestRotationOffsetAngle, 0));
					//go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 0.5f) * pairingRotationVelocity);

					if (distanceFromAcceptorPos < stackForceDistance)
					{
						// update docked flags - used for nanowires
						ring.dockedToAcceptor = true;
					}

					if (cheat)
					{
						go.transform.position = acceptorPos;
						go.transform.rotation = targetRotation;
					}
				}

			}
			else
			{
				// no acceptor - switch off corresponding spring constraints
				ring.RingSwitchOffDonorToAcceptorConstraints();
			}

			if (ring.partnerDonor != null)
			{
				var donor = ring.partnerDonor.transform.Find("tf_stack/acceptorPos");
				var donorPos = donor.position;
				var targetRotation = donor.rotation;
				var distanceFromDonorPos = Vector3.Distance(go.transform.position, donorPos);

				var donorRing = ring.partnerDonor;

				bestRotationOffsetAngle = GetBestRotationOffsetAngle(donor.rotation, go);
				//Debug.Log("bestRotationOffsetAngle (donor) is " + bestRotationOffsetAngle);

				//SetDonorConstraints(ring, bestRotationOffsetAngle + ringStackRotation);
				ring.RingSetAcceptorToDonorConstraints(bestRotationOffsetAngle + ring.ringStackingAxialRotation);

				//targetRotation = donor.rotation * Quaternion.Euler(new Vector3(0, bestRotationOffsetAngle, 0));
				//go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * Random.Range(0.1f, 0.5f) * pairingRotationVelocity);

				//if (distanceFromDonorPos < stackForceDistance)
				//{
				//	// update docked flags - used for nanowires
				//	ring.dockedToDonor = true;
				//}

				if (cheat)
				{
					go.transform.position = donorPos;
					go.transform.rotation = targetRotation;
				}
			}
			else
			{
				// no donor - switch off corresponding spring constraint
				ring.RingSwitchOffAcceptorToDonorConstraints();

			}

		}
	}

	void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
	{
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(color, color);
		lr.SetWidth(0.02f, 0.02f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		GameObject.Destroy(myLine, duration);
	}

	void PushRingsDirectly(GameObject go)
	{
		// original implementation of fishtank - uses RB forces and transform lerps to move rings
		{
			
			float bestRotationOffsetAngle = 0.0f;
			var ring = go.GetComponent<Ring>();
			//ring.dockedToDonor = false;
			//ring.dockedToAcceptor = false;
			var rb = ring.GetComponent<Rigidbody>();
			{
				// set approriate drag values
				rb.drag = 1;
				rb.angularDrag = 1;

				// switch all spring constraints off
				ring.RingSwitchOffDonorToAcceptorConstraints();
				ring.RingSwitchOffAcceptorToDonorConstraints();
			}

			if (ring.partnerAcceptor != null)
			{		
				var acceptor = ring.partnerAcceptor.transform.Find("tf_stack/donorPos");
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
					// update docked flags - used for nanowires
					ring.dockedToAcceptor = true;

					// HACK: commented out because there has to be some priority for moving rings based on acceptor / donor pairings
					// unfortunately this causes asymmetry in how ring stacks move
					//go.transform.position = Vector3.MoveTowards(go.transform.position, acceptorPos, Time.deltaTime * pairingForcingVelocity);
					go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * pairingForcingRotationVelocity);

				}

				if (cheat)
				{
					go.transform.position = acceptorPos;
					go.transform.rotation = targetRotation;
				}
				
			}
			if (ring.partnerDonor != null)
			{
				var donor = ring.partnerDonor.transform.Find("tf_stack/acceptorPos");
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
					//// update docked flags - used for nanowires
					//ring.dockedToDonor = true;

					go.transform.position = Vector3.MoveTowards(go.transform.position, donorPos, Time.deltaTime * pairingForcingVelocity);
					go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, targetRotation, Time.deltaTime * pairingForcingRotationVelocity);

				}

				if (cheat)
				{
					go.transform.position = donorPos;
					go.transform.rotation = targetRotation;
				}
			}

		}
	}

	// Use this for initialization
	void Start()
	{

		fishtankScaleInit = gameObject.transform.localScale;
		fishtankPositionInit = gameObject.transform.position;
		fishtankPositionCurrent = gameObject.transform.position;

		SetMenuUIComponents(modeUI);

		phSlider = gameObject.GetComponent<PHSlider>();

		// initialise solvent particle systems
		var mysolventH = Instantiate(solventH, gameObject.transform); // gameObject.transform.position, Quaternion.identity);
		var mysolventOH = Instantiate(solventOH, gameObject.transform); //gameObject.transform.position, Quaternion.identity);
		var mysolventH2O = Instantiate(solventH2O, gameObject.transform); //gameObject.transform.position, Quaternion.identity);

		psSolventH = mysolventH.GetComponentInChildren<ParticleSystem>();
		psSolventOH = mysolventOH.GetComponentInChildren<ParticleSystem>();
		psSolventH2O = mysolventH2O.GetComponentInChildren<ParticleSystem>();

		ParticleSystem.MainModule psHMain = psSolventH.main;
		ParticleSystem.ShapeModule psHShape = psSolventH.shape;
		ParticleSystem.ShapeModule psH2OShape = psSolventH2O.shape;

		psHShape.scale = gameObject.transform.localScale;
		psH2OShape.scale = gameObject.transform.localScale;

		// spawn monomers
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
			SetCartoonRendering(monomer);


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

			if (!ringsUseSpringConstraints)
			{
				//RB spring constraint damper interferes with rb movement
				//repeatedly switching the damper value may also have an effect on the rb simulation - hence bool checks
				var ring = go.GetComponent<Ring>();
				if (ring.sjDonorToAcceptorOn == true)
				{
					ring.RingSwitchOffDonorToAcceptorConstraints();
				}
				if (ring.sjAcceptorToDonorOn == true)
				{
					ring.RingSwitchOffAcceptorToDonorConstraints();
				}
			}

		}

		if (!bounds.Contains(go.transform.position)) // duplicate code needs tidying
		{
			// Wayward go monomer/dimer/ring
			go.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(bounds.center - go.transform.position) * Time.deltaTime * Random.Range(forceTankMin, forceTankMax), ForceMode.Impulse);
		}
		else
		{
			if (masterDimers.Contains(go))
			{
				// tumble master dimers less (scaled down)
				go.GetComponent<Rigidbody>().AddRelativeTorque(torque * 0.1f * Random.onUnitSphere, ForceMode.Impulse);
				go.GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * 0.1f * Time.deltaTime * Random.Range(forceDiffuseMin, forceDiffuseMax), ForceMode.Impulse);
			}
			else
			{
				go.GetComponent<Rigidbody>().AddRelativeTorque(torque * Random.onUnitSphere, ForceMode.Impulse);
				go.GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * Time.deltaTime * Random.Range(forceDiffuseMin, forceDiffuseMax), ForceMode.Impulse);
			}
		}


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
				probabilityDimerMake = 5; //1
				probabilityDimerBreak = 50;
				probabilityRingMake = 0;
				probabilityRingBreak = 100;
				probabilityStackMake = 0;
				col = Color.cyan;
				break;

			case 8:
				probabilityDimerMake = 20; //10
				probabilityDimerBreak = 1;
				probabilityRingMake = 0;
				probabilityRingBreak = 100;
				probabilityStackMake = 0;
				col = Color.blue;
				break;

			case 7:
				probabilityDimerMake = 50; //100
				probabilityDimerBreak = 5; //0
				probabilityRingMake = 10;
				probabilityRingBreak = 2; //5
				probabilityStackMake = 2; //1
				col = Color.gray;
				break;

			case 6:
				probabilityDimerMake = 100;
				probabilityDimerBreak = 2; //0
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
				probabilityStackMake = 70; //50
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

		pHValueChartStatisticsTxt.text = phSlider.GetPhValueStr();
		pHValueChartFishtankLabelTxt.text = phSlider.GetPhValueStr();

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

				psHEmission.rateOverTime = ((9 - (phSlider.GetPhValue() - 3)) ^ 2) * 50;
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
				go.GetComponent<Rigidbody>().velocity = Vector3.ClampMagnitude(go.GetComponent<Rigidbody>().velocity, 1.0f * 2.0f);
				go.GetComponent<Rigidbody>().angularVelocity = Vector3.ClampMagnitude(go.GetComponent<Rigidbody>().angularVelocity, 1.0f * 6.0f);
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

	void UpdateStatistics()
	{
		var monomers = GameObject.FindGameObjectsWithTag("monomer");
		var dimers = GameObject.FindGameObjectsWithTag("dimer");
		var rings = GameObject.FindGameObjectsWithTag("ring");
		monomerCount.text = monomers.Length.ToString();
		dimerCount.text = dimers.Length.ToString();
		ringCount.text = rings.Length.ToString();
		ringCount2.text = ringCount.text;

		int stackLength = 0;
		int numRingsInAllStacks = 0;

		stacks = 0;
		stackLongest = 0;

		// force instant win - for testing
		//if (partyMode && !partyIntro)
		//{
		//	hasWon = true;
		//}


		if (rings.Length > 0) // we have rings
		{
			foreach (var ring in rings)
			{
				var r = ring.GetComponent<Ring>();

				UpdateNanoWireFx(r);

				//find donor end of a stack
				if (r.dockedToAcceptor && !r.dockedToDonor)
				{
					var next = r.GetComponent<Ring>().partnerAcceptor;
					stackLength = 2;
					stacks++;
					//docks++;

					if (next)
					{
						var nextR = next.GetComponent<Ring>();
						while (nextR.dockedToAcceptor)
						{
							next = nextR.GetComponent<Ring>().partnerAcceptor;
							if (next)
							{
								nextR = next.GetComponent<Ring>();
								stackLength++;
							}
							else
							{
								break;
							}
						}
						numRingsInAllStacks += stackLength;
					}
				}
				if (stackLength > stackLongest)
				{
					stackLongest = stackLength;
				}
			}
			if (stackLongest == numMonomers / 2 / 6)
			{
				if (partyMode && !partyIntro)
				{
					hasWon = true;
				}
			}
		}
		stackLongestTxt.text = stackLongest.ToString();
		stackNumberTxt.text = stacks.ToString();

		// do stats
		float monomerFraction = (float)monomers.Length / numMonomers;
		float dimerFraction = (dimers.Length * 2.0f) / numMonomers;
		float ringFraction = (rings.Length * 12.0f) / numMonomers;
		float stackFraction = (numRingsInAllStacks * 12.0f) / numMonomers;
		chartStats.SetStats(monomerFraction, dimerFraction, ringFraction, stackFraction);

		//if (hasWon && !confettiDone && partyMode) 
		//{
		//	fishtankAudioSource.Play();
		//	Vector3 confettiOffset = new Vector3(0f, 2.5f, 0f);
		//	Instantiate(confettiPS, (gameObject.transform.position + confettiOffset), Quaternion.identity);
		//	confettiDone = true;
		//}


	}


	void UpdateNanoWireFx(Ring r)
	{

		// nanowire electric particles
		// turns on fx for rings in stacks of 3 or more
		// this method avoids keeping arrays or lists for stacks ;)

		if (doNanoParticles)
		{
			if (!r.dockedToAcceptor && !r.dockedToDonor)
			{
				//free ring
				NanoWireOff(r);
			}
			else if (r.dockedToAcceptor && r.dockedToDonor)
			{
				// two neighbours docked either side
				NanoWireOn(r);
			}


			if (r.dockedToAcceptor && !r.dockedToDonor)
			{
				if (r.partnerAcceptor != null)
				{
					var myAcceptorRing = r.partnerAcceptor.GetComponent<Ring>();
					if (myAcceptorRing.dockedToAcceptor)
					{
						//(at least) two neighbours docked on Acceptor side
						NanoWireOn(r);
					}
					else
					{
						NanoWireOff(r);
					}
				}
				else
				{
					NanoWireOff(r);
				}

			}

			if (!r.dockedToAcceptor && r.dockedToDonor)
			{
				if (r.partnerDonor != null)
				{
					var myDonorRing = r.partnerDonor.GetComponent<Ring>();
					if (myDonorRing.dockedToDonor)
					{
						//(at least) two neighbours docked on Donor side
						NanoWireOn(r);
					}
					else
					{
						NanoWireOff(r);
					}
				}
				else
				{
					NanoWireOff(r);
				}


			}
		}
		else
		{
			NanoWireOff(r);
		}




	}

	void NanoWireOn(Ring r)
	{
		if (!r.psElectric01.isPlaying)
		{
			r.psElectric01.Play();
			r.SetShaderTrans();

			//

			if (Random.value < 0.5f)
			{
				r.ringAudioSource.clip = sfxElectricity01;

			}
			else
			{
				r.ringAudioSource.clip = sfxElectricity02;
			}
			r.ringAudioSource.volume = 0.05f;
			r.ringAudioSource.loop = true;
			r.ringAudioSource.Play();			
		}

	}

	void NanoWireOff(Ring r)
	{
		if (r.psElectric01.isPlaying)
		{
			r.psElectric01.Stop();
			r.SetShaderVertexCol();

			r.ringAudioSource.Stop();
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
				Ring aRing = a.GetComponent<Ring>();
				Ring bRing = b.GetComponent<Ring>();

				if ((a != b && aRing.partnerAcceptor == null && bRing.partnerAcceptor == null) && (aRing.ringCanStack && bRing.ringCanStack))
				{
					//var testPairAlignDot = Vector3.Dot(a.transform.up, b.transform.up);
					var testPairAlignDot = Vector3.Dot(a.transform.Find("tf_stack/acceptorPos").up, b.transform.Find("tf_stack/acceptorPos").up);
					var dist = Vector3.Distance(a.transform.position, b.transform.position);
					if (dist < minDist && testPairAlignDot < -relateLimitDot)
					{
						// Both a and b are missing acceptors, they're the closest unpaired acceptors in the fishtank, and they're aligned in antiparallel. Designate b to flip
						minDist = dist;
						//Debug.Log(a.name + " and " + b.name + " are both missing acceptors - relate=" + testPairAlignDot + " = suitable for a flip");
						flipper = b;
					}
				}
			}
		}
		if (flipper != null) 
		{
			// Flip the designated ring around, along with it's entire stack
			flipper.transform.Rotate(0, 0, 180, Space.Self);
			//SwitchAcceptorDonorPositions(flipper);

			var donor = flipper.GetComponent<Ring>().partnerDonor;
			while (donor != null)
			{
				donor.transform.transform.Rotate(0, 0, 180, Space.Self);
				//SwitchAcceptorDonorPositions(donor);
				donor = donor.GetComponent<Ring>().partnerDonor;
			}
		}
	}

	void SwitchAcceptorDonorPositions(GameObject ring)
	{
		Vector3 myPosition;
		Vector3 myScale;
		float myRotationY;
		float myRotationZ;

#if (true)
		{
			// 180 z flip the stackingTransform parent of the acceptorPos and donorPos
			myRotationY = ring.transform.Find("tf_stack").localEulerAngles.y;
			myRotationZ = ring.transform.Find("tf_stack").localEulerAngles.z;
			if (myRotationZ == 0)
			{
				myRotationZ = 180;
			}
			else if (myRotationZ == 180)
			{
				myRotationZ = 0;
			}
			ring.transform.Find("tf_stack").localRotation = Quaternion.Euler(0, myRotationY, myRotationZ);
		}
#endif

#if (false)
		{
			// directly manipulate acceptorPos and donorPos
			myPosition = ring.transform.Find("tf_stack/donorPos").localPosition;
			myPosition.y = myPosition.y * -1f;
			ring.transform.Find("tf_stack/donorPos").localPosition = myPosition;

			myRotationY = ring.transform.Find("tf_stack/donorPos").localEulerAngles.y;
			myRotationY = myRotationY * -1f;
			myRotationZ = ring.transform.Find("tf_stack/donorPos").localEulerAngles.z;
			if (myRotationZ == 0)
			{
				myRotationZ = 180;
			}
			else if (myRotationZ == 180)
			{
				myRotationZ = 0;
			}
			ring.transform.Find("tf_stack/donorPos").localRotation = Quaternion.Euler(0, myRotationY, 0);


			myPosition = ring.transform.Find("tf_stack/acceptorPos").localPosition;
			myPosition.y = myPosition.y * -1f;
			ring.transform.Find("tf_stack/acceptorPos").localPosition = myPosition;

			myRotationY = ring.transform.Find("tf_stack/acceptorPos").localEulerAngles.y;
			myRotationY = myRotationY * -1f;
			myRotationZ = ring.transform.Find("tf_stack/acceptorPos").localEulerAngles.z;
			if (myRotationZ == 0)
			{
				myRotationZ = 180;
			}
			else if (myRotationZ == 180)
			{
				myRotationZ = 0;
			}
			ring.transform.Find("tf_stack/acceptorPos").localRotation = Quaternion.Euler(0, myRotationY, 0);
		}
#endif
	}

	float GetBestRotationOffsetAngle(Quaternion partnerRotation, GameObject ring)
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

	void UpdateCartoon()
	{
		renderCartoon = cartoonModeSwitch.GetRenderCartoon();

		if (renderCartoon != renderCartoonLast)
		{
			foreach (var a in GameObject.FindGameObjectsWithTag("monomer"))
			{
				SetCartoonRendering(a);
			}
			foreach (var a in GameObject.FindGameObjectsWithTag("dimer"))
			{
				SetCartoonRendering(a);
			}
			foreach (var a in GameObject.FindGameObjectsWithTag("ring"))
			{
				SetCartoonRendering(a);
			}
			renderCartoonLast = renderCartoon;
		}
	}

	public void SetCartoonRendering(GameObject go)
	{
		if (go.tag == "monomer")
		{
			var monomer = go;
			monomer.GetComponent<MeshRenderer>().enabled = !renderCartoon;
			monomer.transform.Find("mesh_cartoon").gameObject.SetActive(renderCartoon);
		}
		else if (go.tag == "dimer")
		{
			var dimer = go;
			dimer.GetComponent<MeshRenderer>().enabled = !renderCartoon;
			dimer.transform.Find("mesh_cartoon").gameObject.SetActive(renderCartoon);

			dimer.transform.Find("dimerLight").gameObject.SetActive(!renderCartoon);
		}
		else if (go.tag == "ring")
		{
			var ring = go;
			ring.transform.Find("Ring_MeshPart0").gameObject.SetActive(!renderCartoon);
			ring.transform.Find("Ring_MeshPart1").gameObject.SetActive(!renderCartoon);
			ring.transform.Find("mesh_cartoon").gameObject.SetActive(renderCartoon);

			ring.transform.Find("ringLight").gameObject.SetActive(!renderCartoon);
		}
	}


	void UpdatePartyMode()
	{

		partyMode = partyModeSwitch.GetPartyMode();

		SetBGM();

		if (partyModeLast == false && partyMode == true)
		{
			//Party just started
			partyStartTime = Time.timeSinceLevelLoad + 3; //extra time for intro
			phSlider.ResetPhValue();
			hasWon = false;
			confettiDone = false;
			partyIntro = true;
		}

		if (partyModeLast == true && partyMode == false)
		{
			//Party just stopped
			ConfettiOff();
		}

		if (Time.timeSinceLevelLoad < partyStartTime)
		{
			//Party about to start
			scoreboardTimerLabel.text = "Get Ready!";
			scoreboardTimerValue.text = "";
			scoreboardTimerSecs.text = "";
			handheldTimerValue.text = "Get Ready!";
		}
		else
		{
			//Party intro over
			partyIntro = false;
		}

		if (partyMode && !partyIntro && !hasWon)
		{
			//Party in progress
			timePartyingD = (Time.timeSinceLevelLoad - partyStartTime);
			double timePartyingRounded = System.Math.Round(timePartyingD, 1);
			timePartyingF = (float)timePartyingRounded;
			scoreboardTimerLabel.text = "";
			scoreboardTimerValue.text = timePartyingRounded.ToString();
			scoreboardTimerSecs.text = "secs";
			handheldTimerValue.text = timePartyingRounded.ToString() + "s";
		}

		if (partyMode && !partyIntro && hasWon && !confettiDone)
		{
			//Win
			thisWinTime = timePartyingF;
			if (thisWinTime < bestWinTime)
			{
				bestWinTime = thisWinTime;
			}
			ConfettiOn();
		}

		if (bestWinTime < float.PositiveInfinity)
		{
			scoreboardBestTime.text = bestWinTime.ToString();
		}
		else
		{
			scoreboardBestTime.text = "???";
		}

		partyModeLast = partyMode;
	}

	void SetBGM()
	{
		float crossfadeLerp = 0.1f;

		if (partyMode == true)
		{
			bgm_party.volume = Mathf.Lerp(bgm_party.volume, 0.05f, crossfadeLerp);
			bgm_serious.volume = Mathf.Lerp(bgm_serious.volume, 0f, crossfadeLerp);
		}
		else
		{
			bgm_party.volume = Mathf.Lerp(bgm_party.volume, 0f, crossfadeLerp);
			bgm_serious.volume = Mathf.Lerp(bgm_serious.volume, 0.06f, crossfadeLerp);
		}
	}

	void ConfettiOn()
	{
		if (confettiOn == false)
		{
			//fishtankAudioSource.Play();
			fishtankAudioSource.PlayOneShot(sfxCheer, 0.8f);
			Vector3 confettiOffset = new Vector3(0f, 2.5f, 0f);

			if (myConfettiFern == null)
			{
				myConfettiFern = Instantiate(confettiFern, (gameObject.transform.position + confettiOffset), Quaternion.Euler(new Vector3(90, 0, 0)));
			}
			if (myConfettiDonut == null)
			{
				myConfettiDonut = Instantiate(confettiDonut, (gameObject.transform.position + confettiOffset), Quaternion.Euler(new Vector3(90, 0, 0)));
			}
			if (myConfettiHeart == null)
			{
				myConfettiHeart = Instantiate(confettiHeart, (gameObject.transform.position + confettiOffset), Quaternion.Euler(new Vector3(90, 0, 0)));
			}

			emitFern = myConfettiFern.GetComponent<ParticleSystem>().emission;
			emitFern.rateOverTime = 100f;
			emitDonut = myConfettiDonut.GetComponent<ParticleSystem>().emission;
			emitDonut.rateOverTime = 100f;
			emitHeart = myConfettiHeart.GetComponent<ParticleSystem>().emission;
			emitHeart.rateOverTime = 20f;

			confettiOn = true;
			confettiDone = true;
			Invoke("ConfettiOff", 6);
		}
	}

	void ConfettiOff()
	{
		if (confettiOn == true)
		{
			emitFern.rateOverTime = 0f;
			emitDonut.rateOverTime = 0f;
			emitHeart.rateOverTime = 0f;
			confettiOn = false;
		}
	}

	void UpdateScale()
	{

		//if (fishtankScaleFactor != cartoonModeSwitch.GetFishtankScale())
		{
			//fishtankScaleFactor = cartoonModeSwitch.GetFishtankScale();
			fishtankScaleFactor = scaleModeSlider.GetFishtankScale();
            //fishtank
			gameObject.transform.localScale = fishtankScaleInit * fishtankScaleFactor;
			fishtankPositionCurrent.y = fishtankPositionInit.y * fishtankScaleFactor;
			gameObject.transform.localPosition = fishtankPositionCurrent;

            // held objects
            List<GameObject> hands = new List<GameObject>();
            hands.Add(myHand1.gameObject);
            hands.Add(myHand2.gameObject);
            foreach (GameObject hand in hands)
            {
                foreach(Transform child in hand.transform)
                {
                    if (System.Array.IndexOf(tags, child.gameObject.tag) > -1)
                    {
                        child.transform.localScale = ( (fishtankScaleInit * monomerPrefab.transform.localScale.x ) * fishtankScaleFactor);
                    }
                }
            }

            //particles
            ParticleSystem.ShapeModule psHShape = psSolventH.shape;
			ParticleSystem.ShapeModule psH2OShape = psSolventH2O.shape;
			psHShape.scale = gameObject.transform.localScale;
			psH2OShape.scale = gameObject.transform.localScale;

			bounds = gameObject.GetComponent<Collider>().bounds;

			//ring electric wire particles should be scaled
			var rings = GameObject.FindGameObjectsWithTag("ring");
			if (rings.Length > 0) // we have rings
			{
				foreach (var ring in rings)
				{
					var r = ring.GetComponent<Ring>();
					r.psElectric01.transform.localScale = nanowireFxScale * gameObject.transform.localScale;

				}
			}

		}


	}

	void UpdateSimulationMode()
	{
		ringsUseSpringConstraints = simulationSwitch.GetSimulationMode();
	}

	void UpdateNanoMode()
	{
		doNanoParticles = nanoSwitch.GetNanoParticleMode();
	}

	void SetMenuUIComponents(int mode)
	{
		switch (mode)
		{
			case 0:
				pHSliderUI.SetActive(true);
				cartoonRenderUI.SetActive(false);
				fishtankScaleUI.SetActive(false);
				partyModeUI.SetActive(false);
				simulationUI.SetActive(false);
				nanoUI.SetActive(false);
				break;
			case 1:
				pHSliderUI.SetActive(false);
				cartoonRenderUI.SetActive(true);
				fishtankScaleUI.SetActive(false);
				partyModeUI.SetActive(false);
				simulationUI.SetActive(false);
				nanoUI.SetActive(false);
				break;
			case 2:
				pHSliderUI.SetActive(false);
				cartoonRenderUI.SetActive(false);
				fishtankScaleUI.SetActive(true);
				partyModeUI.SetActive(false);
				simulationUI.SetActive(false);
				nanoUI.SetActive(false);
				break;
			case 3:
				pHSliderUI.SetActive(false);
				cartoonRenderUI.SetActive(false);
				fishtankScaleUI.SetActive(false);
				partyModeUI.SetActive(true);
				simulationUI.SetActive(false);
				nanoUI.SetActive(false);
				break;
			case 4:
				pHSliderUI.SetActive(false);
				cartoonRenderUI.SetActive(false);
				fishtankScaleUI.SetActive(false);
				partyModeUI.SetActive(false);
				simulationUI.SetActive(true);
				nanoUI.SetActive(false);
				break;
			case 5:
				pHSliderUI.SetActive(false);
				cartoonRenderUI.SetActive(false);
				fishtankScaleUI.SetActive(false);
				partyModeUI.SetActive(false);
				simulationUI.SetActive(false);
				nanoUI.SetActive(true);
				break;
			case 6:
				pHSliderUI.SetActive(false);
				cartoonRenderUI.SetActive(false);
				fishtankScaleUI.SetActive(false);
				partyModeUI.SetActive(false);
				simulationUI.SetActive(false);
				nanoUI.SetActive(false);
				break;
		}
	}

	void SwitchMenuUIMode(int direction)
	{
		var numUIModes = 7;

		if (direction > 0)
		{
			fishtankAudioSource.PlayOneShot(beepUpSound, 0.4f);
			
		}
		else
		{
			fishtankAudioSource.PlayOneShot(beepDownSound, 0.8f);
		}

		if (true) //(Input.GetKeyDown(KeyCode.Z))
		{
			modeUI += direction;
			if (modeUI == numUIModes)
			{
				modeUI = 0;
			}
			if (modeUI == -1)
			{
				modeUI = (numUIModes - 1);
			}
		}
		SetMenuUIComponents(modeUI);
	}

    void ActivateTractorBeam(Hand hand, SteamVR_LaserPointer laser)
    {
        laser.enabled = true;
        if (laser.holder != null)
        {
            laser.holder.SetActive(true);
        }
        if (laser.reference != null)
        {
            GameObject targetObject = laser.reference.gameObject;
            if (System.Array.IndexOf(tags, targetObject.tag) > -1)
            {
				Vector3 tractorBeam = hand.transform.position - targetObject.transform.position;
				float tractorBeamScale = Mathf.Max(4.0f, tractorBeamAttractionFactor * (Vector3.Magnitude(tractorBeam) / 500.0f));
				targetObject.GetComponent<Rigidbody>().AddForce((tractorBeam * tractorBeamScale), ForceMode.Acceleration);
				//targetObject.GetComponent<Rigidbody>().AddForce((hand.transform.position - targetObject.transform.position) * tractorBeamAttractionFactor, ForceMode.Acceleration);
			}
        }
    }

    void DeactivateTractorBeam(Hand hand, SteamVR_LaserPointer laser)
    {
        laser.holder.SetActive(false);
        laser.enabled = false;
    }

	void UpdateViveControllers()
	{
		var leftI = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
		var rightI = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);

		//Debug.Log("leftI = " + leftI + " rightI = " + rightI);

		// using 'left' myHand1 controller pad for UI menu switching
		// note: SteamVR/InteractionSystem/Teleport/Scripts/Teleport.cs altered to use touchpad.y threshold
		if (myHand1.controller != null)
		{
			bool showMenuHint = false;
			Vector2 touchpad = (myHand1.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
			if ((touchpad.x < -0.2) && (touchpad.y < 0.2))
			{
				if (!myHand1TouchPressedLastLastUpdate &&  myHand1.controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
				{
					myHand1TouchPressedLastLastUpdate = true;
					//Debug.Log("Pad Press left!");
					SwitchMenuUIMode(-1);
				}
				showMenuHint = true;

			}
			if ((touchpad.x > 0.2) && (touchpad.y < 0.2))
			{
				if (!myHand1TouchPressedLastLastUpdate && myHand1.controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
				{
					myHand1TouchPressedLastLastUpdate = true;
					//Debug.Log("Pad Press right!");
					SwitchMenuUIMode(1);
				}
				showMenuHint = true;
			}

			if (showMenuHint)
			{
				menuHintUI.SetActive(true);
			}
			else
			{
				menuHintUI.SetActive(false);
			}

			if (touchpad.y > 0.25) // value set in teleport.cs
			{
				teleportHintUI.SetActive(true);
			}
			else
			{
				teleportHintUI.SetActive(false);
			}

			if (!myHand1.controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
			{
				myHand1TouchPressedLastLastUpdate = false;
			}
		}

		// using controller application menu buttons for UI menu switching
		if (myHand1.controller != null)
		{
			if (myHand1.controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
			{
				//Debug.Log("Left Press Down!");
				SwitchMenuUIMode(-1);
			}
		}
		if (myHand2.controller != null)
		{
			if (myHand2.controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
			{
				//Debug.Log("Right Press Down!");
				SwitchMenuUIMode(1);
			}
		}

        // using grip buttons to activate tractor beam
        ulong gripButton = SteamVR_Controller.ButtonMask.Grip;
        if (myHand1.controller != null)
        {
            SteamVR_LaserPointer laserPointer = myHand1.GetComponent<SteamVR_LaserPointer>();
            if (myHand1.controller.GetPress(gripButton))
            {
                ActivateTractorBeam(myHand1, laserPointer);
            } else if (myHand1.controller.GetPressUp(gripButton)){
                DeactivateTractorBeam(myHand1, laserPointer);
            }
        }
        if (myHand2.controller != null)
        {
            SteamVR_LaserPointer laserPointer = myHand2.GetComponent<SteamVR_LaserPointer>();
            if (myHand2.controller.GetPress(gripButton))
            {
                ActivateTractorBeam(myHand2, laserPointer);
            }
            else if (myHand2.controller.GetPressUp(gripButton))
            {
                DeactivateTractorBeam(myHand2, laserPointer);
            }
        }
    }

	private GameObject GetActiveMenu() {
		GameObject[] menus = {
			pHSliderUI, 
			cartoonRenderUI, 
			fishtankScaleUI, 
			partyModeUI, 
			simulationUI, 
			nanoUI 
		};

		int activeCheck = 0;
		GameObject activeMenu = null;
		foreach (GameObject menu in menus){
			if (menu.active) {
				activeMenu = menu;
				activeCheck++;
			}
		}
		if (activeCheck > 1) {
			Debug.Log("Error: More than one menu active");
		}
		return activeMenu;
	}

	void UpdateKeyboardInput() {
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			phSlider.UpdatePhValue(0.1f);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			phSlider.UpdatePhValue(-0.1f);
		}

		// toggle menus with side arrows.
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			SwitchMenuUIMode(-1);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			SwitchMenuUIMode(1);
		}
	}

	void UpdateSigns()
	{
		// hacky fixed timing test for appearances

		if (System.Math.Round(Time.timeSinceLevelLoad, 1) > 6.0f)
		{
			if (signSplash.GetComponent<CanvasGroup>().alpha > 0)
				{
					signSplash.GetComponent<CanvasGroup>().alpha -= 0.004f;
				}
		}

		if (System.Math.Round(Time.timeSinceLevelLoad, 1) > 10.0f)
		{
			if (chartStatsGO.GetComponent<CanvasGroup>().alpha < 1.0f)
			{
				chartStatsGO.GetComponent<CanvasGroup>().alpha += 0.001f;
			}
		}


	}

	// Update is called once per frame
	void Update()
	{
        ClearRingDockedFlags();
		PushTogether();
		SetRingDockedFlags();
		FixHoverlock();
		//RingRepel();
		ClampRigidBodyDynamics();
		UpdateStatistics();
		UpdateCartoon();
		UpdateScale();
		UpdatePartyMode();
		UpdateSimulationMode();
		UpdateNanoMode();
		//UpdateUIMode();
		UpdateViveControllers();
		UpdateKeyboardInput();
		UpdateSigns();
        UpdateAttractionHaptics();
        UpdateMonomerAttractionParticle();
	}
}
