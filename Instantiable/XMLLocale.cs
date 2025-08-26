using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public class XMLLocale {

		private static readonly LocaleEntry NOT_FOUND = new LocaleEntry(null, "NOTFOUND", "#NULL", "#NULL", "#NULL");

		public readonly string relativePath;
		private readonly Assembly ownerMod;

		private readonly Dictionary<string, LocaleEntry> entries = new Dictionary<string, LocaleEntry>();

		private XmlDocument xmlFile;

		public XMLLocale(Assembly owner, string path) {
			ownerMod = owner;
			relativePath = path;
		}

		public void load() {
			xmlFile = this.loadXML();
			if (xmlFile.DocumentElement == null)
				throw new Exception("No XML file at " + relativePath);
			foreach (XmlElement e in xmlFile.DocumentElement.ChildNodes) {
				LocaleEntry lc = this.constructEntry(e);
				entries[lc.key] = lc;
			}
			SNUtil.log("XML DB '" + this + "' loaded " + entries.Count + " entries: " + string.Join(", ", entries.Keys), ownerMod);/*
			foreach (LocaleEntry e in entries.Values) {
				SNUtil.log(e.ToString());
			}*/
		}

		private XmlDocument loadXML() {
			string loc = Path.GetDirectoryName(ownerMod.Location);
			string path = Path.Combine(loc, relativePath);
			XmlDocument doc = new XmlDocument();
			if (File.Exists(path)) {
				doc.Load(path);
			}
			else {
				SNUtil.log("Could not find XML file " + path + "!", ownerMod);
			}
			return doc;
		}

		private LocaleEntry constructEntry(XmlElement e) {
			return new LocaleEntry(e);
		}

		public LocaleEntry getEntry(string id) {
			if (entries.ContainsKey(id))
				return entries[id];
			SNUtil.log("Could not find locale entry '" + id + "'", ownerMod);
			return NOT_FOUND;
		}

		public IEnumerable<LocaleEntry> getEntries() {
			return entries.Values;
		}

		private static string cleanString(string input) {
			if (string.IsNullOrEmpty(input))
				return input;
			string[] parts = input.Trim().Split('\n');
			for (int i = 0; i < parts.Length; i++) {
				parts[i] = parts[i].Trim();
			}
			return string.Join("\n", parts);
		}

		public class LocaleEntry {

			private readonly XmlElement element;

			public readonly string key;
			public readonly string name;
			public readonly string desc;
			public readonly string pda;

			internal LocaleEntry(XmlElement e) : this(e, e.Name, cleanString(e.getProperty("name")), cleanString(e.getProperty("desc", true)), cleanString(e.getProperty("pda"))) {

			}

			internal LocaleEntry(XmlElement e, string k, string n, string d, string p) {
				key = k;
				name = n;
				desc = d;
				pda = p;

				element = e;
			}

			public override string ToString() {
				return key + ": " + name + " / " + desc;
			}

			public string dump() {
				return element == null ? "<null>" : element.format();
			}

			public bool hasField(string key) {
				return element != null && element.hasProperty(key);
			}

			public T getField<T>(string key) where T : class {
				return this.getField<T>(key, null);
			}

			public T getField<T>(string key, T fallback) {
				if (element == null)
					return fallback;
				Type t = typeof(T);
				if (t == typeof(Int3)) {
					Vector3? vec = element.getVector(key);
					return vec != null && vec.HasValue ? (T)Convert.ChangeType(vec.Value.roundToInt3(), t) : fallback;
				}
				if (t == typeof(Vector3)) {
					Vector3? vec = element.getVector(key);
					return vec != null && vec.HasValue ? (T)Convert.ChangeType(vec.Value, t) : fallback;
				}
				if (t == typeof(Quaternion)) {
					Quaternion? vec = element.getQuaternion(key);
					return vec != null && vec.HasValue ? (T)Convert.ChangeType(vec.Value, t) : fallback;
				}
				if (t == typeof(string)) {
					string get = element.getProperty(key, true);
					return (T)Convert.ChangeType(get == null ? null : cleanString(get), t);
				}
				if (t == typeof(bool)) {
					return (T)Convert.ChangeType(element.getBoolean(key), t);
				}
				if (t == typeof(int)) {
					return (T)Convert.ChangeType(element.getInt(key, (int)(object)fallback, true), t);
				}
				if (t == typeof(float)) {
					double fall = (double)(object)fallback;
					return (T)Convert.ChangeType((float)element.getFloat(key, fall), t);
				}
				if (t == typeof(double)) {
					double fall = (double)(object)fallback;
					return (T)Convert.ChangeType(element.getFloat(key, fall), t);
				}
				throw new Exception("Undefined data type '" + t + "'");
			}

			public IEnumerable<KeyValuePair<string, string>> getFields() {
				List<KeyValuePair<string, string>> li = new List<KeyValuePair<string, string>>();
				foreach (XmlElement e in element.ChildNodes) {
					li.Add(new KeyValuePair<string, string>(e.Name, e.InnerText.Trim()));
				}
				return li;
			}

		}

	}
}
