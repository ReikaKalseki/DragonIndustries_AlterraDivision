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
	internal abstract class GlobalManipulation : ManipulationBase {

		private LocalCheck localApply;

		public sealed override void applyToObject(PlacedObject go) {
			this.applyToGlobalObject(go);
			if (localApply != null && localApply.apply(go.obj))
				this.applyToSpecificObject(go);
		}

		public sealed override void applyToObject(GameObject go) {
			this.applyToGlobalObject(go);
			if (localApply != null && localApply.apply(go))
				this.applyToSpecificObject(go);
		}

		internal abstract void applyToSpecificObject(PlacedObject go);
		internal abstract void applyToSpecificObject(GameObject go);
		internal abstract void applyToGlobalObject(PlacedObject go);
		internal abstract void applyToGlobalObject(GameObject go);

		public override void loadFromXML(XmlElement e) {
			List<XmlElement> li = e.getDirectElementsByTagName("local");
			if (li.Count == 1) {
				string typeName = "ReikaKalseki.SeaToSea."+li[0].getProperty("type");
				Type tt = InstructionHandlers.getTypeBySimpleName(typeName);
				if (tt == null)
					throw new Exception("No class found for '" + typeName + "'!");
				localApply = (LocalCheck)Activator.CreateInstance(tt);
				localApply.loadFromXML(li[0]);
			}
		}

		public override void saveToXML(XmlElement e) {
			if (localApply != null) {
				XmlElement e2 = e.OwnerDocument.CreateElement("local");
				localApply.saveToXML(e2);
				e.AppendChild(e2);
			}
		}

	}
}
