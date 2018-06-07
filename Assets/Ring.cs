
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Ring: MonoBehaviour
{

	public GameObject dimerPrefab;
	public bool shouldBreak = true;
	private VelocityEstimator velEst;
	private bool ringAttached = false;
	private Transform fishtank;

	public GameObject partnerAcceptor;
	public GameObject partnerDonor;
	public bool dockedToAcceptor = false;
	public bool dockedToDonor = false;

	public float age = 0.0f;
	public float delayToGrowNanoParticle = 4.0f;  // cosmetic delay to let accretion particle system get up to speed
	public float timeToGrowNanoParticle = 10.0f;  // time after grow delay before ring is allowed to stack
	public bool ringCanStack = false;

	public Fishtank fishtankScript;
	public GameObject fishtankGO;
	public GameObject myNanoParticle;
	private Vector3 myNanoParticleLocalScaleInit;

	public Light myRingLight;
	public float myRingLightMaxIntensity;
	public float myRingLightCurrentIntensity;


	// Particle Systems
	public GameObject goElectric01;
	public ParticleSystem psElectric01;
	public ParticleSystem.MainModule psElectric01Main;

	public GameObject goAccretion01;
	public ParticleSystem psAccretion01;
	public ParticleSystem.MainModule psAccretion01Main;
	public ParticleSystem.EmissionModule psAccretion01Emission;
	private float psAccretion01EmissionRateInit = 100.0f;
	//public float psAccretion01EmissionRateCurrent;

	// Runtime Shader Interaction
	public Shader shaderVert;
	public Shader shaderTrans;

	public GameObject meshPart0;
	public GameObject meshPart1;

	public Renderer myMeshPart0Renderer;
	public Renderer myMeshPart1Renderer;

	public Color colorMeshPart0;
	public Color colorMeshPart1;

	public SpringJoint sjDonorToAcceptor;
	public SpringJoint sjAcceptorToDonor;


	void Start()
	{
		// runtime shader swap setup

		shaderVert = Shader.Find(" Vertex Colored"); // WTF? unbelievably this shader name has a <space> before the 'V'
		shaderTrans = Shader.Find("Transparent/Diffuse");

		meshPart0 = gameObject.transform.Find("Ring_MeshPart0").gameObject;
		meshPart1 = gameObject.transform.Find("Ring_MeshPart1").gameObject;

		myMeshPart0Renderer = meshPart0.GetComponent<Renderer>();
		myMeshPart1Renderer = meshPart1.GetComponent<Renderer>();

		colorMeshPart0 = myMeshPart0Renderer.material.color;
		colorMeshPart0.a = 0.1f;
		myMeshPart0Renderer.material.SetColor("_Color", colorMeshPart0);

		colorMeshPart1 = myMeshPart1Renderer.material.color;
		colorMeshPart1.a = 0.1f;
		myMeshPart1Renderer.material.SetColor("_Color", colorMeshPart1);

		myNanoParticle = gameObject.transform.Find("Wire").gameObject;
		myNanoParticle.SetActive(true);
		myNanoParticleLocalScaleInit = myNanoParticle.transform.localScale;
		myNanoParticle.transform.localScale = 0.0f * myNanoParticleLocalScaleInit;

		GameObject myRingLightGO = gameObject.transform.Find("ringLight").gameObject;
		myRingLight = myRingLightGO.GetComponent<Light>();
		myRingLightMaxIntensity = myRingLight.intensity;
		myRingLight.intensity = 0.0f;
	}

	void InitialiseSpringJoints()
	{
		sjDonorToAcceptor = gameObject.AddComponent(typeof(SpringJoint)) as SpringJoint;
		sjDonorToAcceptor.connectedBody = null; //
		sjDonorToAcceptor.anchor = new Vector3(0f, 0.39f, 0f); //0.39 is equivalent to transform in ring prefab
		sjDonorToAcceptor.autoConfigureConnectedAnchor = false;
		sjDonorToAcceptor.connectedAnchor = new Vector3(0f, 0f, 0f);
		sjDonorToAcceptor.spring = 0f;
		sjDonorToAcceptor.damper = 50;
		sjDonorToAcceptor.minDistance = 0f;
		sjDonorToAcceptor.maxDistance = 0f;
		sjDonorToAcceptor.tolerance = 0.01f;
		sjDonorToAcceptor.enableCollision = true;


		sjAcceptorToDonor = gameObject.AddComponent(typeof(SpringJoint)) as SpringJoint;
		sjAcceptorToDonor.connectedBody = null; //
		sjAcceptorToDonor.anchor = new Vector3(0f, -0.39f, 0f); 
		sjAcceptorToDonor.autoConfigureConnectedAnchor = false;
		sjAcceptorToDonor.connectedAnchor = new Vector3(0f, 0f, 0f);
		sjAcceptorToDonor.spring = 0f;
		sjAcceptorToDonor.damper = 50;
		sjAcceptorToDonor.minDistance = 0f;
		sjAcceptorToDonor.maxDistance = 0f;
		sjAcceptorToDonor.tolerance = 0.01f;
		sjAcceptorToDonor.enableCollision = true;


		//sjDonor2Acceptor.tag = "chain";
	}

	void Awake()
	{
		velEst = GetComponent<VelocityEstimator>();
		fishtank = transform.parent;
		GameObject fishtankGO = GameObject.Find("fishtank");
		fishtankScript = fishtankGO.GetComponent<Fishtank>();

		var myElectric01 = Instantiate(goElectric01, gameObject.transform);
	
		psElectric01 = myElectric01.GetComponentInChildren<ParticleSystem>();
		psElectric01.transform.localScale = fishtankScript.nanowireFxScale * fishtankGO.transform.localScale;

		if (true)
		{
			var myAccretion01 = Instantiate(goAccretion01, gameObject.transform);

			psAccretion01 = myAccretion01.GetComponentInChildren<ParticleSystem>();
			psAccretion01.transform.localScale = fishtankScript.nanowireFxScale * fishtankGO.transform.localScale;

			psAccretion01Emission = psAccretion01.emission;
			psAccretion01Emission.rateOverTime = psAccretion01EmissionRateInit;
		}

		if (true) //(fishtankScript.ringsUseSpringConstraints)
		{
			InitialiseSpringJoints();
		}
	}

	void Update()
	{
		//psElectric01.transform.localScale = fishtankScript.nanowireFxScale * fishtankGO.transform.localScale;
		age = age + Time.deltaTime;

		if (age < (delayToGrowNanoParticle + timeToGrowNanoParticle))
		{
			float scaleRelativeF = ((age - delayToGrowNanoParticle) / timeToGrowNanoParticle);
			if (scaleRelativeF < 0f)
			{
				scaleRelativeF = 0f;
			}
			myNanoParticle.transform.localScale = scaleRelativeF * myNanoParticleLocalScaleInit;
			myRingLightCurrentIntensity = myRingLightMaxIntensity * scaleRelativeF;
			myRingLight.intensity = myRingLightCurrentIntensity;

			psAccretion01Emission.rateOverTime = psAccretion01EmissionRateInit * (1.05f - scaleRelativeF);
		}
		else
		{
			//myNanoParticle.SetActive(true);
			ringCanStack = true;
		}

	}

	public void SetShaderTrans()
	{
		myMeshPart0Renderer.material.shader = shaderTrans;
		myMeshPart1Renderer.material.shader = shaderTrans;
	}

	public void SetShaderVertexCol()
	{
		myMeshPart0Renderer.material.shader = shaderVert;
		myMeshPart1Renderer.material.shader = shaderVert;
	}

	public void breakRing(Hand currentHand)
	{
		if (currentHand != null)
		{
			// Drop whatever you're holding
			currentHand.otherHand.DetachObject(currentHand.otherHand.currentAttachedObject);
			currentHand.DetachObject(currentHand.currentAttachedObject);
			Debug.Log(currentHand.otherHand.name + " is hovering over " + gameObject.name + " which is attached to " + currentHand.name);
		}

		// Make a dimer and attach it to the hand. This replaces the ring you were just holding.
		var ring2DimerTransform = transform.Find("tf_ring2dimer");
		var dimer = Instantiate(dimerPrefab, ring2DimerTransform.position, transform.rotation, fishtank);
		//var dimer = Instantiate(dimerPrefab, transform.position, transform.rotation, fishtank);
		dimer.GetComponent<Rigidbody>().AddForce(-dimer.transform.forward * Random.RandomRange(0.01f, 0.02f), ForceMode.Impulse);
		dimer.name = "dimer_" + dimer.GetInstanceID();
		fishtankScript.SetCartoonRendering(dimer);

		float minDist = 0;
		if (currentHand != null) {
			minDist = Vector3.Distance (dimer.transform.position, currentHand.otherHand.hoverSphereTransform.position);
		}
		var match = dimer;
		foreach (Transform child in dimer.transform)
		{
			if (child.name.StartsWith("ring"))
			{
				var childDimer = Instantiate(dimerPrefab, child.transform.position, child.transform.rotation, fishtank);
				childDimer.GetComponent<Rigidbody>().AddForce(-childDimer.transform.forward * Random.RandomRange(0.01f, 0.02f), ForceMode.Impulse);
				childDimer.name = "dimer_" + dimer.GetInstanceID();
				fishtankScript.SetCartoonRendering(childDimer);

				if (currentHand != null)
				{
					var dist = Vector3.Distance(child.transform.position, currentHand.otherHand.hoverSphereTransform.position);

					if (dist < minDist)
					{
						minDist = dist;
						match = childDimer;
					}
				}
			}
		}
		Destroy(gameObject);

		if (currentHand != null) {
			var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachOthers;
			currentHand.otherHand.AttachObject(match, attachmentFlags);
		}
	}


	void OnHandHoverBegin(Hand hand)
	{
		if (gameObject == hand.otherHand.currentAttachedObject && shouldBreak && hand.otherHand.AttachedObjects.Count <= 2)
		{
			breakRing(hand);
		}
	}

	void HandAttachedUpdate(Hand hand)
	{
		if (gameObject == hand.currentAttachedObject && shouldBreak)
		{
			Vector3 velocity = velEst.GetVelocityEstimate();
			//Debug.Log("Velocity: " + velocity + velocity.magnitude);

			if (velocity.magnitude > 10.0 && ringAttached)
			{
				breakRing(hand);
				ringAttached = false;
			}
		}
	}

	void OnAttachedToHand(Hand hand)
	{
		ringAttached = true;
		var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachFromOtherHand;
		if (dockedToAcceptor && !partnerAcceptor.GetComponent<Ring>().ringAttached)
		{
			hand.AttachObject(partnerAcceptor, attachmentFlags);
		}
		if (dockedToDonor && !partnerDonor.GetComponent<Ring>().ringAttached)
		{
			hand.AttachObject(partnerDonor, attachmentFlags);
		}
		foreach (var ao in hand.AttachedObjects)
		{
			ao.attachedObject.SetActive(true);
		}
		velEst.BeginEstimatingVelocity();
	}

	void OnDetachedFromHand()
	{
		ringAttached = false;
		velEst.FinishEstimatingVelocity();
	}

}
