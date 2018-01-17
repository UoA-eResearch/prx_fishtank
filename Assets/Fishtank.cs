using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fishtank : MonoBehaviour {

	public GameObject monomerPrefab;
	public int numMonomers = 50;
	private Dictionary<GameObject, GameObject> pairs;
	private Dictionary<GameObject, bool> isTop;
	public float pairingVelocity = .05f;
	public int rotationVelocity = 50;
	private Bounds bounds;

	void FindPairs()
	{
		pairs = new Dictionary<GameObject, GameObject>();
		isTop = new Dictionary<GameObject, bool>();
		var monomers = GameObject.FindGameObjectsWithTag("monomer");
		Debug.Log("There are " + monomers.Length + " monomers around");
		foreach (var a in monomers)
		{
			if (pairs.ContainsKey(a))
			{
				// Already know the pair for this
				continue;
			}
			var minDistance = float.PositiveInfinity;
			var match = a;
			foreach (var b in monomers)
			{
				if (a != b && !pairs.ContainsKey(b)) // Prevent love triangles
				{
					float dist = Vector3.Distance(a.transform.position, b.transform.position);
					if (dist < minDistance)
					{
						minDistance = dist;
						match = b;
					}
				}
			}
			Debug.Log(a.name + "'s closest pair is " + match.name + " with distance " + minDistance);
			pairs[a] = match;
			pairs[match] = a;
			isTop[a] = true;
			isTop[match] = false;
		}
	}

	void PushMonomersTogether()
	{
		var monomers = GameObject.FindGameObjectsWithTag("monomer");
		foreach (var monomer in monomers)
		{
			var partner = pairs[monomer];
			var targetPos = partner.transform.position;
			var offset = new Vector3(.0141f, -.0025f, .0066f);
			var rotationalOffset = new Vector3(0, 180, 0);
			
			if (isTop[monomer])
			{
				targetPos += offset;
			}
			else
			{
				targetPos -= offset;
			}
			monomer.transform.position = Vector3.MoveTowards(monomer.transform.position, targetPos, Time.deltaTime * pairingVelocity);
			var partnersRotation = partner.transform.rotation.eulerAngles;
			if (isTop[monomer])
			{
				var targetRotation = partnersRotation + rotationalOffset;
				monomer.transform.rotation = Quaternion.RotateTowards(monomer.transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * rotationVelocity);
			}
			else
			{
				var targetRotation = Vector3.zero;
				monomer.transform.rotation = Quaternion.RotateTowards(monomer.transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * rotationVelocity);
			}
			if (!bounds.Contains(monomer.transform.position))
			{
				// Wayward monomer
				monomer.transform.position = Vector3.MoveTowards(monomer.transform.position, bounds.center, Time.deltaTime * pairingVelocity);
			}
		}
	}

	// Use this for initialization
	void Start () {
		bounds = gameObject.GetComponent<Collider>().bounds;
		var b = bounds.extents;
		for (int i = 0; i < numMonomers; i++)
		{
			var monomer = Instantiate(monomerPrefab, transform);
			var pos = transform.position + new Vector3(Random.Range(-b.x, b.x), Random.Range(-b.y, b.y), Random.Range(-b.z, b.z));
			monomer.transform.position = pos;
			monomer.transform.rotation = Random.rotation;
			monomer.name = "monomer_" + i;	
		}
		InvokeRepeating("FindPairs", 0, .1f);
	}
	
	// Update is called once per frame
	void Update () {
		PushMonomersTogether();
	}
}
