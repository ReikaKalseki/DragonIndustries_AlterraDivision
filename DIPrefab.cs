using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{		
	public interface DIPrefab<E, T> : DIPrefab<T> where T : PrefabReference { //ONLY EVER IMPLEMENT THIS ON MODPREFABS OR SUBCLASSES THEREOF
			
		E addIngredient(TechType item, int amt);
			
		E addIngredient(ModPrefab item, int amt);
			
		E addIngredient(ItemDef item, int amt);
		
	}
	
	public interface DIPrefab<T> where T : PrefabReference {
		
		float glowIntensity {get; set;}
		
		T baseTemplate {get; set;}
		
		bool isResource();
		
		string getTextureFolder();
		
		void prepareGameObject(GameObject go, Renderer r);
		
	}
	
	public sealed class StringPrefabContainer : PrefabReference {
		
		public readonly string prefab;
		
		public StringPrefabContainer(string s) {
			prefab = s;
		}
		
		public string getPrefabID() {
			return prefab;
		}
		
	}
}
