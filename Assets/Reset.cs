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
		if (Input.GetKeyDown(KeyCode.Q))
		{
			SceneManager.LoadScene("scene_vive");
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			SceneManager.LoadScene("scene_02");
		}
		if (Input.GetKeyDown(KeyCode.Z))
		{
			SceneManager.LoadScene("scene_03");
		}
	}
}
