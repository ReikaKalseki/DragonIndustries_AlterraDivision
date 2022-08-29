﻿using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Xml;

using System.Collections;
using System.Collections.Generic;
using SMLHelper.V2.Handlers;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra
{
	public static class DIExtensions {
		
		public static Vector3 setLength(this Vector3 vec, double amt) {
			return vec.normalized*((float)(amt));
		}
		
		public static Vector3 addLength(this Vector3 vec, double amt) {
			return vec.setLength(vec.magnitude+amt);
		}
		
		public static Vector3 setY(this Vector3 vec, double y) {
			return new Vector3(vec.x, (float)y, vec.z);
		}
		
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
		
		public static XmlElement addProperty(this XmlNode xml, string name, Color c) {
			XmlElement n = xml.OwnerDocument.CreateElement(name);
			n.addProperty("r", c.r);
			n.addProperty("g", c.g);
			n.addProperty("b", c.b);
			n.addProperty("a", c.a);
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
					throw new Exception("No matching tag '"+name+"'! "+xml.format());
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
				throw new Exception("No matching tag '"+name+"'! "+xml.format());
			return nul ? fallback : int.Parse(s);
		}
		
		public static bool getBoolean(this XmlElement xml, string name) {
			XmlElement trash;
			return xml.getBoolean(name, out trash);
		}
		
		public static bool getBoolean(this XmlElement xml, string name, out XmlElement elem) {
			string prop = xml.getProperty(name, out elem, true);
			return !string.IsNullOrEmpty(prop) && bool.Parse(prop);
		}
		
		public static string getProperty(this XmlElement xml, string name, bool allowNull = false) {
			XmlElement trash;
			return xml.getProperty(name, out trash, allowNull);
		}
		
		public static int getRandomInt(this XmlElement xml, string name) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				int min = li[0].getInt("min", 0, true);
				int max = li[0].getInt("max", -1, false);
				return UnityEngine.Random.Range(min, max);
			}
			else {
				throw new Exception("You must have exactly one matching named element for getRandomInt '"+name+"'! "+xml.format());
			}
		}
		
		public static float getRandomFloat(this XmlElement xml, string name) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				double min = li[0].getFloat("min", double.NaN);
				double max = li[0].getFloat("max", double.NaN);
				return UnityEngine.Random.Range((float)min, (float)max);
			}
			else {
				throw new Exception("You must have exactly one matching named element for getRandomFloat '"+name+"'! "+xml.format());
			}
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
				throw new Exception("You must have exactly one matching named tag for getProperty '"+name+"'! "+xml.format());
			}
		}
		
		public static Vector3? getVector(this XmlElement xml, string name, bool allowNull = false) {
			XmlElement trash;
			return getVector(xml, name, out trash, allowNull);
		}
		
		public static Vector3? getVector(this XmlElement xml, string name, out XmlElement elem, bool allowNull = false) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				double x = li[0].getFloat("x", double.NaN);
				double y = li[0].getFloat("y", double.NaN);
				double z = li[0].getFloat("z", double.NaN);
				elem = li[0];
				return new Vector3((float)x, (float)y, (float)z);
			}
			else if (li.Count == 0 && allowNull) {
				elem = null;
				return null;
			}
			else {
				throw new Exception("You must have exactly one matching named element for getVector '"+name+"'! "+xml.format());
			}
		}
		
		public static Quaternion? getQuaternion(this XmlElement xml, string name, bool allowNull = false) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				double x = li[0].getFloat("x", double.NaN);
				double y = li[0].getFloat("y", double.NaN);
				double z = li[0].getFloat("z", double.NaN);
				double w = li[0].getFloat("w", double.NaN);
				return new Quaternion((float)x, (float)y, (float)z, (float)w);
			}
			else if (li.Count == 0 && allowNull) {
				return null;
			}
			else {
				throw new Exception("You must have exactly one matching named element for getQuaternion '"+name+"'! "+xml.format());
			}
		}
		
		public static Color? getColor(this XmlElement xml, string name, bool includeAlpha, bool allowNull = false) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				double r = li[0].getFloat("r", double.NaN);
				double g = li[0].getFloat("g", double.NaN);
				double b = li[0].getFloat("b", double.NaN);
				double a = includeAlpha ? li[0].getFloat("a", double.NaN) : 1;
				return new Color((float)r, (float)g, (float)b, (float)a);
			}
			else if (li.Count == 0 && allowNull) {
				return null;
			}
			else {
				throw new Exception("You must have exactly one matching named element for getColor '"+name+"'! "+xml.format());
			}
		}
		
		public static List<XmlElement> getDirectElementsByTagName(this XmlElement xml, string name) {
			List<XmlElement> li = new List<XmlElement>();
			foreach (XmlNode e in xml.ChildNodes) {
				if (e is XmlElement && e.Name == name)
					li.Add((XmlElement)e);
			}
			return li;
		}
		
		public static XmlNodeList getAllChildrenIn(this XmlElement xml, string name) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			return li.Count == 1 ? li[0].ChildNodes : null;
		}
		
		public static bool hasProperty(this XmlElement xml, string name) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			return li.Count == 1;
		}
		
		public static string format(this XmlElement xml) {
			return xml.OuterXml;
		}
		
		public static Int3 roundToInt3(this Vector3 vec) {
			return new Int3((int)Math.Floor(vec.x), (int)Math.Floor(vec.y), (int)Math.Floor(vec.z));
		}
		
		public static bool isEnumerable(this object o) {
    		if (o == null)
    			return false;
	    	return o is IEnumerable && o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>));
		}
		
		public static bool isList(this object o) {
    		if (o == null)
    			return false;
	    	return o is IList && o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
		}
		
		public static bool isDictionary(this object o) {
		    if (o == null)
		    	return false;
		    return o is IDictionary && o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
		}
		
		public static string toDebugString<K, V>(this IDictionary<K, V> dict) {
		    return "{" + string.Join(",", dict.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
		}
		
		public static string toDebugString<E>(this IEnumerable<E> c) {
		    return "[" + string.Join(",", c.ToArray()) + "]";
		}
		
		public static T copyObject<T>(this T comp, T from) where T : class {
	         Type type = comp.GetType();
	         Type othersType = from.GetType();
	         if (type != othersType) {
	         	throw new Exception("Mismatched types on "+comp+" and "+from+": "+type+" vs "+othersType);
	         }
	
	         BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
	         
	         foreach (PropertyInfo pinfo in type.GetProperties(flags)) {
	             if (pinfo.CanWrite) {
	                 try  {
	                     pinfo.SetValue(comp, pinfo.GetValue(from, null), null);
	                 }
	                 catch {
	                     
	                 }
	             }
	         }
	
	         foreach (FieldInfo finfo in type.GetFields(flags)) {
	             finfo.SetValue(comp, finfo.GetValue(from));
	         }
	         
	         return comp;
	     }
		
	}
}