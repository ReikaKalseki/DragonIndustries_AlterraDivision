using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using SMLHelper.V2.Handlers;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {
	public class VanillaCreatures : PrefabReference {

		private static readonly Dictionary<string, VanillaCreatures> lookup = new Dictionary<string, VanillaCreatures>();

		public static readonly VanillaCreatures PEEPER = new VanillaCreatures("3fcd548b-781f-46ba-b076-7412608deeef").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.MOUNTAINS).addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.GRANDREEF);
		public static readonly VanillaCreatures BOOMERANG = new VanillaCreatures("fa4cfe65-4eaf-4d51-ba0d-e8cc9632fd47").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.KOOSH);
		public static readonly VanillaCreatures SPADEFISH = new VanillaCreatures("d040bec1-0368-4f7c-aed6-93b5e1852d45").addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.MUSHROOM);
		public static readonly VanillaCreatures HOOPFISH = new VanillaCreatures("284ceeb6-b437-4aca-a8bd-d54f336cbef8").addBiome(VanillaBiomes.KELP).addBiome(VanillaBiomes.KOOSH).addBiome(VanillaBiomes.GRANDREEF);
		public static readonly VanillaCreatures BLADDERFISH = new VanillaCreatures("bf9ccd04-60af-4144-aaa1-4ac184c686c2").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaCreatures GARRYFISH = new VanillaCreatures("5de7d617-c04c-4a83-b663-ebf1d3dd90a1").addBiome(VanillaBiomes.SHALLOWS);
		public static readonly VanillaCreatures HOLEFISH = new VanillaCreatures("495befa0-0e6b-400d-9734-227e5a732f75").addBiome(VanillaBiomes.SHALLOWS);
		public static readonly VanillaCreatures HOVERFISH = new VanillaCreatures("0a993944-87d3-441e-b21d-6c314f723cc7").addBiome(VanillaBiomes.KELP);
		public static readonly VanillaCreatures REGINALD = new VanillaCreatures("cf171ce2-e3d2-4cec-9757-60dbd480e486").addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.SPARSE).addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaCreatures SPINEFISH = new VanillaCreatures("539af52c-f4b8-402b-ae88-e641aa031685").addBiome(VanillaBiomes.BLOODKELP).addBiome(VanillaBiomes.BLOODKELPNORTH).addBiome(VanillaBiomes.LOSTRIVER);

		public static readonly VanillaCreatures FLOATER = new VanillaCreatures("b409ed8d-9a73-4140-ac06-3aa60b66aa47").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.REDGRASS); // "bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782" ?
		public static readonly VanillaCreatures ROCKGRUB = new VanillaCreatures("8e82dc63-5991-4c63-a12c-2aa39373a7cf").addBiome(VanillaBiomes.MOUNTAINS).addBiome(VanillaBiomes.SPARSE);
		public static readonly VanillaCreatures SHUTTLEBUG = new VanillaCreatures("fa7fa15c-5515-497f-872d-d6d579a39f60").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.KELP);

		public static readonly VanillaCreatures EYEYE = new VanillaCreatures("79c1aef0-e505-469c-ab36-c22c76aeae44").addBiome(VanillaBiomes.JELLYSHROOM).addBiome(VanillaBiomes.GRANDREEF);
		public static readonly VanillaCreatures OCULUS = new VanillaCreatures("aefe2153-9e68-41cf-9615-253aa6f965aa").addBiome(VanillaBiomes.JELLYSHROOM);
		public static readonly VanillaCreatures REDEYEYE = new VanillaCreatures("f9006f3c-1694-4711-9532-624577e4ac7d").addBiome(VanillaBiomes.ILZ);
		public static readonly VanillaCreatures MAGMARANG = new VanillaCreatures("04c3c51a-fa9c-4fe4-bc89-24061ffa6f26").addBiome(VanillaBiomes.ILZ);

		public static readonly VanillaCreatures SKYRAY = new VanillaCreatures("6a1b444f-138f-46fa-88bb-d673a2ceb689");
		public static readonly VanillaCreatures RABBITRAY = new VanillaCreatures("01872776-2ff8-4214-805b-495001cf183d").addBiome(VanillaBiomes.SHALLOWS);
		public static readonly VanillaCreatures CUDDLEFISH = new VanillaCreatures("d4be3a5d-67c3-4345-af25-7663da2d2898");

		public static readonly VanillaCreatures CRASHFISH = new VanillaCreatures("7d307502-46b7-4f86-afb0-65fe8867f893").addBiome(VanillaBiomes.SHALLOWS);
		public static readonly VanillaCreatures BITER = new VanillaCreatures("4064a71a-c464-4db2-942a-56391fe69951").addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.JELLYSHROOM).addBiome(VanillaBiomes.MOUNTAINS);
		public static readonly VanillaCreatures BLIGHTER = new VanillaCreatures("eed4ec38-0363-40de-84dc-de6dd9b9e876").addBiome(VanillaBiomes.BLOODKELP).addBiome(VanillaBiomes.BLOODKELPNORTH);
		public static readonly VanillaCreatures CAVECRAWLER = new VanillaCreatures("3e0a11f1-e2b2-4c4f-9a8e-0b0a77dcc065"/*, "7ce2ca9d-6154-4988-9b02-38f670e741b8"*/).addBiome(VanillaBiomes.DUNES);
		public static readonly VanillaCreatures BLOODCRAWLER = new VanillaCreatures("830a8fa0-d92d-4683-a193-7531e6968042").addBiome(VanillaBiomes.LOSTRIVER).addBiome(VanillaBiomes.BLOODKELP).addBiome(VanillaBiomes.BLOODKELPNORTH);
		public static readonly VanillaCreatures MESMER = new VanillaCreatures("ad18b555-9073-445e-808a-d8b39d72f22e").addBiome(VanillaBiomes.CRAG).addBiome(VanillaBiomes.KOOSH).addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaCreatures BLEEDER = new VanillaCreatures("3406b655-0390-4ea7-8b75-a5c4705fc568").addBiome(VanillaBiomes.SPARSE);
		public static readonly VanillaCreatures LAVALARVA = new VanillaCreatures("423a8e49-eabe-473b-9b45-4aa52de1596f").addBiome(VanillaBiomes.ILZ).addBiome(VanillaBiomes.ALZ);

		public static readonly VanillaCreatures GASOPOD = new VanillaCreatures("3c13b3a4-ac02-4601-8030-b9d7482cde1e").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.DUNES).addBiome(VanillaBiomes.CRASH);
		public static readonly VanillaCreatures GHOSTRAY = new VanillaCreatures("1826f338-40d2-4b85-8d15-08ea3fa669ad").addBiome(VanillaBiomes.LOSTRIVER).addBiome(VanillaBiomes.COVE);
		public static readonly VanillaCreatures CRIMSONRAY = new VanillaCreatures("575109e0-0adc-4b73-a8cf-cfb49d1571c3").addBiome(VanillaBiomes.ILZ).addBiome(VanillaBiomes.ALZ);
		public static readonly VanillaCreatures JELLYRAY = new VanillaCreatures("5550502d-552b-4f22-8bf2-479d73a7646c").addBiome(VanillaBiomes.DEEPGRAND).addBiome(VanillaBiomes.MUSHROOM);

		public static readonly VanillaCreatures STALKER = new VanillaCreatures("cf8794a1-5cd6-492e-8acf-7da7c940ef70").addBiome(VanillaBiomes.KELP).addBiome(VanillaBiomes.CRASH);
		public static readonly VanillaCreatures BONESHARK = new VanillaCreatures("66072588-f5aa-4a41-a8d4-bb7e8dffee51").addBiome(VanillaBiomes.UNDERISLANDS).addBiome(VanillaBiomes.CRAG).addBiome(VanillaBiomes.MUSHROOM);
		public static readonly VanillaCreatures SANDSHARK = new VanillaCreatures("5e5f00b4-1531-45c0-8aca-84cbd3b580a4").addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.DUNES).addBiome(VanillaBiomes.CRASH);
		public static readonly VanillaCreatures AMPEEL = new VanillaCreatures("e69be2e8-a2e3-4c4c-a979-281fbf221729").addBiome(VanillaBiomes.KOOSH).addBiome(VanillaBiomes.BLOODKELP).addBiome(VanillaBiomes.BLOODKELPNORTH);
		public static readonly VanillaCreatures CRABSQUID = new VanillaCreatures("4c2808fe-e051-44d2-8e64-120ddcdc8abb").addBiome(VanillaBiomes.DEEPGRAND).addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaCreatures CRABSNAKE = new VanillaCreatures("911afe46-6178-4594-b23c-e577e7633622").addBiome(VanillaBiomes.JELLYSHROOM);
		public static readonly VanillaCreatures LAVALIZARD = new VanillaCreatures("2fbb2894-a01a-46b4-8748-e871bf23f646").addBiome(VanillaBiomes.ILZ);
		public static readonly VanillaCreatures RIVERPROWLER = new VanillaCreatures("e82d3c24-5a58-4307-a775-4741050c8a78").addBiome(VanillaBiomes.LOSTRIVER);

		public static readonly VanillaCreatures WARPER = new VanillaCreatures("c7103527-f6fa-4d1e-a75d-146433851557").addBiome(VanillaBiomes.MOUNTAINS).addBiome(VanillaBiomes.DUNES).addBiome(VanillaBiomes.TREADER).addBiome(VanillaBiomes.BLOODKELP).addBiome(VanillaBiomes.BLOODKELPNORTH).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.DEEPGRAND).addBiome(VanillaBiomes.LOSTRIVER).addBiome(VanillaBiomes.ILZ).addBiome(VanillaBiomes.ALZ);
		public static readonly VanillaCreatures PRECURSORCRAB = new VanillaCreatures("4fae8fa4-0280-43bd-bcf1-f3cba97eed77").addBiome(VanillaBiomes.ILZ).addBiome(VanillaBiomes.ALZ);

		public static readonly VanillaCreatures REEFBACK = new VanillaCreatures("8d3d3c8b-9290-444a-9fea-8e5493ecd6fe").addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.MUSHROOM).addBiome(VanillaBiomes.SPARSE).addBiome(VanillaBiomes.UNDERISLANDS).addBiome(VanillaBiomes.GRANDREEF);
		public static readonly VanillaCreatures REEFBACK_BABY = new VanillaCreatures("34765384-821f-41ad-b716-1b68c507e4f2");
		public static readonly VanillaCreatures GIANT_FLOATER = new VanillaCreatures("37ea521a-6be4-437c-8ed7-6b453d9218a8").addBiome(VanillaBiomes.UNDERISLANDS);
		public static readonly VanillaCreatures SEA_TREADER = new VanillaCreatures("35ee775a-d54c-4e63-a058-95306346d582").addBiome(VanillaBiomes.TREADER).addBiome(VanillaBiomes.GRANDREEF);

		public static readonly VanillaCreatures REAPER = new VanillaCreatures("f78942c3-87e7-4015-865a-5ae4d8bd9dcb").addBiome(VanillaBiomes.DUNES).addBiome(VanillaBiomes.MOUNTAINS).addBiome(VanillaBiomes.CRASH);
		public static readonly VanillaCreatures GHOST_LEVIATHAN_BABY = new VanillaCreatures("5ea36b37-300f-4f01-96fa-003ae47c61e5").addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaCreatures GHOST_LEVIATHAN = new VanillaCreatures("54701bfc-bb1a-4a84-8f79-ba4f76691bef").addBiome(VanillaBiomes.BLOODKELPNORTH).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.VOID);
		public static readonly VanillaCreatures SEADRAGON = new VanillaCreatures("ff43eacd-1a9e-4182-ab7b-aa43c16d1e53").addBiome(VanillaBiomes.ILZ).addBiome(VanillaBiomes.ALZ);

		//public static readonly VanillaCreatures EMPEROR = new VanillaCreatures(); //not a prefab
		public static readonly VanillaCreatures EMPEROR_BABY = new VanillaCreatures("09883a6c-9e78-4bbf-9561-9fa6e49ce766");
		public static readonly VanillaCreatures EMPEROR_JUVENILE = new VanillaCreatures("c9a28181-a0eb-4ae5-9fb8-ce57772980f1").addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.BLOODKELPNORTH).addBiome(VanillaBiomes.CRAG).addBiome(VanillaBiomes.DUNES).addBiome(VanillaBiomes.MOUNTAINS);

		public static readonly VanillaCreatures SCHOOL_HOOPFISH = new VanillaCreatures("08cb3290-504b-4191-97ee-6af1588af5c0").addBiome(VanillaBiomes.MOUNTAINS).addBiome(VanillaBiomes.KELP);
		public static readonly VanillaCreatures SCHOOL_SPINEFISH = new VanillaCreatures("2d3ea578-e4fa-4246-8bc9-ed8e66dec781").addBiome(VanillaBiomes.BLOODKELP).addBiome(VanillaBiomes.BLOODKELPNORTH).addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaCreatures SCHOOL_BOOMERANG = new VanillaCreatures("8ffbb5b5-21b4-4687-9118-730d59330c9a").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.CRASH).addBiome(VanillaBiomes.CRAG).addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.KOOSH);
		public static readonly VanillaCreatures SCHOOL_BLADDERFISH = new VanillaCreatures("a7b70c23-8e57-43e0-ab39-e02a29341376").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.GRANDREEF);
		public static readonly VanillaCreatures SCHOOL_HOLEFISH = new VanillaCreatures("ce23b9ee-fd98-4677-9919-20248356f7cf").addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.CRASH);

		private static readonly Dictionary<string, VanillaCreatures> names = new Dictionary<string, VanillaCreatures>();

		public static VanillaCreatures getByName(string n) {
			populateNames();
			return names.ContainsKey(n) ? names[n] : null;
		}

		public static List<VanillaCreatures> getAll() {
			populateNames();
			return new List<VanillaCreatures>(names.Values);
		}

		private static void populateNames() {
			if (names.Count == 0) {
				foreach (FieldInfo f in typeof(VanillaCreatures).GetFields()) {
					if (f.IsStatic && f.FieldType == typeof(VanillaCreatures)) {
						VanillaCreatures vf = (VanillaCreatures)f.GetValue(null);
						names[f.Name] = vf;
						vf.name = f.Name;
					}
				}
			}
		}

		public readonly string prefab;
		public readonly string pathname;

		private string name;

		private readonly List<BiomeBase> nativeBiomes = new List<BiomeBase>();

		private VanillaCreatures(string id) {
			prefab = id;
			pathname = PrefabData.getPrefab(id);
			lookup[prefab] = this;
		}

		private VanillaCreatures addBiome(BiomeBase biome) {
			nativeBiomes.Add(biome);
			return this;
		}

		public bool isNativeToBiome(Vector3 pos) {
			return this.isNativeToBiome(BiomeBase.getBiome(pos));
		}

		public bool isNativeToBiome(BiomeBase b) {
			return nativeBiomes.Contains(b);
		}

		public string getPrefabID() {
			return prefab;
		}

		public static VanillaCreatures getFromID(string pfb) {
			return lookup.ContainsKey(pfb) ? lookup[pfb] : null;
		}

		public string getName() {
			if (name == null)
				populateNames();
			return name;
		}

		public override string ToString() {
			return this.getName();
		}

	}
}
