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
	
	public class DynamicBubbler : MonoBehaviour {
		
		private int bubbleCount;
		private readonly List<ParticleSystem> bubbles = new List<ParticleSystem>();
		
		public Vector3 scatter = Vector3.one*0.05F;
		
		public float currentIntensity = 0;
		
		public DynamicBubbler setBubbleCount(int amt) {
			if (amt != bubbleCount) {
				foreach (ParticleSystem p in bubbles)
					UnityEngine.Object.DestroyImmediate(p.gameObject);
				bubbles.Clear();
				bubbleCount = amt;
			}
			return this;
		}
		
		void Update() {
			while (bubbles.Count < bubbleCount) {
				GameObject go = ObjectUtil.createWorldObject("0dbd3431-62cc-4dd2-82d5-7d60c71a9edf");
				go.transform.SetParent(transform);
				go.transform.localPosition = MathUtil.getRandomVectorAround(Vector3.zero, scatter);
				go.transform.rotation = Quaternion.Euler(270, 0, 0); //not local - force to always be up
				ParticleSystem ps = go.GetComponent<ParticleSystem>();
				ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				go.SetActive(true);
				bubbles.Add(ps);
			}
			
			int bubN = Mathf.CeilToInt(bubbles.Count*currentIntensity);
			for (int i = 0; i < bubbles.Count; i++) {
				if (i < bubN)
					bubbles[i].Play();
				else
					bubbles[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
				bubbles[i].transform.rotation = Quaternion.Euler(270, 0, 0); //not local - force to always be up
			}
		}
		
		public void clear() {
			foreach (ParticleSystem ps in bubbles) {
				ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		
	}
}