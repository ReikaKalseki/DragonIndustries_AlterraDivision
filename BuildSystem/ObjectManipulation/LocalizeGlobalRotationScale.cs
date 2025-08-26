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
	internal class LocalizeGlobalRotationScale : ManipulationBase {

		public override void applyToObject(GameObject go) {
			//NOOP
		}

		public override void applyToObject(PlacedObject go) {
			this.applyToObject(go.obj);
			go.setRotation(go.obj.transform.rotation);
			go.scale = go.obj.transform.localScale;
		}

		public override void loadFromXML(XmlElement e) {

		}

		public override void saveToXML(XmlElement e) {

		}

		public override bool needsReapplication() {
			return false;
		}

	}
}
