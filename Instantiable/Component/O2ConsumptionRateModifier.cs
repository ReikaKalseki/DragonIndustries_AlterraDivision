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

	[Obsolete("Unimplemented")]
	public class O2ConsumptionRateModifier : SelfRemovingComponent {

		public float consumptionFactor = 1;

		[Obsolete("Effect is unimplemented")]
		public static void add(float f, float duration) {
			O2ConsumptionRateModifier m = Player.main.gameObject.AddComponent<O2ConsumptionRateModifier>();
			m.consumptionFactor = f;
			m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
		}

	}
}