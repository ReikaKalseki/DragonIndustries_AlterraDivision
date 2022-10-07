using System;
using System.IO;	
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using SMLHelper.V2.FMod.Interfaces;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class SoundManager {
		
		private static readonly Dictionary<string, FMODAsset> sounds = new Dictionary<string, FMODAsset>();

		public static readonly MODE soundMode3D = MODE.DEFAULT | MODE._3D | MODE.ACCURATETIME | MODE._3D_LINEARSQUAREROLLOFF;
		public static readonly MODE soundMode2D = MODE.DEFAULT | MODE._2D | MODE.ACCURATETIME;
		public static readonly MODE soundModeStreaming = soundMode2D | MODE.CREATESTREAM;
		//public static readonly string pdaBus = "bus:/master/all/all voice/AI voice";
		
		static SoundManager() {
			
		}
		
		public static FMODAsset getSound(string id) {
			return sounds.ContainsKey(id) ? sounds[id] : null;
		}
		
		public static FMODAsset registerPDASound(Assembly a, string id, string path) {
			return registerSound(a, id, path, soundModeStreaming, null, SoundSystem.voiceBus);
		}
		
		public static FMODAsset registerSound(Assembly a, string id, string path, MODE m, Action<Sound> processing = null, Bus? b = null) {
			if (a == null)
				throw new Exception("You must specify a mod to load the sound for!");
			if (sounds.ContainsKey(id))
				throw new Exception("Sound ID '"+id+"' is already taken!");
			string[] args = path.Split('/');
			List<string> li = new List<string>();
			li.Add(Path.GetDirectoryName(a.Location));
			foreach (string s in args) {
				li.Add(s);
			}
			path = Path.Combine(li.ToArray());
			Bus bb = b != null && b.HasValue ? b.Value : SoundSystem.masterBus;
			string bp = null;
			bb.getPath(out bp);
			SNUtil.log("Registered custom sound '"+id+"' @ "+path+" on bus "+bp);
			Sound snd = AudioUtils.CreateSound(path, m);
			if (processing != null)
				processing(snd);
			CustomSoundHandler.RegisterCustomSound(id, snd, bb);
			sounds[id] = SNUtil.getSound(id, id, false);
			return sounds[id];
		}
		
		public static void setup3D(Sound s, float maxDist, float minDist = 0) {
			s.set3DMinMaxDistance(minDist, maxDist);
		}
		
		public static void setLooping(Sound s) {
			uint len;
			s.getLength(out len, TIMEUNIT.MS);
			s.setLoopPoints(0, TIMEUNIT.MS, len, TIMEUNIT.MS);
		}
		
	}
}
