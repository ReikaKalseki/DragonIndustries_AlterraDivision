using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class GenUtil {
		
		public static void registerWorldgen(string prefab, Vector3 pos) {
			registerWorldgen(prefab, pos, Vector3.zero);
		}
		
		public static void registerWorldgen(string prefab, Vector3 pos, Vector3 rot) {
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(prefab, pos, rot));
		}
		
		public static void spawnDatabox(Vector3 pos) {
			spawnDatabox(pos, Vector3.zero);
		}
		
		public static void spawnDatabox(Vector3 pos, Vector3 rot) {
			registerWorldgen("1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6", pos, rot); //compass databox
		}
		
		public static void spawnItemCrate(Vector3 pos, Vector3 rot) {
			registerWorldgen("580154dd-b2a3-4da1-be14-9a22e20385c8", pos, rot); //battery
		}
		
	}
}
