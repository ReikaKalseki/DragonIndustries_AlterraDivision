﻿/*
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
	internal class AddLaserCutterSealed : ManipulationBase {
		
		private int timeToUse;
		private string text;
		
		public override void applyToObject(GameObject go) {
			BulkheadDoor bk = go.GetComponentInChildren<BulkheadDoor>(true);
			if (bk != null)
				go = bk.gameObject;
			Sealed s = go.EnsureComponent<Sealed>();
			s._sealed = true;
			if (!string.IsNullOrEmpty(text)) {
				
			}
			s.maxOpenedAmount = timeToUse;
		}
		
		public override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		public override void loadFromXML(XmlElement e) {
			timeToUse = e.getInt("timeToUse", 100); //100 is the default
			text = e.getProperty("mouseover", true);
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("mouseover", text);
			e.addProperty("timeToUse", timeToUse);
		}
		
	}
}
