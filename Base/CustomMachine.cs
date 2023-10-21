using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public abstract class CustomMachine<M> : Buildable, DIPrefab<CustomMachine<M>, StringPrefabContainer> where M : CustomMachineLogic {
		
		private static readonly MachineSaveHandler saveHandler = new MachineSaveHandler();
		
		private readonly List<PlannedIngredient> recipe = new List<PlannedIngredient>();
		
		public readonly string id;
		
		private readonly Assembly ownerMod;
		
		private PDAManager.PDAPage page;
		
		public float glowIntensity {get; set;}		
		public StringPrefabContainer baseTemplate {get; set;}
		
		protected CustomMachine(string id, string name, string desc, string template) : base(id, name, desc) {
			ownerMod = SNUtil.tryGetModDLL();
			typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
			this.id = id;
			baseTemplate = new StringPrefabContainer(template);
			
			OnFinishedPatching += () => {
				DIMod.machineList[TechType] = this;
				SaveSystem.addSaveHandler(ClassID, saveHandler);
				if (page != null)
					TechnologyUnlockSystem.instance.registerPage(TechType, page);
			};
		}
		
		public CustomMachine<M> addIngredient(ItemDef item, int amt) {
			return addIngredient(item.getTechType(), amt);
		}
		
		public CustomMachine<M> addIngredient(ModPrefab item, int amt) {
			return addIngredient(new ModPrefabTechReference(item), amt);
		}
		
		public CustomMachine<M> addIngredient(TechType item, int amt) {
			return addIngredient(new TechTypeContainer(item), amt);
		}
		
		public CustomMachine<M> addIngredient(TechTypeReference item, int amt) {
			recipe.Add(new PlannedIngredient(item, amt));
			return this;
		}

		public override TechGroup GroupForPDA {
			get {
				return isOutdoors() ? TechGroup.ExteriorModules : TechGroup.InteriorModules;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return isOutdoors() ? TechCategory.ExteriorModule : TechCategory.InteriorModule;
			}
		}
		
		public virtual bool isOutdoors() {
			return false;
		}
		
		protected void initializeStorageContainer(StorageContainer con, int w, int h, string label = null) {
			con.storageRoot.ClassId = ClassID.ToLowerInvariant()+"container";
			if (string.IsNullOrEmpty(label))
				label = FriendlyName;
			con.hoverText = "Use "+label;
			con.storageLabel = label.ToUpperInvariant();
			con.container.containerType = ItemsContainerType.Default;
			con.enabled = true;
			con.Resize(w, h);
		}
		
		public void addFragments(int needed, float scanTime = 5, params TechnologyFragment[] fragments) {
			SNUtil.log("Creating "+fragments.Length+" fragments for "+this+" from "+fragments.toDebugString(), ownerMod);
			foreach (TechnologyFragment m in fragments) {
				m.target = TechType;
				m.fragmentPrefab = GenUtil.getOrCreateFragment(this, m.template, m.objectModify);
				SNUtil.log("Registered fragment "+m.fragmentPrefab.ClassID, ownerMod);
			}
			SNUtil.addPDAEntry(fragments[0].fragmentPrefab, scanTime, null, null, null, e => {
				e.blueprint = TechType;
				e.destroyAfterScan = shouldDeleteFragments();
				e.isFragment = true;
				e.totalFragments = needed;
				e.key = GenUtil.getFragment(TechType, 0).TechType;
				if (page != null)
					e.encyclopedia = page.id;
			});
		}
		
		public void addPDAPage(string text, string pageHeader = null) {
			page = PDAManager.createPage("ency_"+ClassID, FriendlyName, text, isPowerGenerator() ? "Tech/Power" : "Tech/Habitats");
			if (pageHeader != null)
				page.setHeaderImage(TextureManager.getTexture(SNUtil.tryGetModDLL(), "Textures/PDA/"+pageHeader));
			page.register();
			if (IsPatched)
				TechnologyUnlockSystem.instance.registerPage(TechType, page);
		}
		
		public PDAManager.PDAPage getPDAPage() {
			return page;
		}
		
		protected virtual bool isPowerGenerator() {
			return false;
		}
		
		protected virtual bool shouldDeleteFragments() {
			return true;
		}
		
		//protected abstract OrientedBounds[] GetBounds { get; }
			
		public sealed override GameObject GetGameObject() {
			GameObject world = ObjectUtil.getModPrefabBaseObject(this);
			M lgc = world.EnsureComponent<M>();
			lgc.prefab = this;
			float capacity = lgc.getBaseEnergyStorageCapacityBonus();
			if (capacity > 0) {
				PowerSource src = world.EnsureComponent<PowerSource>();
				src.power = 0;
				src.maxPower = capacity;
			}
			
			Constructable ctr = world.EnsureComponent<Constructable>();
			ctr.techType = TechType;
			ctr.allowedInBase = !isOutdoors();
			ctr.allowedInSub = !isOutdoors();
			ctr.allowedOnGround = true;
			ctr.allowedOutside = isOutdoors();
			ctr.allowedOnCeiling = false;
			ctr.allowedOnWall = false;
			ctr.rotationEnabled = true;
			ctr.surfaceType = VFXSurfaceTypes.metal;
			ctr.forceUpright = true;
			ctr.allowedOnConstructables = false;
			LargeWorldEntity lw = world.EnsureComponent<LargeWorldEntity>();
			lw.cellLevel = LargeWorldEntity.CellLevel.Medium;
			initializeMachine(world);
			world.SetActive(true);
			return world;
		}
		
		public virtual bool isResource() {
			return false;
		}
		
		public virtual string getTextureFolder() {
			return "Machines";
		}
		
		public Atlas.Sprite getIcon() {
			return GetItemSprite();
		}
		
		public Assembly getOwnerMod() {
			return ownerMod;
		}
		
		public void prepareGameObject(GameObject go, Renderer[] r) {
			
		}
		
		public virtual void initializeMachine(GameObject go) {
			
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
		protected override sealed TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = RecipeUtil.buildRecipeList(recipe),
				craftAmount = 1
			};
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/"+ObjectUtil.formatFileName(this));
		}
	}
	
	internal class MachineSaveHandler : SaveSystem.SaveHandler {
		
		public override void save(PrefabIdentifier pi) {
			CustomMachineLogic lgc = pi.GetComponentInChildren<CustomMachineLogic>();
			if (lgc)
				lgc.save(data);
		}
		
		public override void load(PrefabIdentifier pi) {
			CustomMachineLogic lgc = pi.GetComponentInChildren<CustomMachineLogic>();
			if (lgc)
				lgc.load(data);
		}
	}
	
	public abstract class DiscreteOperationalMachineLogic : CustomMachineLogic {
				
		public abstract bool isWorking();
		
		public abstract float getProgressScalar();
		
	}
		
	public abstract class CustomMachineLogic : MonoBehaviour {
		
		internal Buildable prefab;
		internal Constructable buildable;
		private SubRoot sub;		
		private StorageContainer storage;
		
		private float lastUpdateTime = -1;
		private float lastDayTime = -1;
		
		private float lastReceived;
		
		private float spawnTime = -1;
		
		void Start() {
			setupSky();
		}
		
		protected virtual float getTickRate() {
			return 0;
		}
		
		public virtual float getBaseEnergyStorageCapacityBonus() {
			return 0;
		}
		
		protected internal virtual void load(XmlElement data) {
			//spawnTime = (float)data.getFloat("spawnTime", float.NaN);
		}
		
		protected internal virtual void save(XmlElement data) {
			//data.addProperty("spawnTime", spawnTime);
		}
		
		private void setupSky() {
			if (prefab == null || !WaterBiomeManager.main || !MarmoSkies.main)
				return;
			mset.Sky baseSky = prefab.CategoryForPDA == TechCategory.ExteriorModule ? WaterBiomeManager.main.GetBiomeEnvironment(transform.position) : MarmoSkies.main.skyBaseInterior;
			if (!baseSky)
				return;
			SkyApplier[] skies = gameObject.GetComponentsInChildren<SkyApplier>(true);
			foreach (SkyApplier sk in skies) {
				if (!sk)
					continue;
				sk.renderers = gameObject.GetComponentsInChildren<Renderer>();
				ObjectUtil.setSky(gameObject, baseSky);
			}
		}
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (prefab == null)
				tryGetPrefab();
			if (!buildable)
				buildable = GetComponent<Constructable>();
			if (spawnTime <= 0)
				spawnTime = time;
			float delta = time-lastUpdateTime;
			if (delta > 0 && delta >= getTickRate()) {
				updateEntity(delta);
				lastUpdateTime = time;
			}
			if (time-lastDayTime >= 5)
				setupSky();
			lastDayTime = time;
			if (!storage) {
				storage = gameObject.GetComponentInChildren<StorageContainer>();
				if (storage)
					initStorage(storage);
			}
			Transform par = transform.parent;
			if (!par || !par.GetComponent<SubRoot>()) {
				findClosestSub();
			}
			if (!sub) {
				sub = gameObject.GetComponentInParent<SubRoot>();
				if (!sub) {
					findClosestSub();
				}
			}
		}
		
		private void tryGetPrefab() {
			TechType tt = CraftData.GetTechType(gameObject);
			if (tt != TechType.None && DIMod.machineList.ContainsKey(tt)) {
				prefab = DIMod.machineList[tt];
			}
		}
		
		protected float getAge() {
			return DayNightCycle.main.timePassedAsFloat-spawnTime;
		}
		
		protected SubRoot getSub() {
			return sub;
		}
		
		protected StorageContainer getStorage() {
			return storage;
		}
		
		protected Constructable getBuildable() {
			return buildable;
		}
		
		protected virtual void initStorage(StorageContainer sc) {
			
		}
		
		protected int addItemToInventory(TechType tt, int amt = 1) {
			StorageContainer sc = getStorage();
			if (!sc)
				return 0;
			int add = 0;
			for (int i = 0; i < amt; i++) {
				GameObject item = ObjectUtil.createWorldObject(CraftData.GetClassIdForTechType(tt), true, false);
				SNUtil.log("Adding "+item+" to "+GetType().Name+" inventory");
				item.SetActive(false);
				if (sc.container.AddItem(item.GetComponent<Pickupable>()) != null)
					add++;
			}
			return add;
		}
		
		protected bool consumePower(float amt) {
			//SNUtil.writeToChat("Wanted "+amt+" from "+sub);
			if (!buildable || !buildable.constructed)
				return false;
			if (!sub)
				return false;
			if (!GameModeUtils.RequiresPower())
				return true;
			//SNUtil.writeToChat(sc+" > "+amt);
			if (amt > 0) {
				float trash;
				sub.powerRelay.ConsumeEnergy(amt, out lastReceived);
				//SNUtil.writeToChat("Got "+lastReceived);
				if (amt-lastReceived > 0.001) {
					//SNUtil.log("Refunding "+lastReceived+" power which was less than requested "+amt);
					sub.powerRelay.AddEnergy(lastReceived, out trash); //refund
				}
				else {
					return true;
				}
			}
			return false;
		}
		
		private void findClosestSub() {
			SNUtil.log("Custom machine "+this+" @ "+transform.position+" did not have proper parent component hierarchy: "+transform.parent, SNUtil.diDLL);
			foreach (SubRoot s in UnityEngine.Object.FindObjectsOfType<SubRoot>()) {
				if (s.isCyclops || !s.isBase)
					continue;
				float dist = Vector3.Distance(s.transform.position, transform.position);
				if (dist > 350)
					continue;
				if (!sub || dist < Vector3.Distance(sub.transform.position, transform.position)) {
					sub = s;
				}
			}
			if (sub) {
				transform.parent = sub.transform;
				SNUtil.log("Custom machine "+this+" @ "+transform.position+" parented to sub: "+sub, SNUtil.diDLL);
			}
			
			foreach (SkyApplier sky in gameObject.GetComponents<SkyApplier>()) {
				sky.renderers = gameObject.GetComponentsInChildren<Renderer>();
				sky.enabled = true;
				sky.RefreshDirtySky();
				sky.ApplySkybox();
			}
		}
		
		protected abstract void updateEntity(float seconds);
		
	}
}
