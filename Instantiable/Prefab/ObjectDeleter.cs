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
	
	public sealed class ObjectDeleter : Spawnable {
		
		private readonly System.Reflection.Assembly ownerMod;
	        
		public ObjectDeleter() : base("ObjectDeleter", "", "") {
			ownerMod = SNUtil.tryGetModDLL(true);
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = new GameObject();
			world.EnsureComponent<ObjectDeleterTag>();
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
			return world;
	    }
	
		class ObjectDeleterTag : MonoBehaviour {
			
			void Start() {
				foreach (PrefabIdentifier go in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(transform.position, transform.localScale.x)) {
					//if (go != this) //delete self too
					UnityEngine.Object.Destroy(go.gameObject);
				}
			}
			
		}
			
	}
}
