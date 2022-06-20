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
	public class BasicCustomOre : Spawnable, DIPrefab<VanillaResources> {
			
		public readonly bool isLargeResource;
		
		public string collectSound = CraftData.defaultPickupSound;
		
		public float glowIntensity {get; set;}		
		public VanillaResources baseTemplate {get; set;}
		
		public BasicCustomOre(XMLLocale.LocaleEntry e, VanillaResources template) : this(e.key, e.name, e.desc, template) {
			
		}
			
		public BasicCustomOre(string id, string name, string desc, VanillaResources template) : base(id, name, desc) {
			baseTemplate = template;
			
			//TODO pickup sound
			if (collectSound != null)
				OnFinishedPatching += () => {CraftData.pickupSoundList[TechType] = collectSound;};
		}
		
		public void registerWorldgen(BiomeType biome, int amt, float chance) {
			SBUtil.log("Adding worldgen "+biome+" x"+amt+" @ "+chance+"% to "+this);
			GenUtil.registerOreWorldgen(this, biome, amt, chance);
		}
		
		public void addPDAEntry(string text, float scanTime = 2, string header = null) {
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.scanTime = scanTime;
			e.locked = true;
			PDAManager.PDAPage page = PDAManager.createPage(""+TechType, FriendlyName, text, "PlanetaryGeology");
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
			return "Resources";
		}
		
	}
}
