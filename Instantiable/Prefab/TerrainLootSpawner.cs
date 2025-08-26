using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class TerrainLootSpawner : Spawnable {

		internal static readonly Dictionary<string, string> spawnIDs = new Dictionary<string, string>();

		public readonly string spawnID;

		//spawn with localScale vec of x=exclusion radius, y=target count, z=max range
		public TerrainLootSpawner(string id, string spawn) : base(id, "", "") {
			if (string.IsNullOrEmpty(spawn))
				throw new Exception("Cannot register a loot spawner of null!");
			spawnID = spawn;
			OnFinishedPatching += () => { spawnIDs[ClassID] = spawnID; };
		}

		public override GameObject GetGameObject() {
			GameObject go = new GameObject();
			go.EnsureComponent<TerrainLootSpawnerTag>().spawnID = spawnID;
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
			return go;
		}

		class TerrainLootSpawnerTag : MonoBehaviour {

			internal string spawnID;

			private int spawned;

			void Update() {
				if (spawnID == null) {
					string id = this.GetComponent<PrefabIdentifier>().classId;
					spawnID = TerrainLootSpawner.spawnIDs.ContainsKey(id) ? TerrainLootSpawner.spawnIDs[id] : null;
					if (spawnID == null)
						SNUtil.log("No spawn ID for prefab " + id + " @ " + transform.position);
					return;
				}
				Vector3 vec = UnityEngine.Random.rotationUniform.eulerAngles.normalized;
				Ray ray = new Ray(transform.position, vec);
				if (UWE.Utils.RaycastIntoSharedBuffer(ray, transform.localScale.z, Voxeland.GetTerrainLayerMask()) > 0) {
					RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
					if (hit.transform != null) {
						foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(hit.point, transform.localScale.x)) {
							if (pi.ClassId == spawnID)
								return;
						}
						GameObject go = ObjectUtil.createWorldObject(spawnID);
						go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
						go.transform.position = hit.point;
						spawned++;
					}
				}
				if (spawned >= transform.localScale.y)
					gameObject.destroy();
			}

		}

	}
}
