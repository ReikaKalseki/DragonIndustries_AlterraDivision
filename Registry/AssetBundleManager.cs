using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public static class AssetBundleManager {

		private static readonly Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();

		static AssetBundleManager() {

		}

		public static AssetBundle getBundle(Assembly a, string path) {
			if (!bundles.ContainsKey(path)) {
				bundles[path] = loadBundle(a, path);
				SNUtil.log("Loaded AssetBundle '" + path + "': ");
				foreach (System.Object obj in bundles[path].LoadAllAssets()) {
					SNUtil.log(" > " + obj);
				}
			}
			return bundles[path];
		}

		private static AssetBundle loadBundle(Assembly a, string relative) {
			string path = Path.Combine(Path.GetDirectoryName(a.Location), "Assets", relative);
			AssetBundle ret = AssetBundle.LoadFromFile(path);
			return !ret ? throw new Exception("Asset bundle not found at path '" + path + "'") : ret;
		}

	}
}
