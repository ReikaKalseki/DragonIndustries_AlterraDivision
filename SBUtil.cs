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
			string id = Assembly.GetExecutingAssembly().GetName().Name.ToUpperInvariant().Replace("PLUGIN_", "");
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
		
		public static void dropItem(long x, long y, long z, string name) {
			if (false) {
		    	
	    	}
	    	else {
	    		log("NO SUCH ITEM TO DROP: "+name);
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
			pre.prefabPlaceholders[0].prefabClassId = CraftData.GetClassIdForTechType(item);
	    }
		
	}
}
