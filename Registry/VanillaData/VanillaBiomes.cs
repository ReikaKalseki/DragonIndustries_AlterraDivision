using System;
using System.IO;
using System.Reflection;
using System.Linq;

using System.Collections.Generic;
using System.Collections.ObjectModel;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra
{
	public class VanillaBiomes : BiomeBase {
		
		public static readonly VanillaBiomes SHALLOWS = new VanillaBiomes(0, "Safe Shallows", 0.25F, "safe", "safeShallows");
		public static readonly VanillaBiomes KELP = new VanillaBiomes(50, "Kelp Forest", 0.4F, "kelp", "kelpForest");
		public static readonly VanillaBiomes REDGRASS = new VanillaBiomes(100, "Grassy Plateaus", 0.33F, "grassy", "grassyPlateaus", "GrassyPlateaus_Tower");
		public static readonly VanillaBiomes MUSHROOM = new VanillaBiomes(150, "Mushroom Forest", 0.67F, "mushroom", "mushroomForest", "tree");
		public static readonly VanillaBiomes JELLYSHROOM = new VanillaBiomes(250, "Jellyshroom Caves", 1F, "jellyshroom", "JellyshroomCaves");
		public static readonly VanillaBiomes GRANDREEF = new VanillaBiomes(300, "Grand Reef", 0.75F, "grandReef", "smokers");
		public static readonly VanillaBiomes DEEPGRAND = new VanillaBiomes(500, "Deep Grand Reef", 0.75F, "deepgrand", "deepGrandReef");
		public static readonly VanillaBiomes KOOSH = new VanillaBiomes(300, "Bulb Zone", 0.33F, "koosh", "kooshZone");
		public static readonly VanillaBiomes DUNES = new VanillaBiomes(350, "Dunes", 0.25F, "dunes", "Dunes_ThermalVents");
		public static readonly VanillaBiomes CRASH = new VanillaBiomes(200, "Crash Zone", -0.5F, "crash", "crashZone", "crashZone_Mesa", "CrashZone_Trench", "CrashZone_NoLoot");
		public static readonly VanillaBiomes CRAG = new VanillaBiomes(200, "Crag Field", 0.25F, "crag", "cragField");
		public static readonly VanillaBiomes SPARSE = new VanillaBiomes(200, "Sparse Reef", 0F, "sparse", "sparseReef", "sparseReef_Deep", "sparseReef_spike");
		public static readonly VanillaBiomes MOUNTAINS = new VanillaBiomes(350, "Mountains", 0F, "mountains");
		public static readonly VanillaBiomes TREADER = new VanillaBiomes(300, "Sea Treader's Path", 0.1F, "seaTreaderPath");
		public static readonly VanillaBiomes UNDERISLANDS = new VanillaBiomes(200, "Underwater Islands", 0.33F, "Underwaterislands", "UnderwaterIslands_ValleyFloor", "Underwaterislands_Island", "Underwaterislands_IslandCave");
		public static readonly VanillaBiomes BLOODKELP = new VanillaBiomes(400, "Blood Kelp Trench", 0.4F, "bloodkelp", "bloodkelp_trench", "bloodkelp_deeptrench");
		public static readonly VanillaBiomes BLOODKELPNORTH = new VanillaBiomes(400, "Northern Blood Kelp", 0.4F, "bloodkelptwo");
		public static readonly VanillaBiomes LOSTRIVER = new VanillaBiomes(700, "Lost River", 0.67F, "LostRiver_BonesField_Corridor", "LostRiver_BonesField_Corridor_Stream", "LostRiver_BonesField", "LostRiver_BonesField_Lake", "LostRiver_BonesField_LakePit", "LostRiver_Corridor", "LostRiver_Junction", "LostRiver_GhostTree_Lower", "LostRiver_GhostTree", "LostRiver_Canyon", "LostRiver_SkeletonCave", "Precursor_LostRiverBase");
		public static readonly VanillaBiomes COVE = new VanillaBiomes(900, "Tree Cove", 1F, "LostRiver_TreeCove");
		public static readonly VanillaBiomes ILZ = new VanillaBiomes(1200, "Inactive Lava Zone", 0.5F, "ILZCorridor", "ILZCorridorDeep", "ILZChamber", "ILZChamber_Dragon", "LavaPit", "LavaFalls", "LavaCastle", "ILZCastleTunnel", "ilzLava");
		public static readonly VanillaBiomes ALZ = new VanillaBiomes(1400, "Active Lava Zone", 0.4F, "LavaLakes", "LavaLakes_LavaPool");
		public static readonly VanillaBiomes AURORA = new VanillaBiomes(0, "Aurora", 0F, "crashedShip"); //not a distinct biome
		public static readonly VanillaBiomes FLOATISLAND = new VanillaBiomes(0, "Floating Island", 0F, "FloatingIsland");
		public static readonly VanillaBiomes MOUNTISLAND = new VanillaBiomes(0, "Mountain Island", 0F, "MountainIsland"); //not a distinct biome
		public static readonly VanillaBiomes VOID = new VanillaBiomes(8192, "Crater Edge", 0F, "void"/*, ""*/);
		
		public readonly float averageDepth;
				
		private VanillaBiomes(float dp, string d, float deco, params string[] ids) : base(d, deco, ids) {
			averageDepth = dp;
		}
		
		public override bool isCaveBiome() {
			return this == ALZ || this == ILZ || this == COVE || this == LOSTRIVER || this == JELLYSHROOM || this == DEEPGRAND;
		}
		
		public override bool existsInSeveralPlaces() {
			return this == SHALLOWS || this == KELP || this == REDGRASS || this == MUSHROOM;
		}
		
		public override bool isInBiome(Vector3 pos) {
			return BiomeBase.getBiome(pos) == this;
		}
		
		public static int compare(VanillaBiomes b1, VanillaBiomes b2) {
			return b1.averageDepth.CompareTo(b2.averageDepth);
		}
	}
}
