using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public abstract class CustomMachine : Buildable {
		
		private readonly Dictionary<TechType, Ingredient> recipe = new Dictionary<TechType, Ingredient>();
		
		public readonly string id;
		private readonly string templatePrefab;
		
		public float glowIntensity = -1;
		
		protected CustomMachine(string id, string name, string desc, string template) : base(id, name, desc) {
			this.id = id;
			templatePrefab = template;
		}
		
		public CustomMachine addIngredient(ItemDef item, int amt) {
			return addIngredient(item.getTechType(), amt);
		}
		
		public CustomMachine addIngredient(Craftable item, int amt) {
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
		
		public override GameObject GetGameObject() {
			GameObject prefab;
			if (UWE.PrefabDatabase.TryGetPrefab(templatePrefab, out prefab)) {
				GameObject world = UnityEngine.Object.Instantiate(prefab);
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				world.EnsureComponent<CustomMachineLogic>().tick = onTick;
				world.EnsureComponent<Constructable>().techType = TechType;
				
				Renderer r = world.GetComponentInChildren<Renderer>();
				SBUtil.swapToModdedTextures(r, this, glowIntensity, "Machines");
				prepareGameObject(world, r);
				//SBUtil.writeToChat("Applying custom texes to "+world+" @ "+world.transform.position);
				return world;
			}
			else {
				SBUtil.writeToChat("Could not fetch template GO for "+this);
				return null;
			}
		}
		
		protected abstract void onTick(GameObject go);
		
		protected virtual void prepareGameObject(GameObject go, Renderer r) {
			
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
