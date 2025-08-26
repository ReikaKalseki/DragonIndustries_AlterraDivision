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

	public class UsableItemRegistry {

		public static readonly UsableItemRegistry instance = new UsableItemRegistry();

		private readonly Dictionary<TechType, Func<Survival, GameObject, bool>> actions = new Dictionary<TechType, Func<Survival, GameObject, bool>>();

		private float lastUse = -1;

		private UsableItemRegistry() {/*
	    	addUsableItem(TechType.Bladderfish, (s, go) => {
	    		Player.main.GetComponent<OxygenManager>().AddOxygen(15f);
	    	    return true;
			});*/
			this.addUsableItem(TechType.FirstAidKit, (s, go) => {
				return Player.main.GetComponent<LiveMixin>().AddHealth(50f) > 0.1f;
			});
			this.addUsableItem(TechType.EnzymeCureBall, (s, go) => {
				Debug.LogWarningFormat(s, "Code should be unreachable for the time being.", Array.Empty<object>());
				InfectedMixin component2 = global::Utils.GetLocalPlayer().gameObject.GetComponent<InfectedMixin>();
				if (component2.IsInfected()) {
					component2.RemoveInfection();
					global::Utils.PlayFMODAsset(s.curedSound, s.transform, 20f);
					return true;
				}
				return false;
			});
		}

		public void addUsableItem(TechType item, Func<Survival, GameObject, bool> onUse) {
			actions[item] = onUse;
		}

		public bool isUsable(TechType tt) {
			return actions.ContainsKey(tt);
		}

		public bool use(TechType tt, Survival s, GameObject go) {
			if (DayNightCycle.main.timePassedAsFloat - lastUse < 0.5) {
				SNUtil.writeToChat("Prevented duplicate use of item " + tt);
				return false;
			}
			lastUse = DayNightCycle.main.timePassedAsFloat;
			bool ret = actions[tt].Invoke(s, go);
			if (ret) {
				ConsumableTracker.instance.onConsume(go, false);
				Inventory.main.container.RemoveItem(go.GetComponent<Pickupable>(), true);
			}
			return ret;
		}
	}

}
