using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {

	public class IrreplaceableItemRegistry {

		public static readonly IrreplaceableItemRegistry instance = new IrreplaceableItemRegistry();

		private readonly Dictionary<TechType, IrreplaceableItemData> items = new Dictionary<TechType, IrreplaceableItemData>();

		public static readonly Action<InventoryItem> DROP_ITEM = (ii) => {
			Pickupable pp = ii.item;
			pp.Drop();
			pp.GetComponent<Rigidbody>().isKinematic = true;
		};

		public static readonly Action<InventoryItem> COLLECT_ITEM = (ii) => Inventory.main.ForcePickup(ii.item);

		private static readonly IrreplaceableItemData DEFAULT = new IrreplaceableItemData(
			(pp, notify) => {
				if (notify)
					ErrorMessage.AddError(Language.main.Get("ItemNotDroppable"));
				return false;
			},
			ii => ii.item.destroyOnDeath = false,
			(v, ii) => COLLECT_ITEM.Invoke(ii)
		);

		private IrreplaceableItemRegistry() {

		}

		public void registerItem(ModPrefab item, IrreplaceableItemData data = null) {
			this.registerItem(item.TechType, data);
		}

		public void registerItem(TechType item, IrreplaceableItemData data = null) {
			items[item] = data == null ? DEFAULT : data;
		}

		public bool isIrreplaceable(TechType tt) {
			return items.ContainsKey(tt);
		}
		public IrreplaceableItemData getEffects(TechType tt) {
			return items.ContainsKey(tt) ? items[tt] : null;
		}

		public class IrreplaceableItemData {

			public readonly Func<Pickupable, bool, bool> onAttemptToDrop;
			public readonly Action<InventoryItem> onDiedWhileHolding;
			public readonly Action<Vehicle, InventoryItem> onLostWithVehicle;

			public IrreplaceableItemData(Func<Pickupable, bool, bool> onDrop, Action<InventoryItem> onDie, Action<Vehicle, InventoryItem> loseVehicle) {
				onAttemptToDrop = onDrop;
				onDiedWhileHolding = onDie;
				onLostWithVehicle = loseVehicle;
			}

		}
	}

}
