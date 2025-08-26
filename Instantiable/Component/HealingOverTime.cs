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

	public class HealingOverTime : MonoBehaviour, CustomSerializedComponent {

		private static readonly float TICK_RATE = 0.25F;

		private float totalToHeal;
		private float healingRemaining;
		private float totalDuration;

		private float healRate;
		private float startTime;

		public HealingOverTime setValues(float total, float seconds) {
			totalToHeal = total;
			totalDuration = seconds;
			healingRemaining = total;
			healRate = totalToHeal / seconds * TICK_RATE;
			return this;
		}

		public void activate() {
			this.CancelInvoke("tick");
			startTime = Time.time;
			this.InvokeRepeating("tick", 0f, TICK_RATE);
		}

		internal void tick() {
			float amt = Mathf.Min(healingRemaining, healRate);
			Player.main.GetComponent<LiveMixin>().AddHealth(amt);
			healingRemaining -= amt;
			if (healingRemaining <= 0)
				this.destroy(false);
		}

		private void OnKill() {
			this.destroy(false);
		}

		public virtual void saveToXML(XmlElement e) {
			e.addProperty("total", totalToHeal);
			e.addProperty("remaining", healingRemaining);
			e.addProperty("duration", totalDuration);
			e.addProperty("rate", healRate);
			e.addProperty("time", startTime);
		}

		public virtual void readFromXML(XmlElement e) {
			totalToHeal = (float)e.getFloat("total", 0);
			healingRemaining = (float)e.getFloat("remaining", 0);
			totalDuration = (float)e.getFloat("duration", 0);
			healRate = (float)e.getFloat("rate", 0);
			this.activate();
			startTime = (float)e.getFloat("time", 0);
		}

	}
}