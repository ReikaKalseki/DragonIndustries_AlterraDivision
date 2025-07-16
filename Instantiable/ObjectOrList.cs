using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {

	/// <summary>
	/// Supply 0-N args, where 0 is interpreted as null, 1 as a single, and 2+ as a collection
	/// </summary>
	public sealed class ObjectOrList<E> {
		
		public readonly bool isCollection;
		private readonly List<E> objects = new List<E>();
	
		public ObjectOrList(params E[] obj) {
			objects.AddRange(obj.AsEnumerable());
			isCollection = objects.Count > 1;
		}
		
		public E value {
			get {
				if (isCollection)
					throw new NotImplementedException("Object is a collection!");
				return objects.Count == 0 ? default(E) : objects[0];
			}
		}
		
		public IReadOnlyCollection<E> values {
			get {
				if (isCollection)
					return objects.AsReadOnly();
				else
					throw new NotImplementedException("Object is not a collection!");
			}
		}
	
		public override string ToString() {
			if (isCollection)
				return objects.toDebugString();
			E obj = value;
			return obj == null ? "null" : obj.ToString();
		}
	
	}
}
