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

using Story;

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
			return createSignal(text.key, text.name, text.desc, text.pda, text.getField<string>("radio"));
		}
		
		public static ModSignal createSignal(string id, string name, string desc, string pda, string prompt) {
			if (signals.ContainsKey(id))
				throw new Exception("Signal ID '"+id+"' already in use!");
			ModSignal sig = new ModSignal(id, name, desc, pda, prompt);
			signals[sig.id] = sig;
			SBUtil.log("Constructed signal "+sig);
			return sig;
		}
		
		public class ModSignal {
			
			public readonly string id;
			public readonly string name;
			public readonly string longName;	
			public readonly string radioText;	
			
			public readonly PDAManager.PDAPage pdaEntry;
			
			private StoryGoal radioMessage;
			private string radioStoryKey;
			
			private Atlas.Sprite icon;
		
			private PingType signalType;
			private GameObject signalHolder;
			private PingInstance signalInstance;
			
			internal ModSignal(string id, string n, string desc, string pda, string prompt) {
				this.id = "signal_"+id;
				name = n;
				longName = desc;
				radioText = prompt;

				pdaEntry = string.IsNullOrEmpty(pda) ? null : PDAManager.createPage("signal_"+id, longName, pda, "DownloadedData");
			}
			
			public void addRadioTrigger(string soundPath, float delay = 0) {
				addRadioTrigger(SoundManager.registerSound("radio_"+id, soundPath, SoundSystem.voiceBus), delay);
			}
			
			public void addRadioTrigger(FMODAsset sound, float delay = 0) {
				radioStoryKey = "radio_"+id;
				radioMessage = SBUtil.addRadioMessage(radioStoryKey, radioText, sound, delay);
			}
			
			public string getRadioStoryKey() {
				return radioStoryKey;
			}
			
			public void register() {
				register(SpriteManager.Get(SpriteManager.Group.Pings, "Signal"));
			}
			
			public void register(Atlas.Sprite icon) {
				if (icon == null || icon == SpriteManager.defaultSprite)
					throw new Exception("Null icon is not allowed");
				signalType = PingHandler.RegisterNewPingType(id, icon);
				LanguageHandler.SetLanguageLine(id, "Signal");
				this.icon = icon;
				
				if (pdaEntry != null)
					pdaEntry.register();
				SBUtil.log("Registered signal "+this);
			}
			
			//ONLY CALL THIS WHEN THE WORLD IS LOADED
			public void build(string prefab, Vector3 pos) {
				if (signalHolder != null)
					return;
				build(SBUtil.createWorldObject(prefab), pos);
			}
			
			public void build(GameObject holder, Vector3 pos, float maxDist = -1) {
				if (signalHolder == null) {
					signalHolder = holder;
					signalHolder.SetActive(false);
					signalHolder.transform.position = pos;
					LargeWorldEntity lw = signalHolder.EnsureComponent<LargeWorldEntity>();
					lw.cellLevel = LargeWorldEntity.CellLevel.Global;
					
					signalInstance = signalHolder.EnsureComponent<PingInstance>();
					signalInstance.pingType = signalType;
					signalInstance.colorIndex = 0;
					signalInstance.origin = signalHolder.transform;
					signalInstance.minDist = 18;
					signalInstance.maxDist = maxDist >= 0 ? maxDist : signalInstance.minDist;
					signalInstance.SetLabel(longName);
					signalInstance.displayPingInManager = false;
					signalInstance.SetVisible(false);
					signalHolder.SetActive(true);
				}
			}
			
			public void fireRadio() {
				if (radioMessage != null)
					StoryGoal.Execute(radioStoryKey, radioMessage.goalType);//radioMessage.Trigger();
			}
		
			public void activate(int delay = 0) {					
				signalInstance.displayPingInManager = true;
				signalInstance.enabled = true;
				signalInstance.SetVisible(true);
				
				SignalInitializer init = signalHolder.EnsureComponent<SignalInitializer>();
				init.ping = signalInstance;
				init.description = longName;
				if (delay > 0)
					init.Invoke("triggerFX", delay);
				else
					init.triggerFX();
				
				
				if (pdaEntry != null)
					pdaEntry.unlock(false);
			}
			
			public void deactivate() { //Will not remove the PDA entry!
				//signalInstance.displayPingInManager = false;
				signalInstance.enabled = false;
				signalInstance.SetVisible(false);
			}
			
			public override string ToString()
			{
				return string.Format("[ModSignal Id={0}, Name={1}, LongName={2}, Radio={3}, PdaEntry={4}, Icon={5}]", id, name, longName, radioText, pdaEntry, icon);
			}
 
			
		}
		
		private class SignalInitializer : MonoBehaviour {
			
			internal string description;
		  
			public PingInstance ping;
		  
			private void Start() {
		    	ping.SetLabel(description);
			}
			
			internal void triggerFX() {
				SBUtil.playSound("event:/player/signal_upload"); //"signal uploaded to PDA"
				SBUtil.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click	
			}
		}
		
	}
}
