using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public class ConsumableTracker : SerializedTracker<ConsumableTracker.ConsumeItemEvent> {

		public static readonly ConsumableTracker instance = new ConsumableTracker();

		private ConsumableTracker() : base("Consumption.dat", true, parseConsumption, null) {

		}

		public void onConsume(GameObject go, bool isEating) {
			onConsume(CraftData.GetTechType(go), isEating);
		}

		public void onConsume(TechType tt, bool isEating) {
			//SNUtil.writeToChat("Log eat of "+tt.AsString());
			if (tt != TechType.None)
				this.add(new ConsumeItemEvent(tt, DayNightCycle.main.timePassedAsFloat, isEating));
		}

		private static ConsumeItemEvent parseConsumption(XmlElement s) {
			return new ConsumeItemEvent(SNUtil.getTechType(s.getProperty("itemType")), s.getFloat("eventTime", -1), s.getBoolean("isEat"));
		}

		public ReadOnlyCollection<ConsumeItemEvent> getEvents() {
			return this.data.AsReadOnly();
		}

		public class ConsumeItemEvent : SerializedTrackedEvent {

			public readonly TechType itemType;
			public readonly bool isEating;

			internal ConsumeItemEvent(TechType tt, double time, bool eat) : base(time) {
				itemType = tt;
				isEating = eat;
			}

			public override void saveToXML(XmlElement e) {
				e.addProperty("itemType", itemType.AsString());
				e.addProperty("isEat", isEating);
			}

			public override string ToString() {
				return string.Format("[ConsumeItemEvent Tech={0}, Time={1}, Eat={2}]", itemType, eventTime, isEating);
			}


		}

	}
}
