using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public static class InventoryUtil {

		public static List<TechType> getVehicleUpgrades(Vehicle v) {
			List<TechType> set = new List<TechType>();
			foreach (int idx in v.slotIndexes.Values) {
				InventoryItem ii = v.GetSlotItem(idx);
				if (ii != null && ii.item)
					set.Add(ii.item.GetTechType());
			}
			return set;
		}

		public static bool vehicleHasUpgrade(Vehicle v, TechType tt) {/*
			foreach (int idx in v.slotIndexes.Values) {
				InventoryItem ii = v.GetSlotItem(idx);
				if (ii != null && ii.item && ii.item.GetTechType() == tt)
					return true;
			}
			return false;*/
			return v.modules.GetCount(tt) > 0;
		}

		public static bool isVehicleUpgradeSelected(Vehicle v, TechType tt) {
			if (!v || v.activeSlot < 0)
				return false;
			InventoryItem ii = v.GetSlotItem(v.activeSlot);
			return ii != null && ii.item.GetTechType() == tt;
		}

		public static List<TechType> getCyclopsUpgrades(SubRoot sub) {
			List<TechType> set = new List<TechType>();
			Equipment modules = sub.isCyclops && sub.upgradeConsole ? sub.upgradeConsole.modules : null;
			if (modules != null) {
				foreach (string slot in SubRoot.slotNames) {
					TechType tt = modules.GetTechTypeInSlot(slot);
					if (tt != TechType.None)
						set.Add(tt);
				}
			}
			return set;
		}

		public static bool cyclopsHasUpgrade(SubRoot sub, TechType tt) {
			Equipment modules = sub.isCyclops && sub.upgradeConsole ? sub.upgradeConsole.modules : null;/*
	    	if (modules != null) {
		    	foreach (string slot in SubRoot.slotNames) {
					TechType tt2 = modules.GetTechTypeInSlot(slot);
					if (tt == tt2)
						return true;
				}
	    	}
			return false;*/
			return modules != null && modules.GetCount(tt) > 0;
		}

		public static List<Battery> getCyclopsPowerCells(SubRoot sub) {
			if (!sub.isCyclops)
				return null;
			List<Battery> ret = new List<Battery>();
			foreach (IPowerInterface p in sub.powerRelay.inboundPowerSources) {
				if (p is BatterySource b)
					ret.Add((Battery)b.battery);
			}
			return ret;
		}

		public static void addItem(TechType tt) {
			GameObject obj = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab(tt));
			if (!obj) {
				SNUtil.writeToChat("Could not spawn item " + tt + ", no prefab");
				return;
			}
			obj.SetActive(false);
			Pickupable pp = obj.GetComponent<Pickupable>();
			if (!pp) {
				SNUtil.writeToChat("Could not add " + Language.main.Get(tt) + " to inventory - no Pickupable");
				return;
			}
			Inventory.main.ForcePickup(pp);
		}
		/*
		public static bool removeItem(ItemsContainer sc, InventoryItem ii) {
			return sc.DestroyItem(ii.item.GetTechType());
		}*/

		public static bool forceRemoveItem(StorageContainer sc, Pickupable pp) {
			return forceRemoveItem(sc.container, getItem(sc.container, pp));
		}

		public static bool forceRemoveItem(StorageContainer sc, InventoryItem ii) {
			return forceRemoveItem(sc.container, ii);
		}

		public static bool forceRemoveItem(ItemsContainer sc, InventoryItem ii) {
			if (sc.RemoveItem(ii.item, true)) {
				ii.item.gameObject.destroy(false);
				return true;
			}
			return false;
		}

		public static void forEachOfType(ItemsContainer sc, TechType tt, Action<InventoryItem> act) {
			IList<InventoryItem> il = sc.GetItems(tt);
			if (il == null || il.Count == 0)
				return;
			List<InventoryItem> li = new List<InventoryItem>(il); //recache since may be removing
			foreach (InventoryItem ii in li) {
				if (ii != null && ii.item)
					act.Invoke(ii);
			}
		}

		public static void forEach(ItemsContainer sc, Action<InventoryItem> act) {
			foreach (KeyValuePair<TechType, ItemsContainer.ItemGroup> kvp in sc._items) {
				foreach (InventoryItem ii in kvp.Value.items) {
					if (ii != null && ii.item)
						act.Invoke(ii);
				}
			}
		}

		public static InventoryItem getItem(ItemsContainer sc, Pickupable pp) {
			return getItem(sc, pp.GetTechType(), ii => ii.item == pp);
		}

		public static InventoryItem getItem(ItemsContainer sc, TechType tt, Predicate<InventoryItem> acceptor = null) {
			IList<InventoryItem> il = sc.GetItems(tt);
			if (il == null || il.Count == 0)
				return null;
			foreach (InventoryItem ii in il) {
				if (ii != null && (acceptor == null || acceptor.Invoke(ii)))
					return ii;
			}
			return null;
		}

		public static IEnumerable<EnergyMixin> getAllHeldChargeables(bool heldOnly = false, bool includeHeld = true) {
			List<EnergyMixin> li = new List<EnergyMixin>();
			if (includeHeld) {
				PlayerTool pt = Inventory.main.GetHeldTool();
				if (pt && pt.energyMixin)
					li.Add(pt.energyMixin);
			}
			if (!heldOnly) {
				li.AddRange(Inventory.main.storageRoot.GetComponentsInChildren<PlayerTool>().Select(t => t ? t.energyMixin : null).Where(e => (bool)e));
			}
			return li;
		}

		public static int getActiveQuickslot() {
			InventoryItem held = Inventory.main.quickSlots.heldItem;
			for (int i = 0; i < Inventory.main.quickSlots.binding.Length; i++) {
				InventoryItem ii = Inventory.main.quickSlots.binding[i];
				if (ii == held)
					return i;
			}
			return -1;
		}
	}
}
