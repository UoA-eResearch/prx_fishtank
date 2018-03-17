using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDonut : MonoBehaviour {

	private GameObject donut;

	private void Awake()
	{
		donut = transform.parent.Find("Donut").gameObject;
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
			Invoke("DonutOff", 100);
		}
	}
}
