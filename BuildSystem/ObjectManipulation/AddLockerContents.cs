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
	public sealed class AddLockerContents : ManipulationBase {

		private readonly List<Item> items = new List<Item>();

		public override void applyToObject(PlacedObject go) {
			this.applyToObject(go.obj);
		}

		public override void applyToObject(GameObject go) {
			//SBUtil.log("adding items to "+go.transform.position+" from trace "+System.Environment.StackTrace);
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			con.ResetContainer();
			foreach (Item s in items) {
				//SBUtil.writeToChat("Added "+s);
				int amt = UnityEngine.Random.Range(s.amountMin, 1+s.amountMax);
				for (int i = 0; i < amt; i++) {
					GameObject item = ObjectUtil.createWorldObject(s.prefab);
					item.SetActive(false);
					item.refillItem();
					con.container.AddItem(item.GetComponent<Pickupable>());
					//item.destroy(false);
				}
			}
		}

		public override void loadFromXML(XmlElement e) {
			items.Clear();
			foreach (XmlElement e2 in e.ChildNodes) {
				Item i = null;
				string type = e2.getProperty("type");
				string n = e2.getProperty("name");
				switch (type) {
					case "prefab":
						i = new Item(n);
						break;
					case "tech":
						i = new Item(SNUtil.getTechType(n));
						break;
					case "resource":
						i = new Item(VanillaResources.getByName(n.ToUpperInvariant()).prefab);
						break;
				}
				if (i == null)
					throw new Exception("Invalid item ref type '" + type + "'");
				if (e2.hasProperty("min") && e2.hasProperty("max")) {
					i.amountMin = e2.getInt("min", 1);
					i.amountMax = e2.getInt("max", 1);
				}
				else if (e2.hasProperty("amount")) {
					int amt = e2.getInt("amount", 1);
					i.amountMin = amt;
					i.amountMax = amt;
				}
				items.Add(i);
			}
		}

		public override void saveToXML(XmlElement e) {
			foreach (Item s in items) {
				XmlElement e2 = e.OwnerDocument.CreateElement("item");
				e2.addProperty("type", "prefab");
				e2.addProperty("name", s.prefab);
				e2.addProperty("min", s.amountMin);
				e2.addProperty("max", s.amountMax);
				e.AppendChild(e2);
			}
		}

		public override bool needsReapplication() {
			return false;
		}

		private class Item {

			internal readonly string prefab;
			internal int amountMin = 1;
			internal int amountMax = 1;

			internal Item(TechType tech) : this(CraftData.GetClassIdForTechType(tech)) {

			}

			internal Item(string pfb) {
				prefab = pfb;
			}

			public override string ToString() {
				return prefab + " x[" + amountMin + "-" + amountMax + "]";
			}


		}

	}
}
