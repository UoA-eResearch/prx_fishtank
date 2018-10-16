using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Reset : MonoBehaviour {
	void OnDetachedFromHand()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R)) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}
