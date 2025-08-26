/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
	internal abstract class GenericMethodCall : ManipulationBase {

		private MethodInfo call;

		public override void applyToObject(GameObject go) {
			call.Invoke(null, new object[] { go });
		}

		public override void applyToObject(PlacedObject go) {
			this.applyToObject(go.obj);
		}

		public override void loadFromXML(XmlElement e) {
			string tn = e.getProperty("typeName");
			string name = e.getProperty("name");
			Type t = InstructionHandlers.getTypeBySimpleName(tn);
			//call = t.GetMethod(name, unchecked((System.Reflection.BindingFlags)0x7fffffff));
			call = t.GetMethod(name, new Type[] { typeof(GameObject) });
		}

		public override void saveToXML(XmlElement e) {
			e.addProperty("typeName", call.DeclaringType.Name);
			e.addProperty("name", call.Name);
		}

	}
}
