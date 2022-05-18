using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public abstract class SeamothModule : CustomEquipable {
		
		protected SeamothModule(XMLLocale.LocaleEntry e) : this(e.key, e.name, e.desc) {

		}
		
		protected SeamothModule(string id, string name, string desc) : base(id, name, desc) {

		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.SeamothUpgrades;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"SeamothModules"};//return new string[]{"DISeamoth"};//new string[]{"SeamothModules"};
			}
		}
		
		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.SeamothModule;
			}
		}

		public override TechType RequiredForUnlock {
			get {
				return TechType.BaseMoonpool;
			}
		}

		public override sealed TechGroup GroupForPDA {
			get {
				return TechGroup.VehicleUpgrades;
			}
		}

		public override sealed TechCategory CategoryForPDA {
			get {
				return TechCategory.VehicleUpgrades;
			}
		}
		
		protected override sealed string getTemplatePrefab() {
			return "WorldEntities/Tools/SeamothElectricalDefense";
		}
	}
}
