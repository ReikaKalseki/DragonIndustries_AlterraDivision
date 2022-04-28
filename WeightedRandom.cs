using System;

using System.Collections.Generic;

using UnityEngine;

using ReikaKalseki.DIAlterra;

//Ported from DragonAPI
namespace ReikaKalseki.DIAlterra {
	
	public sealed class WeightedRandom<V> {

		private readonly Dictionary<V, double> data = new Dictionary<V, double>();
		private double maxWeight = 0;
		private double weightSum;

		public double addEntry(V obj, double weight) {
			if (weight < 0)
				throw new Exception("You cannot have an entry with a negative weight!");
			data[obj] = weight;
			this.weightSum += weight;
			this.maxWeight = Math.Max(this.maxWeight, weight);
			return this.weightSum;
		}

		public double remove(V val) {
			if (data.ContainsKey(val)) {
				double ret = data[val];
				data.Remove(val);
				this.weightSum -= ret;
				return ret;
			}
			return 0;
		}

		public V getRandomEntry() {
			double d = UnityEngine.Random.Range(0, (float)getTotalWeight());
			double p = 0;
			foreach (V obj in data.Keys) {
				p += this.getWeight(obj);
				if (d <= p) {
					return obj;
				}
			}
			return default(V);
		}

		public V getRandomEntry(V fallback, double wt) {
			double sum = this.getTotalWeight()+wt;
			double d = UnityEngine.Random.Range(0, (float)sum);
			double p = 0;
			foreach (V obj in data.Keys) {
				p += this.getWeight(obj);
				if (d <= p) {
					return obj;
				}
			}
			return fallback;
		}

		public double getWeight(V obj) {
			return data.ContainsKey(obj) ? data[obj] : 0;
		}

		public double getMaxWeight() {
			return this.maxWeight;
		}

		public double getTotalWeight() {
			return this.weightSum;
		}

		public bool isEmpty() {
			return data.Count == 0;
		}

		public int size() {
			return data.Count;
		}

		public bool hasEntry(V obj) {
			return data.ContainsKey(obj);
		}

		public override string ToString() {
			return data.ToString();
		}

		public void clear() {
			this.data.Clear();
			this.maxWeight = 0;
			this.weightSum = 0;
		}

		public List<V> getValues() {
			return new List<V>(data.Keys);
		}

		public double getProbability(V val) {
			return this.getWeight(val)/this.getTotalWeight();
		}
	}
}
