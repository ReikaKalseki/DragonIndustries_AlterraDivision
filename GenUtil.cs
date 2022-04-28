using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class GenUtil {
		
		public static SpawnInfo registerWorldgen(PositionedPrefab pfb, Action<GameObject> call = null) {
			return registerWorldgen(pfb.prefabName, pfb.position, pfb.rotation, call);
		}
		
		public static SpawnInfo registerWorldgen(string prefab, Vector3 pos, Vector3? rot = null, Action<GameObject> call = null) {
			return registerWorldgen(prefab, pos, Quaternion.Euler(getOrZero(rot)), call);
		}
		
		public static SpawnInfo registerWorldgen(string prefab, Vector3 pos, Quaternion? rot = null, Action<GameObject> call = null) {
			SpawnInfo info = new SpawnInfo(prefab, pos, getOrIdentity(rot), call);
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			return info;
		}
		
		public static SpawnInfo registerWorldgen(WorldGenerator gen) {
			Action<GameObject> call = go => {
				UnityEngine.Object.Destroy(go);
				gen.generate(new List<GameObject>());
			};
			SpawnInfo info = new SpawnInfo(VanillaResources.LIMESTONE.prefab, gen.position, call);
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			return info;
		}
		
		public static SpawnInfo spawnDatabox(Vector3 pos, TechType tech, Vector3? rot = null) {
			return spawnDatabox(pos, tech, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnDatabox(Vector3 pos, TechType tech, Quaternion? rot = null) {
			Action<GameObject> call = go => SBUtil.setDatabox(go.EnsureComponent<BlueprintHandTarget>(), tech);
			return registerWorldgen("1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6", pos, rot, call); //compass databox
		}
		
		public static SpawnInfo spawnItemCrate(Vector3 pos, TechType item, Vector3? rot = null) {
			return spawnItemCrate(pos, item, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnItemCrate(Vector3 pos, TechType item, Quaternion? rot = null) {
			Action<GameObject> call = go => SBUtil.setCrateItem(go.EnsureComponent<SupplyCrate>(), item);
			return registerWorldgen("580154dd-b2a3-4da1-be14-9a22e20385c8", pos, rot, call); //battery
		}
		
		public static SpawnInfo spawnResource(VanillaResources res, Vector3 pos, Vector3? rot = null) {
			return registerWorldgen(res.prefab, pos, rot);
		}
		
		public static SpawnInfo spawnTechType(TechType tech, Vector3 pos, Vector3? rot = null) {
			SpawnInfo info = new SpawnInfo(tech, pos, getOrZero(rot));
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			return info;
		}
		
		public static Vector3 getOrZero(Vector3? init) {
			return init != null && init.HasValue ? init.Value : Vector3.zero;
		}
		
		public static Quaternion getOrIdentity(Quaternion? init) {
			return init != null && init.HasValue ? init.Value : Quaternion.identity;
		}
		
	}
}
