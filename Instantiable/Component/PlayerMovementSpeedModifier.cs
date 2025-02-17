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
	
	public class PlayerMovementSpeedModifier : SelfRemovingComponent {
		
		public float speedModifier = 1;
		
		public static void add(float modifier, float duration) {
			PlayerMovementSpeedModifier m = Player.main.gameObject.AddComponent<PlayerMovementSpeedModifier>();
			m.speedModifier = modifier;
			m.elapseWhen = DayNightCycle.main.timePassedAsFloat+duration;
		}
		
	}
}