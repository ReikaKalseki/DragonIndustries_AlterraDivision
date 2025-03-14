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
	
	public static class SpawnedItemTracker {
		
		private static readonly string saveFileName = "CSpawns.dat";
		
		private static readonly List<SpawnedItemEvent> spawns = new List<SpawnedItemEvent>();
		private static readonly Dictionary<string, SpawnedItemEvent> spawnedIDs = new Dictionary<string, SpawnedItemEvent>();
		
		static SpawnedItemTracker() {
			IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
			IngameMenuHandler.Main.RegisterOnSaveEvent(handleSave);
		}
		
		public static SpawnedItemEvent addSpawn(TechType tt) {
			SpawnedItemEvent e = new SpawnedItemEvent(tt, (int)DayNightCycle.main.timePassedAsFloat);
			spawns.Add(e);
			return e;
		}
		
		public static bool isSpawned(GameObject p) {
			return getSpawnEvent(p) != null;
		}
		
		public static SpawnedItemEvent getSpawnEvent(GameObject p) {
			SpawnTagCallback c = p.GetComponent<SpawnTagCallback>();
			if (c)
				return c.entry;
			PrefabIdentifier pi = p.GetComponent<PrefabIdentifier>();
			if (!pi)
				return null;
			return spawnedIDs.ContainsKey(pi.Id) ? spawnedIDs[pi.Id] : null;
		}
		
		public static bool isSpawned(Pickupable p) {
			return isSpawned(p.gameObject);
		}
		
		public static SpawnedItemEvent getSpawnEvent(Pickupable p) {
			return getSpawnEvent(p.gameObject);
		}
		
		public static string getData() {
			return spawns.toDebugString();
		}
		
		public static string getDataMap() {
			return spawnedIDs.toDebugString();
		}
		
		public static void handleSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			List<string> content = new List<string>();
			spawns.Sort();
			foreach (SpawnedItemEvent tt in spawns) {
				content.Add(SpawnedItemEvent.getSaveString(tt));
			}
			File.WriteAllLines(path, content.ToArray());
		}
		
		public static void handleLoad() {
			string dir = SNUtil.getCurrentSaveDir();
			string path = Path.Combine(dir, saveFileName);
			if (!File.Exists(path))
				return;
			spawns.Clear();
			spawnedIDs.Clear();
			string[] content = File.ReadAllLines(path);
			foreach (string s in content) {
				SpawnedItemEvent e = SpawnedItemEvent.parse(s);
				SNUtil.log("Restored record of spawned item: "+e);
				if (e != null) {
					spawns.Add(e);
					if (!string.IsNullOrEmpty(e.objectID))
						spawnedIDs[e.objectID] = e;
				}
			}
		}
		
		private class SpawnTagCallback : MonoBehaviour {
			
			internal PrefabIdentifier prefab;
			internal SpawnedItemEvent entry;
			
			public void register() {
				if (!prefab || string.IsNullOrEmpty(prefab.Id)) {
					SNUtil.log("Skipping spawn tag callback for nulled ID: "+prefab+"; entry = "+entry, SNUtil.diDLL);
					return;
				}
				entry.attach(prefab);
				SNUtil.log("Attached spawn tag callback "+entry, SNUtil.diDLL);
			}
			
			void OnDestroy() {
				if (!Player.main)
					return;
				SNUtil.log("Destroying gameobject bearing spawn tag callback "+entry, SNUtil.diDLL);
				SpawnTagSearchCallback s = Player.main.gameObject.EnsureComponent<SpawnTagSearchCallback>();
				s.entry = entry;
				s.Invoke("register", 0.5F);
			}
			
		}
		
		private class SpawnTagSearchCallback : MonoBehaviour {
			
			internal SpawnedItemEvent entry;
			
			public void register() {
				IList<InventoryItem> li = Inventory.main.container.GetItems(entry.itemType);
				if (li == null || li.Count == 0) {
					SNUtil.log("Skipping spawn search tag callback, no matching items for "+entry, SNUtil.diDLL);
					return;
				}
				PrefabIdentifier prefab = li[li.Count-1].item.GetComponent<PrefabIdentifier>();
				if (!prefab || string.IsNullOrEmpty(prefab.Id)) {
					SNUtil.log("Skipping spawn search tag callback for nulled ID: "+prefab+"; entry = "+entry, SNUtil.diDLL);
					return;
				}
				entry.attach(prefab);
				SNUtil.log("Attached spawn search tag callback "+entry, SNUtil.diDLL);
			}
			
		}
		
		public class SpawnedItemEvent : IComparable<SpawnedItemEvent> {
			
			public readonly TechType itemType;
			public readonly int spawnTime;
			
			public string classID {get; private set;}
			public string objectID {get; private set;}
			
			public string tooltip {
				get {
					return "\n<color=#ffc500ff>This item was spawned by command "+Utils.PrettifyTime(spawnTime)+" into the game.</color>";
				}
			}
			
			internal SpawnedItemEvent(TechType tt, int time) {
				itemType = tt;
				spawnTime = time;
			}
			
	    	public int CompareTo(SpawnedItemEvent fx) {
	    		return spawnTime.CompareTo(fx.spawnTime);
	    	}
			
			public void setObject(PrefabIdentifier pi) {
				SpawnTagCallback s = pi.gameObject.EnsureComponent<SpawnTagCallback>();
				s.entry = this;
				s.prefab = pi;
				s.gameObject.SetActive(true);
				s.Invoke("register", 0.5F);
				s.enabled = true;
				SNUtil.log("Registered callback for spawn event "+this, SNUtil.diDLL);
			}
			
			internal void attach(PrefabIdentifier prefab) {
				classID = prefab.ClassId;
				objectID = prefab.Id;
				spawnedIDs[objectID] = this;
				prefab.gameObject.SetActive(false);
			}
			
			internal static string getSaveString(SpawnedItemEvent e) {
				string s = e.itemType.AsString()+","+e.spawnTime.ToString();
				if (!string.IsNullOrEmpty(e.objectID))
					s = s+","+e.classID+","+e.objectID;
				return s;
			}
			
			internal static SpawnedItemEvent parse(string s) {
				string[] parts = s.Split(',');
				SpawnedItemEvent e = new SpawnedItemEvent(SNUtil.getTechType(parts[0]), int.Parse(parts[1]));
				if (parts.Length >= 4) {
					e.classID = parts[2];
					e.objectID = parts[3];
				}
				return e;
			}
			
			public override string ToString() {
				return string.Format("[SpawnedItemEvent ItemType={0}, SpawnTime={1}, ClassID={2}, ObjectID={3}]", itemType, spawnTime, classID, objectID);
			}

			
		}
		
	}
}
