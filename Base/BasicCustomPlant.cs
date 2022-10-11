using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public class BasicCustomPlant : Spawnable, DIPrefab<VanillaFlora> {
		
		public float glowIntensity {get; set;}		
		public VanillaFlora baseTemplate {get; set;}
		
		public readonly BasicCustomPlantSeed seed;
		
		public HarvestType collectionMethod = HarvestType.DamageAlive;
		public int finalCutBonus = 2;
		
		private readonly Assembly ownerMod;
		
		private static readonly Dictionary<TechType, BasicCustomPlant> plants = new Dictionary<TechType, BasicCustomPlant>();
		
		public BasicCustomPlant(XMLLocale.LocaleEntry e, VanillaFlora template, string seedPfb, string seedName = "Seed") : this(e.key, e.name, e.desc, template, seedPfb, seedName) {
			
		}
			
		public BasicCustomPlant(string id, string name, string desc, VanillaFlora template, string seedPfb, string seedName = "Seed") : base(id, name, desc) {
			baseTemplate = template;
			ownerMod = SNUtil.tryGetModDLL();
			seed = new BasicCustomPlantSeed(this, seedPfb, seedName);
			OnFinishedPatching += () => {
				plants[TechType] = this;
				if (collectionMethod != HarvestType.None) {
					seed.Patch();			
					ItemRegistry.instance.addItem(seed);
					plants[seed.TechType] = this;
	        		CraftData.harvestTypeList[TechType] = collectionMethod;
	        		CraftData.harvestOutputList[TechType] = seed.TechType;
	        		CraftData.harvestFinalCutBonusList[TechType] = finalCutBonus;
	        		SNUtil.log("Finished patching "+this+" > "+CraftData.GetHarvestOutputData(TechType), ownerMod);
				}
			};
		}
		
		public void addPDAEntry(string text, float scanTime = 2, string header = null) {
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.scanTime = scanTime;
			e.locked = true;
			PDAManager.PDAPage page = PDAManager.createPage(""+TechType, FriendlyName, text, "Lifeforms");
			page.addSubcategory("Flora").addSubcategory(collectionMethod == HarvestType.None ? "Sea" : "Exploitable");
			if (header != null)
				page.setHeaderImage(TextureManager.getTexture(ownerMod, "Textures/PDA/"+header));
			page.register();
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/"+ObjectUtil.formatFileName(this));
		}
		
		public Atlas.Sprite getSprite() {
			return GetItemSprite();
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer[] r) {

		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
			
		public sealed override GameObject GetGameObject() {
			GameObject go = ObjectUtil.getModPrefabBaseObject(this);
			Pickupable p = go.EnsureComponent<Pickupable>();
			p.isPickupable = false;
			return go;
		}
		
		public Assembly getOwnerMod() {
			return ownerMod;
		}
		
		public bool isResource() {
			return true;
		}
		
		public string getTextureFolder() {
			return "Plants";
		}
		
		public Atlas.Sprite getIcon() {
			return GetItemSprite();
		}
		
		public virtual Plantable.PlantSize getSize() {
			return Plantable.PlantSize.Large;
		}
		
		public virtual float getScaleInGrowbed(bool indoors) {
			return 1;
		}
		
		public virtual bool canGrowAboveWater() {
			return false;
		}
		
		public virtual bool canGrowUnderWater() {
			return true;
		}/*
		
		public virtual float getGrowthTime() {
			return 1200;
		}
		
		public virtual void prepareGrowingPlant(GrowingPlant g) {
			
		}*/
		
		public static BasicCustomPlant getPlant(TechType tt) {
			return plants.ContainsKey(tt) ? plants[tt] : null;
		}
		
	}
	
	public class BasicCustomPlantSeed : Spawnable, DIPrefab<StringPrefabContainer> {
		
		public float glowIntensity {get; set;}		
		public StringPrefabContainer baseTemplate {get; set;}
		
		public readonly BasicCustomPlant plant;
		
		public Atlas.Sprite sprite;
		
		public BasicCustomPlantSeed(BasicCustomPlant p, string pfb, string seedName = "Seed") : base(p.ClassID+"_seed", p.FriendlyName+" "+seedName, p.Description) {
			plant = p;
			sprite = plant.getSprite();
			baseTemplate = new StringPrefabContainer(pfb);
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return sprite;
		}
		
		public override Vector2int SizeInInventory {
			get {return plant.SizeInInventory;}
		}
		/*
		public GrowingPlant getPlant(GameObject go) {
			return go.GetComponent<Plantable>().model.GetComponent<GrowingPlant>();
		}*/
		
		public Atlas.Sprite getIcon() {
			return plant.getIcon();
		}
		
		public Assembly getOwnerMod() {
			return plant.getOwnerMod();
		}
			
		public sealed override GameObject GetGameObject() {
			GameObject go = ObjectUtil.getModPrefabBaseObject(this);
			Pickupable pp = go.EnsureComponent<Pickupable>();
			pp.isPickupable = true;
			
			Plantable p = go.EnsureComponent<Plantable>();
			p.aboveWater = plant.canGrowAboveWater();
			p.underwater = plant.canGrowUnderWater();
			p.isSeedling = true;
			p.plantTechType = plant.TechType;
			p.size = plant.getSize();			
			p.pickupable = pp;
			
			p.modelScale = Vector3.one*plant.getScaleInGrowbed(false);
			p.modelIndoorScale = Vector3.one*plant.getScaleInGrowbed(true);
			
			//GrowingPlant g = getPlant(go);
			//g.growthDuration = plant.getGrowthTime();
			//plant.prepareGrowingPlant(g);
			
			//ObjectUtil.convertTemplateObject(p.model, plant); //this is the GROWING but not grown one
			/*
			GrowingPlant grow = p.model.EnsureComponent<GrowingPlant>();
			grow.seed = p;
			grow.enabled = true;
			
			bool active = grow.grownModelPrefab.active;
			grow.grownModelPrefab = UnityEngine.Object.Instantiate(grow.grownModelPrefab);
			grow.grownModelPrefab.SetActive(active);
			ObjectUtil.convertTemplateObject(grow.grownModelPrefab, plant);
			grow.grownModelPrefab.SetActive(true); //FIXME does not work
			Renderer r = grow.grownModelPrefab.GetComponentInChildren<Renderer>();
			plant.prepareGameObject(grow.grownModelPrefab, r);
			grow.growingTransform = grow.grownModelPrefab.transform;
			grow.growingTransform.gameObject.SetActive(true);*/
			/*
			CapsuleCollider cu = plant.GetGameObject().GetComponentInChildren<CapsuleCollider>();
			if (cu != null) {
				CapsuleCollider cc = p.model.EnsureComponent<CapsuleCollider>();
				cc.radius = cu.radius*0.8F;
				cc.center = cu.center;
				cc.direction = cu.direction;
				cc.height = cu.height;
				cc.material = cu.material;
				cc.name = cu.name;
				cc.enabled = cu.enabled;
				cc.isTrigger = cu.isTrigger;
			}*/
			
			return go;
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer[] r) {
			
		}
		
		public bool isResource() {
			return true;
		}
		
		public string getTextureFolder() {
			return "Items";
		}
		
	}
}
