using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Xml;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra
{
	public static class DIExtensions {
		
		public static XmlElement addProperty(this XmlNode xml, string name, Quaternion quat) {
			XmlElement n = xml.OwnerDocument.CreateElement(name);
			n.addProperty("x", quat.x);
			n.addProperty("y", quat.y);
			n.addProperty("z", quat.z);
			n.addProperty("w", quat.w);
			xml.AppendChild(n);
			return n;
		}
		
		public static XmlElement addProperty(this XmlNode xml, string name, Vector3 vec) {
			XmlElement n = xml.OwnerDocument.CreateElement(name);
			n.addProperty("x", vec.x);
			n.addProperty("y", vec.y);
			n.addProperty("z", vec.z);
			xml.AppendChild(n);
			return n;
		}
		
		public static XmlElement addProperty(this XmlNode xml, string name, int value) {
			return xml.addProperty(name, value.ToString());
		}
		
		public static XmlElement addProperty(this XmlNode xml, string name, double value) {
			return xml.addProperty(name, value.ToString());
		}
		
		public static XmlElement addProperty(this XmlNode xml, string name, string value = null) {
			XmlElement n = xml.OwnerDocument.CreateElement(name);
			if (value != null)
				n.InnerText = value;
			xml.AppendChild(n);
			return n;
		}
		
		public static string getProperty(this XmlElement xml, string name, bool allowNull = false) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				return li[0].InnerText;
			}
			else if (li.Count == 0 && allowNull) {
				return null;
			}
			else {
				throw new Exception("You must have exactly one matching named tag for getProperty '"+name+"'! "+xml.InnerText);
			}
		}
		
		public static Vector3 getVector(this XmlElement xml, string name) {
			XmlElement trash;
			return getVector(xml, name, out trash);
		}
		
		public static Vector3 getVector(this XmlElement xml, string name, out XmlElement elem) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				string x = li[0].getProperty("x");
				string y = li[0].getProperty("y");
				string z = li[0].getProperty("z");
				elem = li[0];
				return new Vector3((float)double.Parse(x), (float)double.Parse(y), (float)double.Parse(z));
			}
			else {
				throw new Exception("You must have exactly one matching named element for getVector '"+name+"'! "+xml.InnerText);
			}
		}
		
		public static Quaternion? getQuaternion(this XmlElement xml, string name, bool allowNull = false) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				string x = li[0].getProperty("x");
				string y = li[0].getProperty("y");
				string z = li[0].getProperty("z");
				string w = li[0].getProperty("w");
				return new Quaternion((float)double.Parse(x), (float)double.Parse(y), (float)double.Parse(z), (float)double.Parse(w));
			}
			else if (li.Count == 0 && allowNull) {
				return null;
			}
			else {
				throw new Exception("You must have exactly one matching named element for getQuaternion '"+name+"'! "+xml.InnerText);
			}
		}
		
		public static List<XmlElement> getDirectElementsByTagName(this XmlElement xml, string name) {
			List<XmlElement> li = new List<XmlElement>();
			foreach (XmlElement e in xml.ChildNodes) {
				if (e is XmlElement && e.Name == name)
					li.Add(e);
			}
			return li;
		}
		
	}
}
