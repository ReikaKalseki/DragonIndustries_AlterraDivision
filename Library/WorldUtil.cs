using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class WorldUtil {
		
		private static readonly Int3 batchOffset = new Int3(13, 19, 13);
		private static readonly Int3 batchOffsetM = new Int3(32, 0, 32);
		private static readonly int batchSize = 160;
		
		private static readonly Dictionary<string, string> biomeNames = new Dictionary<string, string>();
		
		static WorldUtil() {
			mapBiomeName("Safe Shallows", "safeShallows");
			mapBiomeName("Kelp Forest", "kelp");
			mapBiomeName("Grassy Plateaus", "grassy", "grassyPlateaus");
			mapBiomeName("Mushroom Forest", "mushroom");
			mapBiomeName("Grand Reef", "grandReef");
			mapBiomeName("Deep Grand Reef", "deepgrand");
			mapBiomeName("Bulb Zone", "koosh");
			mapBiomeName("Jellyshroom Caves", "jellyshroom");
			mapBiomeName("Underwater Islands", "Underwaterislands");
			mapBiomeName("Mountains", "mountains");
			mapBiomeName("Northern Blood Kelp", "bloodkelptwo");
			mapBiomeName("Blood Kelp Trench", "bloodkelp");
			mapBiomeName("Lost River", "LostRiver_BonesField_Corridor", "LostRiver_BonesField", "LostRiver_Corridor", "LostRiver_Junction", "LostRiver_GhostTree_Lower", "LostRiver_GhostTree", "LostRiver_Canyon", "LostRiver_TreeCove");
			mapBiomeName("Inactive Lava Zone", "ILZCorridor", "ILZCorridorDeep", "ILZChamber", "ILZChamber_Dragon", "LavaPit", "LavaFalls", "LavaCastle");
			mapBiomeName("Active Lava Zone", "LavaLakes");
			mapBiomeName("Dunes", "dunes");
			mapBiomeName("Crash Zone", "crash");
			mapBiomeName("Crater Edge", "void", "");
		}
		
		private static void mapBiomeName(string name, params string[] keys) {
			foreach (string s in keys) {
				biomeNames[s] = name;
			}
		}
		
		public static Int3 getBatch(Vector3 pos) { //"Therefore e.g. batch (12, 18, 12) covers the voxels from (-128, -160, -128) to (32, 0, 32)."
			Int3 coord = pos.roundToInt3();
			coord = coord-batchOffsetM;
			coord.x = (int)Math.Floor(coord.x/(float)batchSize);
			coord.y = (int)Math.Floor(coord.y/(float)batchSize);
			coord.z = (int)Math.Floor(coord.z/(float)batchSize);
			return coord+batchOffset;
		}
		
		/** Returns the min XYZ corner */ 
		public static Int3 getWorldCoord(Int3 batch) { //TODO https://i.imgur.com/sbXjIpq.png
			batch = batch-batchOffset;
			return batch*batchSize+batchOffsetM;
		}
		
		/*
batch_id = ((1117, -268, 568) + (2048.0,3040.0,2048.0)) / 160
batch_id = (3165.0, 2772.0, 2616.0) / 160
batch_id = (19.78125, 17.325, 16.35)
batch_id = (19, 17, 16)
		 */
		
		public static GameObject dropItem(Vector3 pos, TechType item) {
			string id = CraftData.GetClassIdForTechType(item);
			if (id != null) {
				GameObject go = ObjectUtil.createWorldObject(id);
				if (go != null)
					go.transform.position = pos;
				return go;
	    	}
	    	else {
	    		SNUtil.log("NO SUCH ITEM TO DROP: "+item);
	    		return null;
	    	}
		}
		
		public static mset.Sky getSkybox(string biome) {
			int idx = WaterBiomeManager.main.GetBiomeIndex(biome);
			if (idx < 0) {
				SNUtil.writeToChat("Biome '"+biome+"' had no sky lookup. See log for biome table.");
				SNUtil.log(WaterBiomeManager.main.biomeLookup.toDebugString());
				return null;
			}
			return idx < WaterBiomeManager.main.biomeSkies.Count ? WaterBiomeManager.main.biomeSkies[idx] : null;
		}
		
		public static C getClosest<C>(GameObject go) where C : Component {
			double dist = -1;
			C ret = null;
			foreach (C obj in UnityEngine.Object.FindObjectsOfType<C>()) {
				if (!obj)
					continue;
				double dd = Vector3.Distance(go.transform.position, obj.transform.position);
				if (dd < dist || ret == null) {
					ret = obj;
					dist = dd;
				}
			}
			return ret;
		}
		
		public static string getBiomeFriendlyName(string biome) {
			return biomeNames.ContainsKey(biome) ? biomeNames[biome] : biome;
		}
		
	}
}
