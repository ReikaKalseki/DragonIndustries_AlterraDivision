﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public class WorldgenDatabase {
		
		private readonly Assembly ownerMod;
		
		public WorldgenDatabase() {
			ownerMod = SNUtil.tryGetModDLL();
		}
		
		public void load() {
			string root = Path.GetDirectoryName(ownerMod.Location);
			string folder = Path.Combine(root, "XML/WorldgenSets");
			string xml = Path.Combine(root, "XML/worldgen.xml");
			if (Directory.Exists(folder)) {
				string[] files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
				SNUtil.log("Loading worldgen maps from folder '"+folder+"': "+string.Join(", ", files), 0, ownerMod);
				foreach (string file in files) {
					loadXML(file);
				}
			}
			else if (File.Exists(xml)) {
				loadXML(xml);
			}
			else {
				SNUtil.log("Worldgen XML not found!", 0, ownerMod);
			}
		}
		
		private void loadXML(string xml) {
			SNUtil.log("Loading worldgen map from XML @ "+xml, 0, ownerMod);
			XmlDocument doc = new XmlDocument();
			doc.Load(xml);
			foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
				try {
					string count = e.GetAttribute("count");
					string ch = e.GetAttribute("chance");
					int amt = string.IsNullOrEmpty(count) ? 1 : int.Parse(count);
					double chance = string.IsNullOrEmpty(ch) ? 1 : double.Parse(ch);
					for (int i = 0; i < amt; i++) {
						if (UnityEngine.Random.Range(0F, 1F) <= chance) {
							ObjectTemplate ot = ObjectTemplate.construct(e);
							if (ot == null) {
								throw new Exception("No worldgen loadable for '"+e.Name+"' "+e.format()+": NULL");
							}
							else if (ot is CustomPrefab) {
								CustomPrefab pfb = (CustomPrefab)ot;
								if (pfb.isCrate) {
									GenUtil.spawnItemCrate(pfb.position, pfb.tech, pfb.rotation);
							    	//CrateFillMap.instance.addValue(gen.position, gen.tech);
								}
								else if (pfb.isDatabox) {
							        GenUtil.spawnDatabox(pfb.position, pfb.tech, pfb.rotation);
							    	//DataboxTypingMap.instance.addValue(gen.position, gen.tech);
								}
								//else if (gen.isFragment) {
							    //    GenUtil.spawnFragment(gen.position, gen.rotation);
							    //	FragmentTypingMap.instance.addValue(gen.position, gen.tech);
								//}
								else {
									GenUtil.registerWorldgen(pfb, pfb.getManipulationsCallable());
								}
								//SNUtil.log("Loaded worldgen prefab "+pfb+" for "+e.format(), 0, ownerMod);
							}
							else if (ot is WorldGenerator) {
								WorldGenerator gen = (WorldGenerator)ot;
								GenUtil.registerWorldgen(gen);
								SNUtil.log("Loaded worldgenator "+gen+" for "+e.format(), 0, ownerMod);
							}
							else {
								throw new Exception("No worldgen loadable for '"+e.Name+"' "+e.format());
							}	
						}
					}
				}
				catch (Exception ex) {
					SNUtil.log("Could not load element "+e.format(), 0, ownerMod);
					SNUtil.log(ex.ToString(), 0, ownerMod);
				}
			}
		}
	}
}