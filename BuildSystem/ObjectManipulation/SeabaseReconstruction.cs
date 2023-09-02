/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{		
	public static class SeabaseReconstruction {
		
		private static readonly Dictionary<string, SeabasePrefab> dataCache = new Dictionary<string, SeabasePrefab>();
		
		static SeabaseReconstruction() {
			new SeabaseDoorOpener().Patch();
		}
		
		internal static SeabasePrefab getOrCreatePrefab(XmlElement e) {
			string id = e.getProperty("identifier");
			if (!dataCache.ContainsKey(id)) {
				dataCache[id] = new SeabasePrefab(id, e);
				dataCache[id].Patch();
				SNUtil.log("Created worldgen seabase "+id);
			}
			return dataCache[id];
		}
		
		class SeabaseIDHolder : MonoBehaviour {
			
		}
		
		public class WorldgenSeabaseController : MonoBehaviour {
			
			private static readonly string GEN_MARKER = "GenMarker";
			
			internal XmlElement reconstructionData;
			internal string seabaseID;
			
			private float lastBuildTime = -1;
			private float lastSkyTime = -1;
			private float lastModifyTime = -1;
			
			private Vector3 baseCenter = Vector3.zero;
			private int pieceCount;
		
			//private Planter[] planters = null;
			private Animator[] animations = null;
			private StorageContainer[] storages = null;
			private Charger[] chargers = null;
			
			void rebuild(float time) {
				SNUtil.log("Seabase '"+seabaseID+"' undergoing reconstruction", SNUtil.diDLL);
				if (reconstructionData == null) {
					SNUtil.writeToChat("Cannot rebuild worldgen seabase @ "+baseCenter+" - no data");
					return;
				}
				List<XmlElement> li = reconstructionData.getDirectElementsByTagName("part");
				SNUtil.log("Reconstructing base from "+li.Count+" parts", SNUtil.diDLL);
				int idx = 0;
				foreach (XmlElement e2 in li) {
					//SNUtil.log("Reconstructing part #"+idx+" from "+e2.InnerXml, SNUtil.diDLL);
					idx++;
					try {
						CustomPrefab pfb = new CustomPrefab("9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5"); //BaseCell
						pfb.loadFromXML(e2);
						if (baseHasPart(gameObject, pfb) && pfb.prefabName != "9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5") { //ie is loose
							SNUtil.log("Skipped recreate of loose piece: "+pfb, SNUtil.diDLL);
							continue;
						}
						SNUtil.log("Reconstructed BaseCell/loose piece: "+pfb, SNUtil.diDLL);
						GameObject go2 = pfb.createWorldObject();
						go2.transform.parent = gameObject.transform;
						go2.EnsureComponent<WorldgenSeabasePart>();
						ObjectUtil.removeComponent<CustomMachineLogic>(go2);
						go2.EnsureComponent<PreventDeconstruction>();
						baseCenter += go2.transform.position;
						pieceCount++;
						List<XmlElement> li1 = e2.getDirectElementsByTagName("cellData");
						if (li1.Count == 1) {
							foreach (XmlElement e3 in li1[0].getDirectElementsByTagName("component")) {
								CustomPrefab pfb2 = new CustomPrefab("basePart");
								//Base.Piece type = Enum.Parse(typeof(Base.Piece), e3.getProperty("piece"));
								pfb2.loadFromXML(e3);
								if (pfb2.prefabName == PlacedObject.BUBBLE_PREFAB)
									continue;
								SNUtil.log("Reconstructed base component: "+pfb2, SNUtil.diDLL);
								GameObject go3 = pfb2.createWorldObject();
								if (pfb2.prefabName == "RoomWaterParkBottom")
									ObjectUtil.removeChildObject(go3, "BaseWaterParkFloorBottom/Bubbles");
								else if (pfb2.prefabName == "RoomWaterParkHatch")
									ObjectUtil.removeChildObject(go3, "BaseCorridorHatch(Clone)");
								else if (pfb2.prefabName == "CorridorBulkhead")
									go3.EnsureComponent<WorldgenBulkhead>();
								go3.transform.parent = go2.transform;
								rebuildNestedObjects(go3, e3);
								if (!reconstructionData.getBoolean("allowDeconstruct")) {
									ObjectUtil.removeComponent<BaseDeconstructable>(go3);
									ObjectUtil.removeComponent<Constructable>(go3);
									PreventDeconstruction pv = go3.EnsureComponent<PreventDeconstruction>();
									pv.inBase = true;
									pv.inCyclops = true;
									pv.inEscapePod = true;
								}
								List<XmlElement> li0 = e3.getDirectElementsByTagName("supportData");
								if (li0.Count == 1)
									new SeabaseLegLengthPreservation(li0[0]).applyToObject(go3);
								else if (li0.Count == 0)
									new SeabaseLegLengthPreservation(null).applyToObject(go3);
								li0 = e3.getDirectElementsByTagName("modify");
								if (li0.Count == 1) {
									List<ManipulationBase> li2 = new List<ManipulationBase>();
									CustomPrefab.loadManipulations(li0[0], li2);
									foreach (ManipulationBase mb in li2) {
										mb.applyToObject(go3);
									}
								}
							}
						}
						li1 = e2.getDirectElementsByTagName("inventory");
						if (li1.Count == 1) {
							//SNUtil.log("Recreating inventory contents: "+li1[0].OuterXml, SNUtil.diDLL);
							StorageContainer sc = go2.GetComponent<StorageContainer>();
							Charger cg = go2.GetComponent<Charger>();
							Planter p = go2.GetComponent<Planter>();
							if (sc == null && cg == null) {
								SNUtil.log("Tried to deserialize inventory to a null container in "+go2);
								continue;
							}
							GrowbedPropifier pg = null;
							if (p != null) {
								pg = go2.EnsureComponent<GrowbedPropifier>();
							}
							foreach (XmlElement e3 in li1[0].getDirectElementsByTagName("item")) {
								TechType tt = SNUtil.getTechType(e3.getProperty("type"));
								if (tt == TechType.None) {
									SNUtil.log("Could not deserialize item - null TechType: "+e3.OuterXml, SNUtil.diDLL);
								}
								else {
									bool lootCube = false;
									GameObject igo = ObjectUtil.getItem(tt);
									if (igo == null) {
										SNUtil.log("Item did not have prefab, using loot cube: "+e3.OuterXml, SNUtil.diDLL);
										igo = ObjectUtil.lookupPrefab("01de572d-5549-44c6-97cf-645b07d1c79d");
										lootCube = true;
									}
									int amt = e3.getInt("amount", 1);
									string slot = e3.getProperty("slot", true);
									for (int i = 0; i < amt; i++) {
										GameObject igo2 = UnityEngine.Object.Instantiate(igo);
										igo2.SetActive(false);
										Pickupable pp = igo2.GetComponent<Pickupable>();
										pp.SetTechTypeOverride(tt, true);
										InventoryItem item = null;
										if (pp == null) {
											SNUtil.log("Could not deserialize item - no pickupable: "+e3.OuterXml, SNUtil.diDLL);
										} 
										if (cg != null) {
											cg.equipment.AddItem(slot, new InventoryItem(pp), true);
										}
										else if (sc != null) {
											item = sc.container.AddItem(pp);
										}
									}
									SNUtil.log("Added "+tt+" x"+amt, SNUtil.diDLL);
								}
							}/*
							if (sc != null)
								SNUtil.log("Recreated inventory contents: "+sc.container._items.toDebugString(), SNUtil.diDLL);
							if (cg != null)
								SNUtil.log("Recreated charger contents: "+cg.equipment.equipment.toDebugString(), SNUtil.diDLL);
								*/
						}
						GameObject mdl = ObjectUtil.getChildObject(go2, "MachineModel");
						if (mdl)
							mdl.SetActive(true);
					}
					catch (Exception ex) {
						SNUtil.log("Threw exception reconstructing part: "+ex.ToString(), SNUtil.diDLL);
					}
				}
				//ObjectUtil.debugMode = true;
				ObjectUtil.removeChildObject(gameObject, "SubDamageSounds");
				ObjectUtil.removeChildObject(gameObject, "PowerAttach");
				ObjectUtil.removeChildObject(gameObject, "MapRoomFunctionality");
				ObjectUtil.removeChildObject(gameObject, "*x_TechLight_Cone");
				ObjectUtil.removeComponent<Light>(gameObject);
				ObjectUtil.removeComponent<BaseFloodSim>(gameObject);
				ObjectUtil.removeComponent<BaseHullStrength>(gameObject);
				ObjectUtil.removeComponent<BasePowerRelay>(gameObject);
				ObjectUtil.removeComponent<PowerFX>(gameObject);
				ObjectUtil.removeComponent<VoiceNotificationManager>(gameObject);
				ObjectUtil.removeComponent<VoiceNotification>(gameObject);
				ObjectUtil.removeComponent<BaseRoot>(gameObject);
				ObjectUtil.removeComponent<Base>(gameObject);
				ObjectUtil.removeComponent<WaterPark>(gameObject);
				ObjectUtil.removeComponent<CustomMachineLogic>(gameObject);
				ObjectUtil.removeComponent<Constructable>(gameObject); //TODO find a way to not need this so you *can* dismantle parts
				//ObjectUtil.debugMode = false;
				
				baseCenter /= pieceCount;
				
				//ObjectUtil.removeComponent<SkyApplier>(gameObject);				
				/*
				SkyApplier sk = gameObject.EnsureComponent<SkyApplier>();
				sk.renderers = gameObject.GetComponentsInChildren<Renderer>();
				sk.environmentSky = MarmoSkies.main.skyBaseInterior;
				sk.applySky = sk.environmentSky;
				sk.enabled = true;
				sk.ApplySkybox();
				sk.RefreshDirtySky();*/
				
				foreach (UseableDiveHatch c in gameObject.GetComponentsInChildren<UseableDiveHatch>(true)) {
					if (c.gameObject.name.Contains("WaterPark"))
						c.gameObject.EnsureComponent<WorldgenBaseWaterparkHatch>();
					else
						UnityEngine.Object.DestroyImmediate(c); //component not object
				}
				foreach (MapRoomCamera c in gameObject.GetComponentsInChildren<MapRoomCamera>(true)) {
					UnityEngine.Object.DestroyImmediate(c.gameObject);
				}
				SNUtil.log("Finished reconstructing seabase '"+seabaseID+"' @ "+baseCenter+" @ "+time, SNUtil.diDLL);
				//ObjectUtil.dumpObjectData(gameObject);
			}

			void Update() {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (seabaseID == null)
					seabaseID = gameObject.GetComponentInChildren<SeabaseIDHolder>().name;
				if (seabaseID == null) {
					SNUtil.writeToChat("Could not find seabase ID in "+this+" @ "+transform.position);
					return;
				}
				if (reconstructionData == null) {
					reconstructionData = dataCache[seabaseID].data;
				}
				
				if (time-lastBuildTime >= 1) {
					foreach (Transform t in transform) {
						if (t.gameObject.name.Contains("BaseCell") && t.childCount == 0) {
							UnityEngine.Object.Destroy(t.gameObject);
						}
					}
					if (!ObjectUtil.getChildObject(gameObject, "BaseCell")) {/*
						GameObject marker = ObjectUtil.getChildObject(gameObject, GEN_MARKER);
						bool isNew = !marker;
						if (!marker) {
							marker = new GameObject();
							marker.name = GEN_MARKER;
							marker.transform.parent = transform;
						}*/
						rebuild(time);
					}
					lastBuildTime = time;
				}
				if (time-lastSkyTime >= 15) {
					SkyApplier[] skies = gameObject.GetComponentsInChildren<SkyApplier>(true);
					mset.Sky skyAt = WaterBiomeManager.main.GetBiomeEnvironment(baseCenter);
					foreach (SkyApplier sk in skies) {
						if (!sk)
							continue;
						if (sk.environmentSky != MarmoSkies.main.skyBaseGlass && sk.environmentSky != MarmoSkies.main.skyBaseInterior && sk.environmentSky != skyAt) {
							sk.environmentSky = skyAt;
						}/*
						bool glass = true; looks bad
						foreach (Renderer r in sk.renderers) {
							if (r && !r.name.ToLowerInvariant().Contains("glass")) {
								glass = false;
								break;
							}
						}
						if (glass) {
							sk.environmentSky = MarmoSkies.main.skyBaseGlass;
						}*/
						sk.applySky = sk.environmentSky;
						sk.enabled = true;
						sk.ApplySkybox();
						sk.RefreshDirtySky();
					}
					//SNUtil.writeToChat("Set skies: "+skyAt+" @ "+baseCenter);
					lastSkyTime = time;
					ObjectUtil.setActive<Animator>(gameObject, false);
					//GetComponent<LightingController>().state = LightingController.LightingState.Damaged;
				}/*
				if (planters == null) {
					planters = gameObject.GetComponentsInChildren<Planter>();
				}*/
				if (storages == null) {
					storages = gameObject.GetComponentsInChildren<StorageContainer>();
					SNUtil.log("Worldgen Seabase "+seabaseID+" finding storages: "+storages.toDebugString(), SNUtil.diDLL);
				}
				if (chargers == null) {
					chargers = gameObject.GetComponentsInChildren<Charger>();
					SNUtil.log("Worldgen Seabase "+seabaseID+" finding chargers: "+chargers.toDebugString(), SNUtil.diDLL);
				}
				if (animations == null) {
					animations = gameObject.GetComponentsInChildren<Animator>();
					SNUtil.log("Worldgen Seabase "+seabaseID+" finding animations: "+animations.toDebugString(), SNUtil.diDLL);
				}
				if (time-lastModifyTime >= 5) {
					lastModifyTime = time;
					foreach (Animator a in animations)
						if (a)
							a.enabled = false;
					
					foreach (StorageContainer p in storages) {
						if (p.container.IsEmpty() && p.storageRoot.transform.childCount > 0) {
							SNUtil.log("Worldgen Seabase "+seabaseID+" rebuilding storage for "+p+": "+p.storageRoot.GetComponentsInChildren<Pickupable>().toDebugString(), SNUtil.diDLL);
							try {
								foreach (Pickupable pp in p.GetComponentsInChildren<Pickupable>(true)) {
									p.container.AddItem(pp);
								}
							}
							catch (Exception e) {
								SNUtil.log("Exception initializing worldgen seabase inventory @ "+p.transform.position+": "+e, SNUtil.diDLL);
							}
						}
					}
					/*
					foreach (Planter p in planters) {
						if (p.grownPlantsRoot.childCount == 0 && p.storageContainer.storageRoot.transform.childCount > 0) {
							try {
								//p.InitPlantsDelayed();
								foreach (Pickupable pp in p.storageContainer.GetComponentsInChildren<Pickupable>(true)) {
									p.AddItem(new InventoryItem(pp));
								}
							}
							catch (Exception e) {
								SNUtil.log("Exception initializing worldgen seabase planter @ "+p.transform.position+": "+e, SNUtil.diDLL);
							}
						}
					}*/
					foreach (Charger p in chargers) {
						//SNUtil.writeToChat(p+" @ "+p.transform.position+" : "+p.equipment.equippedCount.Count+" : "+p.equipmentRoot.transform.childCount);
						if (p.equipment.equippedCount.Count == 0 && p.equipmentRoot.transform.childCount > 0) {
							SNUtil.log("Worldgen Seabase "+seabaseID+" rebuilding storage for "+p+": "+p.equipmentRoot.GetComponentsInChildren<Pickupable>().toDebugString(), SNUtil.diDLL);
							try {
								int i = 0;
								Pickupable[] pc = p.equipmentRoot.GetComponentsInChildren<Pickupable>(true);
								//SNUtil.writeToChat("PC"+pc.Length+" > "+string.Join(",", p.slots.Keys));
								foreach (string key in p.slots.Keys) {
									p.equipment.AddItem(key, new InventoryItem(pc[i]), true);
									i++;
									if (i >= pc.Length)
										break;
								}
								p.opened = true;
								p.animator.SetBool(p.animParamOpen, true);
								p.ToggleUI(true);
							}
							catch (Exception e) {
								SNUtil.log("Exception initializing worldgen seabase charger @ "+p.transform.position+": "+e, SNUtil.diDLL);
							}
						}
					}
				}
			}
			
		}
		/*
		class BaseHider : MonoBehaviour {
			
			void Update() {
				bool active = gameObject.activeSelf;
				gameObject.SetActive(Vector3.Distance(Player.main.transform.position, transform.position) <= 100);
				if (active != gameObject.activeSelf) {
					SNUtil.writeToChat("Toggling seabase @ "+baseCenter+": "+active+" > "+gameObject.activeSelf);
				}
			}
			
		}*/
		
		private static bool baseHasPart(GameObject main, CustomPrefab pfb) {
			foreach (Transform t in main.transform) {
				PrefabIdentifier pi = t.GetComponent<PrefabIdentifier>();
				if (!pi || pi.ClassId != pfb.prefabName)
					continue;
				if (Vector3.Distance(pfb.position, t.position) >= 0.1)
					continue;
				return true;
			}
			return false;
		}
			
		private static void rebuildNestedObjects(GameObject main, XmlElement e) {
			foreach (XmlElement e2 in e.getDirectElementsByTagName("child")) {
				CustomPrefab pfb = new CustomPrefab(e2.getProperty("prefab"));
				pfb.loadFromXML(e2);
				GameObject go = pfb.createWorldObject();
				if (go != null) {
					go.transform.parent = main.transform;
					rebuildNestedObjects(go, e2);
				}
			}
		}
		
		public class WorldgenBaseWaterparkHatch : MonoBehaviour {
			
			private UseableDiveHatch hatch;
			private bool cleanedModel = false;
			
			void Update() {
				if (!hatch) {
					hatch = gameObject.GetComponent<UseableDiveHatch>();
					ObjectUtil.setActive<Animator>(gameObject, true);
				}
				if (!cleanedModel)
					cleanedModel = ObjectUtil.removeChildObject(gameObject, "BaseCorridorHatch(Clone)") > 0;
			}
			
			public bool isPlayerInside() {
				Vector3 acuCenter = transform.position;
				Vector3 outside = transform.position+transform.forward*4.5F;
				Vector3 pos = Player.main.transform.position;
				return Vector3.Distance(pos, acuCenter) > Vector3.Distance(pos, outside);
			}
			
		}
		
		class GrowbedPropifier : MonoBehaviour {
			
			void Update() {
				Planter p = gameObject.GetComponent<Planter>();
				if (p != null) {
					foreach (Transform t in p.slots) {
						if (t != null) {
							GrowingPlant g = t.gameObject.GetComponentInChildren<GrowingPlant>(true);
							if (g != null)
								g.SetProgress(1);
						}
					}
					foreach (Transform t in p.bigSlots) {
						if (t != null) {
							GrowingPlant g = t.gameObject.GetComponentInChildren<GrowingPlant>(true);
							if (g != null)
								g.SetProgress(1);
						}
					}
					gameObject.GetComponent<StorageContainer>().enabled = false;
				}
			}
			
		}
		
		public static List<mset.Sky> getBiomeSkies() {
			return WaterBiomeManager.main.biomeSkies;
		}
		
		public static int getBiomeIndex(string s) {
			return WaterBiomeManager.main.GetBiomeIndex(s);
		}
		
		internal class WorldgenSeabasePart : MonoBehaviour {
			
			private float lastTickTime;
			
			void Update() {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time-lastTickTime >= 1) {
					lastTickTime = time;
					ObjectUtil.removeComponent<CustomMachineLogic>(gameObject);
				}
			}
			
		}
		
		internal class SeabasePrefab : Spawnable {
		
			internal readonly XmlElement data;
			internal readonly string id;
			
			internal SeabasePrefab(string id, XmlElement e) : base("seabase##C2C##"+id, "Seabase: "+id, "") {
				data = e;
				this.id = id;
			}
			
			public override GameObject GetGameObject() {
				SNUtil.log("Reconstructing seabase with "+data.ChildNodes.Count+" parts", SNUtil.diDLL);
				GameObject go = new GameObject(ClassID);
				go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
				go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				go.EnsureComponent<TechTag>().type = TechType;
				
				WorldgenSeabaseController ws = go.EnsureComponent<WorldgenSeabaseController>();
				ws.reconstructionData = data;
				ws.seabaseID = id;
				GameObject holder = new GameObject();
				holder.name = id;
				holder.EnsureComponent<SeabaseIDHolder>();
				holder.transform.parent = go.transform;
				//go.GetComponent<LightingController>().state = LightingController.LightingState.Damaged;
				//go.EnsureComponent<BaseHider>();
				Vector3 pos = data.getVector("position").Value;
				go.transform.position = pos;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				SNUtil.log("Finished deserializing seabase @ "+pos, SNUtil.diDLL);
				return go;
			}
			
		}
		
		internal class WorldgenBulkhead : MonoBehaviour {
			
			internal BulkheadDoor door;
			
			private float lastOpenerCheckTime = -1;
			
			//private float openTime;
			
			void Update() {
				if (!door)
					door = GetComponentInChildren<BulkheadDoor>();
				
				if (door && door.isOpen && DayNightCycle.main.timePassedAsFloat-lastOpenerCheckTime >= 0.5F && !hasOpener()) {/*
					if (openTime > 4)
						GenUtil.OpenWorldgenSeabaseDoor.lockOpen(door);
					else
						openTime += Time.deltaTime;*/
					lastOpenerCheckTime = DayNightCycle.main.timePassedAsFloat;
					GameObject go = ObjectUtil.createWorldObject("SeabaseDoorOpener");
					go.transform.position = transform.position;
				}
			}
			
			private bool hasOpener() {
				SeabaseDoorOpenerTag bk = WorldUtil.getObjectsNearWithComponent<SeabaseDoorOpenerTag>(transform.position, 2).FirstOrFallback(null);
				return bk != null && Vector3.Distance(bk.transform.position, transform.position) < 2;
			}
			
		}
		
		class SeabaseDoorOpener : Spawnable {
			
			internal SeabaseDoorOpener() : base("SeabaseDoorOpener", "", "") {
				
			}
				
			public override GameObject GetGameObject() {
				GameObject go = new GameObject();
				go.EnsureComponent<PrefabIdentifier>().classId = ClassID;
				go.EnsureComponent<TechTag>().type = TechType;
				go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
				go.EnsureComponent<SeabaseDoorOpenerTag>();
				SphereCollider s = go.EnsureComponent<SphereCollider>();
				s.radius = 2;
				s.center = Vector3.zero;
				s.isTrigger = true;
				return go;
			}
		}
		
		class SeabaseDoorOpenerTag : MonoBehaviour {
			
			private bool hasRun;
			
			void Update() {
				if (!hasRun) {
					WorldgenBulkhead bk = WorldUtil.getClosest<WorldgenBulkhead>(transform.position);
					if (bk && Vector3.Distance(bk.transform.position, transform.position) < 2) {
						bk.door.targetState = true;
						bk.door.SetClips();
						bk.door.ResetAnimations();
						bk.door.animState = bk.door.SetAnimationState(bk.door.doorClipName);
						bk.door.animState.normalizedTime = 1f;
						bk.door.doorAnimation.Sample();
						bk.door.doorClipName = null;
						bk.door.viewClipName = null;
						bk.door.sound = null;
						bk.door.NotifyStateChange();
						hasRun = true;
						ObjectUtil.removeComponent<Sealed>(bk.gameObject);
					}
				}
			}
			
		}
		
	}
}
