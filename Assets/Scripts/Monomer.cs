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
    public ParticleSystem psAttraction;

	public Renderer monomerRenderer;
	public Material legacyMaterial;
	public Shader legacyShader;


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

		if (!fishtankScript.gameSettingsManager.transitionMaterials)
		{
			monomerRenderer.material = legacyMaterial;
			monomerRenderer.material.shader = legacyShader;
		}
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

    public void ActivateAttractionParticle(Transform target, float distance)
    {
        // if particle system not positioned at bond site, relocate.
        if (psAttraction.transform.position != transform.Find("partnerPos").position)
        {
            psAttraction.transform.position = transform.Find("partnerPos").position;
        }
        // TODO: move particle system origin to the partnerPos
        Quaternion lookAt = Quaternion.LookRotation(target.transform.position - transform.position);
        psAttraction.transform.rotation = lookAt;

        var particleMain = psAttraction.main;
        // lifetime half distance for colour gradient transparency purposes.
        particleMain.startLifetime = (distance / particleMain.startSpeed.constantMax)/ 1.9f;
        psAttraction.Play();
    }

    public void DeactivateAttractionParticle()
    {
        psAttraction.Stop();
    }
}
