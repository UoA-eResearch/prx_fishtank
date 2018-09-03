using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monomer : MonoBehaviour {

	private float age = 0.0f;
	private float timeGrowColliders = 5.0f;
	private float myCapsuleColliderRadiusInit;	private float myCapsuleColliderHeightInit;
	private float myShereColliderRaduiusInit;
	private float scaleColliders = 1.0f;

	private bool doScaleColliders = false;

	private CapsuleCollider myCapsuleCollider;
	private SphereCollider mySphereCollider;

	private Rigidbody myRigidbody;

	public Fishtank fishtankScript;
	public ParticleSystem psPartyTrail;

    // WIP adding attraction particle.
    public ParticleSystem psAttractionTrail;


	// Use this for initialization
	void Start () {
		myCapsuleCollider = gameObject.GetComponent<CapsuleCollider>();
		mySphereCollider = gameObject.GetComponent<SphereCollider>();
		myCapsuleColliderRadiusInit= myCapsuleCollider.radius;
		myCapsuleColliderHeightInit = myCapsuleCollider.height;
		myShereColliderRaduiusInit = mySphereCollider.radius;

		myRigidbody = gameObject.GetComponent<Rigidbody>();
	}

	private void Awake()
	{
		GameObject fishtankGO = GameObject.Find("fishtank");
		fishtankScript = fishtankGO.GetComponent<Fishtank>();
	}

	// Update is called once per frame
	void Update ()
	{
		age = age + Time.deltaTime;
		if (doScaleColliders)
		{
			updateScaleColliders();
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
	}

	void updateScaleColliders()
	{
		if (age < timeGrowColliders)
		{
			scaleColliders = 0.8f + 0.2f * (age / timeGrowColliders);
		}
		else
		{
			scaleColliders = 1.0f;
		}
		myCapsuleCollider.radius = scaleColliders * myCapsuleColliderRadiusInit;
		myCapsuleCollider.height = scaleColliders * myCapsuleColliderHeightInit;
		mySphereCollider.radius = scaleColliders * myShereColliderRaduiusInit;
	}
}
