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
	
	public class TemporaryBreathPrevention : SelfRemovingComponent {
		
		public static void add(float duration) {
			TemporaryBreathPrevention m = Player.main.gameObject.AddComponent<TemporaryBreathPrevention>();
			m.elapseWhen = DayNightCycle.main.timePassedAsFloat+duration;
		}
		
	}
}