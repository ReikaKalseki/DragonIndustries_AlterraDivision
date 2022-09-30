using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace ReikaKalseki.DIAlterra
{
	public abstract class SNMod {
		
		public readonly Assembly modDLL = SNUtil.tryGetModDLL();
		public readonly string displayName;
	
		public abstract void loadConfig();
		public abstract void afterConfig();
		public abstract void doPatches();		
		public abstract void addItems();		
		public abstract void loadMain();
		public abstract void loadModInteract();
		public abstract void loadFinal();
		
		protected SNMod(string n) {
			displayName = n;
		}
		
	}
}
