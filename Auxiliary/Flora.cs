using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public interface Flora {

		string getPrefabID();
		bool isNativeToBiome(Vector3 pos);
		bool isNativeToBiome(BiomeBase b, bool cave);

	}
}
