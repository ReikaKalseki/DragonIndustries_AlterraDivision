using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public sealed class BasicCraftingItem : Craftable {
		
		private readonly Dictionary<TechType, Ingredient> recipe = new Dictionary<TechType, Ingredient>();
		public readonly string id;
		
		public TechType unlockRequirement = TechType.None;
		public bool isAdvanced = false;
		public Atlas.Sprite sprite = SpriteManager.Get(TechType.PrecursorIonCrystal);
		
		public BasicCraftingItem(string id, string name, string desc) : base(id, name, desc) {
			this.id = id;
		}
		
		public TechType getTechType() {
			TechType tech = TechType.None;
			TechTypeHandler.TryGetModdedTechType(id, out tech);
			return tech;
		}
		
		public BasicCraftingItem addIngredient(TechType item, int amt) {
			if (recipe.ContainsKey(item))
				recipe[item].amount += amt;
			else
				recipe[item] = new Ingredient(item, amt);
			return this;
		}

		public override TechType RequiredForUnlock {
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
				return isAdvanced ? TechCategory.AdvancedMaterials : TechCategory.BasicMaterials;
			}
		}
		
		public override GameObject GetGameObject()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("WorldEntities/Natural/benzene"));
			TechTag component = gameObject.GetComponent<TechTag>();
			UniqueIdentifier component2 = gameObject.GetComponent<PrefabIdentifier>();
			component.type = TechType;
			component2.ClassId = ClassID;
			return gameObject;
		}
		
		protected override TechData GetBlueprintRecipe()
		{
			return new TechData
			{
				Ingredients = new List<Ingredient>(recipe.Values),
				craftAmount = 1
			};
		}
		
		protected override Atlas.Sprite GetItemSprite()
		{
			return sprite;
		}
	}
}
