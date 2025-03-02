using System;
using System.Reflection;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public class WorldCollectedItem : Spawnable, DIPrefab<StringPrefabContainer> {
		
		private readonly List<PlannedIngredient> recipe = new List<PlannedIngredient>();
		public readonly string id;
		
		public Atlas.Sprite sprite = null;
		public Vector2int inventorySize = new Vector2int(1, 1);
		public readonly List<PlannedIngredient> byproducts = new List<PlannedIngredient>();
		public Action<Renderer> renderModify = null;
		
		public float glowIntensity {get; set;}
		public StringPrefabContainer baseTemplate {get; set;}
		
		protected readonly Assembly ownerMod;
		
		public WorldCollectedItem(XMLLocale.LocaleEntry e, string template) : this(e.key, e.name, e.desc, template) {
			
		}
		
		public WorldCollectedItem(string id, string name, string desc, string template) : base(id, name, desc) {
			ownerMod = SNUtil.tryGetModDLL();
			typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
			this.id = id;
			
			baseTemplate = new StringPrefabContainer(template.Contains("/") ? PrefabData.getPrefabID(template) : template);
			
			OnFinishedPatching += () => {ItemRegistry.instance.addItem(this);};
		}
			
		public sealed override GameObject GetGameObject() {
			GameObject go = ObjectUtil.getModPrefabBaseObject(this);
			if (renderModify != null)
				renderModify.Invoke(go.GetComponentInChildren<Renderer>());
			return go;
		}
		
		public Assembly getOwnerMod() {
			return ownerMod;
		}
		
		public virtual bool isResource() {
			return true;
		}
		
		public virtual string getTextureFolder() {
			return "Items/World";
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer[] r) {
			
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return sprite == null ? base.GetItemSprite() : sprite;
		}
		
		public Atlas.Sprite getIcon() {
			return GetItemSprite();
		}
		
		public sealed override Vector2int SizeInInventory {
			get {
				return inventorySize;
			}
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
		public void addPostPatchCallback(Action a) {
			OnFinishedPatching += () => {a();};
		}
	}
}
