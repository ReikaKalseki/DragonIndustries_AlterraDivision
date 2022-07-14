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
		
		public BasicCustomPlant(XMLLocale.LocaleEntry e, VanillaFlora template) : this(e.key, e.name, e.desc, template) {
			
		}
			
		public BasicCustomPlant(string id, string name, string desc, VanillaFlora template) : base(id, name, desc) {
			baseTemplate = template;
			seed = new BasicCustomPlantSeed(this);
			OnFinishedPatching += () => {
				if (collectionMethod != HarvestType.None) {
					seed.Patch();
	        		CraftData.harvestTypeList[TechType] = collectionMethod;
	        		CraftData.harvestOutputList[TechType] = seed.TechType;
	        		CraftData.harvestFinalCutBonusList[TechType] = finalCutBonus;
	        		SNUtil.log("Finished patching "+this+" > "+CraftData.GetHarvestOutputData(TechType));
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
				page.setHeaderImage(TextureManager.getTexture("Textures/PDA/"+header));
			page.register();
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite("Textures/Items/"+ObjectUtil.formatFileName(this));
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer r) {

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
		
		public bool isResource() {
			return true;
		}
		
		public string getTextureFolder() {
			return "Plants";
		}
		
		public Plantable.PlantSize getSize() {
			return Plantable.PlantSize.Large;
		}
		
	}
	
	public class BasicCustomPlantSeed : Spawnable, DIPrefab<StringPrefabContainer> {
		
		public float glowIntensity {get; set;}		
		public StringPrefabContainer baseTemplate {get; set;}
		
		public readonly BasicCustomPlant plant;
		
		public BasicCustomPlantSeed(BasicCustomPlant p) : base(p.ClassID+"_seed", p.FriendlyName+" Seed", p.Description) {
			plant = p;
			baseTemplate = new StringPrefabContainer("daff0e31-dd08-4219-8793-39547fdb745e");
		}
			
		public sealed override GameObject GetGameObject() {
			GameObject go = ObjectUtil.getModPrefabBaseObject(this);
			Pickupable pp = go.EnsureComponent<Pickupable>();
			pp.isPickupable = true;
			
			Plantable p = go.EnsureComponent<Plantable>();
			p.aboveWater = false;
			p.underwater = true;
			p.isSeedling = true;
			p.plantTechType = plant.TechType;
			p.size = plant.getSize();
			
			p.pickupable = pp;
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
			grow.growingTransform.gameObject.SetActive(true);
			
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
			}
			
			return go;
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public bool isResource() {
			return true;
		}
		
		public string getTextureFolder() {
			return "Items";
		}
		
	}
}
