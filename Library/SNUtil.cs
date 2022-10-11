using System;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Collections.Generic;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class SNUtil {
	    
	    private static FMODAsset unlockSound;
	    
	    public static readonly Assembly diDLL = Assembly.GetExecutingAssembly();
	    public static readonly Assembly smlDLL = Assembly.GetAssembly(typeof(ModPrefab));
	    public static readonly Assembly gameDLL = Assembly.GetAssembly(typeof(BoneShark));
	    public static readonly Assembly gameDLL2 = Assembly.GetAssembly(typeof(FMODAsset));
	    
	    private static readonly HashSet<Assembly> assembliesToSkip = new HashSet<Assembly>(){
            diDLL,
            smlDLL,
            gameDLL,
            gameDLL2
        };
		
		//private static readonly Dictionary<string, TechType> moddedTechs = new Dictionary<string, TechType>();
		
		internal static Assembly tryGetModDLL() {
			Assembly di = Assembly.GetExecutingAssembly();
			StackFrame[] sf = new StackTrace().GetFrames();
	        if (sf == null || sf.Length == 0)
	        	return Assembly.GetCallingAssembly();
	        foreach (StackFrame f in sf) {
	        	Assembly a = f.GetMethod().DeclaringType.Assembly;
	        	if (a != di && a != smlDLL && a != gameDLL && a != gameDLL2 && a.Location.Contains("QMods"))
	                return a;
	        }
	        log("Could not find valid mod assembly: "+string.Join("\n", sf.Select<StackFrame, string>(s => s.GetMethod()+" in "+s.GetMethod().DeclaringType)), diDLL);
	        return Assembly.GetCallingAssembly();
		}
		
		public static void log(string s, Assembly a = null, int indent = 0) {
			while (s.Length > 4096) {
				string part = s.Substring(0, 4096);
				log(part, a);
				s = s.Substring(4096);
			}
			string id = (a != null ? a : tryGetModDLL()).GetName().Name.ToUpperInvariant().Replace("PLUGIN_", "");
			if (indent > 0) {
				s = s.PadLeft(s.Length+indent, ' ');
			}
			UnityEngine.Debug.Log(id+": "+s);
		}
		
		public static int getInstallSeed() {
			int seed = diDLL.Location.GetHashCode();
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
			seed ^= unchecked(((long)diDLL.Location.GetHashCode()) << 32);
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
					//log("Tech list: "+string.Join(", ", Enum.GetNames(typeof(TechType))));
					return TechType.None;
				}
			}
			return ret;
		}
		
		public static void writeToChat(string s) {
			while (s.Length >= 4096) {
				string part = s.Substring(0, 4096);
				ErrorMessage.AddMessage(part);
				s = s.Substring(4096);
			}
			ErrorMessage.AddMessage(s);
		}
		
		public static void showPDANotification(string text, string soundPath) {
			PDANotification pda = Player.main.gameObject.AddComponent<PDANotification>();
			pda.enabled = true;
			pda.text = text;
			pda.sound = SoundManager.getSound(soundPath);
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
		
		public static Story.StoryGoal addRadioMessage(string key, string text, string soundPath) {
			return addRadioMessage(key, text, SoundManager.registerPDASound(SNUtil.tryGetModDLL(), "radio_"+key, soundPath));
		}
		
		public static Story.StoryGoal addRadioMessage(string key, string text, FMODAsset sound) {
			return addVOLine(key, Story.GoalType.Radio, text, sound);
		}
		
		public static Story.StoryGoal addVOLine(string key, Story.GoalType type, string text, FMODAsset sound) {
			Story.StoryGoal sg = new Story.StoryGoal(key, type, 0);
			PDALogHandler.AddCustomEntry(key, key, null, sound);
			LanguageHandler.SetLanguageLine(key, text);
			return sg;
		}
		
		public static void teleportPlayer(Player ep, Vector3 to) {
		  	if (ep.currentMountedVehicle != null) {
				ep.currentMountedVehicle.transform.position = to+(ep.currentMountedVehicle.transform.position-ep.transform.position);
		  	}
			ep.transform.position = to;
		}
		
		public static void addPDAEntry(Spawnable pfb, float scanTime, string pageCategory = null, string pageText = null, string pageHeader = null, Action<PDAScanner.EntryData> modify = null) {
			PDAManager.PDAPage page = null;
			if (pageCategory != null && !string.IsNullOrEmpty(pageText)) {
				page = PDAManager.createPage(""+pfb.TechType, pfb.FriendlyName, pageText, pageCategory);
				if (pageHeader != null)
					page.setHeaderImage(TextureManager.getTexture(SNUtil.tryGetModDLL(), "Textures/PDA/"+pageHeader));
				page.register();
			}
			addPDAEntry(pfb, scanTime, page, modify);
		}
		
		public static void addPDAEntry(Spawnable pfb, float scanTime, PDAManager.PDAPage page = null, Action<PDAScanner.EntryData> modify = null) {
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = pfb.TechType;
			e.scanTime = scanTime;
			e.locked = true;
			if (modify != null) {
				modify(e);
			}
			if (page != null) {
				e.encyclopedia = page.id;
				SNUtil.log("Bound scanner entry for "+pfb.FriendlyName+" to "+page.id);
			}
			else {
				SNUtil.log("Scanner entry for "+pfb.FriendlyName+" had no ency page.");
			}
			PDAHandler.AddCustomScannerEntry(e);
		}
		
		public static void triggerTechPopup(TechType tt) {
		   	KnownTech.AnalysisTech at = new KnownTech.AnalysisTech();
		   	at.techType = tt;
		   	at.unlockMessage = "NotificationBlueprintUnlocked";
		   	at.unlockSound = getUnlockSound();
		   	uGUI_PopupNotification.main.OnAnalyze(at, true);
		}
	    
	    public static FMODAsset getUnlockSound() {
	    	if (unlockSound == null) {
	    		foreach (KnownTech.AnalysisTech kt in KnownTech.analysisTech) {
	    			if (kt.unlockMessage == "NotificationBlueprintUnlocked") {
	    				unlockSound = kt.unlockSound;
	    				break;
	    			}
	    		}
	    	}
	    	return unlockSound;
	    }
		
	}
}
