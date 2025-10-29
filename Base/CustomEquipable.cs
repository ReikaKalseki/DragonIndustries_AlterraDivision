using System;
using System.Collections.Generic;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public abstract class CustomEquipable : Equipable, DIPrefab<CustomEquipable, StringPrefabContainer> {

		private readonly List<PlannedIngredient> recipe = new List<PlannedIngredient>();
		public readonly string id;

		private readonly Assembly ownerMod;

		public float glowIntensity { get; set; }
		public bool isArmor { get; set; }
		public StringPrefabContainer baseTemplate { get; set; }

		public TechType dependency = TechType.None;
		private PDAManager.PDAPage page;

		protected CustomEquipable(XMLLocale.LocaleEntry e, string template) : this(e.key, e.name, e.desc, template) {
			if (!string.IsNullOrEmpty(e.pda)) {
				page = PDAManager.createPage("ency_" + ClassID, FriendlyName, e.pda, "Tech/Equipment");
				string header = e.getString("header");
				if (header != null)
					page.setHeaderImage(TextureManager.getTexture(SNUtil.tryGetModDLL(), "Textures/PDA/" + header));
				page.register();
			}
		}

		protected CustomEquipable(string id, string name, string desc, string template) : base(id, name, desc) {
			ownerMod = SNUtil.tryGetModDLL();
			typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
			this.id = id;

			baseTemplate = new StringPrefabContainer(template.Contains("/") ? PrefabData.getPrefabID(template) : template);

			OnFinishedPatching += () => {
				ItemRegistry.instance.addItem(this);
				if (page != null)
					TechnologyUnlockSystem.instance.registerPage(TechType, page);
			};
		}
		/*
		public TechType getTechType() {
			TechType tech = TechType.None;
			TechTypeHandler.TryGetModdedTechType(id, out tech);
			return tech;
		}*/

		public CustomEquipable addIngredient(ItemDef item, int amt) {
			return this.addIngredient(item.getTechType(), amt);
		}

		public CustomEquipable addIngredient(ModPrefab item, int amt) {
			return this.addIngredient(new ModPrefabTechReference(item), amt);
		}

		public CustomEquipable addIngredient(TechType item, int amt) {
			return this.addIngredient(new TechTypeContainer(item), amt);
		}

		public CustomEquipable addIngredient(TechTypeReference item, int amt) {
			recipe.Add(new PlannedIngredient(item, amt));
			return this;
		}

		public override sealed TechType RequiredForUnlock {
			get {
				return dependency == TechType.Unobtanium ? TechType.None : dependency;
			}
		}

		public override sealed bool UnlockedAtStart {
			get {
				return dependency != TechType.Unobtanium && RequiredForUnlock == TechType.None && CompoundTechsForUnlock == null;
			}
		}

		public void preventNaturalUnlock() {
			dependency = TechType.Unobtanium;
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
				return new string[] { "Personal", "Equipment" };//return new string[]{"DISeamoth"};//new string[]{"SeamothModules"};
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Personal;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return isArmor ? TechCategory.Equipment : TechCategory.Tools;
			}
		}

		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
		}

		public sealed override GameObject GetGameObject() {
			return ObjectUtil.getModPrefabBaseObject(this);
		}

		public virtual void prepareGameObject(GameObject go, Renderer[] r) {

		}

		public Assembly getOwnerMod() {
			return ownerMod;
		}

		public virtual bool isResource() {
			return false;
		}

		public virtual string getTextureFolder() {
			return "Items/Tools";
		}

		public Atlas.Sprite getIcon() {
			return this.GetItemSprite();
		}

		protected override sealed TechData GetBlueprintRecipe() {
			return new TechData {
				Ingredients = RecipeUtil.buildRecipeList(recipe),
				craftAmount = 1,
				LinkedItems = this.getAuxCrafted()
			};
		}

		public virtual List<TechType> getAuxCrafted() {
			return new List<TechType>();
		}
	}
}
