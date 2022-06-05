﻿using System;
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
		private static readonly Dictionary<TechType, Databox> databoxes = new Dictionary<TechType, Databox>();
		private static readonly Dictionary<TechType, Crate> crates = new Dictionary<TechType, Crate>();
		
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
			return registerWorldgen(getOrCreateDatabox(tech), pos, rot);
		}
		
		public static SpawnInfo spawnPDA(Vector3 pos, PDAManager.PDAPage page, Vector3? rot = null) {
			return spawnPDA(pos, page, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnPDA(Vector3 pos, PDAManager.PDAPage page, Quaternion? rot = null) {
			return registerWorldgen(page.getPDAClassID(), pos, rot);
		}
		
		public static SpawnInfo spawnItemCrate(Vector3 pos, TechType item, Vector3? rot = null) {
			return spawnItemCrate(pos, item, Quaternion.Euler(getOrZero(rot)));
		}
		
		public static SpawnInfo spawnItemCrate(Vector3 pos, TechType item, Quaternion? rot = null) {
			return registerWorldgen(getOrCreateCrate(item), pos, rot);
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
		
		public static string getOrCreateCrate(TechType tech) {
			Crate box = crates.ContainsKey(tech) ? crates[tech] : null;
			if (box == null) {
				box = new Crate(tech, "580154dd-b2a3-4da1-be14-9a22e20385c8"); //battery
				crates[tech] = box;
				box.Patch();
			}
			return box.ClassID;
		}
		
		public static string getOrCreateDatabox(TechType tech) {
			Databox box = databoxes.ContainsKey(tech) ? databoxes[tech] : null;
			if (box == null) {
				box = new Databox(tech, "1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6"); //compass databox
				databoxes[tech] = box;
				box.Patch();
			}
			return box.ClassID;
		}
		
		abstract class ContainerPrefab : Spawnable, DIPrefab<StringPrefabContainer> {
	        
			internal readonly TechType containedTech;
		
			public float glowIntensity {get; set;}		
			public StringPrefabContainer baseTemplate {get; set;}
	        
	        internal ContainerPrefab(TechType tech, string template) : base("container_"+tech, "", "") {
				containedTech = tech;
				baseTemplate = new StringPrefabContainer(template);
	        }
			
	        public override GameObject GetGameObject() {
				return SBUtil.getModPrefabBaseObject<StringPrefabContainer>(this);
	        }
			
			public bool isResource() {
				return false;
			}
			
			public string getTextureFolder() {
				return null;
			}
			
			public abstract void prepareGameObject(GameObject go, Renderer r);
			
		}
		
		class Databox : ContainerPrefab {
	        
	        internal Databox(TechType tech, string template) : base(tech, template) {
				
	        }
			
			public override void prepareGameObject(GameObject go, Renderer r) {
	            BlueprintHandTarget bpt = go.EnsureComponent<BlueprintHandTarget>();
	            bpt.unlockTechType = containedTech;
	            bpt.primaryTooltip = containedTech.AsString();
				string arg = Language.main.Get(containedTech);
				string arg2 = Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(containedTech));
				bpt.secondaryTooltip = Language.main.GetFormat<string, string>("DataboxToolipFormat", arg, arg2);
				bpt.alreadyUnlockedTooltip = Language.main.GetFormat<string, string>("DataboxAlreadyUnlockedToolipFormat", arg, arg2);
			}
			
		}
		
		class Crate : ContainerPrefab {
	        
	        internal Crate(TechType tech, string template) : base(tech, template) {
				
	        }
			
			public override void prepareGameObject(GameObject go, Renderer r) {
				PrefabPlaceholdersGroup pre = go.EnsureComponent<PrefabPlaceholdersGroup>();
				if (pre.prefabPlaceholders.Length != 1) {
					pre.prefabPlaceholders = new PrefabPlaceholder[1];
					pre.prefabPlaceholders[0] = go.AddComponent<PrefabPlaceholder>();
				}
				pre.prefabPlaceholders[0].prefabClassId = CraftData.GetClassIdForTechType(containedTech);
				pre.prefabPlaceholders[0].highPriority = true;
			}
			
		}
		
	}
}
