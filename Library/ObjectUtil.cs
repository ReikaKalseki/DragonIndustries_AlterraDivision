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
	public static class ObjectUtil {
		
		public static bool debugMode;
	    
	    private static readonly HashSet<string> anchorPods = new HashSet<string>() {
			VanillaFlora.ANCHOR_POD_SMALL1.getPrefabID(),
			VanillaFlora.ANCHOR_POD_SMALL2.getPrefabID(),
			VanillaFlora.ANCHOR_POD_MED1.getPrefabID(),
			VanillaFlora.ANCHOR_POD_MED2.getPrefabID(),
			VanillaFlora.ANCHOR_POD_LARGE.getPrefabID(),
	    };
	    
		private static readonly HashSet<string> containmentDragonRepellents = new HashSet<string>() {
		   	"c5512e00-9959-4f57-98ae-9a9962976eaa",
		   	"542aaa41-26df-4dba-b2bc-3fa3aa84b777",
		   	"5bcaefae-2236-4082-9a44-716b0598d6ed",
		   	"20ad299d-ca52-48ef-ac29-c5ec5479e070",
		 	"430b36ae-94f3-4289-91ac-25475ad3bf74"
		};
	    
		private static readonly HashSet<string> coralTubes = new HashSet<string>() {
		   	"06562999-e575-4b02-b880-71d37616b5b9",
		   	"691723cf-d5e9-482f-b5af-8491b2a318b1",
		   	"f0295655-8f4f-4b18-b67d-925982a472d7",
		};
		
		public static bool isAnchorPod(GameObject go) {
			return isObjectInSet(go, anchorPods);
		}
		
		public static bool isCoralTube(GameObject go) {
			return isObjectInSet(go, coralTubes);
		}
		
		public static bool isDragonRepellent(GameObject go) {
			return isObjectInSet(go, containmentDragonRepellents);
		}
		
		public static bool isObjectInSet(GameObject go, HashSet<string> prefabs) {
			if (!go)
				return false;
			PrefabIdentifier pi = go.FindAncestor<PrefabIdentifier>();
			return pi && prefabs.Contains(pi.ClassId);
		}
		
		public static bool isFarmedPlant(GameObject go) {
			return go.FindAncestor<Planter>();
		}
	    
	    public static GameObject createSeamothSparkSphere(SeaMoth sm, bool active = true) {
			ElectricalDefense def = sm.seamothElectricalDefensePrefab.GetComponent<ElectricalDefense>();
			GameObject sphere = def.fxElecSpheres[0];
	    	GameObject go = Utils.SpawnZeroedAt(sphere, sm.transform, false);
	    	if (active)
	    		go.SetActive(true);
	    	return go;
	    }
		
		public static void makeMapRoomScannable(GameObject go, TechType tt, bool moving = false) {
			ResourceTracker res = go.EnsureComponent<ResourceTracker>();
			res.prefabIdentifier = go.GetComponent<PrefabIdentifier>();
			res.techType = tt;
			res.overrideTechType = tt;
			res.pickupable = go.GetComponentInChildren<Pickupable>();
			res.rb = go.GetComponentInChildren<Rigidbody>();
			if (moving)
				res.gameObject.EnsureComponent<ResourceTrackerUpdater>().tracker = res;
		}
		
		public static bool isPDA(GameObject go) {
			if (!go.GetComponent<StoryHandTarget>())
				return false;
			PrefabPlaceholdersGroup pp = go.GetComponent<PrefabPlaceholdersGroup>();
			return pp && pp.prefabPlaceholders != null && pp.prefabPlaceholders.Length > 0 && pp.prefabPlaceholders[0] && pp.prefabPlaceholders[0].prefabClassId == "4e8d9640-dd23-46ca-99f2-6924fcf250a4";
		}
		
		public static bool isBaseModule(TechType tt, bool includeFoundations) {
			switch(tt) {
				case TechType.BaseRoom:
				case TechType.BaseCorridorGlass:
				case TechType.BaseCorridor:
				case TechType.BaseCorridorI:
				case TechType.BaseCorridorL:
				case TechType.BaseCorridorT:
				case TechType.BaseCorridorX:
				case TechType.BaseCorridorGlassI:
				case TechType.BaseCorridorGlassL:
				case TechType.BaseMoonpool:
				case TechType.BaseObservatory:
				case TechType.BaseMapRoom:
					return true;
				case TechType.BaseFoundation:
					return includeFoundations;
				default:
					return false;
			}
		}
		
		public static void removeComponent(GameObject go, Type tt) {
			foreach (Component c in go.GetComponentsInChildren(tt)) {
				if (c is MonoBehaviour)
					((MonoBehaviour)c).enabled = false;
				UnityEngine.Object.DestroyImmediate(c);
			}
		}
		
		public static void removeComponent<C>(GameObject go) where C : Component {
			applyToComponents<C>(go, true, true, true);
		}
		
		public static void setActive<C>(GameObject go, bool active) where C : Component {
			applyToComponents<C>(go, false, true, active);
		}
		
		private static void applyToComponents<C>(GameObject go, bool destroy, bool setA, bool setTo) where C : Component {
			foreach (Component c in go.GetComponentsInChildren<C>(true)) {
				if (debugMode)
					SNUtil.log("Affecting component "+c+" in "+go+" @ "+go.transform.position+": "+destroy+"/"+setTo+"("+setA+")", SNUtil.diDLL);
				if (c is MonoBehaviour && setA)
					((MonoBehaviour)c).enabled = setTo;
				if (destroy)
					UnityEngine.Object.DestroyImmediate(c);
			}
		}
		
		public static void dumpObjectData(GameObject go) {
			dumpObjectData(go, 0);
		}
		
		private static void dumpObjectData(GameObject go, int indent) {
			if (!go) {
				SNUtil.log("null object");
				return;
			}
			SNUtil.log("object "+go, SNUtil.diDLL, indent);
			SNUtil.log("chain "+go.GetFullHierarchyPath(), SNUtil.diDLL, indent);
			SNUtil.log("components: "+string.Join(", ", (object[])go.GetComponents<Component>()), SNUtil.diDLL, indent);
			Pickupable p = go.GetComponent<Pickupable>();
			if (p) {
				SNUtil.log("pickup: "+p.GetTechType()+" = "+p.isPickupable, SNUtil.diDLL, indent);
			}
			TechTag tag = go.GetComponent<TechTag>();
			if (tag) {
				SNUtil.log("techtag: "+tag.type, SNUtil.diDLL, indent);
			}
			ResourceTracker res = go.GetComponent<ResourceTracker>();
			if (res) {
				SNUtil.log("resource: "+res.name+" = "+res.techType, SNUtil.diDLL, indent);
			}
			EntityTag e = go.GetComponent<EntityTag>();
			if (e) {
				SNUtil.log("entity: "+e.name+" = "+e.tag, SNUtil.diDLL, indent);
			}
			Plantable pp = go.GetComponent<Plantable>();
			if (pp) {
				SNUtil.log("plantable: "+pp.name+" = "+pp.plantTechType, SNUtil.diDLL, indent);
				SNUtil.log("plant: ", SNUtil.diDLL, indent);
				dumpObjectData(pp.growingPlant, indent+1);
			}
			LiveMixin live = go.GetComponent<LiveMixin>();
			if (live) {
				SNUtil.log("live: "+live.name+" = "+live.health+"/"+live.maxHealth+" = "+live.IsAlive(), SNUtil.diDLL, indent);
			}
			InfectedMixin infect = go.GetComponent<InfectedMixin>();
			if (infect) {
				SNUtil.log("infected: "+infect.name+" = "+infect.infectedAmount, SNUtil.diDLL, indent);
			}
			SNUtil.log("transform: "+go.transform, SNUtil.diDLL, indent);
			if (go.transform != null) {
				SNUtil.log("position: "+go.transform.position, SNUtil.diDLL, indent);
				SNUtil.log("transform object: "+go.transform.gameObject, SNUtil.diDLL, indent);
				for (int i = 0; i < go.transform.childCount; i++) {
					SNUtil.log("child object #"+i+": ", SNUtil.diDLL, indent);
					dumpObjectData(go.transform.GetChild(i).gameObject, indent+3);
				}
			}
		}
		
		public static void dumpObjectData(Component go) {
			dumpObjectData(go, 0);
		}
		
		private static void dumpObjectData(Component go, int indent) {
			if (!go) {
				SNUtil.log("null component");
				return;
			}
			SNUtil.log("component "+go, SNUtil.diDLL, indent);
			dumpObjectData(go.gameObject);
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
		
		public static int removeChildObject(GameObject go, string name) {
			GameObject find = getChildObject(go, name);
			int found = 0;
			while (find != null) {
				find.SetActive(false);
				UnityEngine.Object.DestroyImmediate(find);
				find = getChildObject(go, name);
				found++;
			}
			return found;
		}
		
		public static List<GameObject> getChildObjects(GameObject go, string name) {
			bool startWild = name[0] == '*';
			bool endWild = name[name.Length-1] == '*';
			string seek = name;
			if (startWild)
				seek = seek.Substring(1);
			if (endWild)
				seek = seek.Substring(0, seek.Length-1);
			//SNUtil.writeToChat(seek+" > "+startWild+"&"+endWild);
			List<GameObject> ret = new List<GameObject>();
			foreach (Transform t in go.transform) {
				string n = t.gameObject.name;
				bool match = false;
				if (startWild && endWild) {
					match = n.Contains(seek);
				}
				else if (startWild) {
					match = n.EndsWith(seek, StringComparison.InvariantCulture);
				}
				else if (endWild) {
					match = n.StartsWith(seek, StringComparison.InvariantCulture);
				}
				else {
					match = n == seek;
				}
				//SNUtil.writeToChat(seek+"&&"+n+" > "+match);
				if (match) {
					ret.Add(t.gameObject);
				}
			}
			return ret;
		}
		
		public static GameObject getChildObject(GameObject go, string name) {
			if (name == "*")
				return go.transform.childCount > 0 ? go.transform.GetChild(0).gameObject : null;
			bool startWild = name[0] == '*';
			bool endWild = name[name.Length-1] == '*';
			if (startWild || endWild) {
				if (debugMode)
					SNUtil.log("Looking for child wildcard match "+name+" > "+startWild+", "+endWild, SNUtil.diDLL);
				return findFirstChildMatching(go, name, startWild, endWild);
			}
			else {
			 	Transform t = go.transform.Find(name);
			 	if (t != null)
			 		return t.gameObject;
			 	t = go.transform.Find(name+"(Clone)");
			 	if (t != null)
			 		return t.gameObject;
			 	t = go.transform.Find(name+"(Placeholder)");
			 	return t != null ? t.gameObject : null;
			}
		}
		
		public static GameObject findFirstChildMatching(GameObject go, string s0, bool startWild, bool endWild) {
			string s = s0;
			if (startWild)
				s = s.Substring(1);
			if (endWild)
				s = s.Substring(0, s.Length-1);
			foreach (Transform t in go.transform) {
				string name = t.gameObject.name;
				bool match = false;
				if (startWild && endWild) {
					match = name.Contains(s);
				}
				else if (startWild) {
					match = name.EndsWith(s, StringComparison.InvariantCulture);
				}
				else if (endWild) {
					match = name.StartsWith(s, StringComparison.InvariantCulture);
				}
				if (match) {
					return t.gameObject;
				}
				else {
					if (debugMode)
						SNUtil.log("Found no match for "+s0+" against "+t.gameObject.GetFullHierarchyPath(), SNUtil.diDLL);
					GameObject inner = findFirstChildMatching(t.gameObject, s0, startWild, endWild);
					if (inner)
						return inner;
				}
			}
			return null;
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
		
		public static GameObject createWorldObject(TechType tt, bool clone = true, bool makeActive = true) {
			return createWorldObject(CraftData.GetClassIdForTechType(tt), clone, makeActive);
		}
			
		public static GameObject createWorldObject(string id, bool clone = true, bool makeActive = true) {
			GameObject prefab = lookupPrefab(id);
			if (prefab) {
				GameObject go = clone ? UnityEngine.Object.Instantiate(prefab) : prefab;
				if (go) {
					go.SetActive(makeActive);
					return go;
				}
				else {
					SNUtil.writeToChat("Prefab found and placed but resulted in null?!");
					return null;
				}
			}
			else {
				SNUtil.writeToChat("Prefab not found for id '"+id+"' ["+PrefabData.getPrefab(id)+"].");
				return null;
			}
		}
		
		public static GameObject getItem(TechType tt) {
			TechType seek = tt;
			string sg = tt.AsString(false);
			if (sg.EndsWith("EggUndiscovered", StringComparison.InvariantCultureIgnoreCase)) {
				seek = (TechType)Enum.Parse(typeof(TechType), sg.Replace("EggUndiscovered", "Egg"));
			}
			switch(tt) { //special handling if any
				default:
					break;
			}
			GameObject pfb = UnityEngine.Object.Instantiate(lookupPrefab(seek));
			pfb.SetActive(false);
			if (seek != tt) {
				Pickupable pp = pfb.GetComponentInChildren<Pickupable>();
				if (pp)
					pp.SetTechTypeOverride(tt);
			}
			return pfb;
		}
		
		public static GameObject lookupPrefab(TechType tt) {
			/*
			string id = CraftData.GetClassIdForTechType(tt);
			return string.IsNullOrEmpty(id) ? null : lookupPrefab(id);*/
			return CraftData.GetPrefabForTechType(tt);
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
				res = (int)Enum.Parse(typeof(Base.Piece), n);
				piece = Base.pieces[res];
			}
			GameObject ret = piece != null && piece.HasValue ? getBasePiece(piece.Value, clone) : null;
			if (ret && clone && res == (int)Base.Piece.RoomWaterParkHatch) {
				foreach (Transform t in ret.transform) {
					if (t && t.name == "BaseCorridorHatch(Clone)")
						UnityEngine.Object.DestroyImmediate(t.gameObject);
				}
			}
			return ret;
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
			else {
				world = createWorldObject(pfb.baseTemplate.getPrefabID(), true, false);
			}
			if (!world) {
				SNUtil.writeToChat("Could not fetch template GO for "+pfb);
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
			Renderer[] r = go.GetComponentsInChildren<Renderer>(true);
			if (r != null && r.Length > 0 && pfb.getTextureFolder() != null)
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
			GameObject world = ObjectUtil.createWorldObject(tech, true, false);
			world.transform.position = go.transform.position;
			world.transform.rotation = go.transform.rotation;
			world.transform.localScale = go.transform.localScale;
			UnityEngine.Object.Destroy(go);
		}
		
		public static Light addLight(GameObject go) {
			GameObject child = new GameObject();
			child.transform.parent = go.transform;
			child.transform.localPosition = Vector3.zero;
			child.name = "Light Entity";
			return child.AddComponent<Light>();
		}
		
		public static T copyComponent<T>(GameObject from, GameObject to) where T : Component {
			T tgt = to.EnsureComponent<T>();
			tgt.copyObject(from.GetComponent<T>());
			return tgt;
		}
		
		public static void ignoreCollisions(GameObject from, params GameObject[] with) {
			foreach (GameObject go in with) {
				foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) {
					foreach (Collider c0 in from.GetComponentsInChildren<Collider>(true)) {
						Physics.IgnoreCollision(c0, c);
					}
				}
			}
		}
		
		public static void setSky(GameObject go, mset.Sky sky) {
			if (!go)
				return;
			go.EnsureComponent<SkyApplier>();
			foreach (SkyApplier sk in go.GetComponentsInChildren<SkyApplier>(true)) {
				if (!sk)
					continue;
				if (sk.renderers == null)
					sk.renderers = go.GetComponentsInChildren<Renderer>();
				sk.environmentSky = sky;
				sk.applySky = sk.environmentSky;
				sk.enabled = true;
				sk.ApplySkybox();
				sk.RefreshDirtySky();
			}
		}
		
		public static void fullyEnable(GameObject go) {
			go.SetActive(true);
			foreach (Behaviour mb in go.GetComponentsInChildren<Behaviour>(true)) {
				mb.enabled = true;
				mb.gameObject.SetActive(true);
			}
		}
		
		public static void addCyclopsHologramWarning(Component sub, GameObject go, Sprite spr = null) {
			CyclopsHolographicHUD hud = sub.GetComponentInChildren<CyclopsHolographicHUD>();
			if (hud) {
				hud.AttachedLavaLarva(go);
				if (spr != null) {
					foreach (CyclopsHolographicHUD.LavaLarvaIcon ping in hud.lavaLarvaIcons) {
						if (ping.refGo.Equals(go)) {
							ping.creatureIcon.GetComponentInChildren<UnityEngine.UI.Image>().sprite = spr;
						}
					}
				}
			}
		}
		
		public static bool isOnScreen(GameObject go, Camera c) {
		      Plane[] planes = GeometryUtility.CalculateFrustumPlanes(c);
		      if (GeometryUtility.TestPlanesAABB(planes, new Bounds(go.transform.position, Vector3.one*0.25F)))
		          return true;
		      else
		          return false;
		}
		
		public static bool isLookingAt(Transform looker, Vector3 pos, float maxAng) {
			return Vector3.Angle(looker.forward, pos-looker.transform.position) <= maxAng;
		}
		
		public static bool isRoom(GameObject go, bool allowTunnelConnections) {
			return isPieceType(go, allowTunnelConnections, "BaseRoom");
		}
		
		public static bool isMoonpool(GameObject go, bool allowTunnelConnections, bool allowRoof) {
			if (!allowRoof) {
				BaseExplicitFace face = go.GetComponentInParent<BaseExplicitFace>();
				return face && face.gameObject.name.StartsWith("BaseMoonpoolCoverSide", StringComparison.InvariantCultureIgnoreCase);
			}
			return isPieceType(go, allowTunnelConnections, "BaseMoonpool");
		}
		
		private static bool isPieceType(GameObject go, bool allowTunnelConnections, string type) {
			if (!allowTunnelConnections) {
				GameObject g2 = go;
				while (g2.transform.parent && !g2.name.StartsWith("Base", StringComparison.InvariantCultureIgnoreCase))
					g2 = g2.transform.parent.gameObject;
				if (g2.name.Contains("Corridor") || g2.name.Contains("Hatch"))
					return false;
			}
			BaseCell bc = go.FindAncestor<BaseCell>();
			return bc && getChildObject(bc.gameObject, type);
		}
		
		public static bool isInWater(GameObject go) {
			return go.transform.position.y <= Ocean.main.GetOceanLevel() && isLoose(go) && !WorldUtil.isPrecursorBiome(go.transform.position);
		}
		
		public static bool isLoose(GameObject go) {
			Transform t = go.transform.parent;
			return !t || t.name == "SerializerEmptyGameObject" || t.name == "CellRoot(Clone)";
		}
		
		public static bool isLODRenderer(Renderer r) {
			return !r.name.Contains("LOD1") && !r.name.Contains("LOD2") && !r.name.Contains("LOD3");
		}
		
		public static Renderer[] getNonLODRenderers(GameObject go) {
			return go.GetComponentsInChildren<Renderer>().Where(r => !isLODRenderer(r)).ToArray();
		}
		
		public static bool isPlayer(Component c, bool allowChildren = false) {
			return allowChildren ? (bool)c.gameObject.FindAncestor<Player>() : c.gameObject == Player.main.gameObject;
		}
		
		public static bool isPlayerOrCreature(Component c, bool allowChildren = false) {
			return isPlayer(c, allowChildren) || (allowChildren ? (bool)c.gameObject.FindAncestor<Creature>() : (bool)c.gameObject.GetComponent<Creature>());
		}/*
		
		public static GameObject getRootObjectFromCollider(Component c) {
			ColliderPrefabLink cp = c.GetComponent<ColliderPrefabLink>();
			if (cp)
				return cp.root;
			PrefabIdentifier pi = c.gameObject.FindAncestor<PrefabIdentifier>();
			return pi.gameObject;
		}
		
		public static C getRootComponentFromCollider<C>(Component c) where C : Component {
			ColliderPrefabLink cp = c.GetComponent<ColliderPrefabLink>();
			return cp && cp.root ? cp.root.GetComponent<C>() : c.gameObject.FindAncestor<C>();
		}*/
		
		public static BaseCell getBaseRoom(BaseRoot bb, GameObject go) {
			BaseCell par = go.transform.parent.GetComponent<BaseCell>();
			if (par)
				return par;
			return getBaseRoom(bb, go.transform.position);
		}
		
		public static BaseCell getBaseRoom(BaseRoot bb, Vector3 pos) {
			foreach (BaseCell bc in bb.GetComponentsInChildren<BaseCell>()) {
				GameObject room = ObjectUtil.getChildObject(bc.gameObject, "BaseRoom");
				if (!room)
					continue;
				//Bounds box = new Bounds(room.transform.position, new Vector3(4.5F, 1.5F, 4.5F));
				if (MathUtil.isPointInCylinder(room.transform.position, pos, 4.75, 1.75)) {
				//if (box.Contains()) {
					return bc;
				}
			}
			return null;
		}
		
		public static List<PrefabIdentifier> getBaseObjectsInRoom(BaseRoot bb, BaseCell room) { //automatically skips contents of inventories
			List<PrefabIdentifier> li = new List<PrefabIdentifier>();
			getBaseObjects(bb, pi => {
				if (getBaseRoom(bb, pi.gameObject) == room)
					li.Add(pi);
				}
			);
			return li;
		}
		
		public static void getBaseObjects(BaseRoot bb, Action<PrefabIdentifier> acceptor) { //automatically skips contents of inventories
			iterateChildPrefabs(bb.transform, acceptor);
		}
		
		private static void iterateChildPrefabs(Transform from, Action<PrefabIdentifier> acceptor) { //do not recurse into PIs inside other PIs (eg invs)
			foreach (Transform t in from.transform) {
				PrefabIdentifier at = t.GetComponent<PrefabIdentifier>();
				if (at)
					acceptor(at);
				else
					iterateChildPrefabs(t, acceptor);
			}
		}
		
		public static List<PrefabIdentifier> getBaseObjects(BaseRoot bb) {
			List<PrefabIdentifier> li = new List<PrefabIdentifier>();
			getBaseObjects(bb, li.Add);
			return li;
		}
		
		public static bool isOnBase(BaseRoot bb, Component c) {
			Transform baseObj = bb.transform;
			Transform t = c.transform;
			while (t != null) {
				if (t == baseObj)
					return true;
				t = t.parent;
			}
			return false;
		}
		
	}
}
