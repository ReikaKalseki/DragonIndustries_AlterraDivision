using System;
using System.IO;
using System.Reflection;
using System.Linq;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra
{
	public class VanillaCreatures : PrefabReference {
		
		private static readonly Dictionary<string, VanillaCreatures> lookup = new Dictionary<string, VanillaCreatures>();
		
		public static readonly VanillaCreatures PEEPER = new VanillaCreatures("3fcd548b-781f-46ba-b076-7412608deeef");
		public static readonly VanillaCreatures BOOMERANG = new VanillaCreatures("fa4cfe65-4eaf-4d51-ba0d-e8cc9632fd47");
		public static readonly VanillaCreatures SPADEFISH = new VanillaCreatures("d040bec1-0368-4f7c-aed6-93b5e1852d45");
		public static readonly VanillaCreatures HOOPFISH = new VanillaCreatures("284ceeb6-b437-4aca-a8bd-d54f336cbef8");
		public static readonly VanillaCreatures BLADDERFISH = new VanillaCreatures("bf9ccd04-60af-4144-aaa1-4ac184c686c2");
		public static readonly VanillaCreatures GARRYFISH = new VanillaCreatures("5de7d617-c04c-4a83-b663-ebf1d3dd90a1");
		public static readonly VanillaCreatures HOLEFISH = new VanillaCreatures("495befa0-0e6b-400d-9734-227e5a732f75");
		public static readonly VanillaCreatures HOVERFISH = new VanillaCreatures("0a993944-87d3-441e-b21d-6c314f723cc7");
		public static readonly VanillaCreatures REGINALD = new VanillaCreatures("cf171ce2-e3d2-4cec-9757-60dbd480e486");
		public static readonly VanillaCreatures SPINEFISH = new VanillaCreatures("539af52c-f4b8-402b-ae88-e641aa031685");
		
		public static readonly VanillaCreatures FLOATER = new VanillaCreatures("b409ed8d-9a73-4140-ac06-3aa60b66aa47"); // "bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782" ?
		public static readonly VanillaCreatures ROCKGRUB = new VanillaCreatures("8e82dc63-5991-4c63-a12c-2aa39373a7cf");
		public static readonly VanillaCreatures SHUTTLEBUG = new VanillaCreatures("fa7fa15c-5515-497f-872d-d6d579a39f60");
		
		public static readonly VanillaCreatures EYEYE = new VanillaCreatures("79c1aef0-e505-469c-ab36-c22c76aeae44");
		public static readonly VanillaCreatures OCULUS = new VanillaCreatures("aefe2153-9e68-41cf-9615-253aa6f965aa");
		public static readonly VanillaCreatures REDEYEYE = new VanillaCreatures("f9006f3c-1694-4711-9532-624577e4ac7d");
		public static readonly VanillaCreatures MAGMARANG = new VanillaCreatures("04c3c51a-fa9c-4fe4-bc89-24061ffa6f26");
		
		public static readonly VanillaCreatures SKYRAY = new VanillaCreatures("6a1b444f-138f-46fa-88bb-d673a2ceb689");
		public static readonly VanillaCreatures RABBITRAY = new VanillaCreatures("01872776-2ff8-4214-805b-495001cf183d");
		public static readonly VanillaCreatures CUDDLEFISH = new VanillaCreatures("d4be3a5d-67c3-4345-af25-7663da2d2898");
		
		public static readonly VanillaCreatures CRASHFISH = new VanillaCreatures("7d307502-46b7-4f86-afb0-65fe8867f893");
		public static readonly VanillaCreatures BITER = new VanillaCreatures("4064a71a-c464-4db2-942a-56391fe69951");
		public static readonly VanillaCreatures BLIGHTER = new VanillaCreatures("eed4ec38-0363-40de-84dc-de6dd9b9e876");
		public static readonly VanillaCreatures SANDCRAB = new VanillaCreatures("3e0a11f1-e2b2-4c4f-9a8e-0b0a77dcc065");
		public static readonly VanillaCreatures BLOODCRAWLER = new VanillaCreatures("7ce2ca9d-6154-4988-9b02-38f670e741b8"); //or maybe "830a8fa0-d92d-4683-a193-7531e6968042"
		public static readonly VanillaCreatures MESMER = new VanillaCreatures("ad18b555-9073-445e-808a-d8b39d72f22e");
		public static readonly VanillaCreatures BLEEDER = new VanillaCreatures("3406b655-0390-4ea7-8b75-a5c4705fc568");
		public static readonly VanillaCreatures LAVALARVA = new VanillaCreatures("423a8e49-eabe-473b-9b45-4aa52de1596f");
		
		public static readonly VanillaCreatures GASOPOD = new VanillaCreatures("3c13b3a4-ac02-4601-8030-b9d7482cde1e");
		public static readonly VanillaCreatures GHOSTRAY = new VanillaCreatures("1826f338-40d2-4b85-8d15-08ea3fa669ad");
		public static readonly VanillaCreatures CRIMSONRAY = new VanillaCreatures("575109e0-0adc-4b73-a8cf-cfb49d1571c3");
		public static readonly VanillaCreatures JELLYRAY = new VanillaCreatures("5550502d-552b-4f22-8bf2-479d73a7646c");
		
		public static readonly VanillaCreatures STALKER = new VanillaCreatures("cf8794a1-5cd6-492e-8acf-7da7c940ef70");
		public static readonly VanillaCreatures BONESHARK = new VanillaCreatures("66072588-f5aa-4a41-a8d4-bb7e8dffee51");
		public static readonly VanillaCreatures SANDSHARK = new VanillaCreatures("5e5f00b4-1531-45c0-8aca-84cbd3b580a4");
		public static readonly VanillaCreatures AMPEEL = new VanillaCreatures("e69be2e8-a2e3-4c4c-a979-281fbf221729");
		public static readonly VanillaCreatures CRABSQUID = new VanillaCreatures("4c2808fe-e051-44d2-8e64-120ddcdc8abb");
		public static readonly VanillaCreatures CRABSNAKE = new VanillaCreatures("911afe46-6178-4594-b23c-e577e7633622");
		public static readonly VanillaCreatures LAVALIZARD = new VanillaCreatures("2fbb2894-a01a-46b4-8748-e871bf23f646");
		public static readonly VanillaCreatures RIVERPROWLER = new VanillaCreatures("e82d3c24-5a58-4307-a775-4741050c8a78");
		public static readonly VanillaCreatures WARPER = new VanillaCreatures("c7103527-f6fa-4d1e-a75d-146433851557");
		
		public static readonly VanillaCreatures REEFBACK = new VanillaCreatures("8d3d3c8b-9290-444a-9fea-8e5493ecd6fe");
		public static readonly VanillaCreatures REEFBACK_BABY = new VanillaCreatures("34765384-821f-41ad-b716-1b68c507e4f2");
		public static readonly VanillaCreatures GIANT_FLOATER = new VanillaCreatures("37ea521a-6be4-437c-8ed7-6b453d9218a8");
		public static readonly VanillaCreatures SEA_TREADER = new VanillaCreatures("35ee775a-d54c-4e63-a058-95306346d582");
		
		public static readonly VanillaCreatures REAPER = new VanillaCreatures("f78942c3-87e7-4015-865a-5ae4d8bd9dcb");
		public static readonly VanillaCreatures GHOST_LEVIATHAN_BABY = new VanillaCreatures("5ea36b37-300f-4f01-96fa-003ae47c61e5");
		public static readonly VanillaCreatures GHOST_LEVIATHAN = new VanillaCreatures("54701bfc-bb1a-4a84-8f79-ba4f76691bef");
		public static readonly VanillaCreatures SEADRAGON = new VanillaCreatures("ff43eacd-1a9e-4182-ab7b-aa43c16d1e53");
		
		public readonly string prefab;
		public readonly string pathname;
				
		private VanillaCreatures(string id) {
			prefab = id;
			pathname = PrefabData.getPrefab(id);
			lookup[prefab] = this;
		}
		
		public string getPrefabID() {
			return prefab;
		}
		
		public static VanillaCreatures getFromID(string pfb) {
			return lookup.ContainsKey(pfb) ? lookup[pfb] : null;
		}
		
	}
}
