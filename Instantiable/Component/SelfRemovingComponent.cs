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

	public abstract class SelfRemovingComponent : MonoBehaviour {

		public float elapseWhen;

		protected virtual void Update() {
			if (DayNightCycle.main.timePassedAsFloat >= elapseWhen) {
				this.destroy(false);
			}
		}

		private void OnKill() {
			this.destroy(false);
		}

	}
}