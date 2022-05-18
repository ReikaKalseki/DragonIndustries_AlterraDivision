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
	public class BasicCustomPlant : Spawnable {
			
		public readonly VanillaFlora baseTemplate;
		
		public float glowIntensity = -1;
		
		public BasicCustomPlant(XMLLocale.LocaleEntry e, VanillaFlora template) : this(e.key, e.name, e.desc, template) {
			
		}
			
		public BasicCustomPlant(string id, string name, string desc, VanillaFlora template) : base(id, name, desc) {
			baseTemplate = template;
		}
		
		public void addPDAEntry(string text, float scanTime = 2, string header = null) {
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.scanTime = scanTime;
			e.locked = true;
			PDAManager.PDAPage page = PDAManager.createPage(""+TechType, FriendlyName, text, "Lifeforms").addSubcategory("Flora").addSubcategory("Exploitable");
			if (header != null)
				page.setHeaderImage(TextureManager.getTexture("Textures/PDA/"+header));
			page.register();
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
		}
			
		public sealed override GameObject GetGameObject() {
			GameObject prefab;
			if (UWE.PrefabDatabase.TryGetPrefab(baseTemplate.getRandomPrefab(false), out prefab)) {
				GameObject world = UnityEngine.Object.Instantiate(prefab);
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				world.EnsureComponent<ResourceTracker>().techType = TechType;
				world.EnsureComponent<ResourceTracker>().overrideTechType = TechType;
				Renderer r = world.GetComponentInChildren<Renderer>();
				SBUtil.swapToModdedTextures(r, this, glowIntensity, "Plants");
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
