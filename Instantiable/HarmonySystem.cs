using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using HarmonyLib;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
	public class HarmonySystem {

		public readonly Harmony harmonyInstance;
		public readonly Type patchHolder;
		public readonly Assembly owner;

		public HarmonySystem(string id, Assembly modDLL, Type p) { //2.0 take a SNMod as an argument and get those proeprties from it
			harmonyInstance = new Harmony(id);
			patchHolder = p;
			owner = modDLL;
			Harmony.DEBUG = true;
			FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log.txt");
		}

		public void apply() {
			string msg = "Ran "+harmonyInstance.Id+" mod register, started harmony";
			FileLog.Log(msg + " (harmony log)");
			SNUtil.log(msg);
			try {
				if (File.Exists(FileLog.logPath))
					File.Delete(FileLog.logPath);
			}
			catch (Exception ex) {
				SNUtil.log("Could not clean up harmony log: " + ex);
			}
			try {
				InstructionHandlers.runPatchesIn(harmonyInstance, patchHolder);
			}
			catch (Exception ex) {
				FileLog.Log("Caught exception when running " + harmonyInstance.Id + " patchers!");
				FileLog.Log(ex.Message);
				FileLog.Log(ex.StackTrace);
				FileLog.Log(ex.ToString());
			}
		}


	}
}
