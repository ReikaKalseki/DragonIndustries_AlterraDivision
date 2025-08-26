using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public static class WorldUtil {

		private static readonly Int3 batchOffset = new Int3(13, 19, 13);
		private static readonly Int3 batchOffsetM = new Int3(32, 0, 32);
		private static readonly int batchSize = 160;

		public static readonly Vector3 DUNES_METEOR = new Vector3(-1125, -380, 1130);
		public static readonly Vector3 LAVA_DOME = new Vector3(-273, -1355, -152);
		public static readonly Vector3 SUNBEAM_SITE = new Vector3(301, 15, 1086);
		public static readonly Vector3 DEGASI_FLOATING_BASE = new Vector3(-763, 20, -1104);
		public static readonly Vector3 DEGASI_JELLY_BASE = new Vector3(85, -260, -356);
		public static readonly Vector3 DEGASI_DGR_BASE = new Vector3(-643, -505, -944.5F);

		public readonly static Vector3 lavaCastleCenter = new Vector3(-49, -1242, 118);
		public readonly static float lavaCastleInnerRadius = 65;//75;
		public readonly static float lavaCastleRadius = Vector3.Distance(new Vector3(-116, -1194, 126), lavaCastleCenter)+32;

		private static readonly Vector3 auroraPoint1 = new Vector3(746, 0, -362-50);
		private static readonly Vector3 auroraPoint2 = new Vector3(1295, 0, 110-50);
		private static readonly float auroraPointRadius = 275;

		//public static HashSet<PositionedPrefab> registeredGeysers = new HashSet<PositionedPrefab>();

		private static readonly HashSet<Vector3> geysers = new HashSet<Vector3>();
		/*
		public static void dumpGeysers() {
			string file = BuildingHandler.instance.dumpPrefabs("geysers", registeredGeysers);
			SNUtil.writeToChat("Exported "+registeredGeysers.Count+" geysers to "+file);
		}
		*/
		public enum CompassDirection {
			NORTH,
			EAST,
			SOUTH,
			WEST,
		}

		public static CompassDirection getOpposite(CompassDirection dir) {
			switch (dir) {
				case CompassDirection.NORTH:
					return CompassDirection.SOUTH;
				case CompassDirection.EAST:
					return CompassDirection.WEST;
				case CompassDirection.SOUTH:
					return CompassDirection.NORTH;
				case CompassDirection.WEST:
					return CompassDirection.EAST;
			}
			throw new Exception("Unrecognized direction");
		}

		public static readonly Dictionary<CompassDirection, Vector3> compassAxes = new Dictionary<CompassDirection, Vector3>(){
			{CompassDirection.NORTH, new Vector3(0, 0, 1)},
			{CompassDirection.EAST, new Vector3(1, 0, 0)},
			{CompassDirection.SOUTH, new Vector3(0, 0, -1)},
			{CompassDirection.WEST, new Vector3(-1, 0, 0)},
		};

		//private static readonly Dictionary<string, string> biomeNames = new Dictionary<string, string>();

		static WorldUtil() {
			string file = Path.Combine(Path.GetDirectoryName(SNUtil.diDLL.Location), "geysers.xml");
			if (File.Exists(file)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(file);
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					try {
						PositionedPrefab pfb = (PositionedPrefab)ObjectTemplate.construct(e);
						geysers.Add(pfb.position);
					}
					catch (Exception ex) {
						SNUtil.log(ex.ToString());
						SNUtil.writeToChat("Could not load XML block, threw exception: " + ex.ToString() + " from " + e.format());
					}
				}
			}
		}
		/*
		private static void mapBiomeName(string name, params string[] keys) {
			foreach (string s in keys) {
				biomeNames[s] = name;
			}
		}*/

		public static Int3 getBatch(Vector3 pos) { //"Therefore e.g. batch (12, 18, 12) covers the voxels from (-128, -160, -128) to (32, 0, 32)."
			Int3 coord = pos.roundToInt3();
			coord -= batchOffsetM;
			coord.x = (int)Math.Floor(coord.x / (float)batchSize);
			coord.y = (int)Math.Floor(coord.y / (float)batchSize);
			coord.z = (int)Math.Floor(coord.z / (float)batchSize);
			return coord + batchOffset;
		}

		/// <returns>The min XYZ corner</returns>
		public static Int3 getWorldCoord(Int3 batch) { //TODO https://i.imgur.com/sbXjIpq.png
			batch -= batchOffset;
			return (batch * batchSize) + batchOffsetM;
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
				SNUtil.log("NO SUCH ITEM TO DROP: " + item);
				return null;
			}
		}

		public static mset.Sky getSkybox(string biome, bool allowNotFoundError = true) {
			BiomeBase bb = BiomeBase.getBiome(biome);
			if (bb is CustomBiome)
				return ((CustomBiome)bb).getSky();
			int idx = WaterBiomeManager.main.GetBiomeIndex(biome);
			if (idx < 0) {
				if (allowNotFoundError) {
					SNUtil.writeToChat("Biome '" + biome + "' had no sky lookup. See log for biome table.");
					SNUtil.log(WaterBiomeManager.main.biomeLookup.toDebugString());
				}
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

		/// <remarks>Will not find things without colliders!
		/// 
		/// Avoid using this with components that will result in many findings, as you then end up iterating a large list. Use the getter version instead.
		/// </remarks>
		public static HashSet<C> getObjectsNearWithComponent<C>(Vector3 pos, float r) where C : MonoBehaviour {
			return getObjectsNear(pos, r, go => UWE.Utils.GetComponentInHierarchy<C>(go));
		}

		/// <remarks>Will not find things without colliders!</remarks>
		public static HashSet<GameObject> getObjectsNearMatching(Vector3 pos, float r, Predicate<GameObject> check) {
			return getObjectsNear(pos, r, go => check(go) ? go : null);
		}

		/// <remarks>Will not find things without colliders!</remarks>
		public static HashSet<GameObject> getObjectsNear(Vector3 pos, float r) {
			return getObjectsNear<GameObject>(pos, r, null);
		}

		/// <remarks>Will not find things without colliders!</remarks>
		public static HashSet<R> getObjectsNear<R>(Vector3 pos, float r, Func<GameObject, R> converter = null) where R : UnityEngine.Object {
			HashSet<R> set = new HashSet<R>();
			getObjectsNear(pos, r, go => { set.Add(go); return false; }, converter);
			return set;
		}

		/// <remarks>Will not find things without colliders!</remarks>
		public static void getGameObjectsNear(Vector3 pos, float r, Action<GameObject> getter) {
			getObjectsNear<GameObject>(pos, r, getter, null);
		}

		/// <remarks>Will not find things without colliders!</remarks>
		public static void getObjectsNear<R>(Vector3 pos, float r, Action<R> getter, Func<GameObject, R> converter = null) where R : UnityEngine.Object {
			getObjectsNear<R>(pos, r, obj => { getter(obj); return false; }, converter);
		}

		/// <remarks>Will not find things without colliders!</remarks>
		public static void getObjectsNear<R>(Vector3 pos, float r, Func<R, bool> getter, Func<GameObject, R> converter = null) where R : UnityEngine.Object {
			HashSet<GameObject> found = new HashSet<GameObject>();
			foreach (RaycastHit hit in Physics.SphereCastAll(pos, r, Vector3.up, 0.1F)) {
				if (hit.transform) {
					GameObject go = UWE.Utils.GetEntityRoot(hit.transform.gameObject);
					if (!go)
						go = hit.transform.gameObject;
					if (!go)
						continue;
					if (found.Contains(go)) //prevent duplicates
						continue;
					found.Add(go);
					UnityEngine.Object obj = converter == null ? (UnityEngine.Object)go : converter(go);
					if (obj) {
						if (getter((R)obj))
							return;
					}
				}
			}
		}

		/// <remarks>Will not find things without colliders!</remarks>
		public static GameObject areAnyObjectsNear(Vector3 pos, float r, Predicate<GameObject> check) {
			GameObject ret = null;
			getObjectsNear<GameObject>(pos, r, go => { ret = go; return true; }, go => check(go) ? go : null);
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
			return plant != null && plant.isNativeToBiome(go.transform.position);
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

		public static bool isInDRF(Vector3 pos) {
			return VanillaBiomes.LOSTRIVER.isInBiome(pos) && isPrecursorBiome(pos);
		}

		public static bool isInLavaCastle(Player ep) {
			return ep.IsInsideWalkable() && ep.precursorOutOfWater && isInLavaCastle(ep.transform.position);
		}

		public static bool isInLavaCastle(Vector3 pos) {
			return VanillaBiomes.ILZ.isInBiome(pos) && isPrecursorBiome(pos);
		}

		public static bool isInsideAurora2D(Vector3 pos, float extra = 0) {
			return MathUtil.getDistanceToLineSegment(pos, auroraPoint1, auroraPoint2) <= auroraPointRadius + extra;
		}

		public static bool isMountainIsland(Vector3 pos) {
			return pos.y > 1 && ((pos - SUNBEAM_SITE).sqrMagnitude <= 2500 || VanillaBiomes.MOUNTAINS.isInBiome(pos));
		}

		public static string getRegionalDescription(Vector3 pos, bool includeDepth) {
			if ((pos - LAVA_DOME).sqrMagnitude <= 6400)
				return "Lava Dome";
			if ((pos - DUNES_METEOR).sqrMagnitude <= 14400)
				return "Meteor Crater";
			if (isMountainIsland(pos))
				return "Mountain Island";
			float dist = (pos-lavaCastleCenter).sqrMagnitude;
			if (dist <= (lavaCastleInnerRadius * lavaCastleInnerRadius) + 225)
				return "Lava Castle (Interior)";
			if (dist <= (lavaCastleRadius * lavaCastleRadius) + 900)
				return "Lava Castle";
			BiomeBase bb = BiomeBase.getBiome(pos);
			if (BiomeBase.isUnrecognized(bb))
				return "Unknown Biome @ " + pos;
			if (bb == VanillaBiomes.LOSTRIVER || bb == VanillaBiomes.CRASH) {
				switch (DIHooks.getBiomeAt(WaterBiomeManager.main.GetBiome(pos), pos)) {
					case "LostRiver_BonesField_Corridor":
					case "LostRiver_BonesField_Corridor_Stream":
					case "LostRiver_BonesField":
					case "LostRiver_BonesField_Lake":
					case "LostRiver_BonesField_LakePit":
						return "Bones Field";
					case "LostRiver_GhostTree_Lower":
					case "LostRiver_GhostTree":
						return "Ghost Forest";
					case "LostRiver_Junction":
						return "Lost River Junction";
					case "LostRiver_Canyon":
					case "LostRiver_SkeletonCave":
						return "Ghost Canyon";
					case "Precursor_LostRiverBase":
						return "Disease Research Facility";
					case "LostRiver_Corridor":
						return "Lost River Corridor";
					case "crashZone_Mesa":
						return "Crash Zone Mesas";
				}
			}
			if (bb == VanillaBiomes.CRASH) {
				if (isInsideAurora2D(pos, 100)) {
					string ret = "The Aurora";
					if (pos.y >= 5 && CrashedShipExploder.main.IsExploded()) {
						ret += " (Inside)";
					}
					else {
						//Vector3 ship = (auroraPoint1+auroraPoint2)*0.5F;//CrashedShipExploder.main.transform.position;
						Vector3 point = MathUtil.getClosestPointToLineSegment(pos, auroraPoint1, auroraPoint2);
						float angle = Vector3.SignedAngle(auroraPoint2-auroraPoint1, pos-point, Vector3.up);
						angle = (angle + 360) % 360;
						if (Mathf.Abs(angle) <= 30)
							ret += " (Front)";
						if (Mathf.Abs(angle - 180) <= 30)
							ret += " (Rear)";
						if (Mathf.Abs(angle - 90) <= 45)
							ret += " (Far Side)";
						if (Mathf.Abs(angle - 270) <= 45)
							ret += " (Near Side)";
					}
					return ret;
				}
			}
			string biome = bb.displayName;
			int depth = (int)-pos.y;
			string ew = pos.x < 0 ? "West" : "East";
			string ns = pos.z > 0 ? "North" : "South";
			if (!bb.existsInSeveralPlaces() || Math.Abs(pos.x) < 250 || Math.Abs(pos.x) < Math.Abs(pos.z) / 2.5F)
				ew = "";
			if (!bb.existsInSeveralPlaces() || Math.Abs(pos.z) < 250 || Math.Abs(pos.z) < Math.Abs(pos.x) / 2.5F)
				ns = "";
			if (Vector3.Distance(pos, getNearestGeyserPosition(pos)) <= 50) {
				ew = "";
				ns = "";
				biome += " Geyser";
			}
			bool pre = !string.IsNullOrEmpty(ew) || !string.IsNullOrEmpty(ns);
			string loc = ns+ew+(pre ? " " : "")+biome;
			if (includeDepth)
				loc += " (" + depth + "m)";
			return loc;
		}

		public static Vector3 getNearestGeyserPosition(Vector3 pos) {
			Vector3 nearest = new Vector3(0, 8000, 0);
			foreach (Vector3 at in geysers) {
				if ((at - pos).sqrMagnitude < (nearest - pos).sqrMagnitude)
					nearest = at;
			}
			return nearest;
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
			p.gameObject.destroy(false, dur + killOffset);
			PrefabIdentifier pi = p.gameObject.FindAncestor<PrefabIdentifier>();
			if (pi)
				pi.destroy();
			LargeWorldEntity lw = p.gameObject.FindAncestor<LargeWorldEntity>();
			if (lw)
				lw.destroy();
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

		public static GameObject reparentAllNearTo(string name, Vector3 pos, float r, Predicate<GameObject> check) {
			GameObject ctr = new GameObject(name);
			ctr.transform.position = pos;
			WorldUtil.getObjectsNear<GameObject>(pos, r, go => ObjectUtil.reparentTo(ctr, go), go => go.isRootObject() && check.Invoke(go) ? go : null);
			return ctr;
		}

		public static void reparentAllNearTo(GameObject ctr, Vector3 pos, float r, Predicate<GameObject> check) {
			ctr.transform.position = pos;
			WorldUtil.getObjectsNear<GameObject>(pos, r, go => ObjectUtil.reparentTo(ctr, go), go => go.isRootObject() && check.Invoke(go) ? go : null);
		}

		public static GameObject getBatch(int x, int y, int z) {
			Transform root = LargeWorld.main.transform.parent;
			return root.gameObject.getChildObject("Batches/Batch " + x + "," + y + "," + z + " objects");
		}

		public static StasisSphere createStasisSphere(Vector3 pos, float r, float pwr = 1) {
			GameObject sph = ObjectUtil.lookupPrefab(TechType.StasisRifle).GetComponent<StasisRifle>().effectSpherePrefab;
			sph = UnityEngine.Object.Instantiate(sph);
			sph.SetActive(true);
			sph.fullyEnable();
			sph.transform.position = pos;
			StasisSphere ss = sph.GetComponent<StasisSphere>();
			ss.fieldEnergy = pwr;
			ss.time = Mathf.Lerp(ss.minTime, ss.maxTime, ss.fieldEnergy);
			ss.radius = r;
			ss.EnableField();
			ss.soundActivate.Stop(true);
			SNUtil.log("Created stasis sphere of radius " + ss.radius + ", duration " + ss.time + ", energy " + ss.fieldEnergy);
			return ss;
		}

		class TransientParticleTag : MonoBehaviour {

			public void stop() {
				this.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
			}

			void OnDestroy() {
				if (gameObject)
					gameObject.destroy(false);
			}

			void OnDisable() {
				if (gameObject)
					gameObject.destroy(false);
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
