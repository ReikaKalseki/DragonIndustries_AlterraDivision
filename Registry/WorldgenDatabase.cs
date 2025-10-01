using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public class WorldgenDatabase {

		private readonly Assembly ownerMod;

		private readonly List<PositionedPrefab> objects = new List<PositionedPrefab>();
		private readonly List<WorldGenerator> generators = new List<WorldGenerator>();

		public WorldgenDatabase() {
			ownerMod = SNUtil.tryGetModDLL();
		}

		public void load(Predicate<string> loadFile = null) {
			string root = Path.GetDirectoryName(ownerMod.Location);
			string folder = Path.Combine(root, "XML/WorldgenSets");
			objects.Clear();
			if (Directory.Exists(folder)) {
				IEnumerable<string> files = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).Where(this.isLoadableWorldgenXML);
				SNUtil.log("Loading worldgen maps from folder '" + folder + "': " + string.Join(", ", files), ownerMod);
				foreach (string file in files) {
					if (loadFile != null && !loadFile.Invoke(Path.GetFileNameWithoutExtension(file))) {
						SNUtil.log("Skipping worldgen map file @ " + file, ownerMod);
						continue;
					}
					this.loadXML(file);
				}
			}
			else {
				SNUtil.log("Worldgen XMLs not found!", ownerMod);
			}
		}

		private bool isLoadableWorldgenXML(string file) {
			string ext = Path.GetExtension(file);
			if (ext == ".xml")
				return true;
			if (ext == ".gen") {
				bool xml = File.Exists(file.Replace(".gen", ".xml"));
				if (xml)
					SNUtil.log("Skipping packed worldgen XML " + file + " as an unpacked version is present", ownerMod);
				return !xml;
			}
			return false;
		}

		private void loadXML(string file) {
			SNUtil.log("Loading worldgen map from XML file @ " + file, ownerMod);
			string xml;
			if (file.EndsWith(".gen", StringComparison.InvariantCultureIgnoreCase)) {
				byte[] arr;
				using (FileStream inp = File.OpenRead(file)) {
					using (GZipStream zip = new GZipStream(inp, CompressionMode.Decompress, true)) {
						using (MemoryStream mem = new MemoryStream()) {
							zip.CopyTo(mem);
							arr = mem.ToArray();
						}
					}
				}
				arr = arr.Reverse().Skip(8).Where((b, idx) => idx % 2 == 0).ToArray();
				xml = System.Text.Encoding.UTF8.GetString(arr);
			}
			else {
				xml = File.ReadAllText(file);
			}
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
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
								throw new Exception("No worldgen loadable for '" + e.Name + "' " + e.format() + ": NULL");
							}
							else if (ot is CustomPrefab pfb) {
								/*
								if (pfb.isCrate) {
									//GenUtil.spawnItemCrate(pfb.position, pfb.tech, pfb.rotation);
									GenUtil.registerWorldgen(pfb.prefabName, )
										
							    	//CrateFillMap.instance.addValue(gen.position, gen.tech);
								}
								else */
								if (pfb.isDatabox) {
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
								//SNUtil.log("Loaded worldgen prefab "+pfb+" for "+e.format(), ownerMod);
								objects.Add(pfb);
								loaded++;
							}
							else if (ot is WorldGenerator gen) {
								GenUtil.registerWorldgen(gen);
								generators.Add(gen);
								//SNUtil.log("Loaded worldgenator "+gen+" for "+e.format(), ownerMod);
							}
							else {
								throw new Exception("No worldgen loadable for '" + e.Name + "' " + e.format());
							}
						}
					}
				}
				catch (Exception ex) {
					SNUtil.log("Could not load element " + e.format(), ownerMod);
					SNUtil.log(ex.ToString(), ownerMod);
				}
			}
			SNUtil.log("Loaded " + loaded + " worldgen elements from file " + file);
		}

		public int getCount() {
			return objects.Count;
		}

		public int getCount(string classID, Vector3? near = null, float dist = -1) {
			return getPositions(classID, near, dist).Count;
		}

		public int getCount<G>(Vector3? near = null, float dist = -1) where G : WorldGenerator {
			int ret = 0;
			foreach (WorldGenerator pfb in generators) {
				if (pfb is G) {
					if (dist < 0 || near == null || !near.HasValue || Vector3.Distance(near.Value, pfb.position) <= dist)
						ret++;
				}
			}
			return ret;
		}

		public List<PositionedPrefab> getPositions(string classID, Vector3? near = null, float dist = -1) {
			List<PositionedPrefab> ret = new List<PositionedPrefab>();
			foreach (PositionedPrefab pfb in objects) {
				if (pfb.prefabName == classID || classID == "*") {
					if (dist < 0 || near == null || !near.HasValue || Vector3.Distance(near.Value, pfb.position) <= dist)
						ret.Add(pfb);
				}
			}
			if (ret.Count == 0) {
				SNUtil.log("Found no prefabs of ID '" + classID + "' during a search! All prefabs:\n"+objects.toDebugString("\n"));
			}
			return ret;
		}

		public PositionedPrefab getByID(string id) {
			foreach (PositionedPrefab pfb in objects) {
				if (pfb.getXMLID() == id) {
					return pfb;
				}
			}
			return null;
		}
	}
}
