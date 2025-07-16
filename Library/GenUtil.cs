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
		
		private static readonly Dictionary<LargeWorldEntity.CellLevel, WorldGeneratorPrefab> worldGeneratorPrefabs = new Dictionary<LargeWorldEntity.CellLevel, WorldGeneratorPrefab>();
		
		private static readonly Dictionary<string, WorldGenerator> generatorTable = new Dictionary<string, WorldGenerator>();
		
		static GenUtil() {	
			crates[0] = new Dictionary<TechType, Crate>();
			crates[1] = new Dictionary<TechType, Crate>();
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
			if (string.IsNullOrEmpty(prefab))
				throw new Exception("Tried to register worldgen of null!");
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
				string id = gen.uniqueID;
				SNUtil.log("Placing world generator "+gen+" ["+id+"]");
				generatorTable[id] = gen;
				go.EnsureComponent<WorldGeneratorHolder>().generatorID = id;
			};
			SpawnInfo info = new SpawnInfo(getOrCreateWorldgenHolder(gen).ClassID, gen.position, call);
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			SNUtil.log("Queuing world generator "+gen);
			return info;
		}
		
		private static WorldGeneratorPrefab getOrCreateWorldgenHolder(WorldGenerator gen) {
			LargeWorldEntity.CellLevel lvl = gen.getCellLevel();
			if (!worldGeneratorPrefabs.ContainsKey(lvl)) {
				worldGeneratorPrefabs[lvl] = new WorldGeneratorPrefab(lvl);
				worldGeneratorPrefabs[lvl].Patch();
			}
			return worldGeneratorPrefabs[lvl];
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
		
		public static Spawnable getOrCreateCrate(TechType tech, bool needsCutter = false, string goal = null) {
			int idx = needsCutter ? 1 : 0;
			Crate box = crates[idx].ContainsKey(tech) ? crates[idx][tech] : null;
			if (box == null) {
				box = new Crate(tech, needsCutter, goal);
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
			
		public static bool fireGenerator(WorldGenerator gen, List<GameObject> generatedObjects) {
			WorldgenLog.log("Running world generator " + gen);
			if (gen.generate(generatedObjects)) {
				WorldgenLog.log("Generator " + gen + " complete. Generation list (" + generatedObjects.Count + "):");
				foreach (GameObject go in generatedObjects)
					WorldgenLog.log(go);
				if (generatedObjects.Count == 0) {
					string msg = "Warning: Nothing generated!";
					WorldgenLog.log(msg);
				}
				return true;
			}
			else {
				SNUtil.log("Generator " + gen + " failed, trying again in one second", SNUtil.diDLL);
				return false;
			}
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
		
		class Crate : Spawnable {
			
			private readonly bool needsCutter;
			private readonly string storyGoal;
			private readonly TechType containedItem;
	        
			internal Crate(TechType tech, bool c = false, string goal = null) : base("Crate_"+tech.AsString()+"_"+c, "Supply Crate", "") {
				containedItem = tech;
				needsCutter = c;
				storyGoal = goal;
				
				//SNUtil.log("Creating Crate_"+tech.AsString()+"_"+c);
				//SNUtil.log(new System.Diagnostics.StackTrace().ToString());
				
				OnFinishedPatching += () => {
					SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<CrateManagement>().addField("isOpened").addField("itemGrabbed"));
				};
	        }
			
			public override GameObject GetGameObject() {
				GameObject pfb = ObjectUtil.lookupPrefab("580154dd-b2a3-4da1-be14-9a22e20385c8");
				GameObject go = new GameObject(ClassID+"(Clone)");
				go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				go.EnsureComponent<TechTag>().type = TechType;
				go.EnsureComponent<LargeWorldEntity>().cellLevel = pfb.GetComponent<LargeWorldEntity>().cellLevel;
				Animation a = pfb.GetComponentInChildren<Animation>();
				GameObject mdl = UnityEngine.Object.Instantiate(a.gameObject);
				mdl.transform.SetParent(go.transform);
				mdl.transform.localRotation = Quaternion.identity;
				mdl.transform.localPosition = new Vector3(0, 0.36F, 0.02F);
				mdl.transform.localScale = Vector3.one*5.5F;
				CrateManagement mgr = go.EnsureComponent<CrateManagement>();
				mgr.itemToSpawn = containedItem;
				mgr.collectionGoal = storyGoal;
				
				if (needsCutter) {
					go.EnsureComponent<Sealed>()._sealed = true;
				}
				return go;
			}
			
		}
		/*
		internal class EmptyCrate : Spawnable {
	        
	        internal EmptyCrate() : base("EmptyCrate", "Empty Crate", "") {
				
	        }
			
			public override GameObject GetGameObject() {
				GameObject pfb = ObjectUtil.lookupPrefab("580154dd-b2a3-4da1-be14-9a22e20385c8");
				GameObject go = new GameObject("EmptyCrate(Clone)");
				go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				go.EnsureComponent<TechTag>().type = TechType;
				go.EnsureComponent<LargeWorldEntity>().cellLevel = pfb.GetComponent<LargeWorldEntity>().cellLevel;
				Animation a = pfb.GetComponentInChildren<Animation>();
				GameObject mdl = UnityEngine.Object.Instantiate(a.gameObject);
				mdl.transform.SetParent(go.transform);
				mdl.transform.localRotation = Quaternion.identity;
				mdl.transform.localPosition = new Vector3(0, 0.36F, 0.02F);
				mdl.transform.localScale = Vector3.one*5.5F;
				CrateManagement mgr = go.EnsureComponent<CrateManagement>();
				mgr.autoOpen = true;
				return go;
			}
			
		}
		*/
		internal class CrateManagement : MonoBehaviour, IHandTarget {
			
			public bool isOpened;
			public bool itemGrabbed;
			public TechType itemToSpawn;
			
			private string openAnimation = "Open_SupplyCrate";
			private string openText = "Open_SupplyCrate";
			private string snapOpenAnimation;
			private FMODAsset openSound;
			
			public string collectionGoal;
			
			private Sealed laserSeal;
			
			private Pickupable itemInside;
			
			void Start() {
				laserSeal = GetComponent<Sealed>();
				SupplyCrate sc = ObjectUtil.lookupPrefab("580154dd-b2a3-4da1-be14-9a22e20385c8").GetComponent<SupplyCrate>();
				openText = sc.openText;
				openSound = sc.openSound;
				openAnimation = sc.openClipName;
				snapOpenAnimation = sc.snapOpenOnLoad;
				Invoke("delayedStart", 0.5F);
			}
			
			void delayedStart() {
				if (isOpened && !GetComponentInChildren<Animation>().Play(snapOpenAnimation)) {
					Invoke("delayedStart", 0.5F);
					return;
				}
				if (itemToSpawn != TechType.None) {
					cacheItem();
					if (!itemInside && !itemGrabbed && (collectionGoal == null || !Story.StoryGoalManager.main.IsGoalComplete(collectionGoal))) {
						itemInside = ObjectUtil.createWorldObject(itemToSpawn).GetComponent<Pickupable>();
						SNUtil.log("Filling crate @ "+transform.position+" with "+itemInside);
						itemInside.transform.SetParent(transform);
						itemInside.transform.localPosition = new Vector3(0, 0.33F, 0);
						itemInside.transform.localRotation = Quaternion.identity;
						itemInside.transform.localScale = Vector3.one;
						itemInside.GetComponent<Rigidbody>().isKinematic = true;
					}
				}
			}
			
			void Update() {
				if (itemInside)
					itemInside.isPickupable = isOpened;
				if (string.IsNullOrEmpty(collectionGoal))
					collectionGoal = "Crate_"+transform.position.ToString("0.0").Trim();
			}

			private void cacheItem() {
				this.itemInside = GetComponentInChildren<Pickupable>();
			}

			public void OnHandHover(GUIHand h) {
				this.cacheItem();
				bool flag = false;
				if (!this.isOpened) {
					if (!this.laserSeal || !this.laserSeal.IsSealed()) {
						HandReticle.main.SetInteractText(this.openText);
					}
					else {
						HandReticle.main.SetInteractText("Sealed_SupplyCrate", "SealedInstructions");
					}
					flag = true;
				}
				else
				if (this.itemInside) {
					HandReticle.main.SetInteractText("TakeItem_SupplyCrate");
					flag = true;
				}
				if (flag) {
					HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
				}
			}

			public void OnHandClick(GUIHand h) {
				this.cacheItem();
				if (!this.laserSeal || !this.laserSeal.IsSealed()) {
					if (!this.isOpened) {
						this.isOpened = true;
						Utils.PlayFMODAsset(this.openSound, transform, 20f);
						Animation a = GetComponentInChildren<Animation>();
						if (a) {
							a.Play(openAnimation);
						}
						return;
					}
					if (this.itemInside) {
						Inventory.main.Pickup(this.itemInside, false);
						clear();
					}
				}
			}
			
			public void onPickup(Pickupable p) {
				if (p && p.GetTechType() == itemToSpawn) {
					clear();
				}
			}
			
			private void clear() {
				itemGrabbed = true;
				this.itemInside = null;
				if (collectionGoal != null)
					Story.StoryGoal.Execute(collectionGoal, Story.GoalType.Story);
			}
			
		}
		
		class WorldGeneratorPrefab : Spawnable {
			
			public readonly LargeWorldEntity.CellLevel cellLevel;
			
			internal WorldGeneratorPrefab(LargeWorldEntity.CellLevel lvl) : base("WorldGeneratorHolder_"+lvl.ToString(), "", "") {
				cellLevel = lvl;
			}
			
			public override GameObject GetGameObject() {
				GameObject go = new GameObject("WorldGeneratorHolder");
				go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				go.EnsureComponent<TechTag>().type = TechType;
				go.EnsureComponent<LargeWorldEntity>().cellLevel = cellLevel;
				go.EnsureComponent<WorldGeneratorHolder>();
				return go;
			}
			
		}
		
		class WorldGeneratorHolder : MonoBehaviour {
			
			internal string generatorID;
			
			private WorldGenerator generatorInstance;
			
			private readonly List<GameObject> generatedObjects = new List<GameObject>();
			
			internal bool generate() {
				generatorInstance = generatorTable.ContainsKey(generatorID) ? generatorTable[generatorID] : null;
				if (generatorInstance == null) {
					SNUtil.log("WorldGen holder '" + generatorID + "' @ " + transform.position + " had no generator!", SNUtil.diDLL);
					return false;
				}
				bool flag = GenUtil.fireGenerator(generatorInstance, generatedObjects);
				if (flag) {
					UnityEngine.Object.Destroy(gameObject);
				}
				return flag;
			}
			
			void Start() {
				Invoke("tryGenerate", 0);
			}
			
			void tryGenerate() {
				if (!generate())
					Invoke("tryGenerate", 1F);
			}
			
		}
		
	}
}
