using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class GenUtil {
		
		public static readonly Bounds allowableGenBounds = MathUtil.getBounds(-2299, -3100, -2299, 2299, 150, 2299);
		
		private static readonly HashSet<string> alreadyRegisteredGen = new HashSet<string>();
		
		public static void registerOreWorldgen(BasicCustomOre ore, BiomeType biome, int amt, float chance) {
			registerOreWorldgen(ore, ore.isLargeResource, biome, amt, chance);
		}
		
		public static void registerOreWorldgen(Spawnable sp, bool large, BiomeType biome, int amt, float chance) {
			registerSlotWorldgen(sp.ClassID, sp.PrefabFileName, sp.TechType, large, biome, amt, chance);
		}
		
		public static void registerSlotWorldgen(string id, string file, TechType tech, bool large, BiomeType biome, int amt, float chance) {
			if (alreadyRegisteredGen.Contains(id)) {
		        LootDistributionHandler.EditLootDistributionData(id, biome, chance, amt); //will add if not present
			}
			else {		        
				LootDistributionData.BiomeData b = new LootDistributionData.BiomeData{biome = biome, count = amt, probability = chance};
		        List<LootDistributionData.BiomeData> li = new List<LootDistributionData.BiomeData>{b};
		        UWE.WorldEntityInfo info = new UWE.WorldEntityInfo();
		        info.cellLevel = large ? LargeWorldEntity.CellLevel.Medium : LargeWorldEntity.CellLevel.Near;
		        info.classId = id;
		        info.localScale = Vector3.one;
		        info.slotType = large ? EntitySlot.Type.Medium : EntitySlot.Type.Small;
		        info.techType = tech;
		       	WorldEntityDatabaseHandler.AddCustomInfo(id, info);
		        LootDistributionHandler.AddLootDistributionData(id, file, li, info);
		        
				alreadyRegisteredGen.Add(id);
			}
		}
		
		public static SpawnInfo registerWorldgen(PositionedPrefab pfb, Action<GameObject> call = null) {
			return registerWorldgen(pfb.prefabName, pfb.position, pfb.rotation, call);
		}
		
		public static SpawnInfo registerWorldgen(string prefab, Vector3 pos, Vector3? rot = null, Action<GameObject> call = null) {
			return registerWorldgen(prefab, pos, Quaternion.Euler(getOrZero(rot)), call);
		}
		
		public static SpawnInfo registerWorldgen(string prefab, Vector3 pos, Quaternion? rot = null, Action<GameObject> call = null) {
			validateCoords(pos);
			SpawnInfo info = new SpawnInfo(prefab, pos, getOrIdentity(rot), call);
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			//SBUtil.log("Registering prefab "+prefab+" @ "+pos);
			return info;
		}
		
		public static SpawnInfo registerWorldgen(WorldGenerator gen) {
			validateCoords(gen.position);
			Action<GameObject> call = go => {
				UnityEngine.Object.Destroy(go);
				SBUtil.log("Running world generator "+gen);
				gen.generate(new List<GameObject>());
			};
			SpawnInfo info = new SpawnInfo(VanillaResources.LIMESTONE.prefab, gen.position, call);
			CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(info);
			SBUtil.log("Queuing world generator "+gen);
			return info;
		}
		
		public static SpawnInfo spawnDatabox(Vector3 pos, TechType tech, Vector3? rot = null) {
			return spawnDatabox(pos, tech, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnDatabox(Vector3 pos, TechType tech, Quaternion? rot = null) {
			Action<GameObject> call = go => SBUtil.setDatabox(go.EnsureComponent<BlueprintHandTarget>(), tech);
			return registerWorldgen("1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6", pos, rot, call); //compass databox
		}
		
		public static SpawnInfo spawnPDA(Vector3 pos, PDAManager.PDAPage page, Vector3? rot = null) {
			return spawnPDA(pos, page, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnPDA(Vector3 pos, PDAManager.PDAPage page, Quaternion? rot = null) {
			Action<GameObject> call = go => SBUtil.setPDAPage(go.EnsureComponent<StoryHandTarget>(), page);
			return registerWorldgen("0f1dd54e-b36e-40ca-aa85-d01df1e3e426", pos, rot, call); //blood kelp PDA
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
			validateCoords(pos);
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
		
		private static void validateCoords(Vector3 pos) {
			if (!allowableGenBounds.Contains(pos))
				throw new Exception("Registered worldgen is out of bounds @ "+pos+"; allowable range is "+allowableGenBounds.min+" > "+allowableGenBounds.max);
		}
		
	}
}
