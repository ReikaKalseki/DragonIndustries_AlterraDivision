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
			double distsq = -1;
			C ret = null;
			foreach (C obj in UnityEngine.Object.FindObjectsOfType<C>()) {
				if (!obj)
					continue;
				double dd = (pos-obj.transform.position).sqrMagnitude;
				if (dd < distsq || ret == null) {
					ret = obj;
					distsq = dd;
				}
			}
			return ret;
		}
		
		/** 
  			Will not find things without colliders!
			Avoid using this with components that will result in many findings, as you then end up iterating a large list. Use the getter version instead.
		 */
		public static HashSet<C> getObjectsNearWithComponent<C>(Vector3 pos, float r) where C : MonoBehaviour {
			return getObjectsNear(pos, r, go => go.GetComponentInChildren<C>());
		}
		
		/** Will not find things without colliders! */
		public static HashSet<GameObject> getObjectsNearMatching(Vector3 pos, float r, Predicate<GameObject> check) {
			return getObjectsNear(pos, r, go => check(go) ? go : null);
		}
		
		/** Will not find things without colliders! */
		public static HashSet<GameObject> getObjectsNear(Vector3 pos, float r) {
			return getObjectsNear<GameObject>(pos, r, null);
		}
		
		/** Will not find things without colliders! */
		public static HashSet<R> getObjectsNear<R>(Vector3 pos, float r, Func<GameObject, R> converter = null) where R : UnityEngine.Object {
			HashSet<R> set = new HashSet<R>();
			getObjectsNear(pos, r, obj => set.Add(obj), converter);
			return set;
		}
		
		/** Will not find things without colliders! */
		public static void getGameObjectsNear(Vector3 pos, float r, Action<GameObject> getter) {
			getObjectsNear<GameObject>(pos, r, getter, null);
		}
		
		/** Will not find things without colliders! */
		public static void getObjectsNear<R>(Vector3 pos, float r, Action<R> getter, Func<GameObject, R> converter = null) where R : UnityEngine.Object {
			getObjectsNear<R>(pos, r, obj => {getter(obj); return false;}, converter);
		}
		
		/** Will not find things without colliders! */
		public static void getObjectsNear<R>(Vector3 pos, float r, Func<R, bool> getter, Func<GameObject, R> converter = null) where R : UnityEngine.Object {
			foreach (RaycastHit hit in Physics.SphereCastAll(pos, r, Vector3.up, 0.1F)) {
				if (hit.transform) {
					GameObject go = UWE.Utils.GetEntityRoot(hit.transform.gameObject);
					if (!go)
						go = hit.transform.gameObject;
					if (!go)
						continue;
					UnityEngine.Object obj = converter == null ? (UnityEngine.Object)go : converter(go);
					if (obj) {
						if (getter((R)obj))
							return;
					}
				}
			}
		}
		
		/** Will not find things without colliders! */
		public static GameObject areAnyObjectsNear(Vector3 pos, float r, Predicate<GameObject> check) {
			GameObject ret = null;
			getObjectsNear<GameObject>(pos, r, go => {ret = go; return true;}, go => check(go) ? go : null);
			return ret;
		}
		
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
	   		if (BiomeBase.getBiome(pos).isCaveBiome())
	   			return true;
	   		string b = WaterBiomeManager.main.GetBiome(pos, false);
	   		return !string.IsNullOrEmpty(b) && b.ToLowerInvariant().Contains("_cave");
		}
		
		public static bool isInWreck(Vector3 pos) {
	   		string biome = WaterBiomeManager.main.GetBiome(pos, false);
	   		return !string.IsNullOrEmpty(biome) && biome.ToLowerInvariant().Contains("wreck");
		}
		
		public static bool lineOfSight(GameObject o1, GameObject o2, Func<RaycastHit, bool> filter = null) {
			return lineOfSight(o1, o2, o1.transform.position, o2.transform.position, filter);
		}
		
		public static bool lineOfSight(GameObject o1, GameObject o2, Vector3 pos1, Vector3 pos2, Func<RaycastHit, bool> filter = null) {/*
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
				if (filter != null && !filter.Invoke(hit))
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
		
		public static List<RaycastHit> getTerrainMountedPositionsAround(Vector3 pos, float range, int num) {
			List<RaycastHit> ret = new List<RaycastHit>();
			for (int i = 0; i < num; i++) {
				Vector3 diff = new Vector3(UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-range, range)).setLength(UnityEngine.Random.Range(0.01F, range));
				Vector3 pos2 = (pos+diff).setY(pos.y+15);
				RaycastHit? hit = getTerrainVectorAt(pos2, 25);
				if (hit.HasValue)
					ret.Add(hit.Value);
			}
			return ret;
		}
		
		public static RaycastHit? getTerrainVectorAt(Vector3 pos, float maxDown = 1, Vector3? axis = null) {
			Ray ray = new Ray(pos, axis.HasValue ? axis.Value : Vector3.down);
			return UWE.Utils.RaycastIntoSharedBuffer(ray, maxDown, Voxeland.GetTerrainLayerMask()) > 0 ? UWE.Utils.sharedHitBuffer[0] : (RaycastHit?)null;
		}
		
		public static bool isPrecursorBiome(Vector3 pos) {
			string over = AtmosphereDirector.main.GetBiomeOverride();
			return !string.IsNullOrEmpty(over) && over.ToLowerInvariant().Contains("precursor");
		}
		/*
		public static bool isScannerRoomInRange(Vector3 position, bool needFunctional = true, float maxRange = 500, TechType scanningFor = TechType.None) {
			foreach (MapRoomFunctionality room in getObjectsNearWithComponent<MapRoomFunctionality>(position, maxRange)) {
				bool working = !needFunctional || room.CheckIsPowered();
				bool finding = scanningFor == TechType.None || room.typeToScan == scanningFor;
				if (working && finding && Vector3.Distance(room.transform.position, position) <= room.GetScanRange())
					return true;
			}
			return false;
		}
		*/
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
