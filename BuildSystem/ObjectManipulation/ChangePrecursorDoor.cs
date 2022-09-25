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
	internal class ChangePrecursorDoor : ManipulationBase {
		
		private PrecursorKeyTerminal.PrecursorKeyType targetType; 
		
		static ChangePrecursorDoor() {
			
		}
		
		public override void applyToObject(GameObject go) {
			PrecursorKeyTerminal pk = go.GetComponentInChildren<PrecursorKeyTerminal>();
			if (pk == null) {
				foreach (Component c in go.GetComponentsInChildren<Component>()) {
					SNUtil.log("extra Component "+c+"/"+c.GetType()+" in "+c.gameObject);
				}
			}
			pk.acceptKeyType = targetType;
		}
		
		public override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		public override void loadFromXML(XmlElement e) {
			targetType = (PrecursorKeyTerminal.PrecursorKeyType)Enum.Parse(typeof(PrecursorKeyTerminal.PrecursorKeyType), e.InnerText);
		}
		
		public override void saveToXML(XmlElement e) {
			e.InnerText = Enum.GetName(typeof(PrecursorKeyTerminal.PrecursorKeyType), targetType);
		}
		
	}
}
