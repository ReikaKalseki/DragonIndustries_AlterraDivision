using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public abstract class CustomEquipable : Equipable, DIPrefab<CustomEquipable, StringPrefabContainer> {
		
		private readonly Dictionary<TechType, Ingredient> recipe = new Dictionary<TechType, Ingredient>();
		public readonly string id;
		
		public float glowIntensity {get; set;}		
		public StringPrefabContainer baseTemplate {get; set;}
		
		protected CustomEquipable(XMLLocale.LocaleEntry e, string template) : this(e.key, e.name, e.desc, template) {
			
		}
		
		protected CustomEquipable(string id, string name, string desc, string template) : base(id, name, desc) {
			this.id = id;
			
			baseTemplate = new StringPrefabContainer(template);
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
		
		public CustomEquipable addIngredient(ModPrefab item, int amt) {
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

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Fabricator;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Personal", "Equipment"};//return new string[]{"DISeamoth"};//new string[]{"SeamothModules"};
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Personal;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Tools;
			}
		}
			
		public sealed override GameObject GetGameObject() {
			return SBUtil.getModPrefabBaseObject(this);
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public bool isResource() {
			return false;
		}
		
		public string getTextureFolder() {
			return "Items/Tools";
		}
		
		protected override sealed TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = new List<Ingredient>(recipe.Values),
				craftAmount = 1
			};
		}
	}
}
