using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
					go.gameObject.destroy(false);
				}
			}

		}

	}
}
