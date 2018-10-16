using System.Collections;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour {

	private bool useTouchCycling = false;

	// Use this for initialization
	void Awake()
	{
		XmlDocument doc = new XmlDocument();
		string settingsPath = Application.streamingAssetsPath + "/config.xml";
		doc.Load(settingsPath);
		XmlNode touchCycling = doc.DocumentElement.SelectSingleNode("/root/input/touchpad-menu-cycling");
		if (touchCycling.InnerText == "true") {
			useTouchCycling = true;
		} else if (touchCycling.InnerText == "false") {
			useTouchCycling = false;
		} else {
			useTouchCycling = true;
		}
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	public bool getTouchCycling() {
		return useTouchCycling;
	}
}
