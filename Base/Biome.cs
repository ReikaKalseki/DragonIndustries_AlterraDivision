using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{
	public abstract class Biome {
		
		private static readonly Dictionary<string, Biome> biomeList = new Dictionary<string, Biome>();
		
		public readonly string biomeName;
		
		private static float lastMusicControlTime = -1;
		
		protected Biome(string name) {
			biomeName = name;
			biomeList[name] = this;
		}
		
		public abstract bool isInBiome(Vector3 vec);
		public abstract double getDistanceToBiome(Vector3 vec);
		
		public virtual void register() {
			
		}
		
		public abstract VanillaMusic[] getMusicOptions();
		public abstract Vector3 getFogColor(Vector3 orig);
		public abstract float getSunIntensity(float orig);
		public abstract float getFogDensity(float orig);
		
		public static Biome getBiome(string name) {
			return biomeList.ContainsKey(name) ? biomeList[name] : null;
		}
		
		public static void tickMusic(DayNightCycle cyc) {
			float dur = cyc.timePassedAsFloat-lastMusicControlTime;
			if (dur >= 1) {
				lastMusicControlTime = cyc.timePassedAsFloat;
				Player ep = Player.main;
				if (ep) {
					Vector3 pos = ep.transform.position;
					string biome = WaterBiomeManager.main.GetBiome(pos);
					Biome b = getBiome(biome);
					if (b != null) {
						VanillaMusic[] mus = b.getMusicOptions();
						if (mus != null) {
							foreach (VanillaMusic vm in VanillaMusic.getAll()) {
								vm.disable();
							}
							foreach (VanillaMusic vm in mus) {
								vm.enable();
								vm.setToBiome(biome);
							}
						}
					}
					else {
						foreach (VanillaMusic vm in VanillaMusic.getAll()) {
							vm.reset();
						}
					}
				}
			}
		}
	}
}
