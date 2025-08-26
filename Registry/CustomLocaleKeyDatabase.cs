using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
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
			registerKey(e.key, e.desc);
		}

		public static void registerKey(string key, string text) {
			SNUtil.log("Mapped locale key '" + key + "' to \"" + text + "\"", SNUtil.diDLL);
			if (DIHooks.hasWorldLoadStarted())
				LanguageHandler.SetLanguageLine(key, text);
			else
				localeKeys[key] = text;
		}

		public static void onLoad() {
			foreach (KeyValuePair<string, string> kvp in localeKeys)
				LanguageHandler.SetLanguageLine(kvp.Key, kvp.Value);
		}

	}
}
