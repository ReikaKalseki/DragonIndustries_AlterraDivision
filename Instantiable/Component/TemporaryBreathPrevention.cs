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

	public class TemporaryBreathPrevention : SelfRemovingComponent {

		public static void add(float duration) {
			TemporaryBreathPrevention m = Player.main.gameObject.AddComponent<TemporaryBreathPrevention>();
			m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
		}

	}
}