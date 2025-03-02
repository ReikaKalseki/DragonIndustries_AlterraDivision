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
	
	public sealed class ScannerRoomMarker : Spawnable {
		
		private readonly System.Reflection.Assembly ownerMod;
		
		public readonly TechType markerType;
	        
		public ScannerRoomMarker(TechType markAs) : base("ScannerRoomMarker_"+markAs.AsString(), "", "") {
			markerType = markAs;
			ownerMod = SNUtil.tryGetModDLL();
			SpriteHandler.RegisterSprite(markerType, TextureManager.getSprite(ownerMod, "Textures/"+markerType.AsString()));
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
