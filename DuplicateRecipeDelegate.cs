using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public sealed class DuplicateRecipeDelegate : PdaItem {
		
		public readonly PdaItem prefab;
		public readonly TechType basis;
		private readonly TechData recipe;
		
		public Atlas.Sprite sprite = null;
		public TechType unlock = TechType.None;
		public TechCategory category = TechCategory.Misc;
		public TechGroup group = TechGroup.Uncategorized;
		
		public DuplicateRecipeDelegate(PdaItem s, TechData r) : base(s.ClassID+"_delegate", s.FriendlyName, s.Description) {
			basis = s.TechType;
			prefab = s;
			recipe = r;
			unlock = s.RequiredForUnlock;
			group = s.GroupForPDA;
			category = s.CategoryForPDA;
		}
		
		public DuplicateRecipeDelegate(TechType from, TechData r) : base(from.AsString()+"_delegate", Language.main.Get(from), Language.main.Get("Tooltip_"+from.AsString())) {
			basis = from;
			prefab = null;
			recipe = r;
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
	}
}
