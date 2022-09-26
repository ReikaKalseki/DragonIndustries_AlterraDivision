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
	public abstract class ObjectTemplate {
		
		private static readonly Dictionary<string, Func<XmlElement, ObjectTemplate>> types = new Dictionary<string, Func<XmlElement, ObjectTemplate>>();
		//private static readonly Dictionary<Type, string> typeIDs = new Dictionary<Type, string>();
		
		protected ObjectTemplate() {
			
		}
		
		public abstract void loadFromXML(XmlElement e);
		public abstract void saveToXML(XmlElement e);
		
		public abstract string getTagName();
		
		public abstract string getID();
		
		public static void registerType(string tagname, Func<XmlElement, ObjectTemplate> ctr) {
			if (types.ContainsKey(tagname))
				throw new Exception("Tag name '"+tagname+"' already in use!");
			SNUtil.log("Registered XML object ref type "+ctr.Method.DeclaringType.Name+"::"+ctr.Method.Name+" as "+tagname, SNUtil.diDLL);
			types[tagname] = ctr;
			//typeIDs[t] = tagname;
		}
		
		public static ObjectTemplate construct(XmlElement e) {
			if (types.Count == 0)
				throw new Exception("No object types registered!");
			string key = e.Name;
			if (!types.ContainsKey(key))
				throw new Exception("Nonexistent object type '"+e.Name+"'! Types: "+string.Join(",", types.Keys));
			if (key == "object" && !e.hasProperty("prefab") && e.hasProperty("type")) //quickfix for back compat
				key = "generator";
			Func<XmlElement, ObjectTemplate> builder = types[key];
			try {
				ObjectTemplate ot = builder(e);
				if (ot == null)
					return null;
				try {
					ot.loadFromXML(e);
				}
				catch (Exception ex) {
					throw new Exception("Unable to load object xml block of type '"+key+"': "+e.OuterXml, ex);
				}
				return ot;
			}
			catch (Exception ex) {
				throw new Exception("Unable to construct object from xml block of type '"+key+"': "+e.OuterXml, ex);
			}
		}
	}
}
