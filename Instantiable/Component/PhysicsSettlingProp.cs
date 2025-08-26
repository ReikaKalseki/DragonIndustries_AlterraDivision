using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {

	public class PhysicsSettlingProp : MonoBehaviour {

		public static readonly Dictionary<string, List<PositionedPrefab>> locations = new Dictionary<string, List<PositionedPrefab>>();

		public PositionedPrefab prefabMarker { get; private set; }

		public int freeTime { get; private set; }
		public string key { get; private set; }

		public Rigidbody body { get; private set; }

		public Predicate<PhysicsSettlingProp> destroyCondition;

		private float time;

		public static void export(string key) {
			if (!locations.ContainsKey(key)) {
				SNUtil.writeToChat("No physprops with key '" + key + "'");
				return;
			}
			List<PositionedPrefab> li = locations[key];
			string file = BuildingHandler.instance.dumpPrefabs(key, li);
			SNUtil.writeToChat("Exported " + li.Count + " physprops of key '" + key + "' to " + file);
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

		void Start() {
			body = this.GetComponentInChildren<Rigidbody>();
		}

		public void Update() {
			if (!body)
				body = this.GetComponentInChildren<Rigidbody>();
			time += Time.deltaTime;
			this.onUpdate();
			if (destroyCondition != null && destroyCondition.Invoke(this))
				gameObject.destroy(false);
			else if (time > 15F && body.velocity.magnitude < 0.05 && body.angularVelocity.magnitude < 0.05 && !this.GetComponent<PropulseCannonAmmoHandler>())
				this.fixInPlace();
		}

		protected virtual void onUpdate() {

		}

		public void init(string key, int duration) {
			freeTime = duration;
			this.key = key;
			this.Start();
			this.Invoke("fixInPlace", freeTime);
		}

		public void fixInPlace() {
			if (body.isKinematic)
				return;
			body.isKinematic = true;
			prefabMarker = new PositionedPrefab(this.GetComponent<PrefabIdentifier>());
			addPrefab(key, prefabMarker);
			//SNUtil.log("Locked "+prefabMarker+" at time "+time);
			//this.destroy(false);
		}

		public void unlock() {
			if (!body.isKinematic)
				return;
			removePrefab(key, prefabMarker);
			prefabMarker = null;
			time = 0;
			body.isKinematic = false;
			this.Invoke("fixInPlace", freeTime);
		}

		public void bump(float vel) {
			this.bump(UnityEngine.Random.onUnitSphere * vel);
		}

		public void bump(Vector3 vec) {
			this.unlock();
			body.velocity = vec;
		}

		void OnDestroy() {
			removePrefab(key, prefabMarker);
		}

	}
}
