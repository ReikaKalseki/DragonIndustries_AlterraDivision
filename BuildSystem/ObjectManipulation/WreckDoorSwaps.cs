/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
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
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
	public class WreckDoorSwaps : ManipulationBase {

		private static readonly string DOOR_FRAME = "055b3160-f57b-46ba-80f5-b708d0c8180e";

		private List<DoorSwap> swaps = new List<DoorSwap>();

		public override void applyToObject(GameObject go) {
			WreckDoorSwapper sw = go.EnsureComponent<WreckDoorSwapper>();
			sw.swaps = swaps;
			sw.Invoke("applyDelayed", 2);
			SNUtil.log("Queuing door swaps " + swaps.toDebugString("\n")+" on wreck "+go.name+" @ "+go.transform.position);
		}

		public override void applyToObject(PlacedObject go) {
			this.applyToObject(go.obj);
		}

		public override void loadFromXML(XmlElement e) {
			swaps.Clear();
			foreach (XmlElement e2 in e.getDirectElementsByTagName("door")) {
				DoorSwap d = new DoorSwap(e2.getVector("position").Value, e2.getProperty("type"));
				swaps.Add(d);
			}
		}

		public override void saveToXML(XmlElement e) {
			foreach (DoorSwap d in swaps) {
				XmlElement e2 = e.OwnerDocument.CreateElement("door");
				e2.addProperty("position", d.position);
				e2.addProperty("type", d.doorType);
				e.AppendChild(e2);
			}
		}

		public static void setupRepairableDoor(GameObject panel) {
			WeldableWallPanelGeneric weld = panel.EnsureComponent<WeldableWallPanelGeneric>();
			LiveMixin lv = weld.GetComponentInChildren<LiveMixin>();
			lv.data.canResurrect = true;
		}

		public class DoorSwap {

			public readonly Vector3 position;
			public readonly string doorType;

			internal static readonly Dictionary<string, string> doorPrefabs = new Dictionary<string, string>{
				{"Blocked", "d79ab37f-23b6-42b9-958c-9a1f4fc64cfd"},
				{"Handle", "d9524ffa-11cf-4265-9f61-da6f0fe84a3f"},
				{"Laser", "6f01d2df-03b8-411f-808f-b3f0f37b0d5c"},
				{"Repair", "b86d345e-0517-4f6e-bea4-2c5b40f623b4"},
				{"Openable", "b86d345e-0517-4f6e-bea4-2c5b40f623b4"},
				{"Delete", "b86d345e-0517-4f6e-bea4-2c5b40f623b4"},
			};

			internal static readonly HashSet<string> doorPrefabIDs = new HashSet<string>(doorPrefabs.Values);

			public DoorSwap(Vector3 pos, string t) {
				position = pos;
				doorType = t;
			}

			public void applyTo(GameObject go) {
				SNUtil.log("Matched to door "+go.transform.position+", converted to "+doorType, SNUtil.diDLL);
				Transform par = go.transform.parent;
				GameObject put = ObjectUtil.createWorldObject(doorPrefabs[doorType], true, true);
				if (put == null) {
					SNUtil.writeToChat("Could not find prefab for door type " + doorType);
					return;
				}
				put.transform.position = go.transform.position;
				put.transform.rotation = go.transform.rotation;
				put.transform.parent = par;
				go.destroy();
				StarshipDoor d = put.GetComponent<StarshipDoor>();
				if (d) {
					if (doorType == "Delete") {
						put.removeChildObject("Starship_doors_manual_01/Starship_doors_automatic");
					}
					else if (doorType == "Openable") {
						d.UnlockDoor();
					}
					else if (doorType == "Repair") {
						d.LockDoor();
						GameObject panel = ObjectUtil.createWorldObject("bb16d2bf-bc85-4bfa-a90e-ddc7343b0ac2", true, true);
						panel.transform.position = put.transform.position;
						panel.transform.rotation = put.transform.rotation;
						setupRepairableDoor(panel);
					}
				}
			}

			public override string ToString() {
				return string.Format("[DoorSwap @ {0}, type={1}]", position, doorType);
			}



		}

		public static bool areWreckDoorSwapsPending(GameObject go) {
			WreckDoorSwapper wr = go.GetComponent<WreckDoorSwapper>();
			return wr && wr.swaps.Count > 0;
		}

		class WreckDoorSwapper : MonoBehaviour {
			
			internal List<DoorSwap> swaps = new List<DoorSwap>();

			private void doSimpleSearch(GameObject doors, List<DoorSwap> unfound) {
				foreach (DoorSwap d in swaps) {
					bool found = false;
					if (doors) {
						foreach (Transform t in doors.transform) {
							if (!t || !t.gameObject)
								continue;
							Vector3 pos = t.position;
							//SNUtil.log("Checking door "+t.position);
							if (Vector3.Distance(d.position, pos) <= 0.5) {
								found = true;
								d.applyTo(t.gameObject);
							}
						}
					}
					if (!found) {
						unfound.Add(d);
					}
				}
			}

			private void doPrefabSearch(List<DoorSwap> unfound) {
				foreach (PrefabIdentifier pi in GetComponentsInChildren<PrefabIdentifier>(true)) {
					if (pi && (pi.ClassId == DOOR_FRAME || DoorSwap.doorPrefabIDs.Contains(pi.ClassId))) {
						try {
							for (int i = unfound.Count - 1; i >= 0; i--) {
								DoorSwap d = unfound[i];
								if (Vector3.Distance(d.position, pi.transform.position) <= 0.5) {
									d.applyTo(pi.gameObject);
									unfound.RemoveAt(i);
									break;
								}
							}
							if (unfound.Count == 0)
								break;
						}
						catch (Exception e) {
							SNUtil.log("Threw exception processing PI '"+pi+"': " + e);
							throw e;
						}
					}
				}
			}

			private void printCandidates(GameObject doors, List<DoorSwap> unfound) {
				string has = "Door candidates:{\n";
				if (doors) {
					foreach (Transform t in doors.transform) {
						if (t) {
							has += "[DOOR] "+t.name + " @ " + t.transform.position + "\n";
						}
					}
				}
				foreach (PrefabIdentifier pi in GetComponentsInChildren<PrefabIdentifier>()) {
					if (pi && (pi.ClassId == DOOR_FRAME || DoorSwap.doorPrefabIDs.Contains(pi.ClassId))) {
						has += "[PREFAB] "+pi.name + " [" + pi.ClassId + "] @ " + pi.transform.position + "\n";
					}
				}
				SNUtil.log(has, SNUtil.diDLL);
				SNUtil.log("}\nTrying again in 2s", SNUtil.diDLL);
			}

			public void applyDelayed() {
				GameObject doors = gameObject.getChildObject("Doors");
				List<DoorSwap> unfound = new List<DoorSwap>();
				try {
					doSimpleSearch(doors, unfound);
				}
				catch (Exception e) {
					SNUtil.log("Threw exception doing simple search: " + e);
				}
				if (unfound.Count > 0) {
					SNUtil.log("Some door swaps in wreck @ " + transform.position + " found no easy match, checking all PIs\n" + unfound.toDebugString("\n"), SNUtil.diDLL);
					try {
						doPrefabSearch(unfound);
					}
					catch (Exception e) {
						SNUtil.log("Threw exception processing PIs: "+e);
					}
				}
				if (unfound.Count > 0) {
					SNUtil.log("Some door swaps (" + unfound.Count + "/" + swaps.Count + ") for " + gameObject.name + " @ " + transform.position + " found no match!!\n" + unfound.toDebugString("\n"), SNUtil.diDLL);
					try {
						printCandidates(doors, unfound);
					}
					catch (Exception e) {
						SNUtil.log("Threw exception printing candidates: " + e);
					}
					Invoke("applyDelayed", 2);
					swaps = unfound;
				}
				else {
					SNUtil.log("Door swaps completed in "+ gameObject.name + " @ " + transform.position);
					this.destroy();
				}
			}
		}

	}
}
