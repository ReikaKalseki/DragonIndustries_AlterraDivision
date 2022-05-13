using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using UnityEngine;

using ReikaKalseki.DIAlterra;

//Ported from DragonAPI
namespace ReikaKalseki.DIAlterra {
	
	public sealed class WeightedRandom<V> {
	
		private readonly Dictionary<V, double> data = new Dictionary<V, double>();
		private double maxWeight = 0;
		private double weightSum;
		private bool isDynamic = false;
	
		public double addEntry(V obj, double weight) {
			if (weight < 0)
				throw new Exception("You cannot have an entry with a negative weight!");
			data[obj] = weight;
			this.weightSum += weight;
			this.maxWeight = Math.Max(this.maxWeight, weight);
			this.isDynamic |= obj is DynamicWeight;
			return this.weightSum;
		}
	
		public double addDynamicEntry(DynamicWeight wt) {
			return this.addEntry((V)wt, wt.getWeight());
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
	
		public V getRandomEntry(V fallback, double wt = 0) {
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
			if (obj is DynamicWeight)
				return ((DynamicWeight)obj).getWeight();
			return data.ContainsKey(obj) ? data[obj] : 0;
		}
	
		public double getMaxWeight() {
			if (this.isDynamic) {
				double max = 0;
				foreach (V obj in this.data.Keys) {
					double wt = this.getWeight(obj);
					max = Math.Max(max, wt);
				}
				return max;
			}
			return this.maxWeight;
		}
	
		public double getTotalWeight() {
			if (this.isDynamic) {
				double sum = 0;
				foreach (V obj in this.data.Keys) {
					double wt = this.getWeight(obj);
					sum += wt;
				}
				return sum;
			}
			return this.weightSum;
		}
	
		public bool isEmpty() {
			return size() == 0;
		}
	
		public int size() {
			return data.Count;
		}
	
		public bool hasEntry(V obj) {
			return data.ContainsKey(obj);
		}
	
		public string toString() {
			return data.ToString();
		}
	
		public void setSeed(long seed) {
			UnityEngine.Random.InitState((int)(seed) ^ ((int)(seed >> 32)));
		}
	
		public void clear() {
			this.data.Clear();
			this.maxWeight = 0;
			this.weightSum = 0;
		}
	
		public ICollection<V> getValues() {
			return new ReadOnlyCollection<V>(data.Keys.ToList()); //TODO remove redundant tolist
		}
	
		public double getProbability(V val) {
			return this.getWeight(val)/this.getTotalWeight();
		}
	
	}
	
	public interface DynamicWeight {
	
		double getWeight();
		
	}
}
