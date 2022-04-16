using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class SBUtil {
		
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
		
	}
}
