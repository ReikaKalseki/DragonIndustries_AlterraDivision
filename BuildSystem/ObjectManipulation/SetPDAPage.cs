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
	[Obsolete]
	internal class SetPDAPage : ManipulationBase {
		
		private string pageID;
		
		public SetPDAPage(string id) : base() {
			pageID = id;
		}
		
		public SetPDAPage() {
			
		}
		
		public override void applyToObject(GameObject go) {
			ObjectUtil.setPDAPage(go.EnsureComponent<StoryHandTarget>(), PDAManager.getPage(pageID));
		}
		
		public override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		public override void loadFromXML(XmlElement e) {
			pageID = e.getProperty("page");
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("page", pageID);
		}
		
	}
}
