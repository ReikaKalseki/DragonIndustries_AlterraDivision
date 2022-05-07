using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class SignalManager {
		
		private static readonly Dictionary<string, ModSignal> signals = new Dictionary<string, ModSignal>();
		
		static SignalManager() {
			
		}
		
		public static ModSignal getSignal(string id) {
			return signals.ContainsKey(id) ? signals[id] : null;
		}
		
		public static ModSignal createSignal(XMLLocale.LocaleEntry text) {	
			return createSignal(text.key, text.name, text.desc, text.pda, text.getField<string>("prompt"));
		}
		
		public static ModSignal createSignal(string id, string name, string desc, string pda, string prompt) {
			if (signals.ContainsKey(id))
				throw new Exception("Signal ID '"+id+"' already in use!");
			ModSignal sig = new ModSignal(id, name, desc, pda, prompt);
			signals[sig.id] = sig;
			SBUtil.log("Registered signal "+sig.id);
			return sig;
		}
		
		public class ModSignal {
			
			public readonly string id;
			public readonly string name;
			public readonly string longName;
			public readonly string pdaEntry;	
			public readonly string pdaPrompt;		
		
			private PingType signalType;
			private GameObject signalHolder;
			private PingInstance signalInstance;
			
			internal ModSignal(string id, string n, string desc, string pda, string prompt) {
				this.id = id;
				name = n;
				longName = desc;
				pdaEntry = pda;
				pdaPrompt = prompt;				
			}
			
			public void register(Atlas.Sprite icon, params string[] pdaCategories) {
				signalType = PingHandler.RegisterNewPingType(id, icon);
				
				PDAEncyclopedia.EntryData dat = new PDAEncyclopedia.EntryData();
				dat.audio = null;
				dat.key = "signal_"+id;
				dat.timeCapsule = false;
				dat.unlocked = true;
				List<string> li = pdaCategories.ToList();
				li.Insert(0, "DownloadedData");
				dat.nodes = li.ToArray();
				dat.path = string.Join("/", li);
				PDAEncyclopediaHandler.AddCustomEntry(dat);
			}
			
			//ONLY CALL THIS WHEN THE WORLD IS LOADED
			public void build(string prefab, Vector3 pos) {
				build(SBUtil.createWorldObject(prefab), pos);
			}
			
			public void build(GameObject holder, Vector3 pos, float maxDist = 9999) {
				signalHolder = holder;
				signalHolder.transform.position = pos;
				LargeWorldEntity lw = signalHolder.EnsureComponent<LargeWorldEntity>();
				lw.cellLevel = LargeWorldEntity.CellLevel.Global;
				
				signalInstance = signalHolder.EnsureComponent<PingInstance>();
				signalInstance.pingType = signalType;
				signalInstance.origin = signalHolder.transform;
				signalInstance.minDist = 18;
				signalInstance.maxDist = maxDist;
				signalInstance.SetLabel(longName);
				signalInstance.displayPingInManager = false;
				signalInstance.SetVisible(false);
			}
		
			public void activate() {
				signalInstance.displayPingInManager = true;
				signalInstance.enabled = true;
				signalInstance.SetVisible(true);
				
				SignalInitializer init = signalHolder.EnsureComponent<SignalInitializer>();
				init.ping = signalInstance;
				init.description = longName;
				
				PDAEncyclopedia.Add("signal_"+id, true);
			}
			
		}
		
		private class SignalInitializer : MonoBehaviour {
			
			internal string description;
		  
			public PingInstance ping;
		  
			private void Start() {
		    	ping.SetLabel(description);
			}
		}
		
	}
}
