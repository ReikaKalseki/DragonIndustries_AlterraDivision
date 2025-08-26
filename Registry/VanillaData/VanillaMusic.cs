using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

using SMLHelper.V2.Handlers;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {
	public class VanillaMusic {

		private static readonly Dictionary<string, VanillaMusic> table = new Dictionary<string, VanillaMusic>();

		//Where does Crush Depth play?
		public static readonly VanillaMusic KOOSH = new VanillaMusic("kooshAmbience", 2, 48); //Islands Beneath The Sea
		public static readonly VanillaMusic BKELP = new VanillaMusic("BloodKelpAmbience", 2, 12); //Lava Castle
		public static readonly VanillaMusic DUNES = new VanillaMusic("dunes", 1, 41); //Alien Expanse
		public static readonly VanillaMusic GRANDREEF = new VanillaMusic("grandReefAmbience", 1, 22); //Lost River
		public static readonly VanillaMusic DEEPGRAND = new VanillaMusic("deepGrandReefAmbience", 1, 44); //Blood Crawlers
		public static readonly VanillaMusic UNDERISLANDS = new VanillaMusic("UnderwaterIslandsAmbience", 0, 0); //silent?
		public static readonly VanillaMusic SHALLOWS = new VanillaMusic("safeShallowsAmbience", 2, 59); //Into The Unknown
		public static readonly VanillaMusic KELP = new VanillaMusic("kelpForestAmbience", 2, 10); //In Bloom
		public static readonly VanillaMusic OBSERVATORY = new VanillaMusic("observatoryAmbience", 1, 23); //Observatory Zen
		public static readonly VanillaMusic MOUNTAINS = new VanillaMusic("mountainsAmbience", 1, 23); //Fear The Reapers
		public static readonly VanillaMusic MUSHROOM = new VanillaMusic("mushroomForestAmbience", 0, 0); //?
		public static readonly VanillaMusic REDGRASS = new VanillaMusic("grassyPlateausAmbience", 0, 0); //?
		public static readonly VanillaMusic LOSTRIVER = new VanillaMusic("lostRiverAmbience", 1, 22); //Lost River
		public static readonly VanillaMusic JELLYSHROOM = new VanillaMusic("jellyshroomCaveAmbience", 2, 30); //Crash Site
		public static readonly VanillaMusic FLOATINGISLAND = new VanillaMusic("floatingIslandAmbience", 0, 0); //?
		public static readonly VanillaMusic LAVAZONE = new VanillaMusic("lavaZoneAmbience", 2, 12); //Lava Castle
		public static readonly VanillaMusic ILZ = new VanillaMusic("inactiveLavaZone", 1, 57); //Bring a Medpack
		public static readonly VanillaMusic ALZ = new VanillaMusic("alz", 1, 40); //Ahead Slow
		public static readonly VanillaMusic AURORA = new VanillaMusic("crashedShipAmbience", 1, 31); //Dark Matter Reactor
		public static readonly VanillaMusic TREADER = new VanillaMusic("seaTreaderPath", 2, 48); //Islands Beneath The Sea
		public static readonly VanillaMusic COVE = new VanillaMusic("LostRiverTreeCove", 1, 41); //Ghost Tree
		public static readonly VanillaMusic COVETREE = new VanillaMusic("LostRiverGhostTree", 0, 0); //silent?
		public static readonly VanillaMusic SPARSE = new VanillaMusic("SparseReefAmbience", 2, 48); //Islands Beneath The Sea
		public static readonly VanillaMusic CRASH = new VanillaMusic("crashZoneAmbience", 0, 0); //?
		public static readonly VanillaMusic WRECK = new VanillaMusic("WreckAmbience", 1, 0); //not a music track, just the wreckage near pod 6
		public static readonly VanillaMusic GENERATOR = new VanillaMusic("generatorRoomAmbience", 1, 57); //Bring a Medpack
		public static readonly VanillaMusic SCANNER = new VanillaMusic("mapRoomAmbience", 0, 5); //Scanner Room Ambient

		private readonly string objectName;

		private string activationBiome;

		private float length;

		private VanillaMusic(string id, int min, int sec) {
			objectName = id;
			table[objectName] = this;

			length = (min * 60) + sec;
		}

		public override string ToString() {
			return objectName;
		}

		private GameObject getObject() {
			return Player.main.gameObject.getChildObject("SpawnPlayerSounds/PlayerSounds(Clone)/waterAmbience/music/" + objectName);
		}

		public void reset() {
			GameObject go = this.getObject();
			go.SetActive(true);
			if (!string.IsNullOrEmpty(activationBiome))
				go.GetComponent<FMODGameParams>().onlyInBiome = activationBiome;
		}

		public void play() {
			this.enable();
			;//getObject().GetComponent<FMOD_CustomLoopingEmitter>().Play();
			 //SoundManager.playSound(getObject().GetComponent<FMOD_CustomLoopingEmitter>().asset.path);
			FMODUWE.PlayOneShot(this.getObject().GetComponent<FMOD_CustomLoopingEmitter>().asset, Player.main.transform.position, 1);
		}

		public void enable() {
			this.getObject().SetActive(true);
		}

		public void stop() {
			//getObject().GetComponent<FMOD_CustomLoopingEmitter>().Stop();
		}

		public float getLength() {
			return length;//getObject().GetComponent<FMOD_CustomLoopingEmitter>().length;
		}

		public void disable() {
			this.getObject().SetActive(false);
		}

		public void setToBiome(string biome) {
			FMODGameParams par = this.getObject().GetComponent<FMODGameParams>();
			activationBiome = par.onlyInBiome;
			par.onlyInBiome = biome;
			par.gameObject.SetActive(true);
		}

		public static IEnumerable<VanillaMusic> getAll() {
			return new ReadOnlyCollection<VanillaMusic>(new List<VanillaMusic>(table.Values));
		}
		/*
		public static void disableAllMusic() {
			GameObject go = Player.main.gameObject.getChildObject("SpawnPlayerSounds/PlayerSounds(Clone)/waterAmbience/music");
			foreach (FMODGameParams par in go.GetComponentsInChildren<FMODGameParams>(true)) {
				par.gameObject.SetActive(false);
			}
		}*/

	}
}
