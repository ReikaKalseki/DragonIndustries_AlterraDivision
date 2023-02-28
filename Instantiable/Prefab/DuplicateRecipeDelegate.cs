using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
		public Assembly ownerMod;
		
		private static readonly Dictionary<TechType, List<DuplicateItemDelegate>> delegates = new Dictionary<TechType, List<DuplicateItemDelegate>>();
		private static readonly HashSet<TechType> delegateItems = new HashSet<TechType>();
		
		public DuplicateRecipeDelegate(PdaItem s, string suff = "") : base(s.ClassID+"_delegate"+getIndexSuffix(s.TechType), s.FriendlyName+suff, s.Description) {
			basis = s.TechType;
			prefab = s;
			unlock = s.RequiredForUnlock;
			group = s.GroupForPDA;
			category = s.CategoryForPDA;
			nameSuffix = suff;
			if (s is DIPrefab<PrefabReference>)
				ownerMod = ((DIPrefab<PrefabReference>)s).getOwnerMod();
			OnFinishedPatching += onPatched;
		}
		
		public DuplicateRecipeDelegate(TechType from, string suff = "") : base(from.AsString()+"_delegate"+getIndexSuffix(from), "", "") {
			basis = from;
			prefab = null;
			sprite = SpriteManager.Get(from);
			nameSuffix = suff;
			OnFinishedPatching += onPatched;
		}
		
		private void onPatched() {
			addDelegate(this);
			if (ownerMod == null)
				throw new Exception("Delegate item "+basis+"/"+TechType+" has no source mod!");
		}
		
		private static string getIndexSuffix(TechType tt) {
			int count = delegates.ContainsKey(tt) ? delegates[tt].Count : 0;
			return count <= 0 ? "" : count.ToString();
		}
		
		public static void addDelegate(DuplicateItemDelegate d) {
			TechType tt = d.getBasis();
			List<DuplicateItemDelegate> li = delegates.ContainsKey(tt) ? delegates[tt] : new List<DuplicateItemDelegate>();
			li.Add(d);
			delegates[tt] = li;
			delegateItems.Add(((Spawnable)d).TechType);
			SNUtil.log("Registering delegate item "+d, d.getOwnerMod());
		}
		
		public static IEnumerable<DuplicateItemDelegate> getDelegates(TechType of) {
			return delegates.ContainsKey(of) ? (IEnumerable<DuplicateItemDelegate>)delegates[of].AsReadOnly() : new List<DuplicateItemDelegate>();
		}
		
		public static bool isDelegateItem(TechType tt) {
			return delegateItems.Contains(tt);
		}
		
		public static void updateLocale() {
			foreach (List<DuplicateItemDelegate> li in delegates.Values) {
				foreach (DuplicateItemDelegate d in li) {
					if (d.getPrefab() == null || !string.IsNullOrEmpty(d.getNameSuffix())) {
						TechType tt = d.getBasis();
						TechType dt = ((ModPrefab)d).TechType;
						LanguageHandler.SetLanguageLine(dt.AsString(), Language.main.Get(tt)+d.getNameSuffix());
						LanguageHandler.SetLanguageLine("Tooltip_"+dt.AsString(), d.getTooltip());
						SNUtil.log("Relocalized "+d+" > "+dt.AsString()+" > "+Language.main.Get(dt), d.getOwnerMod());
					}
				}
			}
		}
		
		public string getTooltip() {
			return Language.main.Get("Tooltip_"+basis.AsString());
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
		
		public Assembly getOwnerMod() {
			return ownerMod;
		}
	}
	
	public interface DuplicateItemDelegate {
		
		string getNameSuffix();
		
		PdaItem getPrefab();
		
		TechType getBasis();
		
		string getTooltip();
		
		Assembly getOwnerMod();
		
	}
}
