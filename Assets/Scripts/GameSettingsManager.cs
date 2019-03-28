using System;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour {

	private bool useTouchCycling = false;

	public bool useButtonHoldOverloads { get; private set; }
	public bool grabSplitStack { get; private set; }
    private XmlDocument configXml;

    public GameSettingsManager()
    {
        InitSettingsFile();
	    useTouchCycling = GetXmlNodeValue("input/touchpad-menu-cycling");
	    useButtonHoldOverloads = GetXmlNodeValue("input/button-hold-overloads");
	    grabSplitStack = GetXmlNodeValue("input/grab-split-stack");
    }

	// Use this for initialization
	void Awake()
	{
        InitSettingsFile();
	    useTouchCycling = GetXmlNodeValue("input/touchpad-menu-cycling");
	    useButtonHoldOverloads = GetXmlNodeValue("input/button-hold-overloads");
	    grabSplitStack = GetXmlNodeValue("input/grab-split-stack");
	}

    public void InitSettingsFile()
    {
		configXml = new XmlDocument();
		string settingsPath = Application.streamingAssetsPath + "/config.xml";
		configXml.Load(settingsPath);
    }

    private Boolean GetXmlNodeValue(String path)
    {
        path = "/root/" + path;
		XmlNode node = configXml.DocumentElement.SelectSingleNode(path);
        var nodeValue = node.InnerText == "true" ? true : false;
        // Debug.Log(path);
        // Debug.Log(nodeValue);
        return nodeValue;
    }

	public bool UseTouchCycling() {
		return useTouchCycling;
	}
}
