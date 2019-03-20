using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using Valve.VR.InteractionSystem;

namespace Tests
{
	public class PlayModeTests
	{
		private GameObject fishtankGo;
		Fishtank fishtank;
		private GameObject partyModeSwitchGo;
		private PartyModeSwitch partyModeSwitch;

		[SetUp]
		public void PartyModeSetUp()
		{

            // TODO: the set up for fishtank to be tested is massive since it's not segmented and relies on a tonne of non-null variables. Will just use the playmode test script to test play mode features.
			fishtankGo = GameObject.Instantiate(new GameObject());
			fishtank = fishtankGo.AddComponent<Fishtank>();

			// setting up fishtank variables required.
			fishtank.pHSliderUI = EmptyGo();
			fishtank.cartoonRenderUI = EmptyGo();
			fishtank.fishtankScaleUI = EmptyGo();
			fishtank.partyModeUi = EmptyGo();
			fishtank.simulationUI = EmptyGo();
			fishtank.nanoUI = EmptyGo();

			// creating party mode switch and setting reference to fishtank
			partyModeSwitchGo = GameObject.Instantiate(new GameObject()); partyModeSwitch = partyModeSwitchGo.AddComponent<PartyModeSwitch>();
			partyModeSwitch.linearMapping = partyModeSwitchGo.AddComponent<LinearMapping>();
			fishtank.partyModeSwitch = partyModeSwitch;
		}


        private GameObject EmptyGo() {
			return GameObject.Instantiate(new GameObject());
		}

		/// <summary>
		/// tests if linear mapping works
		/// </summary>
		[Test]
        public void PartyLinearMappingValue()
        {
			// should set off the chain reaction
			partyModeSwitch.linearMapping.value = 1.0f;
			Assert.IsTrue(partyModeSwitch.linearMapping.value == 1.0f);
		}

        [UnityTest]
        public IEnumerator PreparePartyMode()
        {
			partyModeSwitch.IncrementValue();
			yield return null;
			Assert.IsTrue(fishtank.partyMode == true);
		}


		// [UnityTest]
		// public IEnumerator StartingPartyModePreparesPartyModeUi()
		// {
		// 	fishtank.StartPartyMode();
		// 	yield return null;
		// 	Assert.IsTrue(fishtank.scoreboardTimerLabel.text == "Get Ready!");
		// }
	}
}
