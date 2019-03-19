using System.Collections;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour {

	private bool useTouchCycling = false;

	public bool useButtonHoldOverloads { get; private set; }

	// Use this for initialization
	void Awake()
	{
		XmlDocument configXml = new XmlDocument();
		string settingsPath = Application.streamingAssetsPath + "/config.xml";
		configXml.Load(settingsPath);

		XmlNode touchCyclingNode = configXml.DocumentElement.SelectSingleNode("/root/input/touchpad-menu-cycling");
		useTouchCycling = touchCyclingNode.InnerText == "true" ? true : false;

		XmlNode buttonHoldOverloadsNode = configXml.DocumentElement.SelectSingleNode("/root/input/button-hold-overloads");
		useButtonHoldOverloads = buttonHoldOverloadsNode.InnerText == "true" ? true : false;
		Debug.Log(useButtonHoldOverloads);
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	public bool UseTouchCycling() {
		return useTouchCycling;
	}
}
