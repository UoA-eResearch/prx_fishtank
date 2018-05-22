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
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			SceneManager.LoadScene("scene_vive");
		}
		/*
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			SceneManager.LoadScene("scene_02");
		}
		*/
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			SceneManager.LoadScene("scene_03");
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			SceneManager.LoadScene("scene_04");
		}
	}
}
