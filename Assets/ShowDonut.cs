using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDonut : MonoBehaviour {

	private Vector3 small;
	private Vector3 medium;
	private Vector3 big;
	private float startTime;

	public GameObject donut;
	public GameObject rustyDonut;
	public GameObject cartoonDonutPS;
	public GameObject myRingGO;
	public AudioSource ringAudioSource;
	public AudioClip donutSpawnSound;
	public AudioClip rustyDonutSpawnSound;

	//private PartyModeSwitch partyModeSwitch;
	private Fishtank myFishtank;

	private void Awake()
	{
		//partyModeSwitch = FindObjectOfType<PartyModeSwitch>();
		//if (!partyModeSwitch)
		//{
		//	Debug.Log("showDonut can't find PartyModeSwitch");
		//}
		myFishtank = FindObjectOfType<Fishtank>();
		if (!myFishtank)
		{
			Debug.Log("showDonut can't find Fishtank");
		}
		small = donut.transform.localScale * .9f;
		medium = donut.transform.localScale;
		big = donut.transform.localScale * 1.1f;
	}

	private void DonutOff()
	{
		donut.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((other.gameObject.tag == "monomer" || other.gameObject.tag == "dimer" )&& !donut.activeInHierarchy && myFishtank.partyMode)// partyModeSwitch.partying)
		{
			donut.SetActive(true);
			startTime = Time.time;
			Invoke("DonutOff", 5);
			
			var donutVfx = Instantiate(cartoonDonutPS, donut.transform.position, Quaternion.identity);
			Destroy(donutVfx, 4.0f);

			ringAudioSource.PlayOneShot(donutSpawnSound);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.name == "ringCenter" && !rustyDonut.activeInHierarchy && myFishtank.partyMode) // partyModeSwitch.partying)
		{
			rustyDonut.SetActive(true);
			ringAudioSource.PlayOneShot(rustyDonutSpawnSound);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.name == "ringCenter")
		{
			rustyDonut.SetActive(false);
		}
	}

	private void Update()
	{
		if (myFishtank.partyMode) // partyModeSwitch.partying)
		{
			var s = Time.time - startTime;
			var newSmall = Vector3.Lerp(small, medium, s);
			var newBig = Vector3.Lerp(big, medium, s);
			float f = Mathf.PingPong(s * 10, 1);
			donut.transform.localScale = Vector3.Lerp(newSmall, newBig, f);
		}
		else
		{
			donut.SetActive(false);
			rustyDonut.SetActive(false);
		}
	}
}
