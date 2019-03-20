using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

		[Test]
        public void UnitTests1SimplePasses()
        {
			Assert.IsTrue(1 == 1);
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
    }
}
