using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
	public class DelayedKill : MonoBehaviour {

		private DamageType damage;

		public void initialize(float delay, DamageType dmg) {
			damage = dmg;
			Invoke("run", delay);
		}

		private void run() {
			GetComponent<LiveMixin>().Kill(damage);
		}


	}
}
