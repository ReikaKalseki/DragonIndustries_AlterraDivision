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
		
		public HarvestType collectionMethod = HarvestType.DamageAlive;
		public TechTypeReference harvestedItem;
		public int finalCutBonus = 2;
		
		public BasicCustomPlant(XMLLocale.LocaleEntry e, VanillaFlora template) : this(e.key, e.name, e.desc, template) {
			
		}
			
		public BasicCustomPlant(string id, string name, string desc, VanillaFlora template) : base(id, name, desc) {
			baseTemplate = template;
			harvestedItem = new ModPrefabTechReference(this);
			OnFinishedPatching += () => {
				if (collectionMethod != HarvestType.None && harvestedItem != null) {
	        		CraftData.harvestTypeList[TechType] = collectionMethod;
	        		CraftData.harvestOutputList[TechType] = harvestedItem.getTechType();
	        		CraftData.harvestFinalCutBonusList[TechType] = finalCutBonus;
	        		SBUtil.log("Finished patching "+this+" > "+CraftData.GetHarvestOutputData(TechType)+" from "+harvestedItem.GetType()+"="+harvestedItem.getTechType());
				}
			};
		}
		
		public void addPDAEntry(string text, float scanTime = 2, string header = null) {
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.scanTime = scanTime;
			e.locked = true;
			PDAManager.PDAPage page = PDAManager.createPage(""+TechType, FriendlyName, text, "Lifeforms");
			page.addSubcategory("Flora").addSubcategory(collectionMethod == HarvestType.None || harvestedItem == null ? "Sea" : "Exploitable");
			if (header != null)
				page.setHeaderImage(TextureManager.getTexture("Textures/PDA/"+header));
			page.register();
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite("Textures/Items/"+SBUtil.formatFileName(this));
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer r) {
			Pickupable p = go.EnsureComponent<Pickupable>();
			p.isPickupable = false;
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
			
		public sealed override GameObject GetGameObject() {
			return SBUtil.getModPrefabBaseObject(this);
		}
		
		public bool isResource() {
			return true;
		}
		
		public string getTextureFolder() {
			return "Plants";
		}
		
	}
}
