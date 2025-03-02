using System;
using System.Linq;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class GenUtil {
		
		public static readonly Bounds allowableGenBounds = MathUtil.getBounds(-2299, -3100, -2299, 2299, 150, 2299);
		
		private static readonly HashSet<string> alreadyRegisteredGen = new HashSet<string>();
		private static readonly Dictionary<TechType, Databox> databoxes = new Dictionary<TechType, Databox>();
		private static readonly Dictionary<TechType, Crate>[] crates = new Dictionary<TechType, Crate>[2];
		private static readonly Dictionary<TechType, FragmentGroup> fragments = new Dictionary<TechType, FragmentGroup>();
		
		static GenUtil() {
			crates[0] = new Dictionary<TechType, Crate>();
			crates[1] = new Dictionary<TechType, Crate>();
			
			new EmptyCrate().Patch();
			//new OpenWorldgenSeabaseDoor().Patch();
		}
		
		public static void registerOreWorldgen(BasicCustomOre ore, BiomeType biome, int amt, float chance) {
			registerPrefabWorldgen(ore, ore.isLargeResource, biome, amt, chance);
		}
		
		public static void registerPlantWorldgen(BasicCustomPlant ore, BiomeType biome, int amt, float chance) {
			registerPrefabWorldgen(ore, ore.getSize() == Plantable.PlantSize.Large, biome, amt, chance);
		}
		
		public static void registerPrefabWorldgen(Spawnable sp, bool large, BiomeType biome, int amt, float chance) {
			registerPrefabWorldgen(sp, large ? EntitySlot.Type.Medium : EntitySlot.Type.Small, large ? LargeWorldEntity.CellLevel.Medium : LargeWorldEntity.CellLevel.Near, biome, amt, chance);
		}
		
		public static void registerPrefabWorldgen(Spawnable sp, EntitySlot.Type type, LargeWorldEntity.CellLevel size, BiomeType biome, int amt, float chance) {
			registerSlotWorldgen(sp.ClassID, sp.PrefabFileName, sp.TechType, type, size, biome, amt, chance);
		}
		
		public static void registerSlotWorldgen(string id, string file, TechType tech, EntitySlot.Type type, LargeWorldEntity.CellLevel size, BiomeType biome, int amt, float chance) {
			if (alreadyRegisteredGen.Contains(id)) {
		        LootDistributionHandler.EditLootDistributionData(id, biome, chance, amt); //will add if not present
			}
			else {		        
				LootDistributionData.BiomeData b = new LootDistributionData.BiomeData{biome = biome, count = amt, probability = chance};
		        List<LootDistributionData.BiomeData> li = new List<LootDistributionData.BiomeData>{b};
		        UWE.WorldEntityInfo info = new UWE.WorldEntityInfo();
		        info.cellLevel = size;
		        info.classId = id;
		        info.localScale = Vector3.one;
		        info.slotType = type;
		        info.techType = tech;
		       	WorldEntityDatabaseHandler.AddCustomInfo(id, info);
		        LootDistributionHandler.AddLootDistributionData(id, file, li, info);
		        
				alreadyRegisteredGen.Add(id);
			}
		}
		
		public static SpawnInfo registerWorldgen(PositionedPrefab pfb, Action<GameObject> call = null) {
			return registerWorldgen(pfb.prefabName, pfb.position, pfb.rotation, go => {
			    if (!Mathf.Approximately(pfb.scale.x, 1) || !Mathf.Approximately(pfb.scale.y, 1) || !Mathf.Approximately(pfb.scale.z, 1))
					go.transform.localScale = pfb.scale;
			    if (call != null)
			    	call(go);
			});
		}
		
		public static SpawnInfo registerWorldgen(string prefab, Vector3 pos, Vector3? rot = null, Action<GameObject> call = null) {
			return registerWorldgen(prefab, pos, Quaternion.Euler(getOrZero(rot)), call);
		}
		
		public static SpawnInfo registerWorldgen(string prefab, Vector3 pos, Quaternion? rot = null, Action<GameObject> call = null) {
			validateCoords(pos);
			SpawnInfo info = new SpawnInfo(prefab, pos, getOrIdentity(rot), call);
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			//SNUtil.log("Registering prefab "+prefab+" @ "+pos);
			return info;
		}
		
		public static SpawnInfo registerWorldgen(WorldGenerator gen) {
			if (gen == null)
				throw new Exception("You cannot register a null gen!");
			validateCoords(gen.position);
			Action<GameObject> call = go => {
				UnityEngine.Object.Destroy(go);
				SNUtil.log("Running world generator "+gen, SNUtil.diDLL);
				List<GameObject> li = new List<GameObject>();
				gen.generate(li);
				SNUtil.log("Generated approximately "+li.Count+" objects.", SNUtil.diDLL);
			};
			SpawnInfo info = new SpawnInfo(VanillaResources.LIMESTONE.prefab, gen.position, call);
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			SNUtil.log("Queuing world generator "+gen);
			return info;
		}
		
		public static SpawnInfo spawnDatabox(Vector3 pos, TechType tech, Vector3? rot = null) {
			return spawnDatabox(pos, tech, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnDatabox(Vector3 pos, TechType tech, Quaternion? rot = null) {
			return registerWorldgen(getOrCreateDatabox(tech).ClassID, pos, rot);
		}
		
		public static SpawnInfo spawnPDA(Vector3 pos, PDAManager.PDAPage page, Vector3? rot = null) {
			return spawnPDA(pos, page, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnPDA(Vector3 pos, PDAManager.PDAPage page, Quaternion? rot = null) {
			return registerWorldgen(page.getPDAClassID(), pos, rot);
		}
		
		public static SpawnInfo spawnItemCrate(Vector3 pos, TechType item, Vector3? rot = null) {
			return spawnItemCrate(pos, item, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnItemCrate(Vector3 pos, TechType item, Quaternion? rot = null) {
			return registerWorldgen(getOrCreateCrate(item).ClassID, pos, rot);
		}
		
		public static SpawnInfo spawnFragment(Vector3 pos, TechnologyFragment mf, Quaternion? rot = null) {
			return registerWorldgen(mf.fragmentPrefab.ClassID, pos, rot);
		}
		
		public static SpawnInfo spawnFragment(Vector3 pos, Spawnable item, string template, Quaternion? rot = null) {
			return registerWorldgen(getOrCreateFragment(item, template).ClassID, pos, rot);
		}
		
		public static SpawnInfo spawnResource(VanillaResources res, Vector3 pos, Vector3? rot = null) {
			return registerWorldgen(res.prefab, pos, rot);
		}
		
		public static SpawnInfo spawnTechType(TechType tech, Vector3 pos, Vector3? rot = null) {
			validateCoords(pos);
			SpawnInfo info = new SpawnInfo(tech, pos, getOrZero(rot));
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			return info;
		}
		
		public static Vector3 getOrZero(Vector3? init) {
			return init != null && init.HasValue ? init.Value : Vector3.zero;
		}
		
		public static Quaternion getOrIdentity(Quaternion? init) {
			return init != null && init.HasValue ? init.Value : Quaternion.identity;
		}
		
		private static void validateCoords(Vector3 pos) {
			if (!allowableGenBounds.Contains(pos))
				throw new Exception("Registered worldgen is out of bounds @ "+pos+"; allowable range is "+allowableGenBounds.min+" > "+allowableGenBounds.max);
		}
		
		public static ContainerPrefab getOrCreateCrate(TechType tech, bool needsCutter = false, Action<GameObject> modify = null) {
			int idx = needsCutter ? 1 : 0;
			Crate box = crates[idx].ContainsKey(tech) ? crates[idx][tech] : null;
			if (box == null) {
				box = new Crate(tech, "580154dd-b2a3-4da1-be14-9a22e20385c8", needsCutter, modify); //battery
				crates[idx][tech] = box;
				box.Patch();
			}
			return box;
		}
		
		public static ContainerPrefab getOrCreateDatabox(TechType tech, Action<GameObject> modify = null) {
			Databox box = databoxes.ContainsKey(tech) ? databoxes[tech] : null;
			if (box == null) {
				box = new Databox(tech, "1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6", modify); //compass databox
				databoxes[tech] = box;
				box.Patch();
			}
			return box;
		}
		
		public static ContainerPrefab getOrCreateFragment(Spawnable tech, string template, Action<GameObject> modify = null) {
			return getOrCreateFragment(tech.TechType, tech.FriendlyName, template, modify);
		}
		
		public static ContainerPrefab getOrCreateFragment(TechType tech, string name, string template, Action<GameObject> modify = null) {
			FragmentGroup li = fragments.ContainsKey(tech) ? fragments[tech] : null;
			if (li == null) {
				li = new FragmentGroup();
				fragments[tech] = li;
			}
			Fragment f = li.variants.ContainsKey(template) ? li.variants[template] : null;
			if (f == null) {
				f = li.addVariant(tech, name, template, modify);
			}
			return f;
		}
		
		public static ContainerPrefab getFragment(TechType tech, int idx) {
			FragmentGroup li = fragments.ContainsKey(tech) ? fragments[tech] : null;
			if (li == null || li.variantList.Count == 0)
				return null;
			return li.variantList[idx];
		}
		
		public abstract class CustomPrefabImpl : Spawnable, DIPrefab<StringPrefabContainer> {
			
			public float glowIntensity {get; set;}		
			public StringPrefabContainer baseTemplate {get; set;}
			
			private readonly Assembly ownerMod;
	        
	        public CustomPrefabImpl(string name, string template, string display = "") : base(name, display, "") {
				baseTemplate = new StringPrefabContainer(template);
				
				ownerMod = SNUtil.tryGetModDLL();
	        }
			
	        public override sealed GameObject GetGameObject() {
				return ObjectUtil.getModPrefabBaseObject<StringPrefabContainer>(this);
	        }
			
			public void setDisplayName(string s) {
				FriendlyName = s;
			}
			
			public virtual bool isResource() {
				return false;
			}
			
			public virtual string getTextureFolder() {
				return null;
			}
		
			public Atlas.Sprite getIcon() {
				return null;
			}
		
			public Assembly getOwnerMod() {
				return ownerMod;
			}
			
			public abstract void prepareGameObject(GameObject go, Renderer[] r);
			
		}
		
		public abstract class ContainerPrefab : CustomPrefabImpl {
	        
			public readonly TechType containedTech;
			
			private readonly Action<GameObject> modify;
	        
			internal ContainerPrefab(TechType tech, string template, Action<GameObject> m, string pre = "container", string suff = "", string disp = "") : base(pre+"_"+tech+suff, template, disp) {
				if (tech == TechType.None)
					throw new Exception("TechType for worldgen container "+GetType()+" was null!");
				containedTech = tech;
				modify = m;
	        }
			
			internal void modifyObject(GameObject go) {
				if (modify != null)
					modify(go);
			}
			
		}
		
		class Databox : ContainerPrefab, Story.IStoryGoalListener {
	        
	        internal Databox(TechType tech, string template, Action<GameObject> modify) : base(tech, template, modify) {
				
	        }
			
			public override void prepareGameObject(GameObject go, Renderer[] r) {
				Story.StoryGoalManager.main.AddListener(this);
	            BlueprintHandTarget bpt = go.EnsureComponent<BlueprintHandTarget>();
	            bpt.unlockTechType = containedTech;
	            bpt.primaryTooltip = containedTech.AsString();
				string arg = Language.main.Get(containedTech);
				string arg2 = Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(containedTech));
				bpt.secondaryTooltip = Language.main.GetFormat<string, string>("DataboxToolipFormat", arg, arg2);
				bpt.alreadyUnlockedTooltip = Language.main.GetFormat<string, string>("DataboxAlreadyUnlockedToolipFormat", arg, arg2);
				//redundant with the goal//bpt.useSound = SNUtil.getSound("event:/tools/scanner/new_blueprint");
				bpt.onUseGoal = new Story.StoryGoal(bpt.primaryTooltip, Story.GoalType.Encyclopedia, 0);
				
				modifyObject(go);
			}
			
			public void NotifyGoalComplete(string key) {
				if (key == containedTech.AsString()) {
					SNUtil.triggerTechPopup(containedTech);
					TechnologyUnlockSystem.instance.triggerDirectUnlock(containedTech, false);
				}
			}
			
		}
		
		sealed class Fragment : ContainerPrefab {
			
			internal Fragment(TechType tech, string name, string template, Action<GameObject> modify, int index) : base(tech, template, modify, "fragment", "_"+index, name+" Fragment") {
				
			}
			
			public override void prepareGameObject(GameObject go, Renderer[] r) {/*
	            TechFragment bpt = go.EnsureComponent<TechFragment>();
	            bpt.defaultTechType = containedTech;
	            bpt.techList.Clear();
	            bpt.techList.Add(new TechFragment.RandomTech{techType = containedTech, chance = 100});*/
				TechType tt = fragments[containedTech].sharedTechType; // NOT our techtype since needs to be shared!
				go.EnsureComponent<TechTag>().type = tt;
				Pickupable p = go.EnsureComponent<Pickupable>();
				p.overrideTechType = tt;
				ResourceTracker rt = go.EnsureComponent<ResourceTracker>();
				rt.techType = TechType.Fragment;
				rt.overrideTechType = TechType.Fragment;
				rt.prefabIdentifier = go.GetComponent<PrefabIdentifier>();
				rt.pickupable = p;
				p.isPickupable = false;
				modifyObject(go);
			}

			protected override void ProcessPrefab(GameObject go) {
				base.ProcessPrefab(go);
				TechType tt = fragments[containedTech].sharedTechType; // NOT our techtype since needs to be shared!
				go.EnsureComponent<TechTag>().type = tt;
			}
			
		}
		
		class FragmentGroup {
			
			internal readonly Dictionary<string, Fragment> variants = new Dictionary<string, Fragment>();
			internal readonly List<Fragment> variantList = new List<Fragment>();
			
			internal TechType sharedTechType = TechType.None;
			
			public FragmentGroup() {
				
			}
			
			internal Fragment addVariant(TechType tech, string name, string template, Action<GameObject> modify) {
				Fragment f = new Fragment(tech, name, template, modify, variantList.Count);
				f.Patch();
				variants[template] = f;
				variantList.Add(f);
				if (sharedTechType == TechType.None)
					sharedTechType = f.TechType;
				return f;
			}
			
		}
		
		class Crate : ContainerPrefab {
			
			private readonly bool needsCutter;
	        
	        internal Crate(TechType tech, string template, bool c, Action<GameObject> modify) : base(tech, template, modify) {
				needsCutter = c;
	        }
			
			public override void prepareGameObject(GameObject go, Renderer[] r) {
				PrefabPlaceholdersGroup pre = go.EnsureComponent<PrefabPlaceholdersGroup>();
				if (pre.prefabPlaceholders.Length != 1) {
					GameObject pp = null;
					foreach (Transform t in go.transform) {
						if (t.gameObject && t.gameObject != go && t.gameObject.name != "Crate_treasure_chest") {
							pp = t.gameObject;
							break;
						}
					}
					if (pp == null) {
						pp = new GameObject();
						pp.name = containedTech.AsString();
						pp.transform.parent = go.transform;
					}
					pre.prefabPlaceholders = new PrefabPlaceholder[1];
					pre.prefabPlaceholders[0] = pp.AddComponent<PrefabPlaceholder>();
				}
				string id = CraftData.GetClassIdForTechType(containedTech);
				if (string.IsNullOrEmpty(id)) {
					SNUtil.writeToChat("Could not find class id for techtype for crate: "+containedTech);
					id = CraftData.GetClassIdForTechType(TechType.Titanium);
				}
				pre.prefabPlaceholders[0].prefabClassId = id;
				pre.prefabPlaceholders[0].highPriority = true;
				pre.prefabPlaceholders[0].name = containedTech.AsString();
				if (needsCutter) {
					go.EnsureComponent<Sealed>()._sealed = true;
				}
				
				SupplyCrate sp = go.EnsureComponent<SupplyCrate>();
				go.EnsureComponent<CustomCrate>();
				modifyObject(go);
			}
			
		}
		
		internal class EmptyCrate : CustomPrefabImpl {
	        
	        internal EmptyCrate() : base("EmptyCrate", "580154dd-b2a3-4da1-be14-9a22e20385c8") {
				
	        }
			
			public override void prepareGameObject(GameObject go, Renderer[] r) {
				go.GetComponentInChildren<Animation>().Play(go.GetComponent<SupplyCrate>().snapOpenOnLoad);
				ObjectUtil.removeComponent<PrefabPlaceholdersGroup>(go);
				ObjectUtil.removeComponent<CustomCrate>(go);
				ObjectUtil.removeComponent<SupplyCrate>(go);
				ObjectUtil.removeComponent<PrefabPlaceholder>(go);
				go.EnsureComponent<EmptyCrateTag>();
			}
			
		}
		
		class EmptyCrateTag : MonoBehaviour {
			
			private Animation animator;
			
			void Update() {
				if (!animator)
					animator = GetComponentInChildren<Animation>();
				
				if (animator)
					animator.Play("crate_treasure_chest_open_static");
			}
		}
		
		internal class CustomCrate : MonoBehaviour {
			
			private PrefabPlaceholder reference;
			private SupplyCrate crate;
			
			private Pickupable item;
			private bool spawnedItem = false;
			
			void Update() {
				if (!reference)
					reference = GetComponentInChildren<PrefabPlaceholder>();
				if (!crate)
					crate = GetComponent<SupplyCrate>();
				if (reference && !item && !spawnedItem) {
					GameObject go = ObjectUtil.createWorldObject(reference.prefabClassId, true, false);
					go.transform.parent = transform;
					go.SetActive(true);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.GetComponent<Rigidbody>().isKinematic = true;
					item = go.GetComponent<Pickupable>();
					spawnedItem = true;
				}
				cleanDuplicateInternalItems();
				if (reference && crate && crate.open) {
					foreach (PrefabIdentifier pi in GetComponentsInChildren<PrefabIdentifier>()) {
						if (pi && pi.classId == reference.prefabClassId) {
							pi.gameObject.SetActive(true);
							pi.transform.localPosition = Vector3.up*0.22F;
						}
					}
				}
			}
			
			internal void onPickup(Pickupable pp) {
				if (pp == item) {/*
					UnityEngine.Object.DestroyImmediate(reference);
					UnityEngine.Object.DestroyImmediate(GetComponent<PrefabPlaceholdersGroup>());
					UnityEngine.Object.DestroyImmediate(crate);
					*/
					GameObject put = ObjectUtil.createWorldObject("EmptyCrate");
					put.transform.position = transform.position;
					put.transform.rotation = transform.rotation;
					put.transform.localScale = transform.localScale;
					put.GetComponentInChildren<Animation>().Play(crate.snapOpenOnLoad);
					UnityEngine.Object.Destroy(gameObject, 1.5F);
				}
			}
			
			private void cleanDuplicateInternalItems() {
				if (reference && !string.IsNullOrEmpty(reference.prefabClassId)) {
					bool found = false;
					foreach (PrefabIdentifier pi in gameObject.GetComponentsInChildren<PrefabIdentifier>()) {
						if (pi && pi.classId == reference.prefabClassId) {
							if (found)
								UnityEngine.Object.DestroyImmediate(pi.gameObject);
							else
								found = true;
						}
					}
				}
			}
			
		}
		/*
		internal class OpenWorldgenSeabaseDoor : Spawnable {
	        
			internal OpenWorldgenSeabaseDoor() : base("OpenWorldgenSeabaseDoor", "", "") {
				
	        }
			
			public override GameObject GetGameObject() {
				GameObject go = ObjectUtil.getBasePiece(Base.Piece.CorridorBulkhead);
				go.GetComponentInChildren<BulkheadDoor>().gameObject.EnsureComponent<OpenBaseDoorTag>();
				return go;
			}
			
			internal static void lockOpen(BulkheadDoor door) {
				GameObject put = ObjectUtil.createWorldObject("OpenWorldgenSeabaseDoor");
				GameObject obj = door.gameObject.FindAncestor<SeabaseReconstruction.WorldgenBulkhead>().gameObject;
				put.transform.position = obj.transform.position;
				put.transform.rotation = obj.transform.rotation;
				put.transform.localScale = obj.transform.localScale;
				UnityEngine.Object.DestroyImmediate(obj);
				//SNUtil.writeToChat("Created replacement door");
			}
			
		}
		
		class OpenBaseDoorTag : MonoBehaviour {
			
			private BulkheadDoor door;
			
			void Update() {
				if (!door)
					door = GetComponent<BulkheadDoor>();
				
				if (door && !door.isOpen) {
					door.targetState = true;
					door.SetClips();
					door.ResetAnimations();
					door.animState = door.SetAnimationState(door.doorClipName);
					door.animState.normalizedTime = 1f;
					door.doorAnimation.Sample();
					door.doorClipName = null;
					door.viewClipName = null;
					door.sound = null;
					door.NotifyStateChange();
					UnityEngine.Object.DestroyImmediate(door);
				}
			}
		}*/
		
	}
}
