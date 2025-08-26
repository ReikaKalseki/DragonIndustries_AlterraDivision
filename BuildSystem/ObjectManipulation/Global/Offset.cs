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
	internal class Offset : GlobalManipulation {

		private Vector3 translate = Vector3.zero;

		internal override void applyToGlobalObject(GameObject go) {
			go.transform.position = go.transform.position + translate;
		}

		internal override void applyToGlobalObject(PlacedObject go) {
			go.move(translate.x, translate.y, translate.z);
		}

		internal override void applyToSpecificObject(PlacedObject go) {
			this.applyToObject(go);
		}

		internal override void applyToSpecificObject(GameObject go) {
			this.applyToObject(go);
		}

		public override void loadFromXML(XmlElement e) {
			base.loadFromXML(e);
			translate.x = (float)e.getFloat("x", double.NaN);
			translate.y = (float)e.getFloat("y", double.NaN);
			translate.z = (float)e.getFloat("z", double.NaN);
		}

		public override void saveToXML(XmlElement e) {
			base.saveToXML(e);
			e.addProperty("x", translate.x);
			e.addProperty("y", translate.y);
			e.addProperty("z", translate.z);
		}

	}
}
