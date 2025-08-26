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
	public class RandomRotation : ManipulationBase {

		private bool randomX;
		private bool randomY;
		private bool randomZ;

		public override void applyToObject(GameObject go) {
			if (randomX && randomY && randomZ) {
				go.transform.rotation = UnityEngine.Random.rotationUniform;
			}
			else {
				Vector3 angs = go.transform.rotation.eulerAngles;
				if (randomX)
					angs.x = UnityEngine.Random.Range(0F, 360F);
				if (randomY)
					angs.y = UnityEngine.Random.Range(0F, 360F);
				if (randomZ)
					angs.z = UnityEngine.Random.Range(0F, 360F);
				go.transform.rotation = Quaternion.Euler(angs);
			}
		}

		public override void applyToObject(PlacedObject go) {
			if (randomX && randomY && randomZ) {
				go.setRotation(UnityEngine.Random.rotationUniform);
			}
			else {
				Vector3 angs = go.rotation.eulerAngles;
				if (randomX)
					angs.x = UnityEngine.Random.Range(0F, 360F);
				if (randomY)
					angs.y = UnityEngine.Random.Range(0F, 360F);
				if (randomZ)
					angs.z = UnityEngine.Random.Range(0F, 360F);
				go.setRotation(Quaternion.Euler(angs));
			}
		}

		public override void loadFromXML(XmlElement e) {
			randomX = e.getBoolean("x");
			randomY = e.getBoolean("y");
			randomZ = e.getBoolean("z");
		}

		public override void saveToXML(XmlElement e) {
			e.addProperty("x", randomX);
			e.addProperty("y", randomY);
			e.addProperty("z", randomZ);
		}

		public override bool needsReapplication() {
			return false;
		}

	}
}
