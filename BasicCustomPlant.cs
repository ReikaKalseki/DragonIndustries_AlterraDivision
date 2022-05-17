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
		
		private static readonly string[] texTypes = new string[]{"_MainTex", "_SpecTex", "_BumpMap", "_Illum"};
			
		public readonly VanillaFlora baseTemplate;
		
		public float glowIntensity = -1;
		public string glowType = "GlowStrength";
		public Action<Renderer> renderModify;
		
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
				applyMaterialChanges(r);
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
			return TextureManager.getSprite("Textures/Items/"+formatFileName());
		}
		
		protected virtual void prepareGameObject(GameObject go, Renderer r) {
			
		}
			
		private void applyMaterialChanges(Renderer r) {
			//SBUtil.log("render for "+this);
			//SBUtil.dumpObjectData(r);
			bool flag = false;
			foreach (String type in texTypes) {
				Texture2D newTex = TextureManager.getTexture("Textures/Plants/"+formatFileName()+type);
				if (newTex != null) {
					r.materials[0].SetTexture(type, newTex);
					r.sharedMaterial.SetTexture(type, newTex);
					flag = true;
					//SBUtil.writeToChat("Found "+type+" texture @ "+path);
				}
				else {
					//SBUtil.writeToChat("No texture found at "+path);
				}
			}
			if (!flag) {
				SBUtil.log("NO CUSTOM TEXTURES FOUND: "+this);
			}
			if (glowIntensity >= 0) {
				SBUtil.setEmissivity(r, glowIntensity, glowType);/*
				r.materials[0].SetFloat("_"+glowType, glowIntensity);
				r.sharedMaterial.SetFloat("_GlowStrength", glowIntensity);
				r.materials[0].SetFloat("_GlowStrengthNight", glowIntensity);
				r.sharedMaterial.SetFloat("_GlowStrengthNight", glowIntensity);
				
				r.materials[0].SetFloat("_EmissionLM", glowIntensity);
				r.sharedMaterial.SetFloat("_EmissionLM", glowIntensity);
				r.materials[0].SetFloat("_EmissionLMNight", glowIntensity);
				r.sharedMaterial.SetFloat("_EmissionLMNight", glowIntensity);*/
				
				r.materials[0].EnableKeyword("MARMO_EMISSION");
				r.sharedMaterial.EnableKeyword("MARMO_EMISSION");
			}
			if (renderModify != null)
				renderModify(r);
			//SBUtil.log("after modify for "+this);
			//SBUtil.dumpObjectData(r);
		}
			
		private string formatFileName() {
			string n = ClassID;
			System.Text.StringBuilder ret = new System.Text.StringBuilder();
			for (int i = 0; i < n.Length; i++) {
				char c = n[i];
				if (c == '_')
					continue;
				bool caps = i == 0 || n[i-1] == '_';
				if (caps) {
					c = Char.ToUpperInvariant(c);
				}
				else {
					c = Char.ToLowerInvariant(c);
				}
				ret.Append(c);
			}
			return ret.ToString();
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
	}
}
