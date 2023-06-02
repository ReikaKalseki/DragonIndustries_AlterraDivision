using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class WorldUtil {
		
		private static readonly Int3 batchOffset = new Int3(13, 19, 13);
		private static readonly Int3 batchOffsetM = new Int3(32, 0, 32);
		private static readonly int batchSize = 160;
		
		//private static readonly Dictionary<string, string> biomeNames = new Dictionary<string, string>();
		
		static WorldUtil() {
			
		}
		/*
		private static void mapBiomeName(string name, params string[] keys) {
			foreach (string s in keys) {
				biomeNames[s] = name;
			}
		}*/
		
		public static Int3 getBatch(Vector3 pos) { //"Therefore e.g. batch (12, 18, 12) covers the voxels from (-128, -160, -128) to (32, 0, 32)."
			Int3 coord = pos.roundToInt3();
			coord = coord-batchOffsetM;
			coord.x = (int)Math.Floor(coord.x/(float)batchSize);
			coord.y = (int)Math.Floor(coord.y/(float)batchSize);
			coord.z = (int)Math.Floor(coord.z/(float)batchSize);
			return coord+batchOffset;
		}
		
		/** Returns the min XYZ corner */ 
		public static Int3 getWorldCoord(Int3 batch) { //TODO https://i.imgur.com/sbXjIpq.png
			batch = batch-batchOffset;
			return batch*batchSize+batchOffsetM;
		}
		
		/*
batch_id = ((1117, -268, 568) + (2048.0,3040.0,2048.0)) / 160
batch_id = (3165.0, 2772.0, 2616.0) / 160
batch_id = (19.78125, 17.325, 16.35)
batch_id = (19, 17, 16)
		 */
		
		public static GameObject dropItem(Vector3 pos, TechType item) {
			string id = CraftData.GetClassIdForTechType(item);
			if (id != null) {
				GameObject go = ObjectUtil.createWorldObject(id);
				if (go != null)
					go.transform.position = pos;
				return go;
	    	}
	    	else {
	    		SNUtil.log("NO SUCH ITEM TO DROP: "+item);
	    		return null;
	    	}
		}
		
		public static mset.Sky getSkybox(string biome) {
			int idx = WaterBiomeManager.main.GetBiomeIndex(biome);
			if (idx < 0) {
				SNUtil.writeToChat("Biome '"+biome+"' had no sky lookup. See log for biome table.");
				SNUtil.log(WaterBiomeManager.main.biomeLookup.toDebugString());
				return null;
			}
			return idx < WaterBiomeManager.main.biomeSkies.Count ? WaterBiomeManager.main.biomeSkies[idx] : null;
		}
		
		public static C getClosest<C>(GameObject go) where C : Component {
			return getClosest<C>(go.transform.position);
		}
		
		public static C getClosest<C>(Vector3 pos) where C : Component {
			double dist = -1;
			C ret = null;
			foreach (C obj in UnityEngine.Object.FindObjectsOfType<C>()) {
				if (!obj)
					continue;
				double dd = Vector3.Distance(pos, obj.transform.position);
				if (dd < dist || ret == null) {
					ret = obj;
					dist = dd;
				}
			}
			return ret;
		}
		
		public static HashSet<GameObject> getObjectsNear(Vector3 pos, float r) {
			HashSet<GameObject> set = new HashSet<GameObject>();
			/*
			Collider[] hit = Physics.OverlapSphere(pos, r);			
			foreach (Collider rh in hit) {
				if (rh) {
					GameObject go = UWE.Utils.GetEntityRoot(rh.gameObject);
					if (!go)
						go = rh.gameObject;
					set.Add(go);
				}
			}
			*/
			foreach (RaycastHit hit in Physics.SphereCastAll(pos, r, Vector3.up, 0.1F)) {
				if (hit.transform) {
					GameObject go = UWE.Utils.GetEntityRoot(hit.transform.gameObject);
					if (!go)
						go = hit.transform.gameObject;
					set.Add(go);
				}
			}
			return set;
		}
		
		public static HashSet<C> getObjectsNearWithComponent<C>(Vector3 pos, float r) where C : MonoBehaviour {
			HashSet<C> set = new HashSet<C>();
			/*
			Collider[] hit = Physics.OverlapSphere(pos, r);
			if (hit == null || hit.Length == 0)
				return set;
			foreach (Collider rh in hit) {
				if (rh && rh.gameObject) {
					GameObject go = UWE.Utils.GetEntityRoot(rh.gameObject);
					if (!go)
						go = rh.gameObject;
					C com = go ? go.GetComponentInChildren<C>() : null;
					if (com)
						set.Add(com);
				}
			}
			*/
			foreach (RaycastHit hit in Physics.SphereCastAll(pos, r, Vector3.up, 0.1F)) {
				if (hit.transform) {
					GameObject go = UWE.Utils.GetEntityRoot(hit.transform.gameObject);
					if (!go)
						go = hit.transform.gameObject;
					C com = go ? go.GetComponentInChildren<C>() : null;
					if (com)
						set.Add(com);
				}
			}
			return set;
		}
		/*
		public static string getBiomeFriendlyName(string biome) {
			return biomeNames.ContainsKey(biome) ? biomeNames[biome] : biome;
		}
	    */
	    public static bool isPlantInNativeBiome(GameObject go) {
			if (!go)
				return false;
			PrefabIdentifier pi = go.FindAncestor<PrefabIdentifier>();
			TechType tt = CraftData.GetTechType(go);
			if (tt == TechType.None) {
				Plantable p = go.FindAncestor<Plantable>();
				if (p)
					tt = p.plantTechType;
			}
			VanillaFlora vf = VanillaFlora.getFromID(pi ? pi.ClassId : CraftData.GetClassIdForTechType(tt));
			if (vf != null && vf.isNativeToBiome(go.transform.position))
				return true;
			BasicCustomPlant plant = BasicCustomPlant.getPlant(tt);
			if (plant != null && plant.isNativeToBiome(go.transform.position))
				return true;
			return false;
		}
		
		public static bool isInCave(Vector3 pos) {
	   		return BiomeBase.getBiome(pos).isCaveBiome() || WaterBiomeManager.main.GetBiome(pos, false).ToLowerInvariant().Contains("_cave");
		}
		
		public static bool isInWreck(Vector3 pos) {
	   		string biome = WaterBiomeManager.main.GetBiome(pos, false);
	   		return !string.IsNullOrEmpty(biome) && biome.ToLowerInvariant().Contains("wreck");
		}
		
		public static bool lineOfSight(GameObject o1, GameObject o2, bool solidOnly = false) {
			return lineOfSight(o1, o2, o1.transform.position, o2.transform.position, solidOnly);
		}
		
		public static bool lineOfSight(GameObject o1, GameObject o2, Vector3 pos1, Vector3 pos2, bool solidOnly = false) {/*
			RaycastHit hit;
			Physics.Linecast(o1.transform.position, o2.transform.position, out hit);
			if (hit) {
				
			}*/
			Vector3 dd = pos2-pos1;
			RaycastHit[] hits = Physics.RaycastAll(pos1, dd.normalized, dd.magnitude);
			foreach (RaycastHit hit in hits) {
				if (!hit.collider || hit.collider.isTrigger)
					continue;
				if (hit.transform == o1.transform || hit.transform == o2.transform)
					continue;
				if (Array.IndexOf(o1.GetComponentsInChildren<Collider>(), hit.collider) >= 0)
					continue;
				if (Array.IndexOf(o2.GetComponentsInChildren<Collider>(), hit.collider) >= 0)
					continue;
				//SNUtil.writeToChat("Raytrace from "+o1+" to "+o2+" hit "+hit.transform+" @ "+hit.point+" (D="+hit.distance+")");
				return false;
			}
			return true;
		}
		/*
		public static float getLightAtPosition(Vector3 pos, GameLightType types) {
			
		}*/
		
		public static void setParticlesTemporary(ParticleSystem p, float dur, float killOffset = 5) {
			p.Play(true);
			p.gameObject.EnsureComponent<TransientParticleTag>().Invoke("stop", dur);
			UnityEngine.Object.Destroy(p.gameObject, dur+killOffset);
			PrefabIdentifier pi = p.gameObject.FindAncestor<PrefabIdentifier>();
			if (pi)
				UnityEngine.Object.DestroyImmediate(pi);
			LargeWorldEntity lw = p.gameObject.FindAncestor<LargeWorldEntity>();
			if (lw)
				UnityEngine.Object.DestroyImmediate(lw);
		}
		
		public static ParticleSystem spawnParticlesAt(Vector3 pos, string pfb, float dur, bool forceSpawn = false, float killOffset = 5) {
			if (!forceSpawn && Vector3.Distance(pos, Player.main.transform.position) >= 200)
				return null;
			GameObject particle = ObjectUtil.createWorldObject(pfb);
			particle.SetActive(true);
			particle.transform.position = pos;
			ParticleSystem p = particle.GetComponent<ParticleSystem>();
			setParticlesTemporary(p, dur, killOffset);
			return p;
		}
		
		class TransientParticleTag : MonoBehaviour {
			
			public void stop() {
				GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
			}
		
			void OnDestroy() {
				if (gameObject)
					UnityEngine.Object.Destroy(gameObject);
			}
			
			void OnDisable() {
				if (gameObject)
					UnityEngine.Object.Destroy(gameObject);
			}
			
		}
		
	}
	/*
	enum GameLightType {
		FLASHLIGHT,
		SEAGLIDE,
		SEAMOTH,
		EXOSUIT,
		CYCLOPS,
		FLARE,
		SKY,
		OTHER,
	}*/
}
