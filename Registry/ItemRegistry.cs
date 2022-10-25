﻿using System;
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
				SNUtil.log("Fetching item '"+id+"'", SNUtil.tryGetModDLL());
				return registry[id];
			}
			else {
				SNUtil.log("Could not find item '"+id+"'", SNUtil.tryGetModDLL());
				return null;
			}
		}
		
		public Spawnable getItem(TechType tt) {
			if (registryTech.ContainsKey(tt)) {
				SNUtil.log("Fetching item '"+tt+"'", SNUtil.tryGetModDLL());
				return registryTech[tt];
			}
			else {
				SNUtil.log("Could not find item '"+tt+"'", SNUtil.tryGetModDLL());
				return null;
			}
		}
		
		public void addItem(Spawnable di) {
			registry[di.ClassID] = di;
			registryTech[di.TechType] = di;
		}
		
	}
}
