using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class tests : MonoBehaviour {

	public Fishtank ft;
	public PHSlider pHSlider;

	public bool allowWasdMovement = true;


	#if UNITY_EDITOR
	void Start () {
		Time.timeScale *= 6;
		DecrementPh();
	}
	
	// Update is called once per frame
	void Update () {
		if (allowWasdMovement)
		{
			PollWasdMovement();
		}
	}

	private void PollWasdMovement()
	{
		if (Input.GetKey(KeyCode.W))
		{
			Player.instance.transform.position += Time.deltaTime * Vector3.forward;
		}
		if (Input.GetKey(KeyCode.A))
		{
			Player.instance.transform.position -= Time.deltaTime * Vector3.right;
		}
		if (Input.GetKey(KeyCode.S))
		{
			Player.instance.transform.position -= Time.deltaTime * Vector3.forward;
		}
		if (Input.GetKey(KeyCode.D))
		{
			Player.instance.transform.position += Time.deltaTime * Vector3.right;
		}
	}

	private void DecrementPh() {
		for (int i = 0; i < 8; i++)
		{
			pHSlider.DecrementValue();
		}
	}
#endif
}
