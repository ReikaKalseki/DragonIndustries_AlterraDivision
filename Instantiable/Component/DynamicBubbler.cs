using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {

	public class DynamicBubbler : MonoBehaviour {

		public enum Shape {
			BOX,
			SPHERE,
			UPPERHEMI,
		}

		private int bubbleCount;
		private readonly List<ParticleSystem> bubbles = new List<ParticleSystem>();

		public Vector3 scatter = Vector3.one*0.05F;
		public Shape shape = Shape.BOX;

		public float currentIntensity = 0;

		public DynamicBubbler setBubbleCount(int amt) {
			if (amt != bubbleCount) {
				foreach (ParticleSystem p in bubbles)
					p.gameObject.destroy();
				bubbles.Clear();
				bubbleCount = amt;
			}
			return this;
		}

		void Update() {
			while (bubbles.Count < bubbleCount) {
				GameObject go = ObjectUtil.createWorldObject("0dbd3431-62cc-4dd2-82d5-7d60c71a9edf");
				go.transform.SetParent(transform);
				if (shape == Shape.SPHERE || shape == Shape.UPPERHEMI)
					go.transform.localPosition = Vector3.Scale(UnityEngine.Random.insideUnitSphere, scatter);
				else
					go.transform.localPosition = MathUtil.getRandomVectorAround(Vector3.zero, scatter);
				if (shape == Shape.UPPERHEMI)
					go.transform.localPosition = go.transform.localPosition.setY(Mathf.Abs(go.transform.localPosition.y));
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