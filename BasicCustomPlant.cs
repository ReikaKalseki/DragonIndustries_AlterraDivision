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
