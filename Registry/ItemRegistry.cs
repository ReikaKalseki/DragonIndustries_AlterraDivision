using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public class ItemRegistry {
		
		public static readonly ItemRegistry instance = new ItemRegistry();
		
		private readonly Dictionary<string, Spawnable> registry = new Dictionary<string, Spawnable>();
		private readonly Dictionary<TechType, Spawnable> registryTech = new Dictionary<TechType, Spawnable>();
		
		private ItemRegistry() {
			
		}
		
		public Spawnable getItem(string id) {
			if (registry.ContainsKey(id)) {
				SNUtil.log("Fetching item '"+id+"'", SNUtil.tryGetModDLL(true));
				return registry[id];
			}
			else {
				SNUtil.log("Could not find item '"+id+"'", SNUtil.tryGetModDLL(true));
				return null;
			}
		}
		
		public Spawnable getItem(TechType tt, bool doLog = true) {
			if (registryTech.ContainsKey(tt)) {
				if (doLog)
					SNUtil.log("Fetching item '"+tt+"'", SNUtil.tryGetModDLL(true));
				return registryTech[tt];
			}
			else {
				if (doLog)
					SNUtil.log("Could not find item '"+tt+"'", SNUtil.tryGetModDLL(true));
				return null;
			}
		}
		
		public void addItem(Spawnable di) {
			registry[di.ClassID] = di;
			registryTech[di.TechType] = di;
			SNUtil.log("Registering item '"+di+"'", SNUtil.tryGetModDLL(true));
		}
		
	}
}
