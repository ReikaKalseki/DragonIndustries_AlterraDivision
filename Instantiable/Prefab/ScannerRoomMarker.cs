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

	public sealed class ScannerRoomMarker : Spawnable {

		private readonly System.Reflection.Assembly ownerMod;

		public readonly TechType markerType;

		public ScannerRoomMarker(TechType markAs) : base("ScannerRoomMarker_" + markAs.AsString(), "", "") {
			markerType = markAs;
			ownerMod = SNUtil.tryGetModDLL();
			SpriteHandler.RegisterSprite(markerType, TextureManager.getSprite(ownerMod, "Textures/" + markerType.AsString()));
		}

		public override GameObject GetGameObject() {
			GameObject world = new GameObject("ScannerRoomMarker(Clone)");
			world.EnsureComponent<TechTag>().type = TechType;
			PrefabIdentifier pi = world.EnsureComponent<PrefabIdentifier>();
			pi.ClassId = ClassID;
			ResourceTracker tgt = world.EnsureComponent<ResourceTracker>();
			tgt.techType = markerType;
			tgt.overrideTechType = markerType;
			tgt.prefabIdentifier = pi;
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
			return world;
		}

	}
}
