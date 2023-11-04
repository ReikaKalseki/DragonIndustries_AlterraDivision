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
		
		public static readonly VanillaBiomes SHALLOWS = new VanillaBiomes("Safe Shallows", 0.25F, "safe", "safeShallows");
		public static readonly VanillaBiomes KELP = new VanillaBiomes("Kelp Forest", 0.4F, "kelp", "kelpForest");
		public static readonly VanillaBiomes REDGRASS = new VanillaBiomes("Grassy Plateaus", 0.25F, "grassy", "grassyPlateaus");
		public static readonly VanillaBiomes MUSHROOM = new VanillaBiomes("Mushroom Forest", 0.5F, "mushroom", "mushroomForest", "tree");
		public static readonly VanillaBiomes JELLYSHROOM = new VanillaBiomes("Jellyshroom Caves", 1F, "jellyshroom", "JellyshroomCaves");
		public static readonly VanillaBiomes GRANDREEF = new VanillaBiomes("Grand Reef", 0.75F, "grandReef", "smokers");
		public static readonly VanillaBiomes DEEPGRAND = new VanillaBiomes("Deep Grand Reef", 0.75F, "deepgrand", "deepGrandReef");
		public static readonly VanillaBiomes KOOSH = new VanillaBiomes("Bulb Zone", 0.33F, "koosh", "kooshZone");
		public static readonly VanillaBiomes DUNES = new VanillaBiomes("Dunes", 0.25F, "dunes", "Dunes_ThermalVents");
		public static readonly VanillaBiomes CRASH = new VanillaBiomes("Crash Zone", -0.5F, "crash", "crashZone", "crashZone_Mesa");
		public static readonly VanillaBiomes CRAG = new VanillaBiomes("Crag Field", 0.25F, "crag", "cragField");
		public static readonly VanillaBiomes SPARSE = new VanillaBiomes("Sparse Reef", 0F, "sparse", "sparseReef", "sparseReef_Deep", "sparseReef_spike");
		public static readonly VanillaBiomes MOUNTAINS = new VanillaBiomes("Mountains", 0F, "mountains");
		public static readonly VanillaBiomes TREADER = new VanillaBiomes("Sea Treader's Path", 0.1F, "seaTreaderPath");
		public static readonly VanillaBiomes UNDERISLANDS = new VanillaBiomes("Underwater Islands", 0.33F, "Underwaterislands", "UnderwaterIslands_ValleyFloor", "Underwaterislands_Island", "Underwaterislands_IslandCave");
		public static readonly VanillaBiomes BLOODKELP = new VanillaBiomes("Blood Kelp Trench", 0.4F, "bloodkelp", "bloodkelp_trench", "bloodkelp_deeptrench");
		public static readonly VanillaBiomes BLOODKELPNORTH = new VanillaBiomes("Northern Blood Kelp", 0.4F, "bloodkelptwo");
		public static readonly VanillaBiomes LOSTRIVER = new VanillaBiomes("Lost River", 0.67F, "LostRiver_BonesField_Corridor", "LostRiver_BonesField_Corridor_Stream", "LostRiver_BonesField", "LostRiver_BonesField_Lake", "LostRiver_BonesField_LakePit", "LostRiver_Corridor", "LostRiver_Junction", "LostRiver_GhostTree_Lower", "LostRiver_GhostTree", "LostRiver_Canyon", "LostRiver_SkeletonCave", "Precursor_LostRiverBase");
		public static readonly VanillaBiomes COVE = new VanillaBiomes("Tree Cove", 1F, "LostRiver_TreeCove");
		public static readonly VanillaBiomes ILZ = new VanillaBiomes("Inactive Lava Zone", 0.5F, "ILZCorridor", "ILZCorridorDeep", "ILZChamber", "ILZChamber_Dragon", "LavaPit", "LavaFalls", "LavaCastle", "ilzLava");
		public static readonly VanillaBiomes ALZ = new VanillaBiomes("Active Lava Zone", 0.4F, "LavaLakes");
		public static readonly VanillaBiomes VOID = new VanillaBiomes("Crater Edge", 0F, "void"/*, ""*/);
				
		private VanillaBiomes(string d, float deco, params string[] ids) : base(d, deco, ids) {
			
		}
		
		public override bool isCaveBiome() {
			return this == ALZ || this == ILZ || this == COVE || this == LOSTRIVER || this == JELLYSHROOM || this == DEEPGRAND;
		}
		
		public override bool isInBiome(Vector3 pos) {
			return BiomeBase.getBiome(pos) == this;
		}
	}
}
