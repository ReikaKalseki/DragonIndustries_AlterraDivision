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
		
		public static XmlElement addProperty(this XmlNode xml, string name, bool value) {
			return xml.addProperty(name, value.ToString());
		}
		
		public static XmlElement addProperty(this XmlNode xml, string name, string value = null) {
			XmlElement n = xml.OwnerDocument.CreateElement(name);
			if (value != null)
				n.InnerText = value;
			xml.AppendChild(n);
			return n;
		}
		
		public static double getFloat(this XmlElement xml, string name, double fallback) {
			string s = xml.getProperty(name, true);
			if (string.IsNullOrEmpty(s)) {
				if (double.IsNaN(fallback))
					throw new Exception("No matching tag '"+name+"'! "+xml.InnerText);
				else
					return fallback;
			}
			else {
				return double.Parse(xml.getProperty(name));
			}
		}
		
		public static int getInt(this XmlElement xml, string name, int fallback, bool allowFallback = true) {
			string s = xml.getProperty(name, allowFallback);
			bool nul = string.IsNullOrEmpty(s);
			if (nul && !allowFallback)
				throw new Exception("No matching tag '"+name+"'! "+xml.InnerText);
			return nul ? fallback : int.Parse(s);
		}
		
		public static bool getBoolean(this XmlElement xml, string name) {
			XmlElement trash;
			return xml.getBoolean(name, out trash);
		}
		
		public static bool getBoolean(this XmlElement xml, string name, out XmlElement elem) {
			return bool.Parse(xml.getProperty(name, out elem));
		}
		
		public static string getProperty(this XmlElement xml, string name, bool allowNull = false) {
			XmlElement trash;
			return xml.getProperty(name, out trash, allowNull);
		}
		
		public static string getProperty(this XmlElement xml, string name, out XmlElement elem, bool allowNull = false) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				elem = li[0];
				return li[0].InnerText;
			}
			else if (li.Count == 0 && allowNull) {
				elem = null;
				return null;
			}
			else {
				throw new Exception("You must have exactly one matching named tag for getProperty '"+name+"'! "+xml.InnerText);
			}
		}
		
		public static Vector3? getVector(this XmlElement xml, string name, bool allowNull = false) {
			XmlElement trash;
			return getVector(xml, name, out trash, allowNull);
		}
		
		public static Vector3? getVector(this XmlElement xml, string name, out XmlElement elem, bool allowNull = false) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				string x = li[0].getProperty("x");
				string y = li[0].getProperty("y");
				string z = li[0].getProperty("z");
				elem = li[0];
				return new Vector3((float)double.Parse(x), (float)double.Parse(y), (float)double.Parse(z));
			}
			else if (li.Count == 0 && allowNull) {
				elem = null;
				return null;
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
