using System;

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

namespace ReikaKalseki.DIAlterra
{
	public class PositionedPrefab
	{
		[SerializeField]
		internal string prefabName;
		[SerializeField]
		internal Vector3 position;
		[SerializeField]
		internal Quaternion rotation;
		
		public PositionedPrefab(string pfb, Vector3? pos = null, Quaternion? rot = null)
		{
			prefabName = pfb;
			position = GenUtil.getOrZero(pos);
			rotation = GenUtil.getOrIdentity(rot);
		}
		
		public string getPrefab() {
			return prefabName;
		}
		
		public Vector3 getPosition() {
			return new Vector3(position.x, position.y, position.z);
		}
		
		public Quaternion getRotation() {
			return new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
		}
	}
}
