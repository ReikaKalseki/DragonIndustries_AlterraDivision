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
	
	public abstract class SelfRemovingComponent : MonoBehaviour {
		
		public float elapseWhen;
			
		protected virtual void Update() {
			if (DayNightCycle.main.timePassedAsFloat >= elapseWhen) {
				UnityEngine.Object.Destroy(this);
			}
		}
		
		private void OnKill() {
			UnityEngine.Object.Destroy(this);
		}
		
	}
}