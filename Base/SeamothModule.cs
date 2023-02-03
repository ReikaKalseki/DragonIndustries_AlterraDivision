using System;
using System.Collections.Generic;
using System.Linq;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public abstract class SeamothModule : CustomEquipable {
		
		private static readonly Dictionary<TechType, SeamothModuleStorage> storageHandlers = new Dictionary<TechType, SeamothModuleStorage>();
		
		static SeamothModule() {
			storageHandlers[TechType.SeamothTorpedoModule] = new SeamothModuleStorage(null, -1, -1){localeKey = "SeamothTorpedoStorage"};
		}
		
		internal static SeamothModuleStorage getStorageHandler(TechType item) {
			return storageHandlers.ContainsKey(item) ? storageHandlers[item] : null;
		}
		
		internal static void updateLocale() {
			foreach (KeyValuePair<TechType, SeamothModuleStorage> kvp in storageHandlers) {
				if (!string.IsNullOrEmpty(kvp.Value.localeKey) && !string.IsNullOrEmpty(kvp.Value.localizedHoverText)) {
					LanguageHandler.SetLanguageLine(kvp.Value.localeKey, kvp.Value.localizedHoverText);
					SNUtil.log("Relocalized seamoth module tooltip "+kvp.Value.localeKey+" > "+kvp.Value.localizedHoverText, SNUtil.diDLL);
				}
			}
		}
		
		protected SeamothModule(XMLLocale.LocaleEntry e, string template = "92b6424f-7635-4e61-990e-3c40bfad6e9a") : this(e.key, e.name, e.desc, template) {
			
		}
		
		protected SeamothModule(string id, string name, string desc, string template = "92b6424f-7635-4e61-990e-3c40bfad6e9a") : base(id, name, desc, template) { //SeamothElectricalDefense
			dependency = TechType.BaseUpgradeConsole;
			
			OnFinishedPatching += () => {
				if (QuickSlotType == QuickSlotType.Chargeable || QuickSlotType == QuickSlotType.SelectableChargeable) {
					CraftData.maxCharges[TechType] = getMaxCharge();
					CraftData.energyCost[TechType] = getChargingPowerCost();
				}
				SeamothModuleStorage s = getStorage();
				if (s != null) {
					storageHandlers[TechType] = s;
					s.localeKey = "SeamothModuleStorageAccess_"+id;
					s.localizedHoverText = "Access "+name+" storage";
				}
			};
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			SeamothModuleStorage s = getStorage();
			if (s != null) {
				SeamothStorageContainer storage = go.GetComponent<SeamothStorageContainer>();
				if (storage) {
					s.apply(storage);
				}
			}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.SeamothUpgrades;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"SeamothMenu"};//return new string[]{"DISeamoth"};//new string[]{"SeamothModules"};
			}
		}
		
		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.SeamothModule;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.VehicleUpgrades;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.VehicleUpgrades;
			}
		}
		
		protected virtual float getMaxCharge() {
			return CraftData.GetQuickSlotMaxCharge(TechType.SeamothElectricalDefense);
		}
		
		protected virtual float getChargingPowerCost() {
			return 1;
		}
		
		public virtual float getUsageCooldown() {
			return 0;
		}
		
		public virtual SeamothModuleStorage getStorage() {
			return null;
		}
		
		public virtual void onFired(SeaMoth sm, int slotID, float charge) { //charge is 0-1
			
		}
		
		public class SeamothModuleStorage {
			
			public readonly string title;
			public readonly int width;
			public readonly int height;
			private readonly Action<SeamothStorageContainer> additionalModifications;
			public readonly List<TechType> allowedAmmo = new List<TechType>();
			
			public string localeKey {get; internal set;}
			public string localizedHoverText {get; internal set;}
			
			public SeamothModuleStorage(string s, int w, int h) : this(s, w, h, null) {
				
			}
			
			public SeamothModuleStorage(string s, int w, int h, Action<SeamothStorageContainer> a) {
				title = s;
				width = w;
				height = h;
				additionalModifications = a;
			}
			
			internal void apply(SeamothStorageContainer storage) {
				if (title != null)
					storage.storageLabel = title.ToUpperInvariant();
				if (width > 0) {
					storage.width = width;
					//storage.container.sizeX = storage.width;
				}
				if (height > 0) {
					storage.height = height;
					//storage.container.sizeY = storage.height;
				}
				if (height > 0 || width > 0)
					storage.container.Resize(width > 0 ? width : storage.container.sizeX, height > 0 ? height : storage.container.sizeY);
				if (allowedAmmo.Count > 0) {
					storage.allowedTech = allowedAmmo.ToArray();
					storage.container.SetAllowedTechTypes(storage.allowedTech);
				}
			}
			
			public SeamothModuleStorage addAmmo(PrefabReference s) {
				return addAmmo(CraftData.entClassTechTable[s.getPrefabID()]);
			}
			
			public SeamothModuleStorage addAmmo(Spawnable s) {
				return addAmmo(s.TechType);
			}
			
			public SeamothModuleStorage addAmmo(TechType tt) {
				allowedAmmo.Add(tt);
				return this;
			}
		}
	}
}
