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
	
	class HealingOverTime : MonoBehaviour {
		
		private static readonly float TICK_RATE = 0.25F;
		
		private float totalToHeal;
		private float healingRemaining;
		private float totalDuration;
		
		private float healRate;
		private float startTime;
		
		internal void setValues(float total, float seconds) {
			totalToHeal = total;
			totalDuration = seconds;
			healingRemaining = total;
			healRate = totalToHeal/seconds*TICK_RATE;
		}
		
		public void activate() {
			CancelInvoke("tick");
			startTime = Time.time;
			InvokeRepeating("tick", 0f, TICK_RATE);
		}

		public void tick() {
			float amt = Mathf.Min(healingRemaining, healRate);
			Player.main.GetComponent<LiveMixin>().AddHealth(amt);
			healingRemaining -= amt;
			if (healingRemaining <= 0)
				UnityEngine.Object.Destroy(this);
		}
		
		private void OnKill() {
			UnityEngine.Object.Destroy(this);
		}
		
	}
}