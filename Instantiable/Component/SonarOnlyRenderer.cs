using System;
using System.IO;
using System.Xml;
using System.Linq;
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
using ECCLibrary;

namespace ReikaKalseki.DIAlterra {
		
	public class SonarOnlyRenderer : MonoBehaviour {
		
		private SonarScreenFX sonar;
		
		public List<Renderer> renderers = new List<Renderer>();
		
		protected virtual void Update() {
			if (!sonar && Camera.main)
				sonar = Camera.main.GetComponent<SonarScreenFX>();
			float sonarDist = computeSonarDistance();
			foreach (Renderer r in renderers) {
				r.enabled = isBlobVisible(r, sonarDist);
			}
		}
		
		public float computeSonarDistance() {
			return sonar && sonar.enabled ? sonar.pingDistance*350 : -1;
		}
		
		public bool isBlobVisible(Renderer r, float sonarDist, float tolerance = 1) {
			if (sonarDist < 0)
				return false;
			float dist = Vector3.Distance(r.transform.position, sonar.transform.position);
			bool near = dist > sonarDist;
			return near ? dist-sonarDist <= 15*tolerance : sonarDist-dist <= 120*tolerance;
		}
		
	}
}
