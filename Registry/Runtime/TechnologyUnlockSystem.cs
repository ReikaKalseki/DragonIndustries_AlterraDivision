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
	
	public class TechnologyUnlockSystem {
		
		public static readonly TechnologyUnlockSystem instance = new TechnologyUnlockSystem();
	    
	    private readonly Dictionary<TechType, List<TechType>> directUnlocks = new Dictionary<TechType, List<TechType>>();
		
		private TechnologyUnlockSystem() {
	    	
		}
	    
	    public void addDirectUnlock(TechType from, TechType to) {
	    	List<TechType> li = directUnlocks.ContainsKey(from) ? directUnlocks[from] : new List<TechType>();
	    	li.Add(to);
	    	directUnlocks[from] = li;
	    }
		
		public void onLogin() {
	    	foreach (TechType kvp in directUnlocks.Keys) {
	    		if (PDAScanner.complete.Contains(kvp)) {
	    			triggerDirectUnlock(kvp);
	    		}
	    	}
		}
	    
	    public void onBuilt(TechType tt) {
	    	
	    }
	   
		public void triggerDirectUnlock(TechType tt) {
	    	if (DIHooks.getWorldAge() <= 0.25F || !directUnlocks.ContainsKey(tt))
	   			return;
	   		bool any = false;
		   	foreach (TechType unlock in directUnlocks[tt]) {
		   		if (!KnownTech.Contains(unlock)) {
		        	KnownTech.Add(unlock);
		        	any = true;
		    	}
		   	}
	   		if (any) {
		   		SNUtil.log("Triggering direct unlock via "+tt+" of "+directUnlocks[tt].Count+":["+string.Join(", ", directUnlocks[tt].Select<TechType, string>(tc => ""+tc))+"]", SNUtil.diDLL);
		   		SNUtil.triggerTechPopup(tt);
	   		}
		}
	}
	
}
