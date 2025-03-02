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
	
	[Obsolete("Unimplemented")]
	public class O2ConsumptionRateModifier : SelfRemovingComponent {
		
		public float consumptionFactor = 1;
		
		[Obsolete("Effect is unimplemented")]
		public static void add(float f, float duration) {
			O2ConsumptionRateModifier m = Player.main.gameObject.AddComponent<O2ConsumptionRateModifier>();
			m.consumptionFactor = f;
			m.elapseWhen = DayNightCycle.main.timePassedAsFloat+duration;
		}
		
	}
}