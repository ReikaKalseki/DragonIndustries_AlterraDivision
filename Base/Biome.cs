﻿using System;
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
		
		private static float nextMusicChoiceTime = -1;
		private static VanillaMusic currentMusic = null;
		private static Biome currentBiome = null;
		
		protected Biome(string name) {
			biomeName = name;
			biomeList[name] = this;
		}
		
		public abstract bool isInBiome(Vector3 vec);
		public abstract double getDistanceToBiome(Vector3 vec);
		
		public virtual void register() {
			
		}
		
		public virtual float getNextMusicSilencePadding() {
			return 0;
		}
		
		public abstract VanillaMusic[] getMusicOptions();
		public abstract Vector3 getFogColor(Vector3 orig);
		public abstract float getSunIntensity(float orig);
		public abstract float getFogDensity(float orig);
		
		public static Biome getBiome(string name) {
			return !string.IsNullOrEmpty(name) && biomeList.ContainsKey(name) ? biomeList[name] : null;
		}
		
		public static void tickMusic(DayNightCycle cyc) {
			if (cyc.timePassedAsFloat >= nextMusicChoiceTime) {
				Player ep = Player.main;
				if (ep) {
					Vector3 pos = ep.transform.position;
					string biome = WaterBiomeManager.main.GetBiome(pos);
					Biome b = getBiome(biome);
					bool changed = b != currentBiome;
					currentBiome = b;
					if (changed) {
						if (currentMusic != null)
							currentMusic.stop();
						nextMusicChoiceTime = cyc.timePassedAsFloat+1;
						return;
					}
					if (b != null) {
						VanillaMusic[] mus = b.getMusicOptions();
						//SNUtil.writeToChat(b.biomeName+" > "+(mus != null ? string.Join(",", (object[])mus) : "null"));
						if (mus != null) {
							foreach (VanillaMusic vm in VanillaMusic.getAll()) {
								vm.stop();
							}
							if (mus.Length > 0) {/*
								foreach (VanillaMusic vm in VanillaMusic.getAll()) {
									vm.disable();
								}
								foreach (VanillaMusic vm in mus) {
									vm.enable();
									vm.setToBiome(biome);
								}*/
								VanillaMusic track = mus[UnityEngine.Random.Range(0, mus.Length)];
								track.play();
								nextMusicChoiceTime = cyc.timePassedAsFloat+track.getLength()+b.getNextMusicSilencePadding();
								currentMusic = track;
								//SNUtil.writeToChat("Selected play of track "+track+" @ "+cyc.timePassedAsFloat+" > "+nextMusicChoiceTime);
							}
						}
					}
					else {/*
						foreach (VanillaMusic vm in VanillaMusic.getAll()) {
							vm.reset();
						}*/
						nextMusicChoiceTime = cyc.timePassedAsFloat+1;
					}
				}
			}
		}
	}
}
