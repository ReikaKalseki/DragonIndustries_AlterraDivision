using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public class TechTypeMappingConfig<E> {

		private readonly Assembly ownerMod;
		private readonly string filename;
		private readonly ValueParser<E> valueParsing;
		private readonly Action<TechType, E> valueConsumer;

		public TechTypeMappingConfig(string n, ValueParser<E> parser, Action<TechType, E> consumer) {
			ownerMod = SNUtil.tryGetModDLL();
			filename = n;
			valueParsing = parser;
			valueConsumer = consumer;
		}

		public static Action<TechType, E> dictionaryAssign(Dictionary<TechType, E> dict) {
			return (tt, e) => dict[tt] = e;
		}

		public static void loadInline(string n, ValueParser<E> parser, Action<TechType, E> consumer) {
			new TechTypeMappingConfig<E>(n, parser, consumer).load();
		}

		public void load() {
			string path = Path.Combine(Path.GetDirectoryName(ownerMod.Location), Path.Combine("Config", filename+".txt"));
			if (File.Exists(path)) {
				SNUtil.log("Loading TechType mapping file '" + filename + "'.", ownerMod);
				foreach (string raw in File.ReadAllLines(path)) {
					string line = raw.Trim();
					if (line.Length == 0 || line.StartsWith("//", StringComparison.InvariantCultureIgnoreCase))
						continue;
					string[] split = line.Split(new char[]{'='}, StringSplitOptions.RemoveEmptyEntries);
					if (split.Length == 2) {
						TechType find = SNUtil.getTechType(split[0]);
						if (find != TechType.None) {
							if (valueParsing.tryParse(split[1], out E parsed)) {
								valueConsumer.Invoke(find, parsed);
								SNUtil.log("Setting TechType mapping: " + find + " = " + parsed);
							}
							else {
								SNUtil.log("TechType mapping format was invalid; no value parsed for '" + parsed + "'");
							}
						}
						else {
							SNUtil.log("TechType found no matching TechType for '" + split[0] + "'", ownerMod);
						}
					}
					else {
						SNUtil.log("Incorrectly formatted TechType mapping: " + line, ownerMod);
					}
				}
			}
			else {
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				SNUtil.log("TechType mapping file '" + filename + "' not found. Generating default.", ownerMod);
				string[] lines = {
					"//This file contains a list of TechTypes in a key=value format, used to assign them custom mappings.",
					"//TechType names are case-sensitive, and in the case of modded TechTypes equal to their ClassID names.",
					"//To see the TechType used for a given object, you can use the Runtime Editor mod to inspect it and see the TechType on the TechTag, Pickupable, or ResourceTracker component.",
					"//Lines beginning with '//' are comments and will be ignored, as will empty lines.",
					"//There should be one mapping per line, and the value should be in the same format as the following example line:",
					"//SAMPLE_TECH_TYPE="+valueParsing.getSample(),
					"",
				};
				File.WriteAllLines(path, lines);
			}
		}

		public abstract class ValueParser<E> {

			public abstract bool tryParse(string s, out E val);

			public abstract string getSample();

		}

		public class ColorParser : ValueParser<Color> {

			public static readonly ColorParser instance = new ColorParser();

			private ColorParser() {

			}

			public override bool tryParse(string s, out Color val) {
				string[] parts = s.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length >= 3 && int.TryParse(parts[0], out int red) && int.TryParse(parts[1], out int green) && int.TryParse(parts[2], out int blue)) {
					val = new Color(red / 255F, green / 255F, blue / 255F, 1);
					return true;
				}
				else {
					val = Color.white;
					return false;
				}
			}

			public override string getSample() {
				return "255,0,0";
			}

		}

		public class IntParser : ValueParser<int> {

			public static readonly IntParser instance = new IntParser();

			private IntParser() {

			}

			public override bool tryParse(string s, out int val) {
				return !string.IsNullOrEmpty(s) && s[0] == '0' && s[1] == 'x'
					? int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val)
					: int.TryParse(s, out val);
			}

			public override string getSample() {
				return "1";
			}

		}

		public class FloatParser : ValueParser<float> {

			public static readonly FloatParser instance = new FloatParser();

			private FloatParser() {

			}

			public override bool tryParse(string s, out float val) {
				if (!string.IsNullOrEmpty(s) && char.ToUpperInvariant(s[s.Length - 1]) == 'F')
					s = s.Substring(0, s.Length - 1);
				return float.TryParse(s, out val);
			}

			public override string getSample() {
				return "1.0";
			}

		}
	}
}
