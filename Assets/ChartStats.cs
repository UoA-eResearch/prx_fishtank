using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChartStats : MonoBehaviour {


	public GameObject monomerBar;
	public GameObject dimerBar;
	public GameObject ringBar;
	public GameObject stacksBar;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetStats(float monomerFraction, float dimerFraction, float ringFraction, float stacksFraction)
	{
		monomerBar.transform.localScale = new Vector3(monomerBar.transform.localScale.x, monomerFraction, monomerBar.transform.localScale.z);
		dimerBar.transform.localScale = new Vector3(dimerBar.transform.localScale.x, dimerFraction, dimerBar.transform.localScale.z);
		ringBar.transform.localScale = new Vector3 (ringBar.transform.localScale.x, ringFraction, ringBar.transform.localScale.z);
		stacksBar.transform.localScale = new Vector3(stacksBar.transform.localScale.x, stacksFraction, stacksBar.transform.localScale.z);
		//Debug.Log(monomerFraction + " " + dimerFraction + " " + ringFraction + " " + stacksFraction);
	}

}
