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
		
		private static readonly Dictionary<string, SoundData> sounds = new Dictionary<string, SoundData>();

		public static readonly MODE soundMode3D = MODE.DEFAULT | MODE._3D | MODE.ACCURATETIME | MODE._3D_LINEARSQUAREROLLOFF;
		public static readonly MODE soundMode2D = MODE.DEFAULT | MODE._2D | MODE.ACCURATETIME;
		public static readonly MODE soundModeStreaming = soundMode2D | MODE.CREATESTREAM;
		//public static readonly string pdaBus = "bus:/master/all/all voice/AI voice";
		
		static SoundManager() {
			
		}
		
		public static FMODAsset getSound(string id) {
			return sounds.ContainsKey(id) ? sounds[id].asset : buildSound(id);
		}
		
		public static SoundData registerPDASound(Assembly a, string id, string path) {
			return registerSound(a, id, path, soundModeStreaming, null, SoundSystem.voiceBus);
		}
		
		public static SoundData registerSound(Assembly a, string id, string path, MODE m, Action<Sound> processing = null, Bus? b = null) {
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
			if (!File.Exists(path))
				SNUtil.log("Failed to find sound at '"+path+"'!", a);
			Bus bb = b != null && b.HasValue ? b.Value : SoundSystem.masterBus;
			string bp = null;
			bb.getPath(out bp);
			SNUtil.log("Registered custom sound '"+id+"' @ "+path+" on bus "+bp);
			Sound snd = AudioUtils.CreateSound(path, m);
			if (processing != null)
				processing(snd);
			CustomSoundHandler.RegisterCustomSound(id, snd, bb);
			FMODAsset ass = buildSound(id, id, false);
			sounds[id] = new SoundData(id, ass, snd, getLength(snd));
			return sounds[id];
		}
		
		public static void setup3D(Sound s, float maxDist, float minDist = 0) {
			s.set3DMinMaxDistance(minDist, maxDist);
		}
		
		private static float getLength(Sound s) {
			uint len;
			s.getLength(out len, TIMEUNIT.MS);
			return len/1000F;
		}
		
		public static void setLooping(Sound s) {
			s.setLoopPoints(0, TIMEUNIT.MS, (uint)(getLength(s)*1000), TIMEUNIT.MS);
		}
		
		public static void stopSound(string id, bool allowFade = true) {
			if (sounds.ContainsKey(id))
				EventInstance.FMOD_Studio_EventInstance_Stop(sounds[id].internalObject.handle, allowFade ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		
		public static void playSound(string path, bool queue = false) {
			playSoundAt(getSound(path), Player.main.transform.position, queue);
		}
		
		public static void playSoundAt(SoundData snd, Vector3 position, bool queue = false, float distanceFalloff = 16F, float vol = 1) {
			playSoundAt(snd.asset, position, queue, distanceFalloff, vol);
		}
		
		public static void playSoundAt(FMODAsset snd, Vector3 position, bool queue = false, float distanceFalloff = 16F, float vol = 1) {
			if (snd == null) {
				SNUtil.writeToChat("Tried to play null sound @ "+position);
				return;
			}
			if (distanceFalloff > 0) {
				float dist = Vector3.Distance(position, Player.main.transform.position);
				if (dist >= distanceFalloff)
					return;
				else
					vol *= 1-(dist/distanceFalloff);
			}
			if (vol <= 0)
				return;
			//SBUtil.writeToChat("playing sound "+snd.id);
			if (queue)
				PDASounds.queue.PlayQueued(snd);//PDASounds.queue.PlayQueued(path, "subtitle");//PDASounds.queue.PlayQueued(ass);
			else
				FMODUWE.PlayOneShot(snd, position, vol);
		}
		
		public static FMODAsset buildSound(string path, string id = null, bool addBrackets = true) {
			FMODAsset ass = ScriptableObject.CreateInstance<FMODAsset>();
			ass.path = path;
			ass.id = id;
			if (ass.id == null)
				ass.id = VanillaSounds.getID(path);
			if (string.IsNullOrEmpty(ass.id))
				ass.id = path;
			if (addBrackets && ass.id[0] != '{')
				ass.id = "{"+ass.id+"}";
			return ass;
		}
		
		public struct SoundData {
			
			public readonly string id;
			public readonly FMODAsset asset;
			public readonly Sound internalObject;
			public readonly float length;
			
			internal SoundData(string i, FMODAsset a, Sound s, float len) {
				id = i;
				asset = a;
				internalObject = s;
				length = len;
			}
			
		}
		
	}
}
