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
	public sealed class ModifyLight : ModifyComponent<Light> {

		private double range = 1;
		private double intensity = 1;
		private Color? color = Color.white;

		public override void modifyComponent(Light c) {
			c.range = (float)range;
			c.intensity = (float)intensity;
			if (color != null && color.HasValue)
				c.color = color.Value;
		}

		public override void loadFromXML(XmlElement e) {
			range = e.getFloat("range", double.NaN);
			intensity = e.getFloat("intensity", double.NaN);
			color = e.getColor("color", true, true);
		}

		public override void saveToXML(XmlElement e) {
			e.addProperty("intensity", intensity);
			e.addProperty("range", range);
			if (color != null && color.HasValue)
				e.addProperty("color", color.Value);
		}

	}
}
