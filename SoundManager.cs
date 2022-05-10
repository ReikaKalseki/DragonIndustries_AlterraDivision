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
		
		private static readonly Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
		
		static SoundManager() {
			
		}
		
		public static Sound? getSound(string id) {
			return sounds.ContainsKey(id) ? sounds[id] : (Sound?)null;
		}
		
		public static Sound registerSound(string id, string path, SoundChannel ch = SoundChannel.Master) {
			if (sounds.ContainsKey(id))
				throw new Exception("Sound ID '"+id+"' is already taken!");
			Sound s = CustomSoundHandler.RegisterCustomSound(id, path, ch);
			sounds[id] = s;
			return s;
		}
		
	}
}
