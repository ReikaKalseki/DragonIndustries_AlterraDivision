/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{		
	internal sealed class SetInfected : ManipulationBase {
		
		private bool infect;
		
		public override void applyToObject(GameObject go) {
			InfectedMixin inf = go.EnsureComponent<InfectedMixin>();
			inf.enabled = true;
			inf.infectedAmount = infect ? 0.8F : 0;
		}
		
		public override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		public override void loadFromXML(XmlElement e) {
			infect = bool.Parse(e.InnerText);
		}
		
		public override void saveToXML(XmlElement e) {
			e.InnerText = infect.ToString();
		}
		
	}
}
