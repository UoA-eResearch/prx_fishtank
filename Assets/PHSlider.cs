using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class PHSlider : MonoBehaviour
    {

        public LinearMapping linearMapping;
        private float currentLinearMapping = float.NaN;

        //-------------------------------------------------
        void Awake()
        {
            if (linearMapping == null)
            {
                linearMapping = GetComponent<LinearMapping>();
            }
        }


        //-------------------------------------------------
        void Update()
        {
            if (currentLinearMapping != linearMapping.value)
            {
                currentLinearMapping = linearMapping.value;

                var mappedToPH = (currentLinearMapping - 0.0f) / (1.0f - 0.0f) * (12.0f - 3.0f) + 3.0f;
                mappedToPH = Mathf.Round(mappedToPH * 10f) / 10f;
                var displayValue = GameObject.Find("TitleCanvas");
                var text = displayValue.GetComponentInChildren<Text>();
                text.text = "PH value: " + mappedToPH;
            }
        }
    }
}
