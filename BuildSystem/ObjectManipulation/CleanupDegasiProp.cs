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
	internal class CleanupDegasiProp : SwapTexture {

		bool removeLight = false;

		public CleanupDegasiProp() {
			this.init();
		}

		public override void applyToObject(GameObject go) {
			base.applyToObject(go);

			go.removeChildObject("BaseCell/Coral");
			go.removeChildObject("BaseCell/Decals");

			if (removeLight)
				go.removeChildObject("tech_light_deco");
		}

		private void init() {
			this.addSwap("Base_abandoned_Foundation_Platform_01", "Base_Foundation_Platform_01");
			this.addSwap("Base_abandoned_Foundation_Platform_01_normal", "Base_Foundation_Platform_01_normal");
			this.addSwap("Base_abandoned_Foundation_Platform_01_illum", "Base_Foundation_Platform_01_illum");
		}

		public override void loadFromXML(XmlElement e) {
			base.loadFromXML(e);

			bool.TryParse(e.InnerText, out removeLight);

			this.init();
		}

		protected override Texture2D getTexture(string name, string texType) {
			GameObject go = Base.pieces[(int)Base.Piece.Foundation].prefab.gameObject;
			go = go.getChildObject("models/BaseFoundationPlatform");
			return (Texture2D)RenderUtil.extractTexture(go, texType);
		}

	}
}
