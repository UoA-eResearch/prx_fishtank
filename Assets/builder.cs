using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class builder : MonoBehaviour
{

	public GameObject unit01_Prefab;
	public GameObject unit02_Prefab;

	public GameObject[] polyArr;
	private int polyLength = 50;



	// Use this for initialization
	void Start()
	{
		polyArr = new GameObject[polyLength];

		//Instantiate(unit01_Prefab, transform.position, transform.rotation, transform);
		var offsetUnit = new Vector3(0f, -0.25f, 0f);
		for (int i = 0; i < polyLength; i++)
		{
			polyArr[i] = Instantiate(unit02_Prefab, (transform.position + transform.TransformDirection(i * offsetUnit)), transform.rotation, transform);
			//Instantiate(unit01_Prefab, transform);
			if (i == 0)
			{
				Destroy(polyArr[i].GetComponent<SpringJoint>());
			}
			if (i > 0)
			{
				var mySpringJoint = polyArr[i].GetComponent<SpringJoint>();
				mySpringJoint.connectedBody = polyArr[i - 1].GetComponent<Rigidbody>();
				mySpringJoint.anchor = new Vector3(0f, 0.1f, 0f);
				mySpringJoint.connectedAnchor = new Vector3(0f, -0.1f, 0f);
			}
			//var myRigidBody = polyArr[i].GetComponent<Rigidbody>();
			//myRigidBody.WakeUp();
			//myRigidBody.AddForce(0.01f, 0f, 0f, ForceMode.Impulse);
		}
	}

	// Update is called once per frame
	void Update()
	{
		for (int i = 0; i < polyLength; i++)
		{
			//var myRigidBody = polyArr[i].GetComponent<Rigidbody>();
			//myRigidBody.WakeUp();
			//myRigidBody.AddForce(0.01f, 0f, 0f, ForceMode.Impulse);
			//polyArr[i].GetComponent<Rigidbody>().AddRelativeTorque(0.0001f * Random.onUnitSphere, ForceMode.Impulse);
		}
	}
}