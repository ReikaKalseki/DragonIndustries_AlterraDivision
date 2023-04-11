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
	   
		public void triggerDirectUnlock(TechType tt) {
	    	if (!directUnlocks.ContainsKey(tt))
	   			return;
	    	List<TechType> li = directUnlocks[tt];
	    	if (li == null || li.Count == 0)
	    		return;
	    	SNUtil.log("Triggering direct unlock via "+tt+" of "+li.Count+":["+string.Join(", ", li.Select<TechType, string>(tc => ""+tc))+"]", SNUtil.diDLL);
	    	
	    	if (DIHooks.getWorldAge() > 0.25F) {
		    	List<TechType> li2 = new List<TechType>();
		    	foreach (TechType tt2 in li) {
		    		if (KnownTech.Contains(tt2))
		    			continue;
		    		if (DuplicateRecipeDelegate.isDelegateItem(tt2) && !DuplicateRecipeDelegate.getDelegateFromTech(tt2).allowTechUnlockPopups())
		    			continue;
		    		SNUtil.log("Raising progression popup for "+tt2, SNUtil.diDLL);
		    		li2.Add(tt2);
		    	}
		    	if (li2.Count > 1)
		    		SNUtil.triggerMultiTechPopup(li2);
		    	else if (li2.Count == 1)
		    		SNUtil.triggerTechPopup(li2[0]);
	    	}
	    	
		   	foreach (TechType unlock in li) {
		   		if (!KnownTech.Contains(unlock)) {
		        	KnownTech.Add(unlock);
		    	}
		   	}
		}
	}
	
}
