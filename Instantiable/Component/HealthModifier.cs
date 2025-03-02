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
	
	public class HealthModifier : SelfRemovingComponent {
		
		public float damageFactor = 1;
		
		public static void add(float dmg, float duration) {
			HealthModifier m = Player.main.gameObject.AddComponent<HealthModifier>();
			m.damageFactor = dmg;
			m.elapseWhen = DayNightCycle.main.timePassedAsFloat+duration;
		}
		
	}
}