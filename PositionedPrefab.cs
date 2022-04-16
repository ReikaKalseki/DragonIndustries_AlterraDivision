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
		[SerializeField]
		internal Vector3 scale = Vector3.one;
		
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
		
		public Vector3 getScale() {
			return new Vector3(scale.x, scale.y, scale.z);
		}
			
		internal virtual XmlElement asXML(XmlDocument doc) {
			XmlElement n = doc.CreateElement("object");
			n.addProperty("prefab", prefabName);
			n.addProperty("position", position);
			XmlElement rot = n.addProperty("rotation", rotation.eulerAngles);
			rot.addProperty("quaternion", rotation);
			n.addProperty("scale", scale);
			return n;
		}
			
		public override string ToString() {
			return prefabName+" @ "+position+" / "+rotation.eulerAngles;
		}
		
		public static PositionedPrefab fromXML(XmlElement e) {
			string pfb = e.getProperty("prefab");
			if (pfb != null) {
				XmlElement elem;
				Vector3 pos = e.getVector("position");
				Vector3 rot = e.getVector("rotation", out elem);
				Quaternion? quat = elem.getQuaternion("quaternion", true);
				if (quat == null || !quat.HasValue) {
					quat = Quaternion.Euler(rot.x, rot.y, rot.y);
				}
				PositionedPrefab ret = new PositionedPrefab(pfb, pos, quat);
				List<XmlElement> li = e.getDirectElementsByTagName("scale");
				if (li.Count == 1) {
					Vector3 sc = e.getVector("scale");
					ret.scale = sc;
				}
				return ret;
			}
			else {
				SBUtil.writeToChat("Could not load XML block, no prefab: "+e.InnerText);
				return null;
			}
		}
	}
}
