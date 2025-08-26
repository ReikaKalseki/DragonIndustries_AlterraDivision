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

	public sealed class CompoundResource : Spawnable {

		public readonly PrefabReference containedItem;

		public CompoundResource(PrefabReference item, int amount, Vector3 scatter) : base("Compound_" + item.getPrefabID(), "", "") {
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
