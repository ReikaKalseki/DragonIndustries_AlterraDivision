using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using JetBrains.Annotations;

using ReikaKalseki.DIAlterra;

using UnityEngine;

//Ported from DragonAPI
namespace ReikaKalseki.DIAlterra {

	public sealed class CountMap<V> {

		private readonly Dictionary<V, int> data = new Dictionary<V, int>();

		public int this[V obj] { get { return getCount(obj); } }

		public void add(V obj, int amt = 1) {
			data[obj] = data.ContainsKey(obj) ? data[obj] + amt : amt;
		}

		public void remove(V val) {
			if (data.ContainsKey(val))
				data.Remove(val);
		}

		public int getCount(V obj) {
			return data.ContainsKey(obj) ? data[obj] : 0;
		}

		public IEnumerable<V> getItems() {
			return data.Keys;
		}

		public bool isEmpty() {
			return this.size() == 0;
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

		public void clear() {
			data.Clear();
		}

	}
}
