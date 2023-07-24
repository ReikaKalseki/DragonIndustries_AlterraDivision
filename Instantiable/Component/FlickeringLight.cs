using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra {
	
	public class FlickeringLight : MonoBehaviour {
		
		public float dutyCycle = 0.5F;
		public float updateRate = 0.5F;
		
		private Light light;
		
		private float lastUpdate = -1;
		
		void Update() {
			if (!light)
				light = GetComponent<Light>();
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastUpdate >= updateRate) {
				light.enabled = UnityEngine.Random.Range(0F, 1F) <= dutyCycle;
				lastUpdate = time;
			}
		}
		
	}
}