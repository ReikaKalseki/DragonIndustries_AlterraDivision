using System;
using System.Reflection;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public sealed class DuplicateRecipeDelegateWithRecipe : Craftable, DuplicateItemDelegate {
		
		public readonly PdaItem prefab;
		public readonly TechType basis;
		private readonly TechData recipe;
		
		public Atlas.Sprite sprite = null;
		public TechType unlock = TechType.None;
		public TechCategory category = TechCategory.Misc;
		public TechGroup group = TechGroup.Uncategorized;
		public CraftTree.Type craftingType = CraftTree.Type.None;
		public float craftTime = 0.1F;
		public string[] craftingMenuTree = new string[0];
		public Assembly ownerMod;
		public bool allowUnlockPopups = false;
		
		public string suffixName = "";
		
		public DuplicateRecipeDelegateWithRecipe(Craftable s, TechData r) : base(s.ClassID+"_delegate", s.FriendlyName, s.Description) {
			basis = s.TechType;
			prefab = s;
			recipe = r;
			unlock = s.RequiredForUnlock;
			group = s.GroupForPDA;
			category = s.CategoryForPDA;
			craftingType = s.FabricatorType;
			craftTime = s.CraftingTime;
			craftingMenuTree = s.StepsToFabricatorTab;
			suffixName = " (x"+r.craftAmount+")";
			if (s is BasicCraftingItem)
				sprite = ((BasicCraftingItem)s).sprite;
			if (s is DIPrefab<PrefabReference>)
				ownerMod = ((DIPrefab<PrefabReference>)s).getOwnerMod();
			FriendlyName = FriendlyName+suffixName;
			OnFinishedPatching += onPatched;
		}
		
		public DuplicateRecipeDelegateWithRecipe(TechType from, TechData r) : base(from.AsString()+"_delegate", "", "") {
			basis = from;
			prefab = null;
			recipe = r;
			suffixName = r.craftAmount > 1 ? " (x"+r.craftAmount+")" : "";
			sprite = SpriteManager.Get(from);
			OnFinishedPatching += onPatched;
		}
		
		private void onPatched() {
			if (ownerMod == null)
				throw new Exception("Delegate item "+basis.AsString()+"/"+ClassID+" has no source mod!");
			if (sprite == null)
				throw new Exception("Delegate item "+basis+"/"+ClassID+" has no sprite!");
			SNUtil.log("Constructed craftable delegate of "+basis.AsString()+": "+ClassID+" @ "+RecipeUtil.toString(recipe)+" @ "+string.Join("/", craftingMenuTree), ownerMod);
			DuplicateRecipeDelegate.addDelegate(this);
		}
		
		public void setRecipe(int amt = 1) {
			for (int i = 0; i < amt; i++)
				recipe.LinkedItems.Add(basis);
			recipe.craftAmount = 0;
			suffixName = amt > 1 ? " (x"+amt+")" : "";
		}
		
		public override TechGroup GroupForPDA {
			get {
				return group;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return category;
			}
		}

		public override TechType RequiredForUnlock {
			get {
				return unlock;
			}
		}

		public override bool UnlockedAtStart {
			get {
				return unlock == TechType.None;
			}
		}

		public override Vector2int SizeInInventory {
			get {
				return CraftData.GetItemSize(basis);
			}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return craftingType;
			}
		}

		public override float CraftingTime {
			get {
				return craftTime;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return craftingMenuTree;
			}
		}
		
		public override GameObject GetGameObject() {
			return ObjectUtil.createWorldObject(CraftData.GetClassIdForTechType(basis), true, false);
		}
		
		protected override Atlas.Sprite GetItemSprite() {
			return sprite != null ? sprite : base.GetItemSprite();
		}
		
		public override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
		protected override TechData GetBlueprintRecipe() {
			return recipe;
		}
		
		public string getNameSuffix() {
			return suffixName;
		}
		
		public PdaItem getPrefab() {
			return prefab;
		}
		
		public TechType getBasis() {
			return basis;
		}
		
		public Assembly getOwnerMod() {
			return ownerMod;
		}
		
		public string getTooltip() {
			return Language.main.Get("Tooltip_"+basis.AsString());
		}
		
		public bool allowTechUnlockPopups() {
			return allowUnlockPopups;
		}
	}
}
