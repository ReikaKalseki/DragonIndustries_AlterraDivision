﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public class BasicCustomPlant : Spawnable, DIPrefab<FloraPrefabFetch>, Flora {

		public float glowIntensity { get; set; }
		public FloraPrefabFetch baseTemplate { get; set; }

		public readonly BasicCustomPlantSeed seed;

		public HarvestType collectionMethod = HarvestType.DamageAlive;
		public int finalCutBonus = 2;

		private readonly Assembly ownerMod;

		private readonly List<BiomeBase> nativeBiomesCave = new List<BiomeBase>();
		private readonly List<BiomeBase> nativeBiomesSurface = new List<BiomeBase>();

		private static readonly Dictionary<TechType, BasicCustomPlant> plants = new Dictionary<TechType, BasicCustomPlant>();
		private static readonly Dictionary<string, BasicCustomPlant> plantIDs = new Dictionary<string, BasicCustomPlant>();

		public PDAManager.PDAPage pdaPage { get; private set; }

		public BasicCustomPlant(XMLLocale.LocaleEntry e, FloraPrefabFetch template, string seedPfb, string seedName = "Seed") : this(e.key, e.name, e.desc, template, seedPfb, seedName) {

		}

		public BasicCustomPlant(string id, string name, string desc, FloraPrefabFetch template, string seedPfb, string seedName = "Seed") : base(id, name, desc) {
			baseTemplate = template;
			ownerMod = SNUtil.tryGetModDLL();
			typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
			seed = seedPfb == null ? null : new BasicCustomPlantSeed(this, seedPfb, seedName);
			OnFinishedPatching += () => {
				plants[TechType] = this;
				plantIDs[ClassID] = this;
				if (collectionMethod != HarvestType.None || this.generateSeed()) {
					seed.Patch();
					ItemRegistry.instance.addItem(seed);
					setPlantSeed(seed, this);
					CraftData.harvestTypeList[TechType] = collectionMethod;
					CraftData.harvestOutputList[TechType] = seed.TechType;
					CraftData.harvestFinalCutBonusList[TechType] = finalCutBonus;
					SNUtil.log("Finished patching " + this + " > " + CraftData.GetHarvestOutputData(TechType), ownerMod);
				}
			};
		}

		public static void setPlantSeed(ModPrefab seed, BasicCustomPlant plant) {
			plants[seed.TechType] = plant;
			plantIDs[seed.ClassID] = plant;
		}

		public BasicCustomPlant addNativeBiome(BiomeBase b, bool caveOnly = false) {
			nativeBiomesCave.Add(b);
			if (!caveOnly)
				nativeBiomesSurface.Add(b);
			return this;
		}

		public bool isNativeToBiome(Vector3 vec) {
			return this.isNativeToBiome(BiomeBase.getBiome(vec), WorldUtil.isInCave(vec));
		}

		public bool isNativeToBiome(BiomeBase b, bool cave) {
			return (cave ? nativeBiomesCave : nativeBiomesSurface).Contains(b);
		}

		public string getPrefabID() {
			return ClassID;
		}

		public void addPDAEntry(string text, float scanTime = 2, string header = null) {
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.scanTime = scanTime;
			e.locked = true;
			pdaPage = PDAManager.createPage("ency_" + ClassID, FriendlyName, text, "Lifeforms");
			pdaPage.addSubcategory("Flora").addSubcategory(this.isExploitable() ? "Exploitable" : "Sea");
			if (header != null)
				pdaPage.setHeaderImage(TextureManager.getTexture(ownerMod, "Textures/PDA/" + header));
			pdaPage.register();
			e.encyclopedia = pdaPage.id;
			PDAHandler.AddCustomScannerEntry(e);
		}

		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
		}

		public Atlas.Sprite getSprite() {
			return this.GetItemSprite();
		}

		public virtual void prepareGameObject(GameObject go, Renderer[] r) {

		}

		public sealed override string ToString() {
			return base.ToString() + " [" + TechType + "] / " + ClassID + " / " + PrefabFileName + " S=" + seed;
		}

		public sealed override GameObject GetGameObject() {
			GameObject go = ObjectUtil.getModPrefabBaseObject(this);
			Pickupable p = go.EnsureComponent<Pickupable>();
			p.isPickupable = false;
			go.EnsureComponent<ImmuneToPropulsioncannon>();
			return go;
		}

		public Assembly getOwnerMod() {
			return ownerMod;
		}

		protected virtual bool isExploitable() {
			return collectionMethod != HarvestType.None || this.isResource();
		}

		protected virtual bool generateSeed() {
			return collectionMethod != HarvestType.None;
		}

		public virtual bool isResource() {
			return true;
		}

		public virtual string getTextureFolder() {
			return "Plants";
		}

		public Atlas.Sprite getIcon() {
			return this.GetItemSprite();
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

		public virtual void modifySeed(GameObject go) {

		}

		public static BasicCustomPlant getPlant(TechType tt) {
			return plants.ContainsKey(tt) ? plants[tt] : null;
		}

		public static BasicCustomPlant getPlant(string id) {
			return plantIDs.ContainsKey(id) ? plantIDs[id] : null;
		}

	}

	public class FloraPrefabFetch : PrefabReference {

		private string prefab;
		private VanillaFlora flora;

		public FloraPrefabFetch(string pfb) {
			prefab = pfb;
		}

		public FloraPrefabFetch(VanillaFlora f) {
			flora = f;
		}

		public string getPrefabID() {
			return flora == null ? prefab : flora.getRandomPrefab(false);
		}
	}

	public class BasicCustomPlantSeed : Spawnable, DIPrefab<StringPrefabContainer> {

		public float glowIntensity { get; set; }
		public StringPrefabContainer baseTemplate { get; set; }

		public readonly BasicCustomPlant plant;

		public Atlas.Sprite sprite;

		public BasicCustomPlantSeed(BasicCustomPlant p, string pfb, string seedName = "Seed") : base(p.ClassID + "_seed", p.FriendlyName + " " + seedName, p.Description) {
			plant = p;
			typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, p.getOwnerMod());
			sprite = plant.getSprite();
			baseTemplate = new StringPrefabContainer(pfb);
		}

		protected sealed override Atlas.Sprite GetItemSprite() {
			return sprite;
		}

		public override Vector2int SizeInInventory {
			get { return plant.SizeInInventory; }
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

			p.modelScale = Vector3.one * plant.getScaleInGrowbed(false);
			p.modelIndoorScale = Vector3.one * plant.getScaleInGrowbed(true);

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

			plant.modifySeed(go);

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

		public sealed override string ToString() {
			return base.ToString() + " [" + TechType + "] / " + ClassID + " / " + PrefabFileName;
		}

	}
}
