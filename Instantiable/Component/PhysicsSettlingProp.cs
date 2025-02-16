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

namespace ReikaKalseki.DIAlterra {
	
	public class PhysicsSettlingProp : MonoBehaviour {
		
		public static readonly Dictionary<string, List<PositionedPrefab>> locations = new Dictionary<string, List<PositionedPrefab>>();
		
		public PositionedPrefab prefabMarker {get; private set;}
		
		public int freeTime {get; private set;}
		public string key {get; private set;}
		
		public Rigidbody body {get; private set;}
		
		private float time;
		
		public static void export(string key) {
			if (!locations.ContainsKey(key)) {
				SNUtil.writeToChat("No physprops with key '"+key+"'");
				return;
			}
			List<PositionedPrefab> li = locations[key];
			string file = BuildingHandler.instance.dumpPrefabs(key, li);
			SNUtil.writeToChat("Exported "+li.Count+" physprops of key '"+key+"' to "+file);
		}
		
		private static void addPrefab(string key, PositionedPrefab pfb) {
			if (!locations.ContainsKey(key))
				locations[key] = new List<PositionedPrefab>();
			if (!locations[key].Contains(pfb))
				locations[key].Add(pfb);
		}
		
		private static void removePrefab(string key, PositionedPrefab pfb) {
			if (!locations.ContainsKey(key))
				return;
			locations[key].Remove(pfb);
		}
		
		public void Update() {
			if (!body)
				body = GetComponentInChildren<Rigidbody>();
			time += Time.deltaTime;			
			onUpdate();				
			if (time > 15F && body.velocity.magnitude < 0.05 && body.angularVelocity.magnitude < 0.05 && !GetComponent<PropulseCannonAmmoHandler>())
				fixInPlace();
		}
		
		protected virtual void onUpdate() {
			
		}
		
		public void init(string key, int duration) {
			freeTime = duration;
			this.key = key;
			Invoke("fixInPlace", freeTime);
		}
		
		public void fixInPlace() {
			if (body.isKinematic)
				return;
			body.isKinematic = true;
			prefabMarker = new PositionedPrefab(GetComponent<PrefabIdentifier>());
			addPrefab(key, prefabMarker);
			SNUtil.log("Locked "+prefabMarker+" at time "+time);
			//UnityEngine.Object.Destroy(this);
		}
		
		public void unlock() {
			if (!body.isKinematic)
				return;
			removePrefab(key, prefabMarker);
			prefabMarker = null;
			time = 0;
			body.isKinematic = false;
			Invoke("fixInPlace", freeTime);
		}
		
		public void delete() {
			removePrefab(key, prefabMarker);
			UnityEngine.Object.Destroy(gameObject);
		}
		
	}
}
