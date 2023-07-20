using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	
	public static class SaveSystem {
		
		private static readonly Dictionary<string, SaveHandler> handlers = new Dictionary<string, SaveHandler>();
		private static readonly Dictionary<string, XmlElement> saveData = new Dictionary<string, XmlElement>();
		private static readonly string xmlPathRoot;
		private static bool loaded;
		
		static SaveSystem() {
			IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
			IngameMenuHandler.Main.RegisterOnSaveEvent(handleSave);
			
			xmlPathRoot = Path.Combine(Path.GetDirectoryName(SNUtil.diDLL.Location), "persistentData");
		}
		
		public static void addSaveHandler(ModPrefab pfb, SaveHandler h) {
			addSaveHandler(pfb.ClassID, h);
		}
		
		public static void addSaveHandler(string classID, SaveHandler h) {
			handlers[classID] = h;
		}
		
		public static void handleSave() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".dat");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {	
				SaveHandler sh = getHandler(pi, false);
				if (sh != null) {
					SNUtil.log("Found "+sh+" save handler for "+pi.ClassId, SNUtil.diDLL);
					sh.data = doc.CreateElement("object");
					sh.data.SetAttribute("objectID", pi.Id);
					sh.save(pi);
					doc.DocumentElement.AppendChild(sh.data);
				}
			}
			SNUtil.log("Saving "+doc.DocumentElement.ChildNodes.Count+" objects to disk", SNUtil.diDLL);
			Directory.CreateDirectory(xmlPathRoot);
			doc.Save(path);
		}
		
		public static void handleLoad() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".dat");
			if (!File.Exists(path))
				path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".xml");
			if (File.Exists(path)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				saveData.Clear();
				foreach (XmlElement e in doc.DocumentElement.ChildNodes)
					saveData[e.GetAttribute("objectID")] = e;
				SNUtil.log("Loaded "+saveData.Count+" object entries from disk", SNUtil.diDLL);
			}
		}
		
		public static void populateLoad() {
			if (loaded)
				return;
			loaded = true;
			SNUtil.log("Applying saved object entries", SNUtil.diDLL);
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				SaveHandler sh = getHandler(pi, true);
				if (sh != null) {
					SNUtil.log("Found "+sh+" load handler for "+pi.ClassId, SNUtil.diDLL);
					sh.load(pi);
				}
			}
		}
		
		private static SaveHandler getHandler(PrefabIdentifier pi, bool needSaveData) {
			if (pi && !string.IsNullOrEmpty(pi.ClassId)) {
				SaveHandler ret;
				XmlElement elem = null;
				//SNUtil.log("Attempting to load "+pi+" ["+pi.id+"]", SNUtil.diDLL);
				if (needSaveData && handlers.ContainsKey(pi.ClassId) && !saveData.ContainsKey(pi.Id))
					SNUtil.log("Object "+pi+" ["+pi.id+"] had no data to load!", SNUtil.diDLL);
				if (handlers.TryGetValue(pi.ClassId, out ret) && (!needSaveData || saveData.TryGetValue(pi.Id, out elem))) {
					if (elem != null)
						ret.data = elem;
					return ret;
				}
			}
			return null;
		}
		
		public abstract class SaveHandler {
			
			protected internal XmlElement data;
			
			public abstract void save(PrefabIdentifier pi);
			public abstract void load(PrefabIdentifier pi);
			
		}
	
		public sealed class ComponentFieldSaveHandler<C> : SaveHandler where C : MonoBehaviour {
			
			private readonly List<string> fields = new List<string>();
			
			public ComponentFieldSaveHandler() {
				
			}
			
			public ComponentFieldSaveHandler(params string[] f) : this(f.ToList()) {
				
			}
			
			public ComponentFieldSaveHandler(IEnumerable<string> f) {
				fields.AddRange(f);
			}
			
			public ComponentFieldSaveHandler<C> addField(string f) {
				fields.Add(f);
				return this;
			}
			
			public ComponentFieldSaveHandler<C> addAllFields() {
				foreach (FieldInfo fi in typeof(C).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
					addField(fi.Name);
				}
				return this;
			}
			
			public override void save(PrefabIdentifier pi) {
				C com = pi.GetComponentInChildren<C>();
				if (!com)
					return;
				foreach (string s in fields) {
					object val = getField(s).GetValue(com);
					if (val is string)
						data.addProperty(s, (string)val);
					else if (val is int)
						data.addProperty(s, (int)val);
					else if (val is bool)
						data.addProperty(s, (bool)val);
					if (val is float)
						data.addProperty(s, (float)val);
					if (val is double)
						data.addProperty(s, (double)val);
					else if (val is Vector3)
						data.addProperty(s, (Vector3)val);
					else if (val is Quaternion)
						data.addProperty(s, (Quaternion)val);
					else if (val is Color)
						data.addProperty(s, (Color)val);
				}
			}
			
			public override void load(PrefabIdentifier pi) {
				C com = pi.GetComponentInChildren<C>();
				if (!com)
					return;
				foreach (string s in fields) {
					FieldInfo fi = getField(s);
					if (fi.FieldType == typeof(string))
						fi.SetValue(com, data.getProperty(s));
					else if (fi.FieldType == typeof(bool))
						fi.SetValue(com, data.getBoolean(s));
					else if (fi.FieldType == typeof(int))
						fi.SetValue(com, data.getInt(s, 0, false));
					else if (fi.FieldType == typeof(float))
						fi.SetValue(com, (float)data.getFloat(s, float.NaN));
					else if (fi.FieldType == typeof(double))
						fi.SetValue(com, data.getFloat(s, double.NaN));
					else if (fi.FieldType == typeof(Vector3))
						fi.SetValue(com, data.getVector(s));
					else if (fi.FieldType == typeof(Quaternion))
						fi.SetValue(com, data.getQuaternion(s));
					else if (fi.FieldType == typeof(Color))
						fi.SetValue(com, data.getColor(s, true));
				}
			}
			
			private FieldInfo getField(string s) {
				return typeof(C).GetField(s, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			}
			
			public override string ToString() {
				return string.Format("[ComponentFieldSaveHandler Fields={0}]", fields.toDebugString());
			}
 
		}
		
	}
}
