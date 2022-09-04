using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public class CustomBattery : BasicCraftingItem {
		
		public readonly int capacity;
		
		public CustomBattery(XMLLocale.LocaleEntry e, int cap) : this(e.key, e.name, e.desc, cap) {
			
		}
			
		public CustomBattery(string id, string name, string desc, int cap) : base(id, name, desc, "d4bfebc0-a5e6-47d3-b4a7-d5e47f614ed6") {
			capacity = cap;
			sprite = TextureManager.getSprite("Textures/Items/"+ObjectUtil.formatFileName(this));
			OnFinishedPatching += () => {CraftDataHandler.SetEquipmentType(TechType, EquipmentType.BatteryCharger);};
		}

		public sealed override TechCategory CategoryForPDA {
			get {
				return TechCategory.Electronics;
			}
		}

		public sealed override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Resources", "Electronics"};//new string[]{"DIIntermediate"};
			}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<Battery>()._capacity = capacity;
			go.EnsureComponent<Battery>().charge = capacity;
			go.SetActive(false);
		}
		
	}
}
