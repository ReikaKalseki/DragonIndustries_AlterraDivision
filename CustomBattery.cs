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
			
		public CustomBattery(string id, string name, string desc, int cap) : base(id, name, desc, "WorldEntities/Tools/Battery") {
			capacity = cap;
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
		
	}
}
