using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public class XMLLocale {
		
		private static readonly LocaleEntry NOT_FOUND = new LocaleEntry("NOTFOUND", "#NULL", "#NULL", "#NULL");
		
		public readonly string relativePath;
		
		private readonly Dictionary<string, LocaleEntry> entries = new Dictionary<string, LocaleEntry>();
		
		private XmlDocument xmlFile;
		
		public XMLLocale(string path) {
			relativePath = path;
		}
		
		public void load() {
			xmlFile = loadXML();
			foreach (XmlElement e in xmlFile.DocumentElement.ChildNodes) {
				LocaleEntry lc = constructEntry(e);
				entries[lc.key] = lc;
			}
		}
		
		private XmlDocument loadXML() {
			string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = Path.Combine(loc, relativePath);
			XmlDocument doc = new XmlDocument();
			if (File.Exists(path)) {
				doc.Load(path);
			}
			else {
				SBUtil.log("Could not find XML file "+path+"!");
			}
			return doc;
		}
		
		private LocaleEntry constructEntry(XmlElement e) {
			return new LocaleEntry(e);
		}
		
		public LocaleEntry getEntry(string id) {
			return entries.ContainsKey(id) ? entries[id] : NOT_FOUND;
		}
		
		private static string cleanString(string input) {
			string[] parts = input.Trim().Split('\n');
			for (int i = 0; i < parts.Length; i++) {
				parts[i] = parts[i].Trim();
			}
			return string.Join("\n", parts);
		}
		
		public class LocaleEntry {
			
			public readonly string key;
			public readonly string name;
			public readonly string desc;
			public readonly string pda;
			
			internal LocaleEntry(XmlElement e) : this(e.Name, cleanString(e.getProperty("name")), cleanString(e.getProperty("desc")), cleanString(e.getProperty("pda"))) {
				
			}
			
			internal LocaleEntry(string k, string n, string d, string p) {
				key = k;
				name = n;
				desc = d;
				pda = p;
			}
			
		}
		
	}
}
