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
		
		private static readonly Dictionary<string, BiomeBase> biomeList = new Dictionary<string, BiomeBase>();
		
		private static readonly UnknownBiome UNRECOGNIZED = new UnknownBiome();

		public readonly string displayName;
		private readonly List<string> internalNames = new List<string>();
		
		protected BiomeBase(string d, params string[] ids) {
			displayName = d;
			foreach (string id in ids) {
				string path = PrefabData.getPrefab(id);
				internalNames.Add(id);
				registerID(this, id);
			}
		}
		
		private static void registerID(BiomeBase b, string id) {
			id = id.ToLowerInvariant();
			biomeList[id] = b;
			biomeList[id+"_cave"] = b;
			biomeList[id+"_cave_dark"] = b;
			biomeList[id+"_cave_light"] = b;
			biomeList[id+"_cave_trans"] = b;
			biomeList[id+"_caveentrance"] = b;
			biomeList[id+"_caves"] = b;
			biomeList[id+"_geyser"] = b;
			biomeList[id+"_ThermalVent"] = b;
		}
		
		public IEnumerable<string> getIDs() {
			return new ReadOnlyCollection<string>(internalNames);
		}
		
		public override string ToString() {
			return GetType().Name+" ["+string.Join(", ", internalNames)+"]";
		}
		
		public static BiomeBase getBiome(string id) {
			id = id.ToLowerInvariant();
			return biomeList.ContainsKey(id) ? biomeList[id] : UNRECOGNIZED;
		}
		
		public static BiomeBase getBiome(Vector3 pos) {
			string biome = WaterBiomeManager.main.GetBiome(pos, false);
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
