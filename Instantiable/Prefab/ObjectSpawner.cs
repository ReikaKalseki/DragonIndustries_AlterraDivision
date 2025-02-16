using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;
using Story;

namespace ReikaKalseki.DIAlterra {
	
	public sealed class ObjectSpawner : Spawnable {
		
		internal static readonly Dictionary<string, SpawnSet> spawnSets = new Dictionary<string, SpawnSet>();
		
		public readonly SpawnSet spawns;
	        
		public ObjectSpawner(string id, SpawnSet spawns) : base(id, "", "") {
			this.spawns = spawns;
			OnFinishedPatching += () => {spawnSets[ClassID] = spawns;};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = new GameObject();
			go.EnsureComponent<ObjectSpawnerTag>().spawns = spawns;
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
			return go;
	    }
		
		class ObjectSpawnerTag : MonoBehaviour {
		
			internal SpawnSet spawns;
			
			void Update() {
				if (spawns == null) {
					string id = GetComponent<PrefabIdentifier>().classId;
					spawns = ObjectSpawner.spawnSets.ContainsKey(id) ? ObjectSpawner.spawnSets[id] : null;
					if (spawns == null)
						SNUtil.log("No spawn IDs for prefab "+id+" @ "+transform.position);
					return;
				}
				GameObject go = ObjectUtil.createWorldObject(spawns.spawnIDs.getRandomEntry().getPrefabID());
				go.transform.rotation = transform.rotation;
				go.transform.Rotate(go.transform.up, UnityEngine.Random.Range(0F, 360F));
				go.transform.position = transform.position;
				if (spawns.onSpawn != null)
					spawns.onSpawn.Invoke(go, transform);
				UnityEngine.Object.Destroy(gameObject);
			}
			
		}
		
		public class SpawnSet {
			
			public readonly WeightedRandom<PrefabReference> spawnIDs;
			public readonly Action<GameObject, Transform> onSpawn;
			
			public SpawnSet(WeightedRandom<PrefabReference> spawns, Action<GameObject, Transform> act = null) {
				spawnIDs = spawns;
				onSpawn = act;
			}
			
		}
			
	}
}
