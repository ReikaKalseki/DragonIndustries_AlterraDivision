using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{
	public abstract class Biome {
		
		public readonly string biomeName;
		
		protected Biome(string name) {
			biomeName = name;
		}
		
		public abstract VanillaMusic[] getMusicOptions();
		public abstract Color getFogColor();
		public abstract float getSunIntensity();
		public abstract float getFogDensity();
	}
}
