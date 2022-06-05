﻿using System;
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
			PrefabPlaceholdersGroup pre = c.gameObject.EnsureComponent<PrefabPlaceholdersGroup>();
			pre.prefabPlaceholders[0].prefabClassId = CraftData.GetClassIdForTechType(item);
	    }
		
		public static void setDatabox(BlueprintHandTarget bpt, TechType tech) {
    		bpt.unlockTechType = tech;
    	}
		
		public static void setPDAPage(StoryHandTarget tgt, PDAManager.PDAPage page) {
			tgt.goal.goalType = Story.GoalType.Encyclopedia;
			tgt.goal.key = page.id;
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
			
		public static GameObject createWorldObject(string id, bool clone = true, bool makeActive = true) {
			GameObject prefab = lookupPrefab(id);
			if (prefab != null) {
				GameObject go = clone ? UnityEngine.Object.Instantiate(prefab) : prefab;
				if (go != null) {
					if (makeActive)
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
		
		public static void playSound(string path, bool queue = false) {
			playSoundAt(path, Player.main.transform.position, queue);
		}
		
		public static void playSoundAt(string path, Vector3 position, bool queue = false) {
			FMODAsset ass = getSound(path);
			if (queue)
				PDASounds.queue.PlayQueued(ass);
			else
				FMODUWE.PlayOneShot(ass, position);
		}
		
		public static FMODAsset getSound(string path) {
			FMODAsset ass = ScriptableObject.CreateInstance<FMODAsset>();
			ass.path = path;
			ass.id = VanillaSounds.getID(path);
			if (ass.id == null)
				ass.id = path;
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
		
		public static void offsetColliders(GameObject go, Vector3 move) {
			foreach (Collider c in go.GetComponentsInChildren<Collider>()) {
				if (c is SphereCollider) {
					((SphereCollider)c).center = ((SphereCollider)c).center+move;
				}
				else if (c is BoxCollider) {
					((BoxCollider)c).center = ((BoxCollider)c).center+move;
				}
				else if (c is CapsuleCollider) {
					((CapsuleCollider)c).center = ((CapsuleCollider)c).center+move;
				}
				else if (c is MeshCollider) {
					//TODO move to subobject
				}
			}
		}
		
		public static void visualizeColliders(GameObject go) {
			foreach (Collider c in go.GetComponentsInChildren<Collider>()) {
				Vector3 sc = Vector3.one;
				Vector3 off = Vector3.zero;
				PrimitiveType? pm = null;
				if (c is SphereCollider) {
					pm = PrimitiveType.Sphere;
					SphereCollider sp = (SphereCollider)c;
					sc = Vector3.one*sp.radius;
					off = sp.center;
				}
				else if (c is BoxCollider) {
					pm = PrimitiveType.Cube;
					BoxCollider b = (BoxCollider)c;
					sc = b.size/2;
					off = b.center;
				}
				else if (c is CapsuleCollider) {
					pm = PrimitiveType.Capsule;
					CapsuleCollider cc = (CapsuleCollider)c;
					sc = new Vector3(cc.radius, cc.height, cc.radius);
					off = cc.center;
				}
				if (pm != null && pm.HasValue) {
					GameObject vis = GameObject.CreatePrimitive(pm.Value);
					vis.transform.position = off;
					vis.transform.parent = c.transform;
					vis.transform.localScale = sc;
					vis.SetActive(true);
				}
			}
		}
		
		public static void refillItem(GameObject item, TechType batteryType = TechType.Battery) {
			Oxygen o = item.GetComponentInParent<Oxygen>();
			if (o != null) {
				o.oxygenAvailable = o.oxygenCapacity;
			}
			Battery b = item.GetComponentInParent<Battery>();
			if (b != null) {
				b.charge = b.capacity;
			}
			EnergyMixin e = item.GetComponentInParent<EnergyMixin>();
			if (e != null) {
				e.SetBattery(batteryType, 1);
			}
		}
		
		public static GameObject getBasePiece(string n, bool clone = true) {
			if (n.StartsWith("base_", StringComparison.InvariantCultureIgnoreCase))
				n = n.Substring(5);
			Base.PieceDef? piece = null;
			int res = -1;
			if (int.TryParse(n, out res)) {
				piece = Base.pieces[res];
			}
			else {
				piece = Base.pieces[(int)Enum.Parse(typeof(Base.Piece), n)];
			}
			return piece != null && piece.HasValue ? getBasePiece(piece.Value, clone) : null;
		}
		
		public static GameObject getBasePiece(Base.Piece piece, bool clone = true) {
			return getBasePiece(Base.pieces[(int)piece], clone);
		}
		
		public static GameObject getBasePiece(Base.PieceDef piece, bool clone = true) {
			GameObject go = piece.prefab.gameObject;
			if (clone) {
				go = UnityEngine.Object.Instantiate(go);
				go.SetActive(true);
			}
			return go;
		}
		
		public static Texture extractTexture(GameObject go, string texType) {
			return go.GetComponentInChildren<Renderer>().materials[0].GetTexture(texType);
		}
		
		public static void applyGravity(GameObject go) {
			//if (go.GetComponentInChildren<Collider>() == null || go.GetComponentInChildren<Rigidbody>() == null)
			//	return;
			if (go.GetComponentInChildren<Collider>() == null) {
				BoxCollider box = go.AddComponent<BoxCollider>();
				box.center = Vector3.zero;
				box.size = Vector3.one*0.25F;
			}
			//WorldForcesManager.instance.AddWorldForces(go.EnsureComponent<WorldForces>());
			WorldForces wf = go.EnsureComponent<WorldForces>();
			wf.enabled = true;
			wf.handleDrag = true;
			wf.handleGravity = true;
			wf.aboveWaterGravity = 9.81F;
			wf.underwaterGravity = 2;
			wf.underwaterDrag = 0.5F;
			Rigidbody rb = go.EnsureComponent<Rigidbody>();
			rb.constraints = RigidbodyConstraints.None;
			rb.useGravity = false;//true;
			rb.detectCollisions = true;
			rb.isKinematic = false;
			rb.drag = 0.5F;
			rb.angularDrag = 0.05F;/*
			rb.centerOfMass = new Vector3(0, 0.5F, 0);
			rb.inertiaTensor = new Vector3(0.2F, 0, 0.2F);*/
			wf.Awake();
			wf.OnEnable();
			rb.WakeUp();
		}
		
		private static readonly string[] texTypes = new string[]{"_MainTex", "_SpecTex", "_BumpMap", "_Illum"};
		
		public static void swapToModdedTextures<T>(Renderer r, DIPrefab<T> pfb) where T : PrefabReference {
			bool flag = false;
			foreach (String type in texTypes) {
				string path = "Textures/"+pfb.getTextureFolder()+"/"+formatFileName((ModPrefab)pfb)+type;
				Texture2D newTex = TextureManager.getTexture(path);
				if (newTex != null) {
					r.materials[0].SetTexture(type, newTex);
					r.sharedMaterial.SetTexture(type, newTex);
					flag = true;
					//SBUtil.writeToChat("Found "+type+" texture @ "+path);
				}
				else {
					//SBUtil.writeToChat("No texture found at "+path);
				}
			}
			if (!flag) {
				SBUtil.log("NO CUSTOM TEXTURES FOUND: "+pfb);
			}
			if (pfb.glowIntensity > 0) {
				SBUtil.setEmissivity(r, pfb.glowIntensity, "GlowStrength");
				
				r.materials[0].EnableKeyword("MARMO_EMISSION");
				r.sharedMaterial.EnableKeyword("MARMO_EMISSION");
			}
		}
			
		public static string formatFileName(ModPrefab pfb) {
			string n = pfb.ClassID;
			System.Text.StringBuilder ret = new System.Text.StringBuilder();
			for (int i = 0; i < n.Length; i++) {
				char c = n[i];
				if (c == '_')
					continue;
				bool caps = i == 0 || n[i-1] == '_';
				if (caps) {
					c = Char.ToUpperInvariant(c);
				}
				else {
					c = Char.ToLowerInvariant(c);
				}
				ret.Append(c);
			}
			return ret.ToString();
		}
		
		public static GameObject getModPrefabBaseObject<T>(DIPrefab<T> pfb) where T : PrefabReference {
			GameObject world = null;
			GameObject prefab;
			if (pfb is Craftable) {
				world = SBUtil.getItemGO((Craftable)pfb, pfb.baseTemplate.getPrefabID());
			}
			else if (UWE.PrefabDatabase.TryGetPrefab(pfb.baseTemplate.getPrefabID(), out prefab)) {
				world = UnityEngine.Object.Instantiate(prefab);
			}
			else {
				writeToChat("Could not fetch template GO for "+pfb);
				return null;
			}
			if (world == null) {
				writeToChat("Got null for template GO for "+pfb);
				return null;
			}
			world.SetActive(false);
			ModPrefab mod = (ModPrefab)pfb;
			world.EnsureComponent<TechTag>().type = mod.TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = mod.ClassID;
			if (pfb.isResource()) {
				world.EnsureComponent<ResourceTracker>().techType = mod.TechType;
				world.EnsureComponent<ResourceTracker>().overrideTechType = mod.TechType;
			}
			Renderer r = world.GetComponentInChildren<Renderer>();
			swapToModdedTextures(r, pfb);
			pfb.prepareGameObject(world, r);
			//writeToChat("Applying custom texes to "+world+" @ "+world.transform.position);
			return world;
		}
		
		public static void showPDANotification(string text, string soundPath) {
			PDANotification pda = Player.main.gameObject.AddComponent<PDANotification>();
			pda.enabled = true;
			pda.text = text;
			pda.sound = getSound(soundPath);
			pda.Play();
			UnityEngine.Object.Destroy(pda, 15);
		}
		
		public static void convertResourceChunk(GameObject go, TechType tech) {
			/*
			go.EnsureComponent<TechTag>().type = tech;
			go.EnsureComponent<Pickupable>().SetTechTypeOverride(tech);
			go.EnsureComponent<PrefabIdentifier>().ClassId = mod.ClassID;
			go.EnsureComponent<ResourceTracker>().techType = tech;
			go.EnsureComponent<ResourceTracker>().overrideTechType = tech;
			*/
			GameObject prefab = CraftData.GetPrefabForTechType(tech, true);
			GameObject world = UnityEngine.Object.Instantiate(prefab);
			world.transform.position = go.transform.position;
			world.transform.rotation = go.transform.rotation;
			world.transform.localScale = go.transform.localScale;
			UnityEngine.Object.Destroy(go);
		}
		
		public static void addSelfUnlock(TechType tech, PDAManager.PDAPage page = null) {
			KnownTechHandler.Main.SetAnalysisTechEntry(tech, new List<TechType>(){tech});
			if (page != null) {
				PDAScanner.EntryData e = new PDAScanner.EntryData();
				e.key = tech;
				e.scanTime = 5;
				e.locked = true;
				page.register();
				e.encyclopedia = page.id;
				PDAHandler.AddCustomScannerEntry(e);
			}
		}
		
	}
}
