using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class UnitTests1
    {
		private GameObject fishtankGo;
		Fishtank fishtank;

        [SetUp]
        public void SetUpFishtank()
        {
			fishtankGo = GameObject.Instantiate(new GameObject());
			fishtank = fishtankGo.AddComponent<Fishtank>();
		}
            
        [TearDown]
        public void DestroyFishtank()
        {
			// fishtank = new Fishtank();
			fishtank = null;
		}


        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator UnitTests1WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [Test]
        public void SpawnGo()
        {
            var Go = GameObject.Instantiate(new GameObject());
            Assert.IsTrue(Go != null);
        }

        private GameObject SpawnEmptyGo() {
			return GameObject.Instantiate(new GameObject());
		}
    }
}
