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
	public class VanillaMusic {	
		
		//private static readonly Dictionary<string, VanillaMusic> biomeTable = new Dictionary<string, VanillaMusic>();
		
		//Where does Crush Depth play?
		public static readonly VanillaMusic KOOSH = new VanillaMusic("kooshAmbience"); //Islands Beneath The Sea
		public static readonly VanillaMusic BKELP = new VanillaMusic("BloodKelpAmbience"); //Lava Castle
		public static readonly VanillaMusic DUNES = new VanillaMusic("dunes"); //Alien Expanse
		public static readonly VanillaMusic GRANDREEF = new VanillaMusic("grandReefAmbience"); //Lost River
		public static readonly VanillaMusic DEEPGRAND = new VanillaMusic("deepGrandReefAmbience"); //Blood Crawlers
		public static readonly VanillaMusic UNDERISLANDS = new VanillaMusic("UnderwaterIslandsAmbience"); //silent?
		public static readonly VanillaMusic SHALLOWS = new VanillaMusic("safeShallowsAmbience"); //Into The Unknown
		public static readonly VanillaMusic KELP = new VanillaMusic("kelpForestAmbience"); //In Bloom
		public static readonly VanillaMusic OBSERVATORY = new VanillaMusic("observatoryAmbience"); //Observatory Zen
		public static readonly VanillaMusic MOUNTAINS = new VanillaMusic("mountainsAmbience"); //Fear The Reapers
		public static readonly VanillaMusic MUSHROOM = new VanillaMusic("mushroomForestAmbience"); //?
		public static readonly VanillaMusic REDGRASS = new VanillaMusic("grassyPlateausAmbience"); //?
		public static readonly VanillaMusic LOSTRIVER = new VanillaMusic("lostRiverAmbience"); //Lost River
		public static readonly VanillaMusic JELLYSHROOM = new VanillaMusic("jellyshroomCaveAmbience"); //Crash Site
		public static readonly VanillaMusic FLOATINGISLAND = new VanillaMusic("floatingIslandAmbience"); //?
		public static readonly VanillaMusic LAVAZONE = new VanillaMusic("lavaZoneAmbience"); //Lava Castle
		public static readonly VanillaMusic ILZ = new VanillaMusic("inactiveLavaZone"); //Bring a Medpack
		public static readonly VanillaMusic ALZ = new VanillaMusic("alz"); //Ahead Slow
		public static readonly VanillaMusic AURORA = new VanillaMusic("crashedShipAmbience"); //Dark Matter Reactor
		public static readonly VanillaMusic TREADER = new VanillaMusic("seaTreaderPath"); //Islands Beneath The Sea
		public static readonly VanillaMusic COVE = new VanillaMusic("LostRiverTreeCove"); //Ghost Tree
		public static readonly VanillaMusic COVETREE = new VanillaMusic("LostRiverGhostTree"); //silent?
		public static readonly VanillaMusic SPARSE = new VanillaMusic("SparseReefAmbience"); //Islands Beneath The Sea
		public static readonly VanillaMusic CRASH = new VanillaMusic("crashZoneAmbience"); //?
		public static readonly VanillaMusic WRECK = new VanillaMusic("WreckAmbience"); //not a music track, just the wreckage near pod 6
		public static readonly VanillaMusic GENERATOR = new VanillaMusic("generatorRoomAmbience"); //Bring a Medpack
		public static readonly VanillaMusic SCANNER = new VanillaMusic("mapRoomAmbience"); //Scanner Room Ambient
		
		public readonly string objectName;
				
		private VanillaMusic(string id) {
			objectName = id;
		}
		
		public override string ToString() {
			return objectName;
		}
		
		public GameObject getObject() {
			return ObjectUtil.getChildObject(Player.main.gameObject, "SpawnPlayerSounds/PlayerSounds(Clone)/waterAmbience/music/"+objectName);
		}
		
	}
}
