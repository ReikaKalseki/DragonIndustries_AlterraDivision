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
		
		protected CustomEquipable(string id, string name, string desc) : base(id, name, desc) {

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
		
		public override sealed GameObject GetGameObject()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>(getTemplatePrefab()));
			TechTag component = gameObject.GetComponent<TechTag>();
			UniqueIdentifier component2 = gameObject.GetComponent<PrefabIdentifier>();
			component.type = TechType;
			component2.ClassId = ClassID;
			return gameObject;
		}
		
		protected abstract string getTemplatePrefab();
		
		protected override sealed TechData GetBlueprintRecipe()
		{
			return new TechData
			{
				Ingredients = new List<Ingredient>(recipe.Values),
				craftAmount = 1
			};
		}
	}
}
