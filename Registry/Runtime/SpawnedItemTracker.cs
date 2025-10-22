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

	public class SpawnedItemTracker : SerializedTracker<SpawnedItemTracker.SpawnedItemEvent> {

		public static readonly SpawnedItemTracker instance = new SpawnedItemTracker();

		private readonly Dictionary<string, SpawnedItemEvent> spawnedIDs = new Dictionary<string, SpawnedItemEvent>();
		private readonly List<SpawnTagCallback> callbacks = new List<SpawnTagCallback>();

		private float lastTick;

		private SpawnedItemTracker() : base("CSpawns.dat", true, parse, SpawnedItemEvent.parseLegacy) {

		}

		private static SpawnedItemEvent parse(XmlElement s) {
			SpawnedItemEvent e = new SpawnedItemEvent(SNUtil.getTechType(s.getProperty("item")), s.getFloat("eventTime", -1));
			e.setObject(s);
			return e;
		}

		public SpawnedItemEvent addSpawn(TechType tt) {
			SpawnedItemEvent e = new SpawnedItemEvent(tt, (int)DayNightCycle.main.timePassedAsFloat);
			this.add(e);
			return e;
		}

		public bool isSpawned(GameObject p) {
			return this.getSpawnEvent(p) != null;
		}

		public SpawnedItemEvent getSpawnEvent(GameObject p) {
			PrefabIdentifier pi = p.GetComponent<PrefabIdentifier>();
			return !pi ? null : spawnedIDs.ContainsKey(pi.Id) ? spawnedIDs[pi.Id] : null;
		}

		public bool isSpawned(Pickupable p) {
			return this.isSpawned(p.gameObject);
		}

		public SpawnedItemEvent getSpawnEvent(Pickupable p) {
			return this.getSpawnEvent(p.gameObject);
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

		public void tick() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastTick > 0.5F) {
				lastTick = time;

				for (int i = callbacks.Count - 1; i >= 0; i--) {
					SpawnTagCallback tag = callbacks[i];
					if (tag.isReady) {
						bool flag;
						if (tag.needsSearch) {
							tag.search();
							flag = true;
						}
						else {
							flag = tag.register();
						}
						if (flag)
							callbacks.RemoveAt(i);
					}
				}
			}
		}

		private class SpawnTagCallback {

			public readonly PrefabIdentifier prefab;
			public readonly SpawnedItemEvent entry;

			public readonly float creationTime;

			public bool isReady { get {  return DayNightCycle.main.timePassedAsFloat-creationTime >= 0.5F; } }

			public bool needsSearch { get; private set; }

			internal SpawnTagCallback(SpawnedItemEvent e, PrefabIdentifier pi) {
				prefab = pi;
				entry = e;

				creationTime = DayNightCycle.main.timePassedAsFloat;
			}

			internal bool register() {
				if (!prefab || string.IsNullOrEmpty(prefab.Id)) {
					SNUtil.log("Skipping spawn tag callback for nulled ID: " + prefab + "; entry = " + entry, SNUtil.diDLL);
					needsSearch = true;
					return false;
				}
				entry.attach(prefab);
				SNUtil.log("Attached spawn tag callback " + entry, SNUtil.diDLL);
				return true;
			}

			public void search() {
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
				get {//why does time start at 480
					return "\n<color=#ffc500ff>This item was spawned by command " + Utils.PrettifyTime((int)eventTime-480) + " into the game.</color>";
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
				SpawnTagCallback s = new SpawnTagCallback(this, pi);
				instance.callbacks.Add(s);
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
			}

			internal static SpawnedItemEvent parseLegacy(string s) {
				string[] parts = s.Split(',');
				if (parts.Length != 2) {
					SNUtil.log("Error parsing legacy item spawn event '" + s + "'");
					return null;
				}
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
