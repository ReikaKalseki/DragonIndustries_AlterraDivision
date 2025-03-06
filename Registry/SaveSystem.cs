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
		private static readonly Dictionary<string, PlayerSaveHook> playerSaveHandlers = new Dictionary<string, PlayerSaveHook>();
		private static readonly string oldSaveDir;
		private static readonly string saveFileName = "ModData.dat";
		private static bool loaded;
		
		public static bool debugSave = false;
		
		static SaveSystem() {
			IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
			IngameMenuHandler.Main.RegisterOnSaveEvent(handleSave);
			
			oldSaveDir = Path.Combine(Path.GetDirectoryName(SNUtil.diDLL.Location), "persistentData");
			if (Directory.Exists(oldSaveDir) && Directory.Exists(SNUtil.savesDir)) {
				migrateSaveData();
			}
		}
		
		private static void migrateSaveData() {
			SNUtil.log("Migrating save data from "+oldSaveDir+" to "+SNUtil.savesDir);
			bool all = true;
			foreach (string dat in Directory.GetFiles(oldSaveDir)) {
				if (dat.EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase)) {
					string save = Path.Combine(SNUtil.savesDir, Path.GetFileNameWithoutExtension(dat));
					if (Directory.Exists(save)) {
						SNUtil.log("Moving save data "+dat+" to "+save);
						File.Move(dat, Path.Combine(save, saveFileName));
					}
					else {
						SNUtil.log("No save found for '"+dat+", skipping");
						all = false;
					}
				}
			}
			SNUtil.log("Migration complete.");
			if (all) {
				SNUtil.log("All files moved, deleting old folder.");
				Directory.Delete(oldSaveDir);
			}
			else {
				SNUtil.log("Some files could not be moved so the old folder will not be deleted.");
			}
		}
		
		public static void addSaveHandler(ModPrefab pfb, SaveHandler h) {
			addSaveHandler(pfb.ClassID, h);
		}
		
		public static void addSaveHandler(string classID, SaveHandler h) {
			if (handlers.ContainsKey(classID))
				throw new Exception("A save handler is already registered to id '"+classID+"': "+handlers[classID]);
			handlers[classID] = h;
		}
		
		public static void addPlayerSaveCallback<O>(FieldInfo field, Func<O> instance) {
			if (field == null)
				throw new Exception("No such field!");
			addPlayerSaveCallback(new FieldHook<O>(field, instance));
		}
		
		public static void addPlayerSaveCallback(PlayerSaveHook s) {
			if (!XmlReader.IsName(s.id))
				throw new Exception("Invalid non-XMLSafe ID '"+s.id+"'");
			if (playerSaveHandlers.ContainsKey(s.id))
				throw new Exception("Player save hook ID '"+s.id+"' already registered!");
			playerSaveHandlers[s.id] = s;
		}
		
		public static void handleSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {	
				SaveHandler sh = getHandler(pi, false);
				if (sh != null) {
					if (debugSave)
						SNUtil.log("Found "+sh+" save handler for "+pi.ClassId, SNUtil.diDLL);
					sh.data = doc.CreateElement("object");
					sh.data.SetAttribute("objectID", pi.Id);
					sh.save(pi);
					doc.DocumentElement.AppendChild(sh.data);
				}
			}
			XmlElement e = doc.CreateElement("player");
			foreach (PlayerSaveHook t in playerSaveHandlers.Values) {
				if (t.saveAction == null) {
					SNUtil.log("Could not run save handler "+t+" on player: no save hook", SNUtil.diDLL);
					continue;
				}
				try {
					XmlElement e2 = doc.CreateElement(t.id);
					t.saveAction.Invoke(Player.main, e2);
					e.AppendChild(e2);
				}
				catch (Exception ex) {
					SNUtil.log("Save handler "+t+" on player threw "+ex, SNUtil.diDLL);
				}
			}
			doc.DocumentElement.AppendChild(e);
			e = doc.CreateElement("components");
			foreach (CustomSerializedComponent cs in Player.main.GetComponents<CustomSerializedComponent>()) {
				Type t = cs.GetType();
				XmlElement e2 = doc.CreateElement(t.Namespace+"."+t.Name);
				cs.saveToXML(e2);
				e.AppendChild(e2);
			}
			doc.DocumentElement.AppendChild(e);
			SNUtil.log("Saving "+doc.DocumentElement.ChildNodes.Count+" objects to disk", SNUtil.diDLL);
			Directory.GetParent(path).Create();
			doc.Save(path);
		}
		
		public static void handleLoad() {
			string dir = SNUtil.getCurrentSaveDir();
			string path = Path.Combine(dir, saveFileName);
			if (!File.Exists(path))
				path = Path.Combine(dir, saveFileName.Replace(".dat", ".xml"));
			if (File.Exists(path)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				saveData.Clear();
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					saveData[e.Name == "player" || e.Name == "components" ? e.Name : e.GetAttribute("objectID")] = e;
				}
				SNUtil.log("Loaded "+saveData.Count+" object entries from disk", SNUtil.diDLL);
			}
		}
		
		public static void populateLoad() {
			if (loaded)
				return;
			loaded = true;
			SNUtil.log("Applying saved object entries", SNUtil.diDLL);
			if (saveData.ContainsKey("player")) {
				XmlElement e = saveData["player"];
				foreach (XmlElement e2 in e.ChildNodes) {
					if (!playerSaveHandlers.ContainsKey(e2.Name)) {
						SNUtil.log("Skipping player save tag '"+e2.Name+"'; no mapping to a handler", SNUtil.diDLL);
						continue;
					}
					PlayerSaveHook t = playerSaveHandlers[e2.Name];
					if (t.loadAction == null) {
						SNUtil.log("Could not run load handler "+t+" on player: no load hook", SNUtil.diDLL);
						continue;
					}
					try {
						t.loadAction.Invoke(Player.main, e2);
						SNUtil.log("Applied player load action "+t.id, SNUtil.diDLL);
					}
					catch (Exception ex) {
						SNUtil.log("Save handler "+t+" on player threw "+ex, SNUtil.diDLL);
					}
				}
			}
			if (saveData.ContainsKey("components")) {
				XmlElement e = saveData["components"];
				foreach (XmlElement e2 in e.ChildNodes) {
					try {
						Type t = InstructionHandlers.getTypeBySimpleName(e2.Name);
						if (t == null) {
							SNUtil.log("Could not reinstantiate custom serialized component: no type found for '"+e2.Name+"'", SNUtil.diDLL);
							continue;
						}
						CustomSerializedComponent cs = (CustomSerializedComponent)Player.main.gameObject.AddComponent(t);
						cs.readFromXML(e2);
						SNUtil.log("Deserialized new "+t+" onto player", SNUtil.diDLL);
					}
					catch (Exception ex) {
						SNUtil.log("Trying to deserialize "+e2.Name+" on player threw "+ex, SNUtil.diDLL);
					}
				}
			}
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				SaveHandler sh = getHandler(pi, true);
				if (sh != null) {
					SNUtil.log("Found "+sh+" load handler for "+pi.ClassId+" ["+pi.id+"]", SNUtil.diDLL);
					try {
						sh.load(pi);
					}
					catch (Exception e) {
						SNUtil.log("Threw error loading object "+pi.ClassId+" "+pi.Id+": "+e, SNUtil.diDLL);
					}
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
			
		public static void saveToXML(XmlElement e, string s, object val) {
			if (val is string)
				e.addProperty(s, (string)val);
			else if (val is int)
				e.addProperty(s, (int)val);
			else if (val is bool)
				e.addProperty(s, (bool)val);
			if (val is float)
				e.addProperty(s, (float)val);
			if (val is double)
				e.addProperty(s, (double)val);
			else if (val is Vector3)
				e.addProperty(s, (Vector3)val);
			else if (val is Quaternion)
				e.addProperty(s, (Quaternion)val);
			else if (val is Color)
				e.addProperty(s, (Color)val);
		}
		
		public static void setField(XmlElement e, string s, FieldInfo fi, object inst) {
			if (fi.FieldType == typeof(string))
				fi.SetValue(inst, e.getProperty(s, true));
			else if (fi.FieldType == typeof(bool))
				fi.SetValue(inst, e.getBoolean(s));
			else if (fi.FieldType == typeof(int))
				fi.SetValue(inst, e.getInt(s, 0, true));
			else if (fi.FieldType == typeof(float))
				fi.SetValue(inst, (float)e.getFloat(s, 0));
			else if (fi.FieldType == typeof(double))
				fi.SetValue(inst, e.getFloat(s, 0));
			else if (fi.FieldType == typeof(Vector3))
				fi.SetValue(inst, e.getVector(s, true).GetValueOrDefault());
			else if (fi.FieldType == typeof(Quaternion))
				fi.SetValue(inst, e.getQuaternion(s, true).GetValueOrDefault());
			else if (fi.FieldType == typeof(Color))
				fi.SetValue(inst, e.getColor(s, true, true).GetValueOrDefault());
		}
		
		public class FieldHook<O> : PlayerSaveHook {
			
			public readonly FieldInfo field;
			public readonly Func<O> instanceGetter;
			
			public FieldHook(FieldInfo f, Func<O> inst) : base(buildID(f), (ep, e) => SaveSystem.saveToXML(e, "value", f.GetValue(inst.Invoke())), (ep, e) => SaveSystem.setField(e, "value", f, inst.Invoke())) {
				field = f;
				instanceGetter = inst;
			}
			
			private static string buildID(FieldInfo f) {
				return f.DeclaringType.Namespace+"."+f.DeclaringType.Name+"_"+f.Name;
			}
			
		}
	
		public class PlayerSaveHook {
			
			public readonly string id;
			public readonly Action<Player, XmlElement> saveAction;
			public readonly Action<Player, XmlElement> loadAction;
			
			public PlayerSaveHook(string id, Action<Player, XmlElement> s, Action<Player, XmlElement> l) {
				this.id = id;
				saveAction = s;
				loadAction = l;
			}
 
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
					SaveSystem.saveToXML(data, s, val);
				}
			}
			
			public override void load(PrefabIdentifier pi) {
				C com = pi.GetComponentInChildren<C>();
				if (!com)
					return;
				foreach (string s in fields) {
					FieldInfo fi = getField(s);
					SaveSystem.setField(data, s, fi, com);
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
