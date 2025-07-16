using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.DIAlterra {
	
	public static class WorldgenLog {
		
		//private readonly string saveSlot;
		//private readonly string saveFile;
		
		private static readonly List<string> queue = new List<string>();
		
		static WorldgenLog() {
			//saveSlot = SNUtil.getCurrentSaveDir();
			//saveFile = Path.Combine(saveSlot, "Worldgen.log");
			
			IngameMenuHandler.Main.RegisterOnLoadEvent(queue.Clear);
			IngameMenuHandler.Main.RegisterOnSaveEvent(save);
		}
		
		public static void log(GameObject go) {
			PrefabIdentifier classID;
			TechType tt;
			log(go.name+" ("+ObjectUtil.tryGetObjectIdentifiers(go, out classID, out tt)+") @ "+go.transform.position+" / "+go.transform.eulerAngles);
		}
		
		public static void log(string s) {
			queue.Add(s);
			SNUtil.log(s, SNUtil.diDLL);
		}
		
		private static void save() {
			string file = Path.Combine(SNUtil.getCurrentSaveDir(), "Worldgen.log");
			File.AppendAllText(file, string.Join("\n", queue));
			queue.Clear();
		}

	}
	
}
