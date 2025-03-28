using System;
using System.IO;
using System.Reflection;
using System.Xml;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	
	public class TechUnlockTracker : SerializedTracker<TechUnlockTracker.TechUnlock> {
		
		public static readonly TechUnlockTracker instance = new TechUnlockTracker();
		
		private readonly Dictionary<TechType, TechUnlock> unlocks = new Dictionary<TechType, TechUnlock>();
		
		private TechUnlockTracker() : base("Unlocks.dat", true, parseUnlock, parseLegacyUnlock) {
			
		}
		
		public void onUnlock(TechType tt) {
			add(new TechUnlock(tt, DayNightCycle.main.timePassedAsFloat));
		}
		
		public void onScan(PDAScanner.EntryData scan) {
			add(new TechUnlock(scan.key, DayNightCycle.main.timePassedAsFloat, true));
		}
		
		public TechUnlock getUnlock(TechType tt) {
			return unlocks.ContainsKey(tt) ? unlocks[tt] : null;
		}
		
		private static TechUnlock parseUnlock(XmlElement s) {
			return new TechUnlock(SNUtil.getTechType(s.getProperty("tech")), s.getFloat("eventTime", -1), s.getBoolean("isScan"));
		}
		
		private static TechUnlock parseLegacyUnlock(string s) {
			string[] parts = s.Split(',');
			TechType tt = SNUtil.getTechType(parts[0]);
			return new TechUnlock(tt, float.Parse(parts[1]), bool.Parse(parts[2]));
		}
		
		protected override void add(TechUnlock e) {
			base.add(e);
			unlocks[e.tech] = e;
		}
		
		protected override void clear() {
			base.clear();
			unlocks.Clear();
		}
		
		public class TechUnlock : SerializedTrackedEvent {
				
			public readonly TechType tech;
			public readonly bool isScan;
				
			internal TechUnlock(TechType tt, double time, bool scan = false) : base(time) {
				tech = tt;
				isScan = scan;
			}
				
			public override void saveToXML(XmlElement e) {
				e.addProperty("tech", tech.AsString());
				e.addProperty("isScan", isScan);
			}
			
			public override string ToString() {
				return string.Format("[TechUnlock Tech={0}, IsScan={1}, Time={2}]", tech, isScan, eventTime);
			}

				
		}
		
	}
}
