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
	public class BasicCustomOre : Spawnable {
			
		public readonly bool isLargeResource;
		public readonly VanillaResources baseTemplate;
		
		public float glowIntensity = -1;
		public string collectSound = null;
		
		public BasicCustomOre(XMLLocale.LocaleEntry e, VanillaResources template) : this(e.key, e.name, e.desc, template) {
			
		}
			
		public BasicCustomOre(string id, string name, string desc, VanillaResources template) : base(id, name, desc) {
			baseTemplate = template;
			
			//TODO pickup sound
			if (collectSound != null)
				OnFinishedPatching += () => {CraftData.pickupSoundList.Add(TechType, collectSound);};
		}
		
		protected virtual string getPickupSound() {
			return "event:/loot/pickup_glass";
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
			
		public sealed override GameObject GetGameObject() {
			GameObject prefab;
			if (UWE.PrefabDatabase.TryGetPrefab(baseTemplate.prefab, out prefab)) {
				GameObject world = UnityEngine.Object.Instantiate(prefab);
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				world.EnsureComponent<ResourceTracker>().techType = TechType;
				world.EnsureComponent<ResourceTracker>().overrideTechType = TechType;
				Renderer r = world.GetComponentInChildren<Renderer>();
				SBUtil.swapToModdedTextures(r, this, glowIntensity, "Resources");
				prepareGameObject(world, r);
				//SBUtil.writeToChat("Applying custom texes to "+world+" @ "+world.transform.position);
				return world;
			}
			else {
				SBUtil.writeToChat("Could not fetch template GO for "+this);
				return null;
			}
		}
		
		protected override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite("Textures/Items/"+SBUtil.formatFileName(this));
		}
		
		protected virtual void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
	}
}
