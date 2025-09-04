using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
	public class TemporaryFloatingLocker : Spawnable {

		public TemporaryFloatingLocker() : base("TemporaryFloatingLocker", "Temporary Locker", "") {

		}

		public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("9d9ed0b0-df64-45ee-9b90-34386a98b233");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			StorageContainer sc = world.getChildObject("StorageContainer").EnsureComponent<StorageContainer>();
			sc.Resize(6, 10);
			return world;
		}

		public static void createFloatingLocker(Vector3 pos, IEnumerable<Pickupable> li) {
			GameObject go = ObjectUtil.createWorldObject(DIMod.floatingLocker.ClassID);
			go.transform.position = pos;
			go.transform.rotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			TemporaryLockerControlTag tag = go.EnsureComponent<TemporaryLockerControlTag>();
			tag.allowAdd = true;
			StorageContainer sc = go.GetComponentInChildren<StorageContainer>();
			foreach (Pickupable pp in li) {
				sc.container.AddItem(pp);
			}
			tag.allowAdd = false;
		}

		class TemporaryLockerControlTag : MonoBehaviour {

			public bool allowAdd;

			void Start() {
				StorageContainer sc = GetComponentInChildren<StorageContainer>();
				//sc.container.onAddItem += this.updateStoredItem;
				sc.container.onRemoveItem += ii => {
					Invoke("checkEmpty", 0.25F);
				};
				sc.container.isAllowedToAdd = new IsAllowedToAdd((pp, vb) => allowAdd);
			}

			internal void checkEmpty() {
				StorageContainer sc = GetComponentInChildren<StorageContainer>();
				if (sc.isEmpty()) {
					PDA pda = Player.main.GetPDA();
					if (pda && pda.isOpen && pda.ui && pda.ui.currentTab is uGUI_InventoryTab it && it.storage.container == sc.container)
						pda.Close();
					gameObject.destroy();
				}
			}
		}

	}
}
