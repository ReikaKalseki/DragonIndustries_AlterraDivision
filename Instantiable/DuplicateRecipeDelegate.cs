using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public sealed class DuplicateRecipeDelegate : PdaItem, DuplicateItemDelegate {
		
		public readonly PdaItem prefab;
		public readonly TechType basis;
		public readonly string nameSuffix;
		
		public Atlas.Sprite sprite = null;
		public TechType unlock = TechType.None;
		public TechCategory category = TechCategory.Misc;
		public TechGroup group = TechGroup.Uncategorized;
		
		private static readonly List<DuplicateItemDelegate> delegates = new List<DuplicateItemDelegate>();
		
		public DuplicateRecipeDelegate(PdaItem s, string suff = "") : base(s.ClassID+"_delegate", s.FriendlyName+suff, s.Description) {
			basis = s.TechType;
			prefab = s;
			unlock = s.RequiredForUnlock;
			group = s.GroupForPDA;
			category = s.CategoryForPDA;
			nameSuffix = suff;
			delegates.Add(this);
		}
		
		public DuplicateRecipeDelegate(TechType from, string suff = "") : base(from.AsString()+"_delegate", "", "") {
			basis = from;
			prefab = null;
			sprite = SpriteManager.Get(from);
			nameSuffix = suff;
			delegates.Add(this);
		}
		
		public static void addDelegate(DuplicateItemDelegate d) {
			delegates.Add(d);
		}
		
		public static void updateLocale() {
			foreach (DuplicateItemDelegate d in delegates) {
				if (d.getPrefab() == null) {
					TechType tt = d.getBasis();
					TechType dt = ((ModPrefab)d).TechType;
					Language.main.strings[dt.AsString()] = Language.main.strings[tt.AsString()]+d.getNameSuffix();
					Language.main.strings["Tooltip_"+dt.AsString()] = Language.main.strings["Tooltip_"+tt.AsString()];
					SNUtil.log("Relocalized "+d+" > "+Language.main.strings[dt.AsString()]);
				}
			}
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
		
		public string getNameSuffix() {
			return nameSuffix;
		}
		
		public PdaItem getPrefab() {
			return prefab;
		}
		
		public TechType getBasis() {
			return basis;
		}
		
		protected override TechData GetBlueprintRecipe() {
			return null;
		}
	}
	
	public interface DuplicateItemDelegate {
		
		string getNameSuffix();
		
		PdaItem getPrefab();
		
		TechType getBasis();
		
	}
}
