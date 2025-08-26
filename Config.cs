using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {
	public class Config<E> {
		private readonly string filename;
		private readonly Dictionary<string, float> data = new Dictionary<string, float>();
		private readonly Dictionary<string, string> dataString = new Dictionary<string, string>();
		private readonly Dictionary<E, ConfigEntry> entryCache = new Dictionary<E, ConfigEntry>();

		private readonly Assembly owner;

		private bool loaded = false;

		private readonly Dictionary<string, Func<float, float>> overrides = new Dictionary<string, Func<float, float>>();
		private readonly Dictionary<string, Func<string, string>> overridesString = new Dictionary<string, Func<string, string>>();

		public Config(Assembly owner) {
			this.owner = owner;
			filename = /*Environment.UserName+"_"+*/owner.GetName().Name + "_Config.xml";
			this.populateDefaults();
		}

		public void attachOverride(E key, bool val) {
			this.attachOverride(key, val ? 1 : 0);
		}

		public void attachOverride(E key, float val) {
			this.attachOverride(key, f => val);
		}

		public void attachOverride(E key, Func<bool, bool> val) {
			this.attachOverride(key, f => val(f > 0.001) ? 1 : 0);
		}

		public void attachOverride(E key, Func<float, float> val) {
			string k = this.getKey(key);
			overrides[k] = val;
		}

		public void attachOverride(E key, Func<string, string> val) {
			string k = this.getKey(key);
			overridesString[k] = val;
		}
		/*
		private void applyOverrides() {
			foreach (KeyValuePair<string, Func<float>> kvp in overrides) {
				data[kvp.Key] = kvp.Value(data[kvp.Key]);
			}
			foreach (KeyValuePair<string, Func<string>> kvp in overridesString) {
				dataString[kvp.Key] = kvp.Value(dataString[kvp.Key]);
			}
		}
		*/
		private void populateDefaults() {
			foreach (E key in Enum.GetValues(typeof(E))) {
				string name = Enum.GetName(typeof(E), key);
				ConfigEntry e = this.getEntry(key);
				e.enumIndex = name;
				data[name] = e.defaultValue;
				//SNUtil.log("Initializing config entry "+name+" to "+e.formatValue(e.defaultValue)+" hash = "+RuntimeHelpers.GetHashCode(e), owner);
			}
		}

		public void load(bool force = false) {
			if (loaded && !force)
				return;
			string folder = Path.Combine(Path.GetDirectoryName(owner.Location), "Config");
			Directory.CreateDirectory(folder);
			string path = Path.Combine(folder, filename);
			if (File.Exists(path)) {
				SNUtil.log("Loading config file at " + path, owner);
				try {
					XmlDocument doc = new XmlDocument();
					doc.Load(path);
					XmlElement root = (XmlElement)doc.GetElementsByTagName("Settings")[0];
					HashSet<string> missing = new HashSet<string>(data.Keys);
					foreach (XmlNode e in root.ChildNodes) {
						if (!(e is XmlElement))
							continue;
						string name = e.Name;
						try {
							XmlElement val = (XmlElement)(e as XmlElement).GetElementsByTagName("value")[0];
							E key = (E)Enum.Parse(typeof(E), name);
							ConfigEntry entry = this.getEntry(key);
							float raw = entry.parse(val.InnerText);
							float get = raw;
							if (!entry.validate(ref get)) {
								SNUtil.log("Chosen " + name + " value (" + raw + ") was out of bounds, clamped to " + get, owner);
							}
							data[name] = get;
							dataString[name] = val.InnerText;
							missing.Remove(name);
						}
						catch (Exception ex) {
							SNUtil.log("Config entry " + name + " failed to load: " + ex.ToString(), owner);
						}
					}
					string vals = string.Join(";", data.Select(x => x.Key + "=" + x.Value).ToArray());
					SNUtil.log("Config successfully loaded: " + vals, owner);
					if (missing.Count > 0) {
						string keys = string.Join(";", missing.ToArray());
						SNUtil.log("Note: " + missing.Count + " entries were missing from the config and so stayed the default values.", owner);
						SNUtil.log("Missing keys: " + keys, owner);
						//SNUtil.log("It is recommended that you regenerate your config by renaming your current config file, letting a new one generate," +
						// "then copying your changes into the new one.", owner);
						SNUtil.log("Your config will be regenerated (keeping your changes) to add them to the file.", owner);
						File.Delete(path);
						this.generateFile(path, e => this.getFloat(this.getEnum(e)));
					}
				}
				catch (Exception ex) {
					SNUtil.log("Config failed to load: " + ex.ToString(), owner);
				}
			}
			else {
				SNUtil.log("Config file does not exist at " + path + "; generating.", owner);
				this.generateFile(path, e => e.defaultValue);
			}
			//applyOverrides();
			loaded = true;
		}

		private void generateFile(string path, Func<ConfigEntry, float> valGetter) {
			try {
				XmlDocument doc = new XmlDocument();
				XmlElement root = doc.CreateElement("Settings");
				doc.AppendChild(root);
				foreach (E key in Enum.GetValues(typeof(E))) {
					try {
						this.createNode(doc, root, key, valGetter);
					}
					catch (Exception e) {
						SNUtil.log("Could not generate XML node for " + key + ": " + e.ToString(), owner);
					}
				}
				doc.Save(path);
				SNUtil.log("Config successfully generated at " + path, owner);
			}
			catch (Exception ex) {
				SNUtil.log("Config failed to generate: " + ex.ToString(), owner);
			}
		}

		private void createNode(XmlDocument doc, XmlElement root, E key, Func<ConfigEntry, float> valGetter) {
			ConfigEntry e = this.getEntry(key);
			XmlElement node = doc.CreateElement(Enum.GetName(typeof(E), key));

			XmlComment com = doc.CreateComment(e.desc);

			XmlElement val = doc.CreateElement("value");
			float amt = valGetter(e);
			//SNUtil.log(valGetter+": Parsed value "+amt+" for "+key, owner);
			val.InnerText = e.formatValue(amt);
			node.AppendChild(val);

			XmlElement def = doc.CreateElement("defaultValue");
			def.InnerText = e.formatValue(e.defaultValue);
			node.AppendChild(def);
			if (!float.IsNaN(e.vanillaValue)) {
				XmlElement van = doc.CreateElement("vanillaValue");
				van.InnerText = e.formatValue(e.vanillaValue);
				node.AppendChild(van);
			}

			//XmlElement desc = doc.CreateElement("description");
			//desc.InnerText = e.desc;
			//node.AppendChild(desc);

			if (e.type != typeof(bool)) {
				XmlElement min = doc.CreateElement("minimumValue");
				min.InnerText = e.formatValue(e.minValue);
				node.AppendChild(min);
				XmlElement max = doc.CreateElement("maximumValue");
				max.InnerText = e.formatValue(e.maxValue);
				node.AppendChild(max);
			}
			root.AppendChild(com);
			root.AppendChild(node);
		}

		private float getValue(string key) {
			float ret = data.ContainsKey(key) ? data[key] : 0;
			if (overrides.ContainsKey(key))
				ret = overrides[key](ret);
			return ret;
		}

		private string getStringValue(string key) {
			string ret = dataString.ContainsKey(key) ? dataString[key] : null;
			if (overridesString.ContainsKey(key))
				ret = overridesString[key](ret);
			return ret;
		}

		public bool getBoolean(E key) {
			float ret = this.getFloat(key);
			return ret > 0.001;
		}

		public int getInt(E key) {
			float ret = this.getFloat(key);
			return (int)Math.Floor(ret);
		}

		public float getFloat(E key) {
			return this.getValue(this.getKey(key));
		}

		public string getString(E key) {
			return this.getStringValue(this.getKey(key));
		}

		private string getKey(E key) {
			return Enum.GetName(typeof(E), key);
		}
		/*
		public void setValue(E key, float val) {
			data[getKey(key)] = val;
		}
		
		public void setValue(E key, bool val) {
			setValue(key, 0);
		}
		
		public void setValue(E key, string val) {
			dataString[getKey(key)] = val;
		}
		*/
		public ConfigEntry getEntry(E key) {
			if (!entryCache.ContainsKey(key)) {
				entryCache[key] = this.lookupEntry(key);
			}
			return entryCache[key];
		}

		private ConfigEntry lookupEntry(E key) {
			MemberInfo info = typeof(E).GetField(Enum.GetName(typeof(E), key));
			return (ConfigEntry)Attribute.GetCustomAttribute(info, typeof(ConfigEntry));
		}

		private E getEnum(ConfigEntry e) {
			return e.enumIndex == null
				? throw new Exception("Missing index - could not lookup matching enum for " + e + " hash = " + RuntimeHelpers.GetHashCode(e))
				: (E)Enum.Parse(typeof(E), e.enumIndex);
		}
	}
}
