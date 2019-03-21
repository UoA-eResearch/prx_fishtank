using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

namespace Tests
{
    public class PhSliderTests
    {
        private PHSlider phSlider;

        [SetUp]
        public void SetUpSlider()
        {
            phSlider = GameObject.Instantiate(new GameObject()).AddComponent<PHSlider>();
            phSlider.linearMapping = phSlider.gameObject.AddComponent<LinearMapping>();
            phSlider.phText = phSlider.gameObject.AddComponent<Text>();

            
            phSlider.myHandle = CreateEmptyGo(Vector3.one / 2);
            phSlider.myLinearDrive = phSlider.gameObject.AddComponent<LinearDrive>();
            phSlider.myLinearDrive.startPosition = CreateEmptyGo(Vector3.zero).transform;
            phSlider.myLinearDrive.endPosition = CreateEmptyGo(Vector3.one).transform;
        }

        private GameObject CreateEmptyGo(Vector3 position) => GameObject.Instantiate(new GameObject(), position, Quaternion.identity);

        [UnityTest]
        public IEnumerator PhChangesFromLinearMappingInUpdate ()
        {
            phSlider.linearMapping.value = 0;
            yield return null;
            Assert.IsTrue(phSlider.phText.text == "pH low");
        }


        [Test]
        public void SetPhMaxAndSynchroniseToHandle ()
        {
            // phSlider.myHandle = CreateEmptyGo(Vector3.one / 2);
            // phSlider.myLinearDrive = phSlider.gameObject.AddComponent<LinearDrive>();
            // phSlider.myLinearDrive.startPosition = CreateEmptyGo(Vector3.zero).transform;
            // phSlider.myLinearDrive.endPosition = CreateEmptyGo(Vector3.one).transform;

            phSlider.SetPhToMax();
            Assert.IsTrue(phSlider.myHandle.transform.position == Vector3.one);
        }


        [Test]
        public void SetPhMinAndSynchroniseToHandle ()
        {
            phSlider.SetPhToMin();
            Assert.IsTrue(phSlider.myHandle.transform.position == Vector3.zero);
        }

        
        [UnityTest]
        public IEnumerator GetPhValueStringWorking()
        {
            phSlider.linearMapping.value = 0.5f;
            yield return null;
            Assert.IsTrue(phSlider.GetPhValueStr() == "6");
        }
    }
}
