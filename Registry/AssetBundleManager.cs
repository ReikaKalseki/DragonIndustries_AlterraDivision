using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class AssetBundleManager {
		
		private static readonly Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
		
		static AssetBundleManager() {
			
		}
		
		public static AssetBundle getBundle(string path) {
			if (!bundles.ContainsKey(path)) {
				bundles[path] = loadBundle(path);
				SNUtil.log("Loaded AssetBundle '"+path+"': ");
			 	foreach (System.Object obj in bundles[path].LoadAllAssets()) {
			 		SNUtil.log(" > "+obj);
			 	}
			}
			return bundles[path];
		}
		
		private static AssetBundle loadBundle(string relative) {
			string path = Path.Combine(Path.GetDirectoryName(SNUtil.getModDLL().Location), "Assets", relative);
			return AssetBundle.LoadFromFile(path);
		}
		
	}
}
