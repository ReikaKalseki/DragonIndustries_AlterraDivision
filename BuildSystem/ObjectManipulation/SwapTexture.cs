﻿/*
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
using System.Reflection;
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
	public abstract class SwapTexture : ManipulationBase {

		private readonly Dictionary<string, string> swaps = new Dictionary<string, string>();

		protected SwapTexture() {

		}

		protected abstract Texture2D getTexture(string name, string texType);

		protected void addSwap(string from, string to) {
			swaps[from] = to;
		}

		public override void applyToObject(GameObject go) {
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
				foreach (Material m in r.materials) {
					if (m.mainTexture != null) {
						string put = swaps.ContainsKey(m.mainTexture.name) ? swaps[m.mainTexture.name] : null;
						if (put != null) {
							Texture2D tex2 = this.getTexture(put, "main");
							if (tex2 != null)
								m.mainTexture = tex2;
							//else
							//SNUtil.writeToChat("Could not find texture "+put);
						}
					}
					foreach (string n in m.GetTexturePropertyNames()) {
						Texture tex = m.GetTexture(n);
						if (tex is Texture2D) {
							string file = tex.name;
							string put = swaps.ContainsKey(file) ? swaps[file] : null;
							//SNUtil.writeToChat(n+" > "+file+" > "+put);
							if (put != null) {
								Texture2D tex2 = this.getTexture(put, n);
								//SNUtil.writeToChat(">>"+tex2);
								if (tex2 != null)
									m.SetTexture(n, tex2);
								else
									SNUtil.writeToChat("Could not find texture " + put);
							}
						}
					}
				}
				r.UpdateGIMaterials();
			}
		}

		public sealed override void applyToObject(PlacedObject go) {
			this.applyToObject(go.obj);
		}

		public override void loadFromXML(XmlElement e) {
			swaps.Clear();
			foreach (XmlNode n2 in e.ChildNodes) {
				if (n2 is XmlElement e2) {
					swaps[e2.getProperty("from")] = e2.getProperty("to");
				}
			}
		}

		public override void saveToXML(XmlElement e) {
			foreach (KeyValuePair<string, string> kvp in swaps) {
				XmlElement e2 = e.OwnerDocument.CreateElement("swap");
				e2.addProperty("from", kvp.Key);
				e2.addProperty("to", kvp.Value);
				e.AppendChild(e2);
			}
		}

	}
}
