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
	
	public sealed class CompoundResource : Spawnable {
		
		public readonly PrefabReference containedItem;
		
		public CompoundResource(PrefabReference item, int amount, Vector3 scatter) : base("Compound_"+item.getPrefabID(), "", "") {
			containedItem = item;
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = new GameObject();
			world.EnsureComponent<CompoundResourceTag>();
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
			return world;
	    }
	
		class CompoundResourceTag : MonoBehaviour {
			
			void Start() {
				
			}
			
		}
			
	}
}
