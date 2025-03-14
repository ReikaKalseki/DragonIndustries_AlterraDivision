using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra {
	
	public class AoERadiationZone : MonoBehaviour {
		
		private static readonly string TRIGGER_NAME = "RadiationAoE";
		
		public float radius {get; private set; }
		public float innerRadius {get; private set; }
		public float maxIntensity;
		
		private GameObject trigger;
		
		void Start() {
			trigger = ObjectUtil.getChildObject(gameObject, TRIGGER_NAME);
			if (!trigger) {
				trigger = new GameObject(TRIGGER_NAME);
				trigger.transform.SetParent(transform);
				Utils.ZeroTransform(trigger.transform);
				trigger.EnsureComponent<AoERadiationZoneTrigger>().owner = this;
				SphereCollider sc = trigger.EnsureComponent<SphereCollider>();
				sc.radius = radius;
				sc.isTrigger = true;
			}
		}
		
		public void setRadii(float r, float inner) {
			radius = r;
			innerRadius = inner;
			if (trigger)
				trigger.EnsureComponent<SphereCollider>().radius = radius;
		}
		
		public float getScaledIntensity(float dist) {
			if (dist <= innerRadius)
				return maxIntensity;
			if (dist >= radius)
				return 0;
			return (float)MathUtil.linterpolate(dist, innerRadius, radius, maxIntensity, 0, true);
		}
		
	}
	
	class AoERadiationZoneTrigger : MonoBehaviour {
		
		internal AoERadiationZone owner;
		
		void Start() {
			if (!owner)
				owner = gameObject.FindAncestor<AoERadiationZone>();
		}

		private void OnTriggerEnter(Collider other) {
			AoERadiationTracker tracker = other.gameObject.FindAncestor<AoERadiationTracker>();
			if (tracker)
				tracker.active.Add(this.owner);
		}
	
		private void OnTriggerExit(Collider other) {
			AoERadiationTracker tracker = other.gameObject.FindAncestor<AoERadiationTracker>();
			if (tracker)
				tracker.active.Remove(this.owner);
		}
		
	}
	
	public class AoERadiationTracker : MonoBehaviour {
		
		public readonly HashSet<AoERadiationZone> active = new HashSet<AoERadiationZone>();
		
		public float getRadiationIntensity() {
			float r = 0;
			foreach (AoERadiationZone aoe in active) {
				r = Mathf.Max(r, aoe.getScaledIntensity(Vector3.Distance(transform.position, aoe.transform.position)));
			}
			//if (r > 0)
			//	SNUtil.writeToChat(r.ToString("0.00"));
			return r;
		}
		
	}
}