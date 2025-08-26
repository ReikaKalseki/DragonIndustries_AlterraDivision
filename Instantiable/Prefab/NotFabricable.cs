using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public class NotFabricable : BasicCraftingItem {

		public NotFabricable(XMLLocale.LocaleEntry e, string template) : base(e, template) {

		}

		public NotFabricable(string id, string name, string desc, string template) : base(id, name, desc, template) {

		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.None;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Uncategorized;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Misc;
			}
		}

	}
}
