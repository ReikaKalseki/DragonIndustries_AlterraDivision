using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	[Obsolete]
	public class SurvivalEventTracker : SerializedTracker<SurvivalEventTracker.SurvivalEvent> {

		public static readonly SurvivalEventTracker instance = new SurvivalEventTracker();

		private SurvivalEventTracker() : base("SurvivalEvents.dat", true, parse, null) {

		}

		private static SurvivalEvent parse(XmlElement s) {
			string type = s.getProperty("type");
			if (string.IsNullOrEmpty(type))
				return null;
			switch (type) {
				default:
					return null;
			}
		}

		public abstract class SurvivalEvent : SerializedTrackedEvent {

			internal SurvivalEvent(float time) : base(time) {

			}

			public override void saveToXML(XmlElement e) {

			}

		}

	}
}
