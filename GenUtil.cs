using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class GenUtil {
		
		public static void registerWorldgen(PositionedPrefab pfb) {
			registerWorldgen(pfb.prefabName, pfb.position, pfb.rotation);
		}
		
		public static void registerWorldgen(string prefab, Vector3 pos, Vector3? rot = null) {
			registerWorldgen(prefab, pos, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static void registerWorldgen(string prefab, Vector3 pos, Quaternion? rot = null) {
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(prefab, pos, getOrIdentity(rot)));
		}
		
		public static void spawnDatabox(Vector3 pos, Vector3? rot = null) {
			spawnDatabox(pos, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static void spawnDatabox(Vector3 pos, Quaternion? rot = null) {
			registerWorldgen("1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6", pos, rot); //compass databox
		}
		
		public static void spawnItemCrate(Vector3 pos, Vector3? rot = null) {
			spawnItemCrate(pos, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static void spawnItemCrate(Vector3 pos, Quaternion? rot = null) {
			registerWorldgen("580154dd-b2a3-4da1-be14-9a22e20385c8", pos, rot); //battery
		}
		
		public static void spawnResource(VanillaResources res, Vector3 pos, Vector3? rot = null) {
			registerWorldgen(res.prefab, pos, rot);
		}
		
		public static void spawnTechType(TechType tech, Vector3 pos, Vector3? rot = null) {
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(tech, pos, getOrZero(rot)));
		}
		
		public static Vector3 getOrZero(Vector3? init) {
			return init != null && init.HasValue ? init.Value : Vector3.zero;
		}
		
		public static Quaternion getOrIdentity(Quaternion? init) {
			return init != null && init.HasValue ? init.Value : Quaternion.identity;
		}
		
	}
}
