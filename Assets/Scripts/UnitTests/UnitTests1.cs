using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class UnitTests
    {
		private GameObject fishtankGo;
		Fishtank fishtank;

        [SetUp]
        public void SetUpFishtank()
        {
            fishtankGo = SpawnEmptyGo();
			fishtank = fishtankGo.AddComponent<Fishtank>();
		}
            
        [TearDown]
        public void DestroyFishtank()
        {
			// fishtank = new Fishtank();
			fishtank = null;
		}

        [UnityTest]
        public IEnumerator UnitTests1WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [Test]
        public void SpawnGameObjectSuccessfully()
        {
            var Go = SpawnEmptyGo();
            Assert.IsTrue(Go != null);
        }

        /// <summary>
        /// Helper for spawning game objects quicker
        /// </summary>
        /// <returns>empty game object</returns>
        private GameObject SpawnEmptyGo() {
			return Object.Instantiate(new GameObject());
		}

        /// <summary>
        /// Tests if game settings manager returns non-null values from xml nodes.
        /// </summary>
        [Test]
        public void XmlNode_Value_Not_Null()
        {
            GameSettingsManager gameSettingsManager = new GameSettingsManager();
            Assert.IsNotNull(gameSettingsManager.useButtonHoldOverloads);
        }
    }
}
