﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.DIAlterra {
	
	public class LockedPrecursorDoor : Spawnable {
		
		public readonly string id;
		
		public readonly PrecursorKeyTerminal.PrecursorKeyType key;		
		public readonly PositionedPrefab barrierLocation;
		public readonly PositionedPrefab keyTerminalLocation;
		
		internal static readonly Dictionary<string, LockedPrecursorDoor> prefabs = new Dictionary<string, LockedPrecursorDoor>();
	        
		public LockedPrecursorDoor(string id, PrecursorKeyTerminal.PrecursorKeyType key, PositionedPrefab barrier, PositionedPrefab terminal)
			: base("LockedPrecursorDoor_" + id, "", "") {
			this.id = id;
			this.key = key;
			
			barrierLocation = barrier;
			keyTerminalLocation = terminal;
			
			OnFinishedPatching += () => {
				prefabs[ClassID] = this;
			};
		}
			
		public override GameObject GetGameObject() {
			GameObject go = new GameObject("LockedPrecursorDoor_" + id + "(Clone)");
			//SNUtil.log("Spawning LockedPrecursorDoor_"+id+" @ "+go.transform.position);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrecursorGlobalKeyActivator>().doorActivationKey = id;
			go.EnsureComponent<LockedPrecursorDoorTag>();
			go.EnsureComponent<ConstructionObstacle>();
			return go;
		}

		protected override void ProcessPrefab(GameObject go) {
			base.ProcessPrefab(go);
		}
		
		class LockedPrecursorDoorTag : MonoBehaviour {
			
			private PrecursorKeyTerminal terminal;
			private PrecursorDoorway barrier;
			private LockedPrecursorDoor template;
			
			private ChangePrecursorDoor doorColor;
			
			void Update() {
				if (template == null)
					template = LockedPrecursorDoor.prefabs[GetComponent<PrefabIdentifier>().ClassId];
				if (template == null)
					return;
				if (doorColor == null)
					doorColor = new ChangePrecursorDoor(template.key);
				if (!terminal)
					terminal = GetComponentInChildren<PrecursorKeyTerminal>();
				if (!barrier)
					barrier = GetComponentInChildren<PrecursorDoorway>();
				if (!terminal) {
					terminal = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab("c718547d-fe06-4247-86d0-efd1e3747af0")).GetComponent<PrecursorKeyTerminal>();
					terminal.transform.SetParent(transform);
					doorColor.applyToObject(terminal);
				}
				if (!barrier) {
					barrier = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab("d26276ab-0c29-4642-bcb8-1a5f8ee42cb2")).GetComponent<PrecursorDoorway>();
					barrier.transform.SetParent(transform);
				}
				if (!terminal || !barrier)
					return;
				terminal.transform.rotation = template.keyTerminalLocation.rotation;
				terminal.transform.position = template.keyTerminalLocation.position;
				doorColor.applyToObject(terminal);
				barrier.transform.rotation = template.barrierLocation.rotation;
				barrier.transform.position = template.barrierLocation.position;
				barrier.transform.localScale = template.barrierLocation.scale;
			}
			
		}
			
	}
}
