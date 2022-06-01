﻿using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public abstract class CustomMachine<M> : Buildable, DIPrefab<CustomMachine<M>, StringPrefabContainer> where M : CustomMachineLogic {
		
		private readonly List<PlannedIngredient> recipe = new List<PlannedIngredient>();
		
		public readonly string id;
		
		public float glowIntensity {get; set;}		
		public StringPrefabContainer baseTemplate {get; set;}
		
		protected CustomMachine(string id, string name, string desc, string template) : base(id, name, desc) {
			this.id = id;
			baseTemplate = new StringPrefabContainer(template);
		}
		
		public CustomMachine<M> addIngredient(ItemDef item, int amt) {
			return addIngredient(item.getTechType(), amt);
		}
		
		public CustomMachine<M> addIngredient(ModPrefab item, int amt) {
			return addIngredient(new ModPrefabTechReference(item), amt);
		}
		
		public CustomMachine<M> addIngredient(TechType item, int amt) {
			return addIngredient(new TechTypeContainer(item), amt);
		}
		
		public CustomMachine<M> addIngredient(TechTypeReference item, int amt) {
			recipe.Add(new PlannedIngredient(item, amt));
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
			world.EnsureComponent<M>().prefab = this;
			world.EnsureComponent<Constructable>().techType = TechType;
			return world;
		}
		
		public bool isResource() {
			return false;
		}
		
		public string getTextureFolder() {
			return "Machines";
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
		protected override sealed TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = RecipeUtil.buildRecipeList(recipe),
				craftAmount = 1
			};
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite("Textures/Items/"+SBUtil.formatFileName(this));
		}
	}
		
	public abstract class CustomMachineLogic : MonoBehaviour {
		
		internal ModPrefab prefab;
		
		void Start() {
			
		}
		
		void Update() {
			updateEntity(gameObject);
		}
		
		protected abstract void updateEntity(GameObject go);
		
	}
}
