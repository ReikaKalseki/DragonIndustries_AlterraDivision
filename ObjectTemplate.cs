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
		
		public static void registerType(string tagname, Func<XmlElement, ObjectTemplate> ctr) {
			if (types.ContainsKey(tagname))
				throw new Exception("Tag name '"+tagname+"' already in use!");
			types[tagname] = ctr;
			//typeIDs[t] = tagname;
		}
		
		public static ObjectTemplate construct(XmlElement e) {
			if (!types.ContainsKey(e.Name))
				return null;
			Func<XmlElement, ObjectTemplate> builder = types[e.Name];
			ObjectTemplate ot = builder(e);
			ot.loadFromXML(e);
			return ot;
		}
	}
}
