using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDonut : MonoBehaviour {

	private GameObject donut;
	private Vector3 small;
	private Vector3 medium;
	private Vector3 big;
	private float startTime;

	public GameObject cartoonDonutPS;

	private void Awake()
	{
		donut = transform.parent.Find("Donut").gameObject;
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
		if ((other.gameObject.tag == "monomer" || other.gameObject.tag == "dimer" )&& !donut.activeInHierarchy)
		{
			donut.SetActive(true);
			startTime = Time.time;
			Invoke("DonutOff", 5);

			//Nick - why can't I do this here? :-)
			//var donutVfx = Instantiate(cartoonDonutPS, donut.transform.position, Quaternion.identity);
			//Destroy(donutVfx, 4.0f);

		}
	}

	private void Update()
	{
		var s = Time.time - startTime;
		var newSmall = Vector3.Lerp(small, medium, s);
		var newBig = Vector3.Lerp(big, medium, s);
		float f = Mathf.PingPong(s * 10, 1);
		donut.transform.localScale = Vector3.Lerp(newSmall, newBig, f);
	}
}
