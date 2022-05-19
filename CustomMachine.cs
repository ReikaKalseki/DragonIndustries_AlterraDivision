using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public abstract class CustomMachine : Buildable, DIPrefab<CustomMachine, StringPrefabContainer> {
		
		private readonly Dictionary<TechType, Ingredient> recipe = new Dictionary<TechType, Ingredient>();
		
		public readonly string id;
		
		public float glowIntensity {get; set;}		
		public StringPrefabContainer baseTemplate {get; set;}
		
		protected CustomMachine(string id, string name, string desc, string template) : base(id, name, desc) {
			this.id = id;
			baseTemplate = new StringPrefabContainer(template);
		}
		
		public CustomMachine addIngredient(ItemDef item, int amt) {
			return addIngredient(item.getTechType(), amt);
		}
		
		public CustomMachine addIngredient(ModPrefab item, int amt) {
			return addIngredient(item.TechType, amt);
		}
		
		public CustomMachine addIngredient(TechType item, int amt) {
			if (recipe.ContainsKey(item))
				recipe[item].amount += amt;
			else
				recipe[item] = new Ingredient(item, amt);
			return this;
		}

		public override sealed TechGroup GroupForPDA {
			get {
				return TechGroup.InteriorModules;
			}
		}

		public override sealed TechCategory CategoryForPDA {
			get {
				return TechCategory.InteriorModule;
			}
		}
			
		public sealed override GameObject GetGameObject() {
			GameObject world = SBUtil.getModPrefabBaseObject(this);
			world.EnsureComponent<CustomMachineLogic>().tick = onTick;
			world.EnsureComponent<Constructable>().techType = TechType;
			return world;
		}
		
		public bool isResource() {
			return false;
		}
		
		public string getTextureFolder() {
			return "Machines";
		}
		
		protected abstract void onTick(GameObject go);
		
		public virtual void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
		protected override sealed TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = new List<Ingredient>(recipe.Values),
				craftAmount = 1
			};
		}
		
		protected override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite("Textures/Items/"+SBUtil.formatFileName(this));
		}
		
		class CustomMachineLogic : MonoBehaviour {
			
			internal Action<GameObject> tick;
			
			void Start() {
				
			}
			
			void Update() {
				tick(gameObject);
			}
			
		}
	}
}
