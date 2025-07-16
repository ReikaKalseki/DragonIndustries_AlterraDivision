using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{
	public abstract class BiomeBase : IComparable<BiomeBase> {
		
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
		
		private static readonly Dictionary<string, BiomeBase> biomeMap = new Dictionary<string, BiomeBase>();
		private static readonly List<BiomeBase> biomeList = new List<BiomeBase>();
		private static readonly List<CustomBiome> customBiomes = new List<CustomBiome>();
		
		public static readonly UnknownBiome UNRECOGNIZED = new UnknownBiome();

		public readonly string displayName;
		public readonly float sceneryValue;
		private readonly HashSet<string> internalNames = new HashSet<string>();
		
		internal static readonly Dictionary<Vector3, BiomeBase> biomeHoles = new Dictionary<Vector3, BiomeBase>();
		
		internal static void initializeBiomeHoles() {
			biomeHoles[new Vector3(1042.7F, -500F, 919.11F)] = VanillaBiomes.MOUNTAINS;
		}
		
		public static IEnumerable<BiomeBase> getAllBiomes() {
			return new ReadOnlyCollection<BiomeBase>(biomeList);
		}
		
		public static IEnumerable<CustomBiome> getCustomBiomes() {
			return new ReadOnlyCollection<CustomBiome>(customBiomes);
		}
		
		protected BiomeBase(string d, float deco, params string[] ids) {
			sceneryValue = deco;
			displayName = d;
			foreach (string id in ids)
				registerID(this, id);
			biomeList.Add(this);
			if (this is CustomBiome)
				customBiomes.Add((CustomBiome)this);
			SNUtil.log("Registered biome "+displayName+" with ids "+string.Join(", ", ids), SNUtil.tryGetModDLL(true));
		}
		
		public static bool isUnrecognized(BiomeBase bb) {
			return bb == UNRECOGNIZED;
		}
		
		private static void registerID(BiomeBase b, string id) {
			foreach (string s in variants) {
				string key = (id+s).ToLowerInvariant();
				biomeMap[key] = b;
				b.internalNames.Add(key);
			}
		}
		
		public bool containsID(string id) {
			if (id == null)
				return this == VanillaBiomes.VOID;
			return internalNames.Contains(id.ToLowerInvariant());
		}
		
		public IEnumerable<string> getIDs() {
			return new ReadOnlyCollection<string>(internalNames.ToList());
		}
		
		public override string ToString() {
			return GetType().Name+" "+displayName+": ["+string.Join(", ", internalNames)+"]";
		}
		
		public static BiomeBase getBiome(Vector3 pos) {
			//if (logBiomeFetch)
			//	SNUtil.writeToChat("Getting biome at "+pos);
			string biome = DIHooks.getBiomeAt(WaterBiomeManager.main.GetBiome(pos, false), pos); //will fire the event
			//if (logBiomeFetch)
			//	SNUtil.writeToChat("WBM found "+biome);
			if (string.IsNullOrEmpty(biome)) {
				foreach (KeyValuePair<Vector3, BiomeBase> kvp in biomeHoles) {
					if (Vector3.Distance(kvp.Key, pos) <= 125) {
						//if (logBiomeFetch)
						//	SNUtil.writeToChat("Matched to hole "+kvp.Key+", "+kvp.Value);
						return kvp.Value;
					}
				}
			}
			BiomeBase ret = string.IsNullOrEmpty(biome) ? VanillaBiomes.VOID : getBiome(biome);
			//if (logBiomeFetch)
			//	SNUtil.writeToChat("Lookup to "+ret.displayName);
			return ret;
		}
		
		public static BiomeBase getBiome(string id) {
			id = id.ToLowerInvariant();
			return biomeMap.ContainsKey(id) ? biomeMap[id] : UNRECOGNIZED;
		}
		
		public abstract bool isCaveBiome();
		public abstract bool existsInSeveralPlaces();
		
		public abstract bool isInBiome(Vector3 pos);
		
		public int CompareTo(BiomeBase ro) {
			if (this is VanillaBiomes && ro is VanillaBiomes) {
				return VanillaBiomes.compare((VanillaBiomes)this, (VanillaBiomes)ro);
			}
			else if (this is VanillaBiomes) {
				return -1;
			}
			else if (ro is VanillaBiomes) {
				return 1;
			}
			else {
				return String.Compare(displayName, ro.displayName, StringComparison.InvariantCultureIgnoreCase);
			}
		}
	}
		
	public class UnknownBiome : BiomeBase {
		
		internal UnknownBiome() : base("[UNRECOGNIZED BIOME]", 0) {
			
		}
		
		public override bool isCaveBiome() {
			return false;
		}
		
		public override bool existsInSeveralPlaces() {
			return false;
		}
		
		public override bool isInBiome(Vector3 pos) {
			return false;
		}
		
	}
}
