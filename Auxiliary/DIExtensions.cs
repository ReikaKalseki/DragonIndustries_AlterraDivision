using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using FMOD;
using FMOD.Studio;

using SMLHelper.V2.Handlers;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {
	public static class DIExtensions {

		public static string setLeadingCase(this string s, bool upper) {
			return (upper ? char.ToUpperInvariant(s[0]) : char.ToLowerInvariant(s[0])) + s.Substring(1);
		}

		public static string ensureNonNull(this string s) {
			return s == null ? "" : s;
		}

		public static string from(this string s, char cut) {
			return s.Substring(s.IndexOf(cut) + 1);
		}

		public static List<string[]> polySplit(this string s, char s1, char s2) {
			List<string[]> li = new List<string[]>();
			string[] parts = s.Split(s1);
			for (int i = 0; i < parts.Length; i++) {
				li.Add(parts[i].Split(s2));
			}
			return li;
		}

		public static List<List<string[]>> polySplit(this string s, char s1, char s2, char s3) {
			List<List<string[]>> li0 = new List<List<string[]>>();
			string[] parts = s.Split(s1);
			for (int i = 0; i < parts.Length; i++) {
				string[] p = parts[i].Split(s2);
				List<string[]> li2 = new List<string[]>();
				for (int k = 0; k < p.Length; k++) {
					li2.Add(p[k].Split(s3));
				}
				li0.Add(li2);
			}
			return li0;
		}

		public static E convertEnum<E>(this Enum e, E fallback) where E : struct {
			return Enum.TryParse<E>(e.ToString(), out E ret) ? ret : fallback;
		}

		public static bool intersects(this SphereCollider sc, SphereCollider other) {
			Vector3 pos1 = sc.transform.position+sc.center;
			Vector3 pos2 = other.transform.position+other.center;
			float r = Mathf.Min(sc.radius, other.radius);
			return (pos2 - pos1).sqrMagnitude <= r * r;
		}

		public static Vector3 getWorldCenter(this SphereCollider sc) {
			return sc.center + sc.transform.position;
		}

		public static Vector3 getWorldCenter(this BoxCollider sc) {
			return sc.center + sc.transform.position;
		}

		public static Vector3 getWorldCenter(this CapsuleCollider sc) {
			return sc.center + sc.transform.position;
		}

		public static Sprite setTexture(this Sprite s, Texture2D tex) {
			return Sprite.Create(tex, s.textureRect, s.pivot, s.pixelsPerUnit, 0, SpriteMeshType.FullRect, s.border);
		}

		public static VECTOR toFMODVector(this Vector3 vec) {
			VECTOR ret = new VECTOR();
			ret.x = vec.x;
			ret.y = vec.y;
			ret.z = vec.z;
			return ret;
		}

		public static Vector4 setXYZ(this Vector4 vec, Vector3 xyz) {
			vec.x = xyz.x;
			vec.y = xyz.y;
			vec.z = xyz.z;
			return new Vector4(xyz.x, xyz.y, xyz.z, vec.w);
		}

		public static Color exponent(this Color c, float exp) {
			return new Color(Mathf.Pow(c.r, exp), Mathf.Pow(c.g, exp), Mathf.Pow(c.b, exp), Mathf.Pow(c.a, exp));
		}

		public static Color asColor(this Vector3 c) {
			return new Color(c.x, c.y, c.z);
		}

		public static Vector3 exponent(this Vector3 c, float exp) {
			return new Vector3(Mathf.Pow(c.x, exp), Mathf.Pow(c.y, exp), Mathf.Pow(c.z, exp));
		}

		public static Color asColor(this Vector4 c) {
			return new Color(c.x, c.y, c.z, c.w);
		}

		public static Vector4 exponent(this Vector4 c, float exp) {
			return new Vector4(Mathf.Pow(c.x, exp), Mathf.Pow(c.y, exp), Mathf.Pow(c.z, exp), Mathf.Pow(c.w, exp));
		}

		public static Vector3 toVector(this Color c) {
			return new Vector3(c.r, c.g, c.b);
		}

		public static Vector4 toVectorA(this Color c) {
			return new Vector4(c.r, c.g, c.b, c.a);
		}

		public static int toARGB(this Color c) {
			int a = Mathf.RoundToInt(c.a*255) & 0xFF;
			int r = Mathf.RoundToInt(c.r*255) & 0xFF;
			int g = Mathf.RoundToInt(c.g*255) & 0xFF;
			int b = Mathf.RoundToInt(c.b*255) & 0xFF;
			return (a << 24) | (r << 16) | (g << 8) | (b);
		}

		public static Vector3 getXYZ(this Vector4 vec) {
			return new Vector3(vec.x, vec.y, vec.z);
		}

		public static Vector3 setLength(this Vector3 vec, double amt) {
			return vec.normalized * ((float)amt);
		}

		public static Vector3 addLength(this Vector3 vec, double amt) {
			return vec.setLength(vec.magnitude + amt);
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

		public static XmlElement addProperty(this XmlNode xml, string name, Vector4 vec) {
			Quaternion quat = new Quaternion();
			quat.x = vec.x;
			quat.y = vec.y;
			quat.z = vec.z;
			quat.w = vec.w;
			return addProperty(xml, name, quat);
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
			return xml.addProperty(name, value.ToString(CultureInfo.InvariantCulture));
		}

		public static XmlElement addProperty(this XmlNode xml, string name, double value) {
			return xml.addProperty(name, value.ToString(CultureInfo.InvariantCulture));
		}

		public static XmlElement addProperty(this XmlNode xml, string name, bool value) {
			return xml.addProperty(name, value.ToString(CultureInfo.InvariantCulture));
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
			return string.IsNullOrEmpty(s)
				? double.IsNaN(fallback) ? throw new Exception("No matching tag '" + name + "'! " + xml.format()) : fallback
				: double.Parse(xml.getProperty(name), CultureInfo.InvariantCulture);
		}

		public static int getInt(this XmlElement xml, string name, int fallback, bool allowFallback = true) {
			string s = xml.getProperty(name, allowFallback);
			bool nul = string.IsNullOrEmpty(s);
			return nul && !allowFallback
				? throw new Exception("No matching tag '" + name + "'! " + xml.format())
				: nul ? fallback : int.Parse(s, CultureInfo.InvariantCulture);
		}

		public static bool getBoolean(this XmlElement xml, string name) {
			return xml.getBoolean(name, out XmlElement trash);
		}

		public static bool getBoolean(this XmlElement xml, string name, out XmlElement elem) {
			string prop = xml.getProperty(name, out elem, true);
			return !string.IsNullOrEmpty(prop) && bool.Parse(prop);
		}

		public static string getProperty(this XmlElement xml, string name, bool allowNull = false) {
			return xml.getProperty(name, out XmlElement trash, allowNull);
		}

		public static int getRandomInt(this XmlElement xml, string name) {
			List<XmlElement> li = xml.getDirectElementsByTagName(name);
			if (li.Count == 1) {
				int min = li[0].getInt("min", 0, true);
				int max = li[0].getInt("max", -1, false);
				return UnityEngine.Random.Range(min, max);
			}
			else {
				throw new Exception("You must have exactly one matching named element for getRandomInt '" + name + "'! " + xml.format());
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
				throw new Exception("You must have exactly one matching named element for getRandomFloat '" + name + "'! " + xml.format());
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
				throw new Exception("You must have exactly one matching named tag for getProperty '" + name + "'! " + xml.format());
			}
		}

		public static Vector3? getVector(this XmlElement xml, string name, bool allowNull = false) {
			return getVector(xml, name, out XmlElement trash, allowNull);
		}

		public static Vector4? getVector4(this XmlElement xml, string name, bool allowNull = false) {
			Quaternion? quat = getQuaternion(xml, name, allowNull);
			if (quat == null || !quat.HasValue)
				return null;
			Vector4 vec = new Vector4();
			vec.x = quat.Value.x;
			vec.y = quat.Value.y;
			vec.z = quat.Value.z;
			vec.w = quat.Value.w;
			return vec;
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
				throw new Exception("You must have exactly one matching named element for getVector '" + name + "'! " + xml.format());
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
			else {
				return li.Count == 0 && allowNull
					? (Quaternion?)null
					: throw new Exception("You must have exactly one matching named element for getQuaternion '" + name + "'! " + xml.format());
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
			else {
				return li.Count == 0 && allowNull
					? (Color?)null
					: throw new Exception("You must have exactly one matching named element for getColor '" + name + "'! " + xml.format());
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

		public static XmlElement addChild(this XmlElement e, string name) {
			XmlElement e2 = e.OwnerDocument.CreateElement(name);
			e.AppendChild(e2);
			return e2;
		}

		public static Int3 roundToInt3(this Vector3 vec) {
			return new Int3((int)Math.Floor(vec.x), (int)Math.Floor(vec.y), (int)Math.Floor(vec.z));
		}

		public static bool isEnumerable(this object o) {
			return o != null && o is IEnumerable && o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>));
		}

		public static bool isList(this object o) {
			return o != null && o is IList && o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
		}

		public static bool isDictionary(this object o) {
			return o != null && o is IDictionary && o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
		}
		/*
		public static string toDebugString(this IDictionary<object, object> dict) {
			return "{" + string.Join(",", dict.Select(kv => kv.Key + "=" + stringify(kv.Value)).ToArray()) + "}";
		}
		
		public static string toDebugString(this IEnumerable<object> c) {
			return "[" + string.Join(",", c.Select<object, string>(stringify).ToArray()) + "]";
		}
		*/
		public static string toDebugString<K, V>(this IDictionary<K, V> dict) {
			return dict.Count + ":{" + string.Join(",", dict.Select(kv => kv.Key + "=" + stringify(kv.Value)).ToArray()) + "}";//return toDebugString((IDictionary<object, object>)dict);
		}

		public static string toDebugString<E>(this IEnumerable<E> c) {
			return c.Count() + ":[" + string.Join(",", c.Select<E, string>(e => stringify(e)).ToArray()) + "]";//return toDebugString((IEnumerable<object>)c);
		}

		public static E pop<E>(this IList<E> c) {
			E ret = c[0];
			c.RemoveAt(0);
			return ret;
		}

		public static Vector3 getClosest(this IEnumerable<Vector3> li, Vector3 pos) {
			Vector3 ret = Vector3.zero;
			float distSq = float.PositiveInfinity;
			foreach (Vector3 v in li) {
				float dd = (v-pos).sqrMagnitude;
				if (dd < distSq) {
					distSq = dd;
					ret = v;
				}
			}
			return ret;
		}

		public static E[] addToArray<E>(this E[] arr, E add) {
			List<E> li = new List<E>(arr.ToList()) {
				add
			};
			return li.ToArray();
		}

		public static bool overlaps<E>(this ICollection<E> c, ICollection<E> other) {
			foreach (E e in c) {
				if (other.Contains(e)) {
					return true;
				}
			}
			return false;
		}

		public static string stringify(object obj) {
			if (obj == null)
				return "null";
			else if (obj.isDictionary())
				return "dict:" + ((IDictionary<object, object>)obj).toDebugString();
			else if (obj.isEnumerable())
				return "enumerable:" + ((IEnumerable<object>)obj).toDebugString();
			return obj.ToString();
		}

		public static T copyObject<T>(this T comp, T from) where T : class {
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

			foreach (PropertyInfo pinfo in typeof(T).GetProperties(flags)) {
				if (pinfo.CanWrite) {
					try {
						pinfo.SetValue(comp, pinfo.GetValue(from, null), null);
					}
					catch {

					}
				}
			}

			foreach (FieldInfo finfo in typeof(T).GetFields(flags)) {
				finfo.SetValue(comp, finfo.GetValue(from));
			}

			return comp;
		}

		public static string toDetailedString(this WaterscapeVolume.Settings s) {
			return String.Format("Start={0:0.0000}, Murk={1:0.0000}, Absorb={2:0.0000}, AmbScale={3:0.0000}, Emissive={4}, EmisScale={5:0.0000}, Scatter={6:0.0000}, ScatterColor={7}, Sun={8:0.0000}, Temp={9}", s.startDistance, s.murkiness, s.absorption, s.ambientScale, s.emissive.ToString("0.0000"), s.emissiveScale, s.scattering, s.scatteringColor.ToString("0.0000"), s.sunlightScale, s.temperature);
		}

		public static float getLifespan(this StasisSphere s) {
			return s.time * s.fieldEnergy;
		}

	}
}
