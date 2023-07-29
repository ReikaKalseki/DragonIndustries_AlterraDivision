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
	public class PositionedPrefab : ObjectTemplate {
		
		[SerializeField]
		public string prefabName;
		[SerializeField]
		public Vector3 position;
		[SerializeField]
		public Quaternion rotation;
		[SerializeField]
		public Vector3 scale = Vector3.one;
			
		protected Guid? xmlID;
		
		public PositionedPrefab(string pfb, Vector3? pos = null, Quaternion? rot = null, Vector3? sc = null) {
			prefabName = pfb;
			position = GenUtil.getOrZero(pos);
			rotation = GenUtil.getOrIdentity(rot);
			if (sc != null && sc.HasValue)
				scale = sc.Value;
		}
		
		public PositionedPrefab(PrefabIdentifier pi) {
			prefabName = pi.classId;
			position = pi.transform.position;
			rotation = pi.transform.rotation;
			scale = pi.transform.localScale;
		}
		
		public PositionedPrefab(PositionedPrefab pfb) {
			prefabName = pfb.prefabName;
			position = pfb.position;
			rotation = pfb.rotation;
			scale = pfb.scale;
		}
		
		public override string getTagName() {
			return "basicprefab";
		}
		
		public sealed override string getID() {
			return prefabName;
		}
		
		public string getXMLID() {
			return xmlID.HasValue ? xmlID.Value.ToString() : null;
		}
		
		public virtual void replaceObject(string pfb) {
			prefabName = pfb;
		}
		
		public string getPrefab() {
			return prefabName;
		}
		
		public virtual GameObject createWorldObject() {
			GameObject ret = ObjectUtil.createWorldObject(prefabName);
			if (ret != null) {
				ret.transform.position = position;
				ret.transform.rotation = rotation;
				ret.transform.localScale = scale;
			}
			return ret;
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
			
		public override void saveToXML(XmlElement n) {
			n.addProperty("prefab", prefabName);
			n.addProperty("position", position);
			XmlElement rot = n.addProperty("rotation", rotation.eulerAngles);
			rot.addProperty("quaternion", rotation);
			n.addProperty("scale", scale);
				
			if (xmlID != null && xmlID.HasValue)
				n.addProperty("xmlID", xmlID.Value.ToString());
		}
			
		public override string ToString() {
			return prefabName+" @ "+position+" / "+rotation.eulerAngles+" / "+scale;
		}
		
		public override void loadFromXML(XmlElement e) {
			setPrefabName(e.getProperty("prefab"));
			position = e.getVector("position").Value;
			XmlElement elem;
			Vector3? rot = e.getVector("rotation", out elem, true);
			//SBUtil.log("rot: "+rot);
			Quaternion quat = rotation;
			if (rot != null && rot.HasValue) {
				Quaternion? specify = elem.getQuaternion("quaternion", true);
				//SBUtil.log("quat: "+specify);
				if (specify != null && specify.HasValue) {
					quat = specify.Value;
				}
				else {
					quat = Quaternion.Euler(rot.Value.x, rot.Value.y, rot.Value.z);
				}
			}
			rotation = quat;
			//SBUtil.log("use rot: "+rotation+" / "+rotation.eulerAngles);
			Vector3? sc = e.getVector("scale", true);
			if (sc != null && sc.HasValue)
				scale = sc.Value;
			
			string xmlid = e.getProperty("xmlID", true);
			if (!string.IsNullOrEmpty(xmlid)) {
			    xmlID = new Guid(xmlid);
			}
		}
		
		protected virtual void setPrefabName(string name) {
			prefabName = name;
		}
	}
}
