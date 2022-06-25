using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class SNUtil {
		
		private static Assembly modDLL;
		
		private static readonly Int3 batchOffset = new Int3(13, 19, 13);
		private static readonly Int3 batchOffsetM = new Int3(32, 0, 32);
		private static readonly int batchSize = 160;
		
		public static Assembly getModDLL() {
			if (modDLL == null)
				modDLL = Assembly.GetExecutingAssembly();
			return modDLL;
		}
		
		public static int getInstallSeed() {
			int seed = getModDLL().Location.GetHashCode();
			seed &= (~(1 << Environment.ProcessorCount));
			string n = Environment.MachineName;
			if (string.IsNullOrEmpty(n))
				n = Environment.UserName;
			seed ^= (n != null ? n.GetHashCode() : 0);
			seed ^= Environment.OSVersion.VersionString.GetHashCode();
			return seed;
		}
		
		public static int getWorldSeedInt() {
			long seed = getWorldSeed();
			return unchecked((int)((seed & 0xFFFFFFFFL) ^ (seed >> 32)));
		}
		
		public static long getWorldSeed() {
			string path = SaveUtils.GetCurrentSaveDataDir();
			long seed = SaveLoadManager._main.firstStart;
			seed ^= path.GetHashCode();
			seed ^= unchecked(((long)getModDLL().Location.GetHashCode()) << 32);
			return seed;
		}
		
		public static TechType getTechType(string tech) {
			TechType ret;
			if (!Enum.TryParse<TechType>(tech, false, out ret)) {
				if (TechTypeHandler.TryGetModdedTechType(tech, out ret)) {
					return ret;
				}
				else {
					log("TechType '"+tech+"' not found!");
					return TechType.None;
				}
			}
			return ret;
		}
		
		public static void log(string s, int indent = 0) {
			string id = getModDLL().GetName().Name.ToUpperInvariant().Replace("PLUGIN_", "");
			if (indent > 0) {
				s = s.PadLeft(s.Length+indent, ' ');
			}
			Debug.Log(id+": "+s);
		}
		
		public static void writeToChat(string s) {
			ErrorMessage.AddMessage(s);
		}
		
		public static void playSound(string path, bool queue = false) {
			playSoundAt(getSound(path), Player.main.transform.position, queue);
		}
		
		public static void playSoundAt(FMODAsset snd, Vector3 position, bool queue = false) {
			//SBUtil.writeToChat("playing sound "+snd.id);
			if (queue)
				PDASounds.queue.PlayQueued(snd);//PDASounds.queue.PlayQueued(path, "subtitle");//PDASounds.queue.PlayQueued(ass);
			else
				FMODUWE.PlayOneShot(snd, position);
		}
		
		public static FMODAsset getSound(string path, string id = null, bool addBrackets = true) {
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
		
		public static void showPDANotification(string text, string soundPath) {
			PDANotification pda = Player.main.gameObject.AddComponent<PDANotification>();
			pda.enabled = true;
			pda.text = text;
			pda.sound = getSound(soundPath);
			pda.Play();
			UnityEngine.Object.Destroy(pda, 15);
		}
		
		public static void addSelfUnlock(TechType tech, PDAManager.PDAPage page = null) {
			KnownTechHandler.Main.SetAnalysisTechEntry(tech, new List<TechType>(){tech});
			if (page != null) {
				PDAScanner.EntryData e = new PDAScanner.EntryData();
				e.key = tech;
				e.scanTime = 5;
				e.locked = true;
				page.register();
				e.encyclopedia = page.id;
				PDAHandler.AddCustomScannerEntry(e);
			}
		}
		
		public static Story.StoryGoal addRadioMessage(string key, string text, string soundPath, float delay = 0) {
			return addRadioMessage(key, text, SoundManager.registerSound("radio_"+key, soundPath, SoundSystem.voiceBus), delay);
		}
		
		public static Story.StoryGoal addRadioMessage(string key, string text, FMODAsset sound, float delay = 0) {
			return addVOLine(key, Story.GoalType.Radio, text, sound, delay);
		}
		
		public static Story.StoryGoal addVOLine(string key, Story.GoalType type, string text, FMODAsset sound, float delay = 0) {
			Story.StoryGoal sg = new Story.StoryGoal(key, type, delay);
			PDALogHandler.AddCustomEntry(key, key, null, sound);
			LanguageHandler.SetLanguageLine(key, text);
			return sg;
		}
		
		public static void teleportPlayer(Player ep, Vector3 to) {
		  	if (ep.currentMountedVehicle != null) {
		  		ep.currentMountedVehicle.transform.position = to;
		  	}
			ep.transform.position = to;
		}
		
	}
}
