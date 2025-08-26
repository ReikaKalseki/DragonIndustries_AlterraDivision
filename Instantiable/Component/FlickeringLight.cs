using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {

	public class FlickeringLight : MonoBehaviour {

		public float dutyCycle = 0.5F;
		public float updateRate = 0.5F;

		//takes 1/N seconds to fade
		public float fadeRate = 99999;

		public float maxIntensity = -1;
		public float minIntensity = 0;

		public float currentIntensity { get; private set; }

		private float targetIntensity = 0;

		private Light light;

		private float lastUpdate = -1;

		void Update() {
			if (!light)
				light = this.GetComponent<Light>();
			if (!light)
				return;
			if (maxIntensity < 0)
				maxIntensity = light.intensity;
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = Time.deltaTime;
			if (currentIntensity > targetIntensity) {
				currentIntensity = Mathf.Max(targetIntensity, currentIntensity - (dT * fadeRate));
			}
			else if (currentIntensity < targetIntensity) {
				currentIntensity = Mathf.Min(targetIntensity, currentIntensity + (dT * fadeRate));
			}
			light.intensity = currentIntensity;
			if (time - lastUpdate >= updateRate) {
				targetIntensity = UnityEngine.Random.Range(0F, 1F) <= dutyCycle ? maxIntensity : minIntensity;
				lastUpdate = time;
			}
		}

	}
}