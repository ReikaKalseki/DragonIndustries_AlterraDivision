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
			SBUtil.log("Registered signal "+sig);
			return sig;
		}
		
		public class ModSignal {
			
			public readonly string id;
			public readonly string name;
			public readonly string longName;	
			public readonly string pdaPrompt;	
			
			public readonly PDAManager.PDAPage pdaEntry;	
		
			private PingType signalType;
			private GameObject signalHolder;
			private PingInstance signalInstance;
			
			internal ModSignal(string id, string n, string desc, string pda, string prompt) {
				this.id = id;
				name = n;
				longName = desc;
				pdaPrompt = prompt;

				pdaEntry = PDAManager.createPage("signal_"+id, longName, pda, "DownloadedData");
			}
			
			public void register() {
				register(SpriteManager.Get(SpriteManager.Group.Pings, "Signal"));
			}
			
			public void register(Atlas.Sprite icon) {
				signalType = PingHandler.RegisterNewPingType(id, icon);
				LanguageHandler.SetLanguageLine(id, "Signal");
				
				pdaEntry.register();
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
				
				SBUtil.playSound("event:/player/signal_upload"); //"signal uploaded to PDA"
				SBUtil.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click
				
				pdaEntry.unlock(false);
			}
			
			public void deactivate() { //Will not remove the PDA entry!
				signalInstance.displayPingInManager = false;
				signalInstance.enabled = false;
				signalInstance.SetVisible(false);
			}
			
			public override string ToString()
			{
				return string.Format("[ModSignal Id={0}, Name={1}, LongName={2}, PdaPrompt={3}, PdaEntry={4}]", id, name, longName, pdaPrompt, pdaEntry);
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
