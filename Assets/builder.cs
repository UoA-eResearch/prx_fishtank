using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class builder : MonoBehaviour
{

	public GameObject unit01_Prefab;
	public GameObject unit02_Prefab;
	public GameObject unit03_Prefab;

	public GameObject[] polyArr;
	private int polyLength = 50;



	// Use this for initialization
	void Start()
	{
		polyArr = new GameObject[polyLength];

		var offsetPositionBase = new Vector3(0f, 0f, 0.5f);
		var offsetPositionUnit = new Vector3(0f, -0.2f, 0.1f);
		var offsetRotationUnit = Quaternion.Euler(0, 0, 45);

		for (int i = 0; i < polyLength; i++)
		{
			if (i == 0)
			{
				polyArr[i] = Instantiate(unit03_Prefab, (transform.position + transform.TransformDirection(offsetPositionBase)), transform.rotation * offsetRotationUnit, transform);
			}
			else
			{
				Transform lastUnitTransform = polyArr[i - 1].transform;
				polyArr[i] = Instantiate(unit03_Prefab, (lastUnitTransform.position + lastUnitTransform.TransformDirection(offsetPositionUnit)), lastUnitTransform.rotation * offsetRotationUnit, transform);
			}
			
			
			
			//polyArr[i] = Instantiate(unit03_Prefab, (transform.position + transform.TransformDirection(i * offsetPositionUnit)), transform.rotation, transform);
			//polyArr[i] = Instantiate(unit03_Prefab, (transform.position + transform.TransformDirection(i * offsetPositionUnit)), offsetRotationUnit, transform);
			//Instantiate(unit01_Prefab, transform);
			if (i == 0)
			{
				Destroy(polyArr[i].GetComponent<SpringJoint>());
			}
			if (i > 0)
			{
				var mySpringJoint = polyArr[i].GetComponent<SpringJoint>();
				var springOffsetY = 0.08f;
				mySpringJoint.connectedBody = polyArr[i - 1].GetComponent<Rigidbody>();
				mySpringJoint.anchor = new Vector3(0f, springOffsetY, 0f);
				mySpringJoint.connectedAnchor = new Vector3(0f, -springOffsetY, 0f);
				mySpringJoint.spring = 10000;
			}
			//var myRigidBody = polyArr[i].GetComponent<Rigidbody>();
			//myRigidBody.WakeUp();
			//myRigidBody.AddForce(0.01f, 0f, 0f, ForceMode.Impulse);
		}
		if (false)
		{
			SpringJoint myNewSpringJoint = polyArr[2].AddComponent(typeof(SpringJoint)) as SpringJoint;
			var springOffsetY2 = 0.08f;
			myNewSpringJoint.connectedBody = polyArr[12].GetComponent<Rigidbody>();
			myNewSpringJoint.autoConfigureConnectedAnchor = false;
			myNewSpringJoint.anchor = new Vector3(0f, springOffsetY2, 0f);
			myNewSpringJoint.connectedAnchor = new Vector3(0f, -springOffsetY2, 0f);
			myNewSpringJoint.spring = 100;
			myNewSpringJoint.enableCollision = true;

			if (false)
			{
				Color color = Color.red;
				LineRenderer lr = polyArr[2].AddComponent(typeof(LineRenderer)) as LineRenderer;
				lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
				lr.SetColors(color, color);
				lr.SetWidth(0.1f, 0.1f);
				lr.SetPosition(0, polyArr[2].transform.position);
				lr.SetPosition(1, polyArr[12].transform.position);
			}



		}
		//DrawLine(polyArr[2].transform.position, polyArr[12].transform.position, Color.red, 10.0f);
	}



	void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
	{
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(color, color);
		lr.SetWidth(0.1f, 0.1f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		GameObject.Destroy(myLine, duration);
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
		//DrawLine(polyArr[2].transform.position, polyArr[12].transform.position, Color.red, 0.01f);
	}
}