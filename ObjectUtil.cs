using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class ObjectUtil {
		
		public static void removeComponent<C>(GameObject go) where C : Component {
			foreach (Component c in go.GetComponentsInParent<C>()) {
				if (c is MonoBehaviour)
					((MonoBehaviour)c).enabled = false;
				UnityEngine.Object.DestroyImmediate(c);
			}
		}
		
		public static void dumpObjectData(GameObject go) {
			dumpObjectData(go, 0);
		}
		
		private static void dumpObjectData(GameObject go, int indent) {
			if (go == null) {
				SNUtil.log("null object");
				return;
			}
			SNUtil.log("object "+go.ToString(), indent);
			SNUtil.log("chain "+go.GetFullHierarchyPath(), indent);
			SNUtil.log("components: "+string.Join(", ", (object[])go.GetComponents<Component>()), indent);
			Pickupable p = go.GetComponent<Pickupable>();
			if (p != null) {
				SNUtil.log("pickup: "+p.GetTechType()+" = "+p.isPickupable, indent);
			}
			TechTag tag = go.GetComponent<TechTag>();
			if (tag != null) {
				SNUtil.log("techtag: "+tag.type, indent);
			}
			ResourceTracker res = go.GetComponent<ResourceTracker>();
			if (res != null) {
				SNUtil.log("resource: "+res.name+" = "+res.techType, indent);
			}
			EntityTag e = go.GetComponent<EntityTag>();
			if (e != null) {
				SNUtil.log("entity: "+e.name+" = "+e.tag, indent);
			}
			LiveMixin live = go.GetComponent<LiveMixin>();
			if (live != null) {
				SNUtil.log("live: "+live.name+" = "+live.health+"/"+live.maxHealth+" = "+live.IsAlive(), indent);
			}
			InfectedMixin infect = go.GetComponent<InfectedMixin>();
			if (infect != null) {
				SNUtil.log("infected: "+infect.name+" = "+infect.infectedAmount, indent);
			}
			SNUtil.log("transform: "+go.transform, indent);
			if (go.transform != null) {
				SNUtil.log("position: "+go.transform.position, indent);
				SNUtil.log("transform object: "+go.transform.gameObject, indent);
				for (int i = 0; i < go.transform.childCount; i++) {
					SNUtil.log("child object #"+i+": ", indent);
					dumpObjectData(go.transform.GetChild(i).gameObject, indent+3);
				}
			}
		}
		
		public static void dumpObjectData(Component go) {
			dumpObjectData(go, 0);
		}
		
		private static void dumpObjectData(Component go, int indent) {
			if (go == null) {
				SNUtil.log("null component");
				return;
			}
			SNUtil.log("component "+go.ToString(), indent);
			SNUtil.log("object "+go.gameObject, indent);
			SNUtil.log("chain "+go.gameObject.GetFullHierarchyPath(), indent);
			SNUtil.log("active "+go.gameObject.activeSelf+"/"+go.gameObject.activeInHierarchy);
			SNUtil.log("components: "+string.Join(", ", (object[])go.GetComponents<Component>()), indent);
			Pickupable p = go.GetComponent<Pickupable>();
			if (p != null) {
				SNUtil.log("pickup: "+p.GetTechType()+" = "+p.isPickupable, indent);
			}
			TechTag tag = go.GetComponent<TechTag>();
			if (tag != null) {
				SNUtil.log("techtag: "+tag.type, indent);
			}
			ResourceTracker res = go.GetComponent<ResourceTracker>();
			if (res != null) {
				SNUtil.log("resource: "+res.name+" = "+res.techType, indent);
			}
			EntityTag e = go.GetComponent<EntityTag>();
			if (e != null) {
				SNUtil.log("entity: "+e.name+" = "+e.tag, indent);
			}
			LiveMixin live = go.GetComponent<LiveMixin>();
			if (live != null) {
				SNUtil.log("live: "+live.name+" = "+live.health+"/"+live.maxHealth+" = "+live.IsAlive(), indent);
			}
			InfectedMixin infect = go.GetComponent<InfectedMixin>();
			if (infect != null) {
				SNUtil.log("infected: "+infect.name+" = "+infect.infectedAmount, indent);
			}
			Renderer ren = go is Renderer ? (Renderer)go : go.GetComponent<Renderer>();
			if (ren != null) {
				SNUtil.log("renderer: "+ren.name, indent);
				foreach (Material m in ren.materials) {
					SNUtil.log("material: "+m.name, indent);
					SNUtil.log("color: "+m.color, indent);
					SNUtil.log("tex: "+m.mainTexture, indent);
					SNUtil.log("tex name: "+m.mainTexture.name, indent);
					SNUtil.log("tex pos: "+m.mainTextureOffset, indent);
					SNUtil.log("tex scale: "+m.mainTextureScale, indent);
					foreach (string tex in m.GetTexturePropertyNames()) {
						SNUtil.log("tex ID '"+tex+"': "+m.GetTexture(tex), indent);
						SNUtil.log("tex ID '"+tex+"': "+m.GetTextureOffset(tex), indent);
						SNUtil.log("tex ID '"+tex+"': "+m.GetTextureScale(tex), indent);
					}
				}
			}
			SNUtil.log("transform: "+go.transform, indent);
			if (go.transform != null) {
				SNUtil.log("position: "+go.transform.position, indent);
				SNUtil.log("transform object: "+go.transform.gameObject, indent);
				SNUtil.log("transform parent: "+go.transform.parent, indent);
				if (go.transform.parent != null) {
					SNUtil.log("transform parent object: ", indent);
					dumpObjectData(go.transform.parent.gameObject, indent+3);
				}
				else {
					SNUtil.log("transform parent object: null", indent);
				}
			}
		}
		
		public static void dumpObjectData(Mesh m) {
			SNUtil.log("Mesh "+m+":");
			if (m == null) {
				SNUtil.log("Mesh is null");
				return;
			}
			SNUtil.log("Mesh has "+m.subMeshCount+" submeshes");
			SNUtil.log("Mesh has "+m.vertexCount+" vertices:");
			if (m.isReadable) {
				Vector3[] verts = m.vertices;
				for (int i = 0; i < verts.Length; i++) {
					SNUtil.log("Vertex "+i+": "+verts[i].ToString("F5"));
				}
			}
			else {
				SNUtil.log("[Not readable]");
			}
		}
		
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
		
		public static GameObject removeChildObject(GameObject go, string name) {
			GameObject find = getChildObject(go, name);
			GameObject ret = null;
			while (find != null) {
				find.SetActive(false);
				UnityEngine.Object.DestroyImmediate(find);
				ret = find;
			}
			return ret;
		}
		
		public static GameObject getChildObject(GameObject go, string name) {
		 	Transform t = go.transform.Find(name);
		 	if (t != null)
		 		return t.gameObject;
		 	t = go.transform.Find(name+"(Placeholder)");
		 	if (t != null)
		 		return t.gameObject;
		 	t = go.transform.Find(name+"(Clone)");
		 	return t != null ? t.gameObject : null;
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
					SNUtil.writeToChat("Prefab found and placed succeeeded but resulted in null?!");
					return null;
				}
			}
			else {
				SNUtil.writeToChat("Prefab not found for id '"+id+"'.");
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
			if (pfb is Craftable && false) {
				world = getItemGO((Craftable)pfb, pfb.baseTemplate.getPrefabID());
				world = UnityEngine.Object.Instantiate(world);
			}
			else if (UWE.PrefabDatabase.TryGetPrefab(pfb.baseTemplate.getPrefabID(), out prefab)) {
				world = UnityEngine.Object.Instantiate(prefab);
			}
			else {
				SNUtil.writeToChat("Could not fetch template GO for "+pfb);
				return null;
			}
			if (world == null) {
				SNUtil.writeToChat("Got null for template GO for "+pfb);
				return null;
			}
			world.SetActive(false);
			convertTemplateObject(world, pfb);
			return world;
		}
		
		public static void convertTemplateObject<T>(GameObject go, DIPrefab<T> pfb, bool basicPropertiesOnly = false) where T : PrefabReference {
			ModPrefab mod = (ModPrefab)pfb;
			go.EnsureComponent<TechTag>().type = mod.TechType;
			PrefabIdentifier pi = go.EnsureComponent<PrefabIdentifier>();
			pi.ClassId = mod.ClassID;
			if (pfb.isResource()) {
				ResourceTracker res = go.EnsureComponent<ResourceTracker>();
				res.prefabIdentifier = pi;
				res.techType = mod.TechType;
				res.overrideTechType = mod.TechType;
			}
			if (basicPropertiesOnly)
				return;
			Renderer r = go.GetComponentInChildren<Renderer>(true);
			if (r != null && pfb.getTextureFolder() != null)
				RenderUtil.swapToModdedTextures(r, pfb);
			pfb.prepareGameObject(go, r);
			//writeToChat("Applying custom texes to "+world+" @ "+world.transform.position);
			go.name = pfb.GetType()+" "+mod.ClassID;
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
		
		public static Light addLight(GameObject go) {
			GameObject child = new GameObject();
			child.transform.parent = go.transform;
			child.name = "Light Entity";
			return child.AddComponent<Light>();
		}
		
		public static T copyComponent<T>(GameObject from, GameObject to) where T : Component {
			T tgt = to.EnsureComponent<T>();
			tgt.copyObject(from.GetComponent<T>());
			return tgt;
		}
		
	}
}
