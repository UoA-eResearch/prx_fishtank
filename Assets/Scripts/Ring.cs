
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

	public bool ringHasNanoParticle = false;
	public bool ringCanStack = false;
	public bool ringInterlocked = false;

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
	public ParticleSystem psPartyTrail;

	public GameObject goAccretion01;
	public GameObject goAccretionDisperse;
	public ParticleSystem psAccretion01;
	public ParticleSystem.EmissionModule psAccretion01Emission;
	private float psAccretion01EmissionRateInit = 100.0f;
	//public float psAccretion01EmissionRateCurrent;

	// Runtime Shader Interaction
	public Renderer myMeshPart0Renderer;
	public Renderer myMeshPart1Renderer;

	//public SpringJoint sjDonorToAcceptor;
	//public SpringJoint sjAcceptorToDonor;

	public SpringJoint[] sjDonorToAcceptorArr;
	public SpringJoint[] sjAcceptorToDonorArr;

	public float sjRadialOffset = 0.6f; // radial offset for ring stacking spring joint constraints 
	public float ringMinSpringStrength = 2.0f;
	public float ringSpringsStrengthScale = 1.0f;
	public float ringSpringsTolerance = 0.0075f;
	public int ringSpringsDamper = 50;
	public float ringStackingAxialRotation = 8.09f; // also set in ring prefab transforms as Y rotation

	public bool sjDonorToAcceptorOn = false;
	public bool sjAcceptorToDonorOn = false;

	public AudioSource ringAudioSource;
	public AudioClip sfxRingSpawn;

	// public vars
	public AudioClip sfxElectricity01;
	public AudioClip sfxElectricity02;

	private bool nanoWireOn = false;
	public float materialTransitionSpeed = 1.0f;
	public float minTransparence = 0.2f;
	public float maxTransparence = 1.0f;

	void Start()
	{
		// scripted fail safe references to certain assets
		if (myNanoParticle == null) {
			myNanoParticle = gameObject.transform.Find("Wire").gameObject;
		}
		myNanoParticle.SetActive(true);
		myNanoParticleLocalScaleInit = myNanoParticle.transform.localScale;
		myNanoParticle.transform.localScale = 0.0f * myNanoParticleLocalScaleInit;

		if (myRingLight != null) {
			GameObject myRingLightGO = gameObject.transform.Find("ringLight").gameObject;
			myRingLight = myRingLightGO.GetComponent<Light>();
		}
		myRingLightMaxIntensity = myRingLight.intensity;
		myRingLight.intensity = 0.0f;
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

		ringAudioSource = GetComponent<AudioSource>();
		ringAudioSource.clip = sfxRingSpawn;
		ringAudioSource.loop = false;
		ringAudioSource.Play();

	}

	void ResetNanoParticle()
	{
		age = 0f;
		psAccretion01Emission.rateOverTime = 0.0f;
		ringHasNanoParticle = false;
		//myNanoParticle.transform.localScale = 0f * myNanoParticleLocalScaleInit;
		SetNanoParticleScale(0f);
		if (fishtankScript.doNanoParticles)
		{
			ringCanStack = false;
		}
	}

	void SetNanoParticleScale(float scale)
	{
		myNanoParticle.transform.localScale = scale * myNanoParticleLocalScaleInit;
		myRingLightCurrentIntensity = myRingLightMaxIntensity * scale;
		myRingLight.intensity = myRingLightCurrentIntensity;
	}

	void Update()
	{
        //psElectric01.transform.localScale = fishtankScript.nanowireFxScale * fishtankGO.transform.localScale;
        age = age + Time.deltaTime;

		UpdateRingMaterials();
		

		if (fishtankScript.doNanoParticles)
		{
			if (age < (delayToGrowNanoParticle + timeToGrowNanoParticle))
			{
				float scaleRelativeF = ((age - delayToGrowNanoParticle) / timeToGrowNanoParticle);
				if (scaleRelativeF < 0f)
				{
					scaleRelativeF = 0f;
				}

				SetNanoParticleScale(scaleRelativeF);
				//myNanoParticle.transform.localScale = scaleRelativeF * myNanoParticleLocalScaleInit;
				//myRingLightCurrentIntensity = myRingLightMaxIntensity * scaleRelativeF;
				//myRingLight.intensity = myRingLightCurrentIntensity;

				psAccretion01Emission.rateOverTime = psAccretion01EmissionRateInit * (1.05f - scaleRelativeF);

				ringHasNanoParticle = false;
			}
			else
			{
					ringHasNanoParticle = true;	
			}
		}
		else
		{
			ResetNanoParticle();
			ringCanStack = true;
		}

		if (fishtankScript.partyMode == true)
		{
			if (psPartyTrail.isStopped)
			{
				psPartyTrail.Play();
			}
		}
		else
		{
			if (psPartyTrail.isPlaying)
			{
				psPartyTrail.Stop();
			}
		}

		if (!ringInterlocked)
		{
			if (ringHasNanoParticle)
			{
				ringCanStack = true;
			}
		}
		else
		{
			ResetNanoParticle();
			//age = 0f;
			//psAccretion01Emission.rateOverTime = 0.0f;
			//ringHasNanoParticle = false;
			//ringCanStack = false;
		}




		if (!ringCanStack) 
		{
			//catch cases where manipulated rings have legacy spring constraints active
			if (sjDonorToAcceptorOn)
			{
				RingSwitchOffDonorToAcceptorConstraints();
			}
			if (sjAcceptorToDonorOn)
			{
				RingSwitchOffAcceptorToDonorConstraints();
			}
		}

		// catch cases where rings have legacy spring constraints active - probably bad logic in fishtank.cs :( 
		if (!partnerAcceptor)
		{
			if (sjDonorToAcceptorOn)
			{
				RingSwitchOffDonorToAcceptorConstraints();
			}
		}
		if (!partnerDonor)
		{
			if (sjAcceptorToDonorOn)
			{
				RingSwitchOffAcceptorToDonorConstraints();
			}
		}

	}

	float RingGetSpringFromDistance(float dist)
	{
		float calcSpringStrength;
		calcSpringStrength = ringSpringsStrengthScale * (1.0f / (dist * dist));
		//Debug.Log("distance = " + dist + "  spring = " + calcSpringStrength);
		calcSpringStrength = Mathf.Max(calcSpringStrength, ringMinSpringStrength);
		return calcSpringStrength;
	}


	void InitRadialSpringJoint(SpringJoint sj, int i, float anchorY)
	{
		sj.connectedBody = null; //
		sj.anchor = new Vector3(sjRadialOffset * (Mathf.Sin(i * (60.0f * Mathf.Deg2Rad))), anchorY, sjRadialOffset * (Mathf.Cos(i * (60.0f * Mathf.Deg2Rad))));
		sj.autoConfigureConnectedAnchor = false;
		sj.connectedAnchor = new Vector3(0f, 0f, 0f);
		sj.spring = 0f;
		sj.damper = 0;
		sj.minDistance = 0f;
		sj.maxDistance = 0f;
		sj.tolerance = ringSpringsTolerance;
		sj.enableCollision = true;
	}

	void InitialiseSpringJoints()
	{
		sjDonorToAcceptorArr = new SpringJoint[6];
		sjAcceptorToDonorArr = new SpringJoint[6];

		for (int i = 0; i < 6; i++)
		{
			sjDonorToAcceptorArr[i] = gameObject.AddComponent(typeof(SpringJoint)) as SpringJoint;
			InitRadialSpringJoint(sjDonorToAcceptorArr[i], i, 0f); //0.39 is equivalent to transform in ring prefab
			sjAcceptorToDonorArr[i] = gameObject.AddComponent(typeof(SpringJoint)) as SpringJoint;
			InitRadialSpringJoint(sjAcceptorToDonorArr[i], i, 0f);
		}
	}

	void RingDrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
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

	public void RingSetDonorToAcceptorConstraints(float rotationOffsetAngle)
	{
		var ringA = partnerAcceptor;

		for (int i = 0; i < 6; i++)
		{
			var sj = sjDonorToAcceptorArr[i];

			sj.connectedBody = ringA.GetComponent<Rigidbody>();

			float connectedAnchorX = sjRadialOffset * (Mathf.Sin((i * (Mathf.Deg2Rad * 60.0f)) + (Mathf.Deg2Rad * rotationOffsetAngle)));
			float connectedAnchorY = -0.39f; // -0.39f;
			float connectedAnchorZ = sjRadialOffset * (Mathf.Cos((i * (Mathf.Deg2Rad * 60.0f)) + (Mathf.Deg2Rad * rotationOffsetAngle)));

			sj.connectedAnchor = new Vector3(connectedAnchorX, connectedAnchorY, connectedAnchorZ);
			sj.damper = ringSpringsDamper;

			var startPoint = transform.position + transform.TransformVector(sj.anchor);
			var endPoint = sj.connectedBody.transform.position + sj.connectedBody.transform.TransformVector(sj.connectedAnchor);

			var currentSpringVector = endPoint - startPoint;
			sj.spring = RingGetSpringFromDistance(Vector3.Magnitude(currentSpringVector));

			if (false) // debug draw lines for sj's
			{
				Color constraintColor = Color.green;
				if (Vector3.Distance(startPoint, endPoint) >= (sj.minDistance + sj.tolerance))
				{
					constraintColor = Color.red;
				}
				if (Vector3.Distance(startPoint, endPoint) <= (sj.maxDistance - sj.tolerance))
				{
					constraintColor = Color.yellow;
				}
				RingDrawLine(startPoint, endPoint, constraintColor, 0.02f);
			}

		}
		sjDonorToAcceptorOn = true;
	}

	public void RingSetAcceptorToDonorConstraints(float rotationOffsetAngle)
	{
		var ringD = partnerDonor;

		for (int i = 0; i < 6; i++)
		{
			var sj = sjAcceptorToDonorArr[i];

			sj.connectedBody = ringD.GetComponent<Rigidbody>();

			float connectedAnchorX = sjRadialOffset * (Mathf.Sin((i * (Mathf.Deg2Rad * 60.0f)) + (Mathf.Deg2Rad * rotationOffsetAngle)));
			float connectedAnchorY = 0.39f; // 0.39f;
			float connectedAnchorZ = sjRadialOffset * (Mathf.Cos((i * (Mathf.Deg2Rad * 60.0f)) + (Mathf.Deg2Rad * rotationOffsetAngle)));

			sj.connectedAnchor = new Vector3(connectedAnchorX, connectedAnchorY, connectedAnchorZ);
			//sj.connectedAnchor = new Vector3(ringA.radius * (Mathf.Sin(i * (60.0f * Mathf.Deg2Rad))), 0f, ringA.radius * (Mathf.Cos(i * (60.0f * Mathf.Deg2Rad))));

			sj.damper = ringSpringsDamper;

			var startPoint = transform.position + transform.TransformVector(sj.anchor);
			var endPoint = sj.connectedBody.transform.position + sj.connectedBody.transform.TransformVector(sj.connectedAnchor);

			var currentSpringVector = endPoint - startPoint;
			sj.spring = RingGetSpringFromDistance(Vector3.Magnitude(currentSpringVector));

			if (false) // debug draw lines for sj's
			{
				Color constraintColor = Color.green;
				if (Vector3.Distance(startPoint, endPoint) >= (sj.minDistance + sj.tolerance))
				{
					constraintColor = Color.red;
				}
				if (Vector3.Distance(startPoint, endPoint) <= (sj.maxDistance - sj.tolerance))
				{
					constraintColor = Color.yellow;
				}
				RingDrawLine(startPoint, endPoint, constraintColor, 0.02f);
			}

		}
		sjAcceptorToDonorOn = true;
	}

	public void RingSwitchOffDonorToAcceptorConstraints()
	{
		if (sjDonorToAcceptorOn == true)
		{
			for (int i = 0; i < 6; i++)
			{
				var sj = sjDonorToAcceptorArr[i];
				sj.connectedBody = null;
				sj.damper = 0;
				sj.spring = 0f;
			}
			sjDonorToAcceptorOn = false;
		}
	}

	public void RingSwitchOffAcceptorToDonorConstraints()
	{
		if (sjAcceptorToDonorOn == true)
		{
			for (int i = 0; i < 6; i++)
			{
				var sj = sjAcceptorToDonorArr[i];
				sj.connectedBody = null;
				sj.damper = 0;
				sj.spring = 0f;
			}
			sjAcceptorToDonorOn = false;
		}
	}

	public void NanoWireOn(){
		if (nanoWireOn) {
			return;
		}
		nanoWireOn = true;
		if (!psElectric01.isPlaying)
		{
			psElectric01.Play();
            //r.SetShaderTrans(1.0f);
            // StartCoroutine(TransitionToTransparent());

			if (Random.value < 0.5f)
			{
				ringAudioSource.clip = sfxElectricity01;
			}
			else
			{
				ringAudioSource.clip = sfxElectricity02;
			}
			ringAudioSource.volume = 0.05f;
			ringAudioSource.loop = true;
			ringAudioSource.Play();			
		}
	}

	public void NanoWireOff()
	{
		if (!nanoWireOn) {
			return;
		}
		nanoWireOn = false;
		if (psElectric01.isPlaying)
		{
			psElectric01.Stop();
			ringAudioSource.Stop();
		}
	}

	private void UpdateRingMaterials() {
		Renderer[] subRenderers = { myMeshPart0Renderer, myMeshPart1Renderer };
		foreach (Renderer r in subRenderers) {
			if (nanoWireOn) {
				if (r.material.color.a > minTransparence) {
					Color curColor = r.material.color;
					Color newColor = curColor;
					newColor.a = Mathf.Clamp(newColor.a - (Time.deltaTime/materialTransitionSpeed), minTransparence, maxTransparence);
					r.material.color = newColor;
				}
			} else {
				if (r.material.color.a != maxTransparence) {
					Color curColor = r.material.color;
					Color newColor = curColor;
					newColor.a = Mathf.Clamp(newColor.a + (Time.deltaTime/materialTransitionSpeed), minTransparence, maxTransparence);
					r.material.color = newColor;
				}
			}
		}
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
		dimer.GetComponent<Rigidbody>().AddForce(-dimer.transform.forward * Random.Range(0.01f, 0.02f), ForceMode.Impulse);
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
				childDimer.GetComponent<Rigidbody>().AddForce(-childDimer.transform.forward * Random.Range(0.01f, 0.02f), ForceMode.Impulse);
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
		Instantiate(goAccretionDisperse, this.transform.position, this.transform.rotation);
		Destroy(gameObject);

		if (currentHand != null) {
			var attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachOthers;
			currentHand.otherHand.AttachObject(match, attachmentFlags);
		}
	}

	void OnHandHoverBegin(Hand hand)
	{
		//if (gameObject == hand.otherHand.currentAttachedObject && shouldBreak && hand.otherHand.AttachedObjects.Count <= 2)
		//{
		//	breakRing(hand);
		//}
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
		if (!fishtankScript.ringsUseSpringConstraints)
		{
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
		}
		velEst.BeginEstimatingVelocity();
	}

	void OnDetachedFromHand()
	{
		ringAttached = false;
		velEst.FinishEstimatingVelocity();
	}

}
