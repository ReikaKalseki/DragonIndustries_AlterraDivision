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
	public class Scatter : ManipulationBase {

		private Vector3 range = Vector3.zero;

		public override void applyToObject(GameObject go) {
			float dx = UnityEngine.Random.Range(-range.x, range.x);
			float dy = UnityEngine.Random.Range(-range.y, range.y);
			float dz = UnityEngine.Random.Range(-range.z, range.z);
			go.transform.position = go.transform.position + new Vector3(dx, dy, dz);
		}

		public override void applyToObject(PlacedObject go) {
			double dx = UnityEngine.Random.Range(-range.x, range.x);
			double dy = UnityEngine.Random.Range(-range.y, range.y);
			double dz = UnityEngine.Random.Range(-range.z, range.z);
			go.move(dx, dy, dz);
		}

		public override void loadFromXML(XmlElement e) {
			range = ((XmlElement)e.ParentNode).getVector("Scatter").Value;
		}

		public override void saveToXML(XmlElement e) {
			e.addProperty("x", range.x);
			e.addProperty("y", range.y);
			e.addProperty("z", range.z);
		}

		public override bool needsReapplication() {
			return false;
		}

	}
}
