using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {
	
	public interface Flora {
			
		string getPrefabID();
		bool isNativeToBiome(Vector3 pos);
		bool isNativeToBiome(BiomeBase b, bool cave);
		
	}
}
