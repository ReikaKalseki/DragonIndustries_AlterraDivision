using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {
	public abstract class CustomBiome : BiomeBase {

		private readonly System.Reflection.Assembly ownerMod;

		public Story.BiomeGoal discoveryGoal { get; private set; }

		public SoundManager.SoundData customMusic;

		protected CustomBiome(string id, string name, float deco) : base(name, deco, id) {
			ownerMod = SNUtil.tryGetModDLL();
		}

		public abstract double getDistanceToBiome(Vector3 vec);

		public abstract void register();

		public void onLoad() {
			if (customMusic != null)
				registerMusic();
		}

		public void createDiscoveryStoryGoal(float minStayTime, XMLLocale.LocaleEntry e) {
			if (e.pda == "#NULL")
				SNUtil.log("Error - XML entry '" + e.key + "' is missing a sound field: " + e.dump());
			this.createDiscoveryStoryGoal(minStayTime, e.desc, SoundManager.registerPDASound(SNUtil.tryGetModDLL(), "BiomeGoal_" + mainID, e.pda).asset);
		}

		public void createDiscoveryStoryGoal(float minStayTime, string text, FMODAsset sound) {
			discoveryGoal = new Story.BiomeGoal();
			discoveryGoal.key = "Goal_Biome" + mainID;
			discoveryGoal.goalType = Story.GoalType.PDA;
			discoveryGoal.delay = 0;
			discoveryGoal.biome = mainID;
			discoveryGoal.minStayDuration = minStayTime;
			SNUtil.addVOLine(discoveryGoal, text, sound);
			StoryHandler.instance.registerTickedGoal(discoveryGoal);
			SNUtil.log("Created BiomeGoal for " + this);
		}

		private void registerMusic() {
			try {
				GameObject go = registerBiomeMusic(mainID, customMusic.asset);
				if (!go)
					throw new Exception("Object not created!");
				SNUtil.log("Registered custom music " + go.name + " for " + this, ownerMod);
			}
			catch (Exception e) {
				SNUtil.log("ERROR: Failed to register custom music for " + this+": "+e, ownerMod);
			}
		}

		public static GameObject registerBiomeMusic(string name, FMODAsset sound) {
			GameObject orig = VanillaMusic.COVE.getObject();
			GameObject go = orig.clone();
			go.name = name + "Ambience";
			go.GetComponent<FMOD_CustomLoopingEmitter>().asset = sound;
			go.GetComponent<FMODGameParams>().onlyInBiome = name;
			go.transform.SetParent(orig.transform.parent);
			go.transform.localPosition = Vector3.zero;
			return go;
		}

		public abstract mset.Sky getSky();

		public virtual float getMurkiness(float orig) {
			return orig;
		}

		public virtual float getScatteringFactor(float orig) {
			return orig;
		}

		public virtual Vector3 getColorFalloff(Vector3 orig) {
			return orig;
		}

		public virtual float getFogStart(float orig) {
			return orig;
		}

		public virtual float getScatterFactor(float orig) {
			return orig;
		}

		public virtual Color getWaterColor(Color orig) {
			return orig;
		}

		public virtual float getSunScale(float orig) {
			return orig;
		}

		public virtual Vector4 getEmissiveVector(Vector4 orig) {
			return orig;
		}
	}
}
