using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using Story;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{	
	public class ModLogger {
		
		private readonly Assembly ownerMod;
		private readonly string prefixString;
		
		public ModLogger(string pfx = null) {
			ownerMod = Assembly.GetCallingAssembly();
			prefixString = pfx != null ? pfx : ownerMod.GetName().Name.ToUpperInvariant().Replace("PLUGIN_", "");
		}
		
		public void log(string s, int indent = 0) {
			while (s.Length > 4096) {
				string part = s.Substring(0, 4096);
				log(part);
				s = s.Substring(4096);
			}
			if (indent > 0) {
				s = s.PadLeft(s.Length+indent, ' ');
			}
			UnityEngine.Debug.Log(prefixString+": "+s);
		}
		
	}
}
