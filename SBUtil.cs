using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class SBUtil {
		
		private static Assembly modDLL;
		
		private static readonly Int3 batchOffset = new Int3(13, 19, 13);
		private static readonly Int3 batchOffsetM = new Int3(32, 0, 32);
		private static readonly int batchSize = 160;
		
		public static Assembly getModDLL() {
			if (modDLL == null)
				modDLL = Assembly.GetExecutingAssembly();
			return modDLL;
		}
		
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
		
		public static string getBiome(Vector3 pos) {
			BiomeProperties b = LargeWorld.main.GetBiomeProperties(pos.roundToInt3().XZ());
			return b != null ? b.name : "null";
		}
		
		public static void removeComponent<C>(GameObject go) where C : Component {
			Component c = go.GetComponent<C>();
			if (c != null)
				UnityEngine.Object.Destroy(c);
		}
		
		public static int getInstallSeed() {
			int seed = getModDLL().Location.GetHashCode();
			seed &= (~(1 << Environment.ProcessorCount));
			string n = Environment.MachineName;
			if (string.IsNullOrEmpty(n))
				n = Environment.UserName;
			seed ^= (n != null ? n.GetHashCode() : 0);
			seed ^= Environment.OSVersion.VersionString.GetHashCode();
			return seed;
		}
		
		public static int getWorldSeedInt() {
			long seed = getWorldSeed();
			return unchecked((int)((seed & 0xFFFFFFFFL) ^ (seed >> 32)));
		}
		
		public static long getWorldSeed() {
			string path = SaveUtils.GetCurrentSaveDataDir();
			long seed = SaveLoadManager._main.firstStart;
			seed ^= path.GetHashCode();
			seed ^= unchecked(((long)getModDLL().Location.GetHashCode()) << 32);
			return seed;
		}
		
		public static TechType getTechType(string tech) {
			TechType ret;
			if (!Enum.TryParse<TechType>(tech, false, out ret)) {
				if (TechTypeHandler.TryGetModdedTechType(tech, out ret)) {
					return ret;
				}
				else {
					log("TechType '"+tech+"' not found!");
					return TechType.None;
				}
			}
			return ret;
		}
		
		public static void log(string s, int indent = 0) {
			string id = getModDLL().GetName().Name.ToUpperInvariant().Replace("PLUGIN_", "");
			if (indent > 0) {
				s = s.PadLeft(s.Length+indent, ' ');
			}
			Debug.Log(id+": "+s);
		}
		
		public static void writeToChat(string s) {
			ErrorMessage.AddMessage(s);
		}
		
		public static void dumpObjectData(GameObject go) {
			dumpObjectData(go, 0);
		}
		
		private static void dumpObjectData(GameObject go, int indent) {
			if (go == null) {
				log("null object");
				return;
			}
			log("object "+go.ToString(), indent);
			log("chain "+go.GetFullHierarchyPath(), indent);
			log("components: "+string.Join(", ", (object[])go.GetComponents<Component>()), indent);
			Pickupable p = go.GetComponent<Pickupable>();
			if (p != null) {
				log("pickup: "+p.GetTechType()+" = "+p.isPickupable, indent);
			}
			TechTag tag = go.GetComponent<TechTag>();
			if (tag != null) {
				log("techtag: "+tag.type, indent);
			}
			ResourceTracker res = go.GetComponent<ResourceTracker>();
			if (res != null) {
				log("resource: "+res.name+" = "+res.techType, indent);
			}
			EntityTag e = go.GetComponent<EntityTag>();
			if (e != null) {
				log("entity: "+e.name+" = "+e.tag, indent);
			}
			LiveMixin live = go.GetComponent<LiveMixin>();
			if (live != null) {
				log("live: "+live.name+" = "+live.health+"/"+live.maxHealth+" = "+live.IsAlive(), indent);
			}
			InfectedMixin infect = go.GetComponent<InfectedMixin>();
			if (infect != null) {
				log("infected: "+infect.name+" = "+infect.infectedAmount, indent);
			}
			log("transform: "+go.transform, indent);
			if (go.transform != null) {
				log("position: "+go.transform.position, indent);
				log("transform object: "+go.transform.gameObject, indent);
				log("transform parent: "+go.transform.parent, indent);
				if (go.transform.parent != null) {
					log("transform parent object: ", indent);
					dumpObjectData(go.transform.parent.gameObject, indent+3);
				}
				else {
					log("transform parent object: null", indent);
				}
			}
		}
		
		public static void dumpObjectData(Component go) {
			dumpObjectData(go, 0);
		}
		
		private static void dumpObjectData(Component go, int indent) {
			if (go == null) {
				log("null component");
				return;
			}
			log("component "+go.ToString(), indent);
			log("object "+go.gameObject, indent);
			log("chain "+go.gameObject.GetFullHierarchyPath(), indent);
			log("components: "+string.Join(", ", (object[])go.GetComponents<Component>()), indent);
			Pickupable p = go.GetComponent<Pickupable>();
			if (p != null) {
				log("pickup: "+p.GetTechType()+" = "+p.isPickupable, indent);
			}
			TechTag tag = go.GetComponent<TechTag>();
			if (tag != null) {
				log("techtag: "+tag.type, indent);
			}
			ResourceTracker res = go.GetComponent<ResourceTracker>();
			if (res != null) {
				log("resource: "+res.name+" = "+res.techType, indent);
			}
			EntityTag e = go.GetComponent<EntityTag>();
			if (e != null) {
				log("entity: "+e.name+" = "+e.tag, indent);
			}
			LiveMixin live = go.GetComponent<LiveMixin>();
			if (live != null) {
				log("live: "+live.name+" = "+live.health+"/"+live.maxHealth+" = "+live.IsAlive(), indent);
			}
			InfectedMixin infect = go.GetComponent<InfectedMixin>();
			if (infect != null) {
				log("infected: "+infect.name+" = "+infect.infectedAmount, indent);
			}
			Renderer ren = go is Renderer ? (Renderer)go : go.GetComponent<Renderer>();
			if (ren != null) {
				log("renderer: "+ren.name, indent);
				foreach (Material m in ren.materials) {
					log("material: "+m.name, indent);
					log("color: "+m.color, indent);
					log("tex: "+m.mainTexture, indent);
					log("tex name: "+m.mainTexture.name, indent);
					log("tex pos: "+m.mainTextureOffset, indent);
					log("tex scale: "+m.mainTextureScale, indent);
					foreach (string tex in m.GetTexturePropertyNames()) {
						log("tex ID '"+tex+"': "+m.GetTexture(tex), indent);
						log("tex ID '"+tex+"': "+m.GetTextureOffset(tex), indent);
						log("tex ID '"+tex+"': "+m.GetTextureScale(tex), indent);
					}
				}
			}
			log("transform: "+go.transform, indent);
			if (go.transform != null) {
				log("position: "+go.transform.position, indent);
				log("transform object: "+go.transform.gameObject, indent);
				log("transform parent: "+go.transform.parent, indent);
				if (go.transform.parent != null) {
					log("transform parent object: ", indent);
					dumpObjectData(go.transform.parent.gameObject, indent+3);
				}
				else {
					log("transform parent object: null", indent);
				}
			}
		}
		
		public static GameObject dropItem(Vector3 pos, TechType item) {
			string id = CraftData.GetClassIdForTechType(item);
			if (id != null) {
				GameObject go = createWorldObject(id);
				if (go != null)
					go.transform.position = pos;
				return go;
	    	}
	    	else {
	    		log("NO SUCH ITEM TO DROP: "+item);
	    		return null;
	    	}
		}
		/*
		public static System.Collections.IEnumerator getPrefab(string id, out GameObject pfb) {
		    UWE.IPrefabRequest request = UWE.PrefabDatabase.GetPrefabAsync(id);
		    yield return request;
		    GameObject original;
		    request.TryGetPrefab(out original);
		    pfb = original;
		}*/
		
		public static GameObject getItemGO(Craftable item, string template) {
			return getItemGO(item.TechType, item.ClassID, template);
		}
		
		public static GameObject getItemGO(TechType tech, string template) {
			return getItemGO(tech, Enum.GetName(tech.GetType(), tech), template);
		}
		
		public static GameObject getItemGO(TechType tech, string id, string template) {
			GameObject prefab = Resources.Load<GameObject>(template);
			if (prefab == null)
				throw new Exception("Null prefab result during item '"+template+"' lookup");
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
			if (gameObject == null)
				throw new Exception("Null item result during item '"+template+"' build");
			TechTag component = gameObject.EnsureComponent<TechTag>();
			UniqueIdentifier component2 = gameObject.EnsureComponent<PrefabIdentifier>();
			if (component == null)
				throw new Exception("Null techtag result during item '"+template+"' repopulate");
			if (component2 == null)
				throw new Exception("Null UID result during item '"+template+"' repopulate");
			component.type = tech;
			component2.ClassId = id;
			return gameObject;
		}
    
	    public static void setCrateItem(SupplyCrate c, TechType item) {
			PrefabPlaceholdersGroup pre = c.gameObject.GetComponentInParent<PrefabPlaceholdersGroup>();
			pre.prefabPlaceholders[0].prefabClassId = CraftData.GetClassIdForTechType(item); //TODO crate item DOES NOT SEEM  TO WORK
	    }
		
		public static void setDatabox(BlueprintHandTarget bpt, TechType tech) {
    		bpt.unlockTechType = tech;
    	}
		
		public static bool objectCollidesPosition(GameObject go, Vector3 pos) {
			if (go.transform != null) {
				Collider c = go.GetComponentInParent<Collider>();
				if (c != null && c.enabled) {
					return (c.ClosestPoint(pos) - pos).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
				}
				Renderer r = go.GetComponentInChildren<Renderer>();
				if (r != null && r.enabled) {
					return r.bounds.Contains(pos);
				}
			}
			return false;
		}
		
		public static string getPrefabID(GameObject go) {
			if (go == null)
				return null;
			PrefabIdentifier p = go.GetComponentInParent<PrefabIdentifier>();
			if (p != null)
				return p.classId;
			TechType type = CraftData.GetTechType(go);
			return CraftData.GetClassIdForTechType(type);
		}
			
		public static GameObject createWorldObject(string id) {
			GameObject prefab = lookupPrefab(id);
			if (prefab != null) {
				GameObject go = UnityEngine.Object.Instantiate(prefab);
				if (go != null) {
					go.SetActive(true);
					return go;
				}
				else {
					writeToChat("Prefab found and placed succeeeded but resulted in null?!");
					return null;
				}
			}
			else {
				writeToChat("Prefab not found for id '"+id+"'.");
				return null;
			}
		}
			
		public static GameObject lookupPrefab(string id) {
			GameObject ret = null;
			if (UWE.PrefabDatabase.TryGetPrefab(id, out ret))
				return ret;
			TechType key;
			if (TechTypeHandler.TryGetModdedTechType(id, out key)) {
				ret = CraftData.GetPrefabForTechType(key);
			}
			return ret;
		}
		
		public static GameObject replaceObject(GameObject obj, string pfb) {
			GameObject repl = createWorldObject(pfb);
			repl.transform.position = obj.transform.position;
			repl.transform.rotation = obj.transform.rotation;
			repl.transform.localScale = obj.transform.localScale;
			return repl;
		}
		
		public static void playSound(string path) {
			
		}
		
		public static FMODAsset getSound(string path) {
			FMODAsset ass = ScriptableObject.CreateInstance<FMODAsset>();
			ass.path = path;
			//ass.id = id;
			return ass;
		}
		
		public static void setEmissivity(Renderer r, float amt, string type) {
			r.materials[0].SetFloat("_"+type, amt);
			r.sharedMaterial.SetFloat("_"+type, amt);
			r.materials[0].SetFloat("_"+type+"Night", amt);
			r.sharedMaterial.SetFloat("_"+type+"Night", amt);
		}
		
		public static void makeTransparent(Renderer r) {
			foreach (Material m in r.materials) {
				m.EnableKeyword("_ZWRITE_ON");
	  			m.EnableKeyword("WBOIT");
				m.SetInt("_ZWrite", 0);
				m.SetInt("_Cutoff", 0);
				m.SetFloat("_SrcBlend", 1f);
				m.SetFloat("_DstBlend", 1f);
				m.SetFloat("_SrcBlend2", 0f);
				m.SetFloat("_DstBlend2", 10f);
				m.SetFloat("_AddSrcBlend", 1f);
				m.SetFloat("_AddDstBlend", 1f);
				m.SetFloat("_AddSrcBlend2", 0f);
				m.SetFloat("_AddDstBlend2", 10f);
				m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack | MaterialGlobalIlluminationFlags.RealtimeEmissive;
				m.renderQueue = 3101;
				m.enableInstancing = true;
			}
		}
		
	}
}
