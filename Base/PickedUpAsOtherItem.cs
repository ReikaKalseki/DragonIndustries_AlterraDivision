using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.DIAlterra {
	
	public class PickedUpAsOtherItem : Spawnable {
		
		protected readonly TechType template;
	        
		private static readonly Dictionary<TechType, List<PickedUpAsOtherItem>> items = new Dictionary<TechType, List<PickedUpAsOtherItem>>();
		private static readonly Dictionary<TechType, PickedUpAsOtherItem> techMap = new Dictionary<TechType, PickedUpAsOtherItem>();
		
		public PickedUpAsOtherItem(string classID, string baseTemplate) : this(classID, CraftData.entClassTechTable[baseTemplate]) {
			
	    }
	        
	    public PickedUpAsOtherItem(string classID, TechType tt) : base(classID, "", "") {
			template = tt;
			
			List<PickedUpAsOtherItem> li = items.ContainsKey(tt) ? items[tt] : new List<PickedUpAsOtherItem>();
			li.Add(this);
			items[tt] = li;
			
			OnFinishedPatching += () => {techMap[TechType] = this;};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = UnityEngine.Object.Instantiate(CraftData.GetPrefabForTechType(template));
			world.SetActive(true);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			Pickupable pp = world.EnsureComponent<Pickupable>();
			pp.SetTechTypeOverride(template);
			prepareGameObject(world);
			return world;
	    }
		
		protected virtual void prepareGameObject(GameObject go) {
			
		}
		
		public TechType getTemplate() {
			return template;
		}
		
		public virtual int getNumberCollectedAs() {
			return 1;
		}
		
		public static PickedUpAsOtherItem getPickedUpAsOther(TechType tt) {
			return techMap.ContainsKey(tt) ? techMap[tt] : null;
		}
		
		public static void updateLocale() {
			foreach (List<PickedUpAsOtherItem> li in items.Values) {
				foreach (PickedUpAsOtherItem d in li) {
					Language.main.strings[d.TechType.AsString()] = Language.main.strings[d.template.AsString()];
					Language.main.strings["Tooltip_"+d.TechType.AsString()] = Language.main.strings["Tooltip_"+d.template.AsString()];
				}
			}
		}
			
	}
}
