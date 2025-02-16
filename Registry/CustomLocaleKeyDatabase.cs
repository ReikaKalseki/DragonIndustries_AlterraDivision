using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	
	public static class CustomLocaleKeyDatabase {
		
		private static readonly Dictionary<string, string> localeKeys = new Dictionary<string, string>();
		
		public static void registerKeys(XMLLocale s) {
			foreach (XMLLocale.LocaleEntry e in s.getEntries()) {
				registerKey(e);
			}
		}
		
		public static void registerKey(XMLLocale.LocaleEntry e) {
			localeKeys[e.key] = e.desc;
			SNUtil.log("Mapped locale key '"+e.key+"' to \""+e.desc+"\"");
		}
		
		public static void registerKey(string key, string text) {
			localeKeys[key] = text;
		}
		
		public static void onLoad() {
			foreach (KeyValuePair<string, string> kvp in localeKeys)
	    		LanguageHandler.SetLanguageLine(kvp.Key, kvp.Value);
		}
		
	}
}
