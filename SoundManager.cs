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
		
		static SoundManager() {
			
		}
		
		public static FMODAsset getSound(string id) {
			return sounds.ContainsKey(id) ? sounds[id] : null;
		}
		
		public static FMODAsset registerSound(string id, string path, SoundChannel ch = SoundChannel.Master) {
			if (sounds.ContainsKey(id))
				throw new Exception("Sound ID '"+id+"' is already taken!");
			Sound s = CustomSoundHandler.RegisterCustomSound(id, path, ch);
			sounds[id] = SBUtil.getSound(path, id);
			return sounds[id];
		}
		
	}
}
