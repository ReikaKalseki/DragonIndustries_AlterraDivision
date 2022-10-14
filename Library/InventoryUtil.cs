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
		
		public static bool vehicleHasUpgrade(Vehicle v, TechType tt) {
			foreach (int idx in v.slotIndexes.Values) {
				InventoryItem ii = v.GetSlotItem(idx);
				if (ii != null && ii.item && ii.item.GetTechType() == tt)
					return true;
			}
			return false;
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
	    	Equipment modules = sub.isCyclops && sub.upgradeConsole ? sub.upgradeConsole.modules : null;
	    	if (modules != null) {
		    	foreach (string slot in SubRoot.slotNames) {
					TechType tt2 = modules.GetTechTypeInSlot(slot);
					if (tt == tt2)
						return true;
				}
	    	}
			return false;
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
		
	}
}
