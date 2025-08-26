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
	internal sealed class AddChild : ManipulationBase {

		private CustomPrefab prefab;
		private string objName;

		public override void applyToObject(GameObject go) {
			if (!string.IsNullOrEmpty(objName)) {
				if (go.getChildObject(objName) != null)
					return;
			}
			GameObject add = ObjectUtil.createWorldObject(prefab.prefabName);
			add.transform.parent = go.transform;
			add.transform.localPosition = prefab.position;
			add.transform.localRotation = prefab.rotation;
			add.transform.localScale = prefab.scale;
			if (!string.IsNullOrEmpty(objName))
				add.name = objName;
			foreach (ManipulationBase mb in prefab.manipulations) {
				mb.applyToObject(add);
			}
		}

		public override void applyToObject(PlacedObject go) {
			this.applyToObject(go.obj);
		}

		public override void loadFromXML(XmlElement e) {
			objName = e.getProperty("name", true);
			prefab = new CustomPrefab(e.getProperty("prefab"));
			prefab.loadFromXML(e);
		}

		public override void saveToXML(XmlElement e) {
			e.addProperty("prefab", prefab.prefabName);
			prefab.saveToXML(e);
			if (!string.IsNullOrEmpty(objName))
				e.addProperty("name", objName);
		}

		public override bool needsReapplication() {
			foreach (ManipulationBase mb in prefab.manipulations) {
				if (mb.needsReapplication())
					return true;
			}
			return false;
		}

	}
}
