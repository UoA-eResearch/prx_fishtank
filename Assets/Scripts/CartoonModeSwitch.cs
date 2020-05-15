using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class CartoonModeSwitch : MonoBehaviour, IMenu
{
	public LinearMapping renderCartoonLM;
	public Text renderCartoonText;
	public bool renderCartoon = false;

	public Transform start;
	public Transform end;
	public GameObject myHandle;

	//public LinearMapping fishtankScaleLM;
	//public Text fishtankScaleText;
	//public float fishtankScale = 1.0f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update ()
	{
		renderCartoonLM.value = myHandle.transform.localPosition.x;
		if (renderCartoonLM.value < 0)
		{
			renderCartoonText.text = "internal structure";
			renderCartoon = true;
			//scoreboard.SetActive(true);
		}
		else
		{
			renderCartoonText.text = "external surface";
			renderCartoon = false;
			//scoreboard.SetActive(false);
		}
		//fishtankScale = 1.0f - ((1.0f - fishtankScaleLM.value) / 2.5f);
		//fishtankScale = 1.2f - ((1.0f - fishtankScaleLM.value) / 1.5f);
		//double _scaleD = System.Math.Round(fishtankScale, 1);
		//fishtankScaleText.text = _scaleD.ToString();
	}

	public void IncrementValue(){
		renderCartoonLM.value = 1f;
		SynchronizeHandleToValue();
	}

	public void DecrementValue() {
		renderCartoonLM.value = 0f;
		SynchronizeHandleToValue();
	}

	public void SynchronizeHandleToValue(){
		myHandle.transform.position = Vector3.Lerp(start.position, end.position, renderCartoonLM.value);
	}

	public bool GetRenderCartoon()
	{
		return renderCartoon;
	}

	//public float GetFishtankScale()
	//{
	//	return fishtankScale;
	//}
}
