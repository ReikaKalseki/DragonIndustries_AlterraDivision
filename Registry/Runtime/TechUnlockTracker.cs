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
	
	public static class TechUnlockTracker {
		
		private static readonly string saveFileName = "Unlocks.dat";
		
		private static readonly Dictionary<TechType, TechUnlock> unlocks = new Dictionary<TechType, TechUnlock>();
		
		static TechUnlockTracker() {
			IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
			IngameMenuHandler.Main.RegisterOnSaveEvent(handleSave);
		}
		
		public static void onUnlock(TechType tt) {
			unlocks[tt] = new TechUnlock(tt, DayNightCycle.main.timePassedAsFloat);
		}
		
		public static void onScan(PDAScanner.EntryData scan) {
			TechType tt = scan.key;
			unlocks[tt] = new TechUnlock(tt, DayNightCycle.main.timePassedAsFloat, true);
		}
		
		public static void forAllUnlocksNewerThan(float thresh, Action<TechType, TechUnlock> forEach) {
			float time = DayNightCycle.main.timePassedAsFloat;
			foreach (KeyValuePair<TechType, TechUnlock> kvp in unlocks) {
				float age = time-kvp.Value.unlockTime;
				if (age < thresh)
					forEach.Invoke(kvp.Key, kvp.Value);
			}
		}
		
		public static void handleSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			/*
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (KeyValuePair<TechType, float> kvp in unlockTimes) {	
				SNUtil.log("Found "+sh+" save handler for "+pi.ClassId, SNUtil.diDLL);
				sh.data = doc.CreateElement("object");
				sh.data.SetAttribute("objectID", pi.Id);
				sh.save(pi);
				doc.DocumentElement.AppendChild(sh.data);
			}
			SNUtil.log("Saving "+doc.DocumentElement.ChildNodes.Count+" objects to disk", SNUtil.diDLL);
			doc.Save(path);
			*/
			List<string> content = new List<string>();
			List<TechUnlock> li = new List<TechUnlock>(unlocks.Values);
			li.Sort();
			foreach (TechUnlock tt in li) {
				content.Add(tt.tech.AsString()+","+tt.unlockTime.ToString("0.0")+","+tt.isScan.ToString());
			}
			File.WriteAllLines(path, content.ToArray());
		}
		
		public static void handleLoad() {
			string dir = SNUtil.getCurrentSaveDir();
			string path = Path.Combine(dir, saveFileName);
			if (File.Exists(path)) {
				/*
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				saveData.Clear();
				foreach (XmlElement e in doc.DocumentElement.ChildNodes)
					saveData[e.Name == "player" ? "player" : e.GetAttribute("objectID")] = e;
				SNUtil.log("Loaded "+saveData.Count+" object entries from disk", SNUtil.diDLL);
				*/
			}
			unlocks.Clear();
			string[] content = File.ReadAllLines(path);
			foreach (string s in content) {
				string[] parts = s.Split(',');
				TechType tt = SNUtil.getTechType(parts[0]);
				unlocks[tt] = new TechUnlock(tt, float.Parse(parts[1]), bool.Parse(parts[2]));
			}
		}
		
		public class TechUnlock : IComparable<TechUnlock> {
			
			public readonly TechType tech;
			public readonly float unlockTime;
			public readonly bool isScan;
			
			internal TechUnlock(TechType tt, float time, bool scan = false) {
				tech = tt;
				unlockTime = time;
				isScan = scan;
			}
			
	    	public int CompareTo(TechUnlock fx) {
	    		return unlockTime.CompareTo(fx.unlockTime);
	    	}
			
		}
		
	}
}
