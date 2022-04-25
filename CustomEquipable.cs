using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public abstract class CustomEquipable : Equipable {
		
		private readonly Dictionary<TechType, Ingredient> recipe = new Dictionary<TechType, Ingredient>();
		public readonly string id;
		
		protected CustomEquipable(string id, string name, string desc) : base(id, name, desc) {
			this.id = id;
		}
		/*
		public TechType getTechType() {
			TechType tech = TechType.None;
			TechTypeHandler.TryGetModdedTechType(id, out tech);
			return tech;
		}*/
		
		public CustomEquipable addIngredient(ItemDef item, int amt) {
			return addIngredient(item.getTechType(), amt);
		}
		
		public CustomEquipable addIngredient(Craftable item, int amt) {
			return addIngredient(item.TechType, amt);
		}
		
		public CustomEquipable addIngredient(TechType item, int amt) {
			if (recipe.ContainsKey(item))
				recipe[item].amount += amt;
			else
				recipe[item] = new Ingredient(item, amt);
			return this;
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Passive;
			}
		}
		
		public override sealed GameObject GetGameObject() {
			return SBUtil.getItemGO(this, getTemplatePrefab());
		}
		
		protected abstract string getTemplatePrefab();
		
		protected override sealed TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = new List<Ingredient>(recipe.Values),
				craftAmount = 1
			};
		}
	}
}
