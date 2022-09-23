﻿using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public sealed class UncraftingRecipeItem : Craftable, DuplicateItemDelegate {
		
		public readonly PdaItem prefab;
		public readonly TechType basis;
		
		public Atlas.Sprite sprite = null;
		public TechCategory category = TechCategory.Misc;
		public TechGroup group = TechGroup.Uncategorized;
		public CraftTree.Type craftingType = CraftTree.Type.None;
		public float craftTime = 1F;
		public string[] craftingMenuTree = new string[0];
		
		public string suffixName = "";
		
		public UncraftingRecipeItem(Craftable s) : base(s.ClassID+"_uncrafting", s.FriendlyName, s.Description) {
			basis = s.TechType;
			prefab = s;
			group = s.GroupForPDA;
			category = s.CategoryForPDA;
			craftingType = s.FabricatorType;
			craftTime = s.CraftingTime;
			craftingMenuTree = s.StepsToFabricatorTab;
			if (s is BasicCraftingItem)
				sprite = ((BasicCraftingItem)s).sprite;
			FriendlyName = FriendlyName+suffixName;
			DuplicateRecipeDelegate.addDelegate(this);
			OnFinishedPatching += () => {SNUtil.log("Constructed uncrafting of "+s.ClassID+": "+TechType+" @ "+string.Join("/", craftingMenuTree));};
		}
		
		public UncraftingRecipeItem(TechType from) : base(from.AsString()+"_uncrafting", "", "") {
			basis = from;
			prefab = null;
			sprite = SpriteManager.Get(from);
			DuplicateRecipeDelegate.addDelegate(this);
			OnFinishedPatching += () => {SNUtil.log("Constructed uncrafting of "+from+": "+TechType+" @ "+string.Join("/", craftingMenuTree));};
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
				return basis;
			}
		}

		public override bool UnlockedAtStart {
			get {
				return false;//unlock == TechType.None;
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
			return RecipeUtil.createUncrafting(basis);
		}
		
		public string getNameSuffix() {
			return " (Uncrafting)";
		}
		
		public PdaItem getPrefab() {
			return prefab;
		}
		
		public TechType getBasis() {
			return basis;
		}
		
		public string getTooltip() {
			return "Reclaiming the crafting ingredients.";
		}
	}
}