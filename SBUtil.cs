using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;

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
			component.type = item.TechType;
			component2.ClassId = item.ClassID;
			return gameObject;
		}
    
	    public static void setCrateItem(SupplyCrate c, TechType item) {
			PrefabPlaceholdersGroup pre = c.gameObject.GetComponentInParent<PrefabPlaceholdersGroup>();
			pre.prefabPlaceholders[0].prefabClassId = CraftData.GetClassIdForTechType(item);
	    }
		
		public static CraftNode getRootNode(CraftTree.Type type) {
			switch(type) {
				case CraftTree.Type.Fabricator:
					return CraftTree.FabricatorScheme();
				case CraftTree.Type.Constructor:
					return CraftTree.ConstructorScheme();
				case CraftTree.Type.Workbench:
					return CraftTree.WorkbenchScheme();
				case CraftTree.Type.SeamothUpgrades:
					return CraftTree.SeamothUpgradesScheme();
				case CraftTree.Type.MapRoom:
					return CraftTree.MapRoomSheme();
				case CraftTree.Type.Centrifuge:
					return CraftTree.CentrifugeScheme();
				case CraftTree.Type.CyclopsFabricator:
					return CraftTree.CyclopsFabricatorScheme();
				case CraftTree.Type.Rocket:
					return CraftTree.RocketScheme();
			}
			return null;
		}
		
		public static void dumpCraftTree(CraftTree.Type type) {
			log("Tree "+type+":");
			CraftNode root = getRootNode(type);
			dumpCraftTreeFromNode(root);
		}
		
		public static void dumpCraftTreeFromNode(CraftNode root) {
			dumpCraftTreeFromNode(root, new List<string>());
		}
		
		private static void dumpCraftTreeFromNode(CraftNode root, List<string> prefix) {
			if (root == null) {
				log(string.Join("/", prefix)+" -> null @ root");
				return;
			}
			List<TreeNode> nodes = root.nodes;
			for (int i = 0; i < nodes.Count; i++) {
				TreeNode node = nodes[i];
				if (node == null) {
					log(string.Join("/", prefix)+" -> null @ "+i);
				}
				else {
					try {
						log(string.Join("/", prefix)+" -> Node #"+i+": "+node.id);
						prefix.Add(node.id);
						dumpCraftTreeFromNode((CraftNode)node, prefix);
						prefix.RemoveAt(prefix.Count-1);
					}
					catch (Exception e) {
						log(e.ToString());
					}
				}
			}
		}
		
	}
}
