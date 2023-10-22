using System;
using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	
	public interface CustomHarvestBehavior {
		
		bool canBeAutoharvested();
		GameObject tryHarvest(GameObject go);
		
	}
}
