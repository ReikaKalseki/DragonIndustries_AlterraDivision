using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public class DuplicateRecipeDelegate : PdaItem, DuplicateItemDelegate {

		public readonly PdaItem prefab;
		public readonly TechType basis;
		public readonly string nameSuffix;

		public Atlas.Sprite sprite = null;
		public TechType unlock = TechType.None;
		public TechCategory category = TechCategory.Misc;
		public TechGroup group = TechGroup.Uncategorized;
		public Assembly ownerMod;
		public bool allowUnlockPopups = false;

		private static readonly Dictionary<TechType, List<DuplicateItemDelegate>> delegates = new Dictionary<TechType, List<DuplicateItemDelegate>>();
		private static readonly Dictionary<TechType, DuplicateItemDelegate> delegateItems = new Dictionary<TechType, DuplicateItemDelegate>();

		public DuplicateRecipeDelegate(PdaItem s, string suff = "") : base(s.ClassID + "_delegate" + getIndexSuffix(s.TechType), s.FriendlyName + suff, s.Description) {
			basis = s.TechType;
			prefab = s;
			unlock = s.RequiredForUnlock;
			group = s.GroupForPDA;
			category = s.CategoryForPDA;
			nameSuffix = suff;
			if (s is DIPrefab<PrefabReference>)
				ownerMod = ((DIPrefab<PrefabReference>)s).getOwnerMod();
			OnFinishedPatching += this.onPatched;
		}

		public DuplicateRecipeDelegate(TechType from, string suff = "") : base(from.AsString() + "_delegate" + getIndexSuffix(from), "", "") {
			basis = from;
			prefab = null;
			sprite = SpriteManager.Get(from);
			nameSuffix = suff;
			OnFinishedPatching += this.onPatched;
		}

		private void onPatched() {
			addDelegate(this);
			if (ownerMod == null)
				throw new Exception("Delegate item " + basis + "/" + ClassID + " has no source mod!");
			if (sprite == null)
				throw new Exception("Delegate item " + basis + "/" + ClassID + " has no sprite!");
		}

		private static string getIndexSuffix(TechType tt) {
			int count = delegates.ContainsKey(tt) ? delegates[tt].Count : 0;
			return count <= 0 ? "" : "_" + (count + 1).ToString();
		}

		public static void addDelegate(DuplicateItemDelegate d) {
			TechType tt = d.getBasis();
			FieldInfo fi = typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic);
			ModPrefab pfb = SNUtil.getModPrefabByTechType(tt);
			Assembly a = pfb == null ? /*SNUtil.gameDLL*/null : (Assembly)fi.GetValue(pfb); //SML does not recognize game DLL and looks for a mod with that DLL, fails, and says error
			if (a == null)
				a = d.getOwnerMod();
			fi.SetValue(d, a);
			fi = typeof(TechTypeHandler).GetField("TechTypesAddedBy", BindingFlags.Static | BindingFlags.NonPublic);
			Dictionary<TechType, Assembly> dict = (Dictionary<TechType, Assembly>)fi.GetValue(null);
			TechType ttsrc = ((Spawnable)d).TechType;
			dict[ttsrc] = a;
			List<DuplicateItemDelegate> li = delegates.ContainsKey(tt) ? delegates[tt] : new List<DuplicateItemDelegate>();
			li.Add(d);
			delegates[tt] = li;
			delegateItems.Add(ttsrc, d);
			SNUtil.log("Registering delegate item " + d + " ref pfb=" + pfb + " in " + a.GetName().Name, d.getOwnerMod());
		}

		public static IEnumerable<DuplicateItemDelegate> getDelegates(TechType of) {
			return delegates.ContainsKey(of) ? (IEnumerable<DuplicateItemDelegate>)delegates[of].AsReadOnly() : new List<DuplicateItemDelegate>();
		}

		public static bool isDelegateItem(TechType tt) {
			return delegateItems.ContainsKey(tt);
		}

		public static DuplicateItemDelegate getDelegateFromTech(TechType tt) {
			return delegateItems[tt];
		}

		public static void updateLocale() {
			foreach (List<DuplicateItemDelegate> li in delegates.Values) {
				foreach (DuplicateItemDelegate d in li) {
					if (d.getPrefab() == null || !string.IsNullOrEmpty(d.getNameSuffix())) {
						TechType tt = d.getBasis();
						TechType dt = ((ModPrefab)d).TechType;
						CustomLocaleKeyDatabase.registerKey(dt.AsString(), Language.main.Get(tt) + d.getNameSuffix());
						CustomLocaleKeyDatabase.registerKey("Tooltip_" + dt.AsString(), d.getTooltip());
						SNUtil.log("Relocalized " + d + " > " + dt.AsString() + " > " + Language.main.Get(dt), d.getOwnerMod());
					}
				}
			}
		}

		public string getTooltip() {
			return Language.main.Get("Tooltip_" + basis.AsString());
		}

		public override sealed TechGroup GroupForPDA {
			get {
				return group;
			}
		}

		public override sealed TechCategory CategoryForPDA {
			get {
				return category;
			}
		}

		public override sealed TechType RequiredForUnlock {
			get {
				return unlock;
			}
		}

		public override sealed GameObject GetGameObject() {
			return ObjectUtil.createWorldObject(CraftData.GetClassIdForTechType(basis), true, false);
		}

		protected override sealed Atlas.Sprite GetItemSprite() {
			return sprite != null ? sprite : base.GetItemSprite();
		}

		public override sealed string ToString() {
			return base.ToString() + " [" + TechType + "] / " + ClassID + " / " + PrefabFileName + " in " + GroupForPDA + "/" + CategoryForPDA;
		}

		public override sealed Vector2int SizeInInventory {
			get {
				return CraftData.GetItemSize(basis);
			}
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

		protected override sealed TechData GetBlueprintRecipe() {
			return null;
		}

		public Assembly getOwnerMod() {
			return ownerMod;
		}

		public bool allowTechUnlockPopups() {
			return allowUnlockPopups;
		}
	}

	public interface DuplicateItemDelegate {

		string getNameSuffix();

		PdaItem getPrefab();

		TechType getBasis();

		string getTooltip();

		Assembly getOwnerMod();

		bool allowTechUnlockPopups();

	}
}
