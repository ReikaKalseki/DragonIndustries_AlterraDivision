using System;
using System.Collections.Generic;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public class BasicCraftingItem : Craftable, DIPrefab<BasicCraftingItem, StringPrefabContainer> {

		private static bool addedTab = false;

		private readonly List<PlannedIngredient> recipe = new List<PlannedIngredient>();
		public readonly string id;

		public int numberCrafted = 1;
		public TechType unlockRequirement = TechType.None;
		public Atlas.Sprite sprite = null;
		public float craftingTime = 0;
		public Vector2int inventorySize = new Vector2int(1, 1);
		public readonly List<PlannedIngredient> byproducts = new List<PlannedIngredient>();
		public string craftingSubCategory = ""+TechCategory.BasicMaterials;
		public Action<Renderer> renderModify = null;

		public float glowIntensity { get; set; }
		public StringPrefabContainer baseTemplate { get; set; }

		protected readonly Assembly ownerMod;

		public BasicCraftingItem(XMLLocale.LocaleEntry e, string template) : this(e.key, e.name, e.desc, template) {

		}

		public BasicCraftingItem(string id, string name, string desc, string template) : base(id, name, desc) {
			ownerMod = SNUtil.tryGetModDLL();
			typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
			this.id = id;

			if (!addedTab) {
				//CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, "DIIntermediate", "Intermediate Products", SpriteManager.Get(TechType.HatchingEnzymes));
				addedTab = true;
			}

			baseTemplate = new StringPrefabContainer(template.Contains("/") ? PrefabData.getPrefabID(template) : template);

			OnFinishedPatching += () => { ItemRegistry.instance.addItem(this); };
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
			return this.addIngredient(item.getTechType(), amt);
		}

		public BasicCraftingItem addIngredient(ModPrefab item, int amt) {
			return this.addIngredient(new ModPrefabTechReference(item), amt);
		}

		public BasicCraftingItem addIngredient(TechType item, int amt) {
			return this.addIngredient(new TechTypeContainer(item), amt);
		}

		public BasicCraftingItem addIngredient(TechTypeReference item, int amt) {
			recipe.Add(new PlannedIngredient(item, amt));
			return this;
		}

		public BasicCraftingItem scaleRecipe(float amt) {
			numberCrafted = (int)Mathf.Max(1, numberCrafted * amt);
			foreach (PlannedIngredient pi in recipe)
				pi.amount = (int)Mathf.Max(1, pi.amount * amt);
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
				TechCategory ret = TechCategory.Misc;
				return Enum.TryParse(craftingSubCategory, out ret) ? ret : TechCategoryHandler.Main.TryGetModdedTechCategory(craftingSubCategory, out ret) ? ret : TechCategory.BasicMaterials;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				//SNUtil.log("Fetching craftingsubcat "+craftingSubCategory+" from "+FriendlyName);
				//RecipeUtil.dumpCraftTree(CraftTree.Type.Fabricator);
				return new string[] { "Resources", craftingSubCategory };
			}
		}

		public sealed override GameObject GetGameObject() {
			GameObject go = ObjectUtil.getModPrefabBaseObject(this);
			if (renderModify != null)
				renderModify(go.GetComponentInChildren<Renderer>());
			return go;
		}

		public Assembly getOwnerMod() {
			return ownerMod;
		}

		public virtual bool isResource() {
			return false;
		}

		public virtual string getTextureFolder() {
			return "Items/World";
		}

		public virtual void prepareGameObject(GameObject go, Renderer[] r) {

		}

		protected sealed override TechData GetBlueprintRecipe() {
			return new TechData {
				Ingredients = RecipeUtil.buildRecipeList(recipe),
				craftAmount = numberCrafted,
				LinkedItems = RecipeUtil.buildLinkedItems(byproducts)
			};
		}

		public TechData getRecipe() {
			return this.GetBlueprintRecipe();
		}

		protected sealed override Atlas.Sprite GetItemSprite() {
			return sprite == null ? base.GetItemSprite() : sprite;
		}

		public Atlas.Sprite getIcon() {
			return this.GetItemSprite();
		}

		public sealed override float CraftingTime {
			get {
				return craftingTime;
			}
		}

		public sealed override Vector2int SizeInInventory {
			get {
				return inventorySize;
			}
		}

		public sealed override string ToString() {
			return base.ToString() + " [" + TechType + "] / " + ClassID + " / " + PrefabFileName;
		}

		public void addPostPatchCallback(Action a) {
			OnFinishedPatching += () => { a(); };
		}
	}
}
