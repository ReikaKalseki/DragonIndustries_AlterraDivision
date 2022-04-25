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
	public abstract class WorldGenerator {
		
		protected readonly Vector3 position;
		
		protected WorldGenerator(Vector3 pos) {
			position = pos;
		}
		
		public abstract void loadFromXML(XmlElement e);
		public abstract void saveToXML(XmlElement e);
		
		public abstract void generate();
	}
}
