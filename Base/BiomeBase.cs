using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{
	public abstract class BiomeBase {
		
		private static readonly List<string> variants = new List<string>(){
			"",
			"_cave",
			"_cave_dark",
			"_cave_light",
			"_cave_trans",
			"_CaveEntrance",
			"_Caves",
			"_Geyser",
			"_ThermalVent",
			"_Skeleton",
			"_Water",
		};
		
		private static readonly Dictionary<string, BiomeBase> biomeList = new Dictionary<string, BiomeBase>();
		
		private static readonly UnknownBiome UNRECOGNIZED = new UnknownBiome();

		public readonly string displayName;
		private readonly List<string> internalNames = new List<string>();
		
		public static readonly Dictionary<Vector3, BiomeBase> biomeHoles = new Dictionary<Vector3, BiomeBase>();
		
		public static void initializeBiomeHoles() {
			biomeHoles[new Vector3(1042.7F, -500F, 919.11F)] = VanillaBiomes.MOUNTAINS;
		}
		
		protected BiomeBase(string d, params string[] ids) {
			displayName = d;
			foreach (string id in ids) {
				string path = PrefabData.getPrefab(id);
				internalNames.Add(id);
				registerID(this, id);
			}
		}
		
		private static void registerID(BiomeBase b, string id) {
			if (id != null)
				id = id.ToLowerInvariant();
			foreach (string s in variants) {
				string key = id+s;
				biomeList[key.ToLowerInvariant()] = b;
				SNUtil.log("Registered biome "+b.displayName+" with id "+key);
			}
		}
		
		public IEnumerable<string> getIDs() {
			return new ReadOnlyCollection<string>(internalNames);
		}
		
		public override string ToString() {
			return GetType().Name+" ["+string.Join(", ", internalNames)+"]";
		}
		
		private static BiomeBase getBiome(string id) {
			id = id.ToLowerInvariant();
			return biomeList.ContainsKey(id) ? biomeList[id] : UNRECOGNIZED;
		}
		
		public static BiomeBase getBiome(Vector3 pos) {
			string biome = WaterBiomeManager.main.GetBiome(pos, false);
			if (string.IsNullOrEmpty(biome)) {
				foreach (KeyValuePair<Vector3, BiomeBase> kvp in biomeHoles) {
					if (Vector3.Distance(kvp.Key, pos) <= 125)
						return kvp.Value;
				}
			}
			return string.IsNullOrEmpty(biome) ? VanillaBiomes.VOID : getBiome(biome);
		}
		
		public abstract bool isCaveBiome();
		
		public abstract bool isInBiome(Vector3 pos);
	}
		
	class UnknownBiome : BiomeBase {
		
		internal UnknownBiome() : base("[UNRECOGNIZED BIOME]") {
			
		}
		
		public override bool isCaveBiome() {
			return false;
		}
		
		public override bool isInBiome(Vector3 pos) {
			return false;
		}
		
	}
}
