using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
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
				this.log(part);
				s = s.Substring(4096);
			}
			if (indent > 0) {
				s = s.PadLeft(s.Length + indent, ' ');
			}
			UnityEngine.Debug.Log(prefixString + ": " + s);
		}

	}
}
