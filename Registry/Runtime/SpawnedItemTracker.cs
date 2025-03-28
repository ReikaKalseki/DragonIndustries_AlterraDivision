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
	
	public class SpawnedItemTracker : SerializedTracker<SpawnedItemTracker.SpawnedItemEvent> {
		
		public static readonly SpawnedItemTracker instance = new SpawnedItemTracker();
		
		private readonly Dictionary<string, SpawnedItemEvent> spawnedIDs = new Dictionary<string, SpawnedItemEvent>();
		
		private SpawnedItemTracker() : base("CSpawns.dat", true, parse, SpawnedItemEvent.parseLegacy) {

		}
		
		private static SpawnedItemEvent parse(XmlElement s) {
			SpawnedItemEvent e = new SpawnedItemEvent(SNUtil.getTechType(s.getProperty("item")), s.getFloat("eventTime", -1));
			e.setObject(s);
			return e;
		}
		
		public SpawnedItemEvent addSpawn(TechType tt) {
			SpawnedItemEvent e = new SpawnedItemEvent(tt, (int)DayNightCycle.main.timePassedAsFloat);
			add(e);
			return e;
		}
		
		public bool isSpawned(GameObject p) {
			return getSpawnEvent(p) != null;
		}
		
		public SpawnedItemEvent getSpawnEvent(GameObject p) {
			SpawnTagCallback c = p.GetComponent<SpawnTagCallback>();
			if (c)
				return c.entry;
			PrefabIdentifier pi = p.GetComponent<PrefabIdentifier>();
			if (!pi)
				return null;
			return spawnedIDs.ContainsKey(pi.Id) ? spawnedIDs[pi.Id] : null;
		}
		
		public bool isSpawned(Pickupable p) {
			return isSpawned(p.gameObject);
		}
		
		public SpawnedItemEvent getSpawnEvent(Pickupable p) {
			return getSpawnEvent(p.gameObject);
		}
		
		public string getDataMap() {
			return spawnedIDs.toDebugString();
		}
		
		protected override void add(SpawnedItemEvent e) {
			base.add(e);
			if (!string.IsNullOrEmpty(e.objectID))
				spawnedIDs[e.objectID] = e;
		}
		
		protected override void clear() {
			base.clear();
			spawnedIDs.Clear();
		}
		
		private class SpawnTagCallback : MonoBehaviour {
			
			internal PrefabIdentifier prefab;
			internal SpawnedItemEvent entry;
			
			public void register() {
				if (!prefab || string.IsNullOrEmpty(prefab.Id)) {
					SNUtil.log("Skipping spawn tag callback for nulled ID: " + prefab + "; entry = " + entry, SNUtil.diDLL);
					return;
				}
				entry.attach(prefab);
				SNUtil.log("Attached spawn tag callback " + entry, SNUtil.diDLL);
			}
			
			void OnDestroy() {
				if (!Player.main)
					return;
				SNUtil.log("Destroying gameobject bearing spawn tag callback " + entry, SNUtil.diDLL);
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
					SNUtil.log("Skipping spawn search tag callback, no matching items for " + entry, SNUtil.diDLL);
					return;
				}
				PrefabIdentifier prefab = li[li.Count - 1].item.GetComponent<PrefabIdentifier>();
				if (!prefab || string.IsNullOrEmpty(prefab.Id)) {
					SNUtil.log("Skipping spawn search tag callback for nulled ID: " + prefab + "; entry = " + entry, SNUtil.diDLL);
					return;
				}
				entry.attach(prefab);
				SNUtil.log("Attached spawn search tag callback " + entry, SNUtil.diDLL);
			}
			
		}
		
	public class SpawnedItemEvent : SerializedTrackedEvent {
			
		public readonly TechType itemType;
			
		public string classID { get; private set; }
		public string objectID { get; private set; }
			
		public string tooltip {
			get {
				return "\n<color=#ffc500ff>This item was spawned by command " + Utils.PrettifyTime((int)eventTime) + " into the game.</color>";
			}
		}
			
		internal SpawnedItemEvent(TechType tt, double time) : base(time) {
			itemType = tt;
		}
			
		public override void saveToXML(XmlElement e) {
			e.addProperty("item", itemType.AsString());
			if (!string.IsNullOrEmpty(objectID)) {
				e.addProperty("class", classID);
				e.addProperty("object", objectID);
			}
		}
			
		public void setObject(PrefabIdentifier pi) {
			SpawnedItemTracker.SpawnTagCallback s = pi.gameObject.EnsureComponent<SpawnedItemTracker.SpawnTagCallback>();
			s.entry = this;
			s.prefab = pi;
			s.gameObject.SetActive(true);
			s.Invoke("register", 0.5F);
			s.enabled = true;
			SNUtil.log("Registered callback for spawn event " + this, SNUtil.diDLL);
		}
		
		internal void setObject(XmlElement s) {
			if (s.hasProperty("object")) {
				classID = s.getProperty("class");
				objectID = s.getProperty("object");
			}
		}
			
		internal void attach(PrefabIdentifier prefab) {
			classID = prefab.ClassId;
			objectID = prefab.Id;
			instance.spawnedIDs[objectID] = this;
			prefab.gameObject.SetActive(false);
		}
			
		internal static SpawnedItemEvent parseLegacy(string s) {
			string[] parts = s.Split(',');
			SpawnedItemEvent e = new SpawnedItemEvent(SNUtil.getTechType(parts[0]), int.Parse(parts[1]));
			if (parts.Length >= 4) {
				e.classID = parts[2];
				e.objectID = parts[3];
			}
			return e;
		}
			
		public override string ToString() {
			return string.Format("[SpawnedItemEvent ItemType={0}, SpawnTime={1}, ClassID={2}, ObjectID={3}]", itemType, eventTime, classID, objectID);
		}

			
	}
		
	}
}
