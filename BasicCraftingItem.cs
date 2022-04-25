using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public class BasicCraftingItem : Craftable {
		
		private static bool addedTab = false;
		
		private readonly Dictionary<TechType, Ingredient> recipe = new Dictionary<TechType, Ingredient>();
		public readonly string id;
		
		public int numberCrafted = 1;
		public TechType unlockRequirement = TechType.None;
		public bool isAdvanced = false;
		public Atlas.Sprite sprite = null;
		public float craftingTime = 0;
		
		public BasicCraftingItem(string id, string name, string desc) : base(id, name, desc) {
			this.id = id;
			
			if (!addedTab) {
				//CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, "DIIntermediate", "Intermediate Products", SpriteManager.Get(TechType.HatchingEnzymes));
				addedTab = true;
			}
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
		
		public BasicCraftingItem addIngredient(Craftable item, int amt) {
			return addIngredient(item.TechType, amt);
		}
		
		public BasicCraftingItem addIngredient(TechType item, int amt) {
			if (recipe.ContainsKey(item))
				recipe[item].amount += amt;
			else
				recipe[item] = new Ingredient(item, amt);
			return this;
		}

		public sealed override TechType RequiredForUnlock {
			get {
				return unlockRequirement;
			}
		}

		public sealed override TechGroup GroupForPDA {
			get {
				return TechGroup.Resources;
			}
		}

		public sealed override TechCategory CategoryForPDA {
			get {
				return isAdvanced ? TechCategory.AdvancedMaterials : TechCategory.BasicMaterials;
			}
		}

		public sealed override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Resources", isAdvanced ? "AdvancedMaterials" : "BasicMaterials"};//new string[]{"DIIntermediate"};
			}
		}
	
		public override GameObject GetGameObject() {
			return SBUtil.getItemGO(this, "WorldEntities/Natural/benzene"); //TODO allow changing for inworld model
		}
		
		protected sealed override TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = new List<Ingredient>(recipe.Values),
				craftAmount = numberCrafted
			};
		}
	
		public TechData getRecipe() {
			return GetBlueprintRecipe();
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return sprite == null ? base.GetItemSprite() : sprite;
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
