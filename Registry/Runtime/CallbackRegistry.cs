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
	
	[Obsolete("This can be done with UnityEvent<StructThatHasArgsAndReturn>")]
	public class CallbackRegistry { //since setting a Func<> or similar in a prefab does not work since Instantiate cannot copy it, but it CAN copy a string reference key
		
		public static readonly CallbackRegistry instance = new CallbackRegistry();
	    
		private readonly Dictionary<string, Callback> calls = new Dictionary<string, Callback>();
		
		private CallbackRegistry() {
			
		}
		
		public void registerCallback(string key, Callback c) {
			if (calls.ContainsKey(key))
				throw new Exception("Callback '"+key+"' already registered to "+calls[key]+"!");
			calls[key] = c;
		}
		
		public class Callback {
			
		}
		
		public class Callback<C> : Callback {
			
			public C currentValue;
			
		}
	}
	
}
