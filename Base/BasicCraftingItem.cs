using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public class BasicCraftingItem : Craftable, DIPrefab<BasicCraftingItem, StringPrefabContainer> {
		
		private static bool addedTab = false;
		
		private readonly List<PlannedIngredient> recipe = new List<PlannedIngredient>();
		public readonly string id;
		
		public int numberCrafted = 1;
		public TechType unlockRequirement = TechType.None;
		public bool isAdvanced = false;
		public bool isElectronics = false;
		public Atlas.Sprite sprite = null;
		public float craftingTime = 0;
		public readonly List<PlannedIngredient> byproducts = new List<PlannedIngredient>();
		
		public float glowIntensity {get; set;}			
		public StringPrefabContainer baseTemplate {get; set;}
		
		public BasicCraftingItem(XMLLocale.LocaleEntry e, string template) : this(e.key, e.name, e.desc, template) {
			
		}
		
		public BasicCraftingItem(string id, string name, string desc, string template) : base(id, name, desc) {
			this.id = id;
			
			if (!addedTab) {
				//CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, "DIIntermediate", "Intermediate Products", SpriteManager.Get(TechType.HatchingEnzymes));
				addedTab = true;
			}
			
			baseTemplate = new StringPrefabContainer(template.Contains("/") ? PrefabData.getPrefabID(template) : template);
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Fabricator;
			}
		}
		
		/*
		public TechType getTechType() {
			TechType tech = TechType.None;
			TechTypeHandler.TryGetModdedTechType(id, out tech);
			return tech;
		}*/
		
		public BasicCraftingItem addIngredient(ItemDef item, int amt) {
			return addIngredient(item.getTechType(), amt);
		}
		
		public BasicCraftingItem addIngredient(ModPrefab item, int amt) {
			return addIngredient(new ModPrefabTechReference(item), amt);
		}
		
		public BasicCraftingItem addIngredient(TechType item, int amt) {
			return addIngredient(new TechTypeContainer(item), amt);
		}
		
		public BasicCraftingItem addIngredient(TechTypeReference item, int amt) {
			recipe.Add(new PlannedIngredient(item, amt));
			return this;
		}

		public sealed override TechType RequiredForUnlock {
			get {
				return unlockRequirement;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Resources;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return isElectronics ? TechCategory.Electronics : (isAdvanced ? TechCategory.AdvancedMaterials : TechCategory.BasicMaterials);
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Resources", isAdvanced ? "AdvancedMaterials" : "BasicMaterials"};//new string[]{"DIIntermediate"};
			}
		}
			
		public sealed override GameObject GetGameObject() {
			return ObjectUtil.getModPrefabBaseObject(this);
		}
		
		public bool isResource() {
			return false;
		}
		
		public string getTextureFolder() {
			return "Items/World";
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		protected sealed override TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = RecipeUtil.buildRecipeList(recipe),
				craftAmount = numberCrafted,
				LinkedItems = RecipeUtil.buildLinkedItems(byproducts)
			};
		}
	
		public TechData getRecipe() {
			return GetBlueprintRecipe();
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return sprite == null ? base.GetItemSprite() : sprite;
		}
		
		public Atlas.Sprite getIcon() {
			return GetItemSprite();
		}

		public sealed override float CraftingTime {
			get {
				return craftingTime;
			}
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
	}
}
