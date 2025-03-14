using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class InventoryUtil {
		
		public static HashSet<TechType> getVehicleUpgrades(Vehicle v) {
			HashSet<TechType> set = new HashSet<TechType>();
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
			return false;*/return v.modules.GetCount(tt) > 0;
		}
		
		public static bool isVehicleUpgradeSelected(Vehicle v, TechType tt) {
			if (!v || v.activeSlot < 0)
				return false;
			InventoryItem ii = v.GetSlotItem(v.activeSlot);
			return ii != null && ii.item.GetTechType() == tt;
		}
		
		public static HashSet<TechType> getCyclopsUpgrades(SubRoot sub) {
			HashSet<TechType> set = new HashSet<TechType>();
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
			foreach (BatterySource b in sub.powerRelay.inboundPowerSources) {
				ret.Add((Battery)b.battery);
			}
			return ret;
		}
		
		public static void addItem(TechType tt) {
			GameObject obj = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab(tt));
			if (!obj) {
				SNUtil.writeToChat("Could not spawn item "+tt+", no prefab");
				return;
			}
			obj.SetActive(false);
			Pickupable pp = obj.GetComponent<Pickupable>();
			if (!pp) {
				SNUtil.writeToChat("Could not add "+Language.main.Get(tt)+" to inventory - no Pickupable");
				return;
			}
			Inventory.main.ForcePickup(pp);
		}
		/*
		public static bool removeItem(ItemsContainer sc, InventoryItem ii) {
			return sc.DestroyItem(ii.item.GetTechType());
		}*/
		
		public static bool forceRemoveItem(StorageContainer sc, InventoryItem ii) {
			return forceRemoveItem(sc.container, ii);
		}
		
		public static bool forceRemoveItem(ItemsContainer sc, InventoryItem ii) {
			if (sc.RemoveItem(ii.item, true)) {
				UnityEngine.Object.Destroy(ii.item.gameObject);
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
			IList<InventoryItem> il = sc.GetItems(pp.GetTechType());
			if (il == null || il.Count == 0)
				return null;
			foreach (InventoryItem ii in il) {
				if (ii != null && ii.item == pp)
					return ii;
			}
			return null;
		}
		
	}
}
