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

	public class HealthModifier : SelfRemovingComponent, CustomSerializedComponent {

		public float damageFactor = 1;

		public static void add(float dmg, float duration) {
			HealthModifier m = Player.main.gameObject.AddComponent<HealthModifier>();
			m.damageFactor = dmg;
			m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
		}

		public virtual void saveToXML(XmlElement e) {
			e.addProperty("endTime", elapseWhen);
			e.addProperty("modifier", damageFactor);
		}

		public virtual void readFromXML(XmlElement e) {
			elapseWhen = (float)e.getFloat("endTime", 0);
			damageFactor = (float)e.getFloat("modifier", 0);
		}

	}
}