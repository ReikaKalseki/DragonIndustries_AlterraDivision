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
		
		public List<SonarRender> renderers = new List<SonarRender>();
		
		protected virtual void Update() {
			if (!sonar && Camera.main)
				sonar = Camera.main.GetComponent<SonarScreenFX>();
			float sonarDist = computeSonarDistance();
			float dT = Time.deltaTime;
			foreach (SonarRender r in renderers) {
				if (!r.renderer)
					continue;
				if (isBlobVisible(r.renderer, sonarDist))
					r.intensity = Mathf.Min(1, r.intensity+dT*r.fadeInSpeed);
				else
					r.intensity = Mathf.Max(0, r.intensity-dT*r.fadeOutSpeed);
				r.renderer.enabled = r.intensity > 0;//;
			}
		}
		
		public float computeSonarDistance() {
			return sonar && sonar.enabled ? sonar.pingDistance*350 : -1;
		}
		
		public bool isBlobVisible(Renderer r, float sonarDist, float tolerance = 1) {
			if (!sonar || sonarDist < 0)
				return false;
			float dist = Vector3.Distance(r.transform.position, sonar.transform.position);
			bool near = dist > sonarDist;
			return near ? dist-sonarDist <= 15*tolerance : sonarDist-dist <= 120*tolerance;
		}
		
		public class SonarRender {
			
			public readonly Renderer renderer;
			public float intensity = 0;
			public float fadeInSpeed = 999;
			public float fadeOutSpeed = 999;
			
			public SonarRender(Renderer r) {
				renderer = r;
			}
			
		}
		
	}
}
