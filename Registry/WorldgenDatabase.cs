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
		
		private readonly List<PositionedPrefab> objects = new List<PositionedPrefab>();
		
		public WorldgenDatabase() {
			ownerMod = SNUtil.tryGetModDLL();
		}
		
		public void load() {
			string root = Path.GetDirectoryName(ownerMod.Location);
			string folder = Path.Combine(root, "XML/WorldgenSets");
			string xml = Path.Combine(root, "XML/worldgen.xml");
			objects.Clear();
			if (Directory.Exists(folder)) {
				string[] files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
				SNUtil.log("Loading worldgen maps from folder '"+folder+"': "+string.Join(", ", files), ownerMod);
				foreach (string file in files) {
					loadXML(file);
				}
			}
			else if (File.Exists(xml)) {
				loadXML(xml);
			}
			else {
				SNUtil.log("Worldgen XML not found!", ownerMod);
			}
		}
		
		private void loadXML(string xml) {
			SNUtil.log("Loading worldgen map from XML @ "+xml, ownerMod);
			XmlDocument doc = new XmlDocument();
			doc.Load(xml);
			int loaded = 0;
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
								SNUtil.log("Loaded worldgen prefab "+pfb+" for "+e.format(), ownerMod);
								objects.Add(pfb);
								loaded++;
							}
							else if (ot is WorldGenerator) {
								WorldGenerator gen = (WorldGenerator)ot;
								GenUtil.registerWorldgen(gen);
								SNUtil.log("Loaded worldgenator "+gen+" for "+e.format(), ownerMod);
							}
							else {
								throw new Exception("No worldgen loadable for '"+e.Name+"' "+e.format());
							}	
						}
					}
				}
				catch (Exception ex) {
					SNUtil.log("Could not load element "+e.format(), ownerMod);
					SNUtil.log(ex.ToString(), ownerMod);
				}
			}
			SNUtil.log("Loaded "+loaded+" worldgen elements from file "+xml);
		}
		
		public int getCount(string classID, Vector3? near = null, float dist = -1) {
			int ret = 0;
			foreach (PositionedPrefab pfb in objects) {
				if (pfb.prefabName == classID) {
					if (dist < 0 || near == null || !near.HasValue || Vector3.Distance(near.Value, pfb.position) <= dist)
						ret++;
				}
			}
			return ret;
		}
	}
}
