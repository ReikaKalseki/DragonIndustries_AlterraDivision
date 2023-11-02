using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {
	
	public class IrreplaceableItemRegistry {
		
		public static readonly IrreplaceableItemRegistry instance = new IrreplaceableItemRegistry();
	    
	    private readonly HashSet<TechType> items = new HashSet<TechType>();
		
		private IrreplaceableItemRegistry() {
	    	
		}
	    
	    public void registerItem(ModPrefab item) {
	    	registerItem(item.TechType);
	    }
	    
	    public void registerItem(TechType item) {
	    	items.Add(item);
	    }
	    
	    public bool isIrreplaceable(TechType tt) {
	    	return items.Contains(tt);
	    }
	}
	
}
