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
		private static readonly Dictionary<PingType, string> types = new Dictionary<PingType, string>();
		
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
			SNUtil.log("Constructed signal "+sig);
			return sig;
		}
		
		public class ModSignal {
			
			public readonly string id;
			public readonly string name;
			public readonly string longName;	
			public readonly string radioText;	
			
			public readonly PDAManager.PDAPage pdaEntry;
			
			private StoryGoal radioMessage;
			
			public readonly Assembly ownerMod;
			
			public string storyGate {get; private set;}
			
			public Atlas.Sprite icon {get; private set;}
			
			public Vector3 initialPosition {get; private set;}
			public float maxDistance {get; private set;}
		
			public PingType signalType {get; private set;}
			internal SignalHolder signalHolder {get; private set;}
			
			internal ModSignal(string id, string n, string desc, string pda, string prompt) {
				this.id = "signal_"+id;
				name = n;
				longName = desc;
				radioText = prompt;

				pdaEntry = string.IsNullOrEmpty(pda) ? null : PDAManager.createPage("signal_"+id, longName, pda, "DownloadedData");
				
				ownerMod = SNUtil.tryGetModDLL();
			}
			
			public void addRadioTrigger(string soundPath) {
				addRadioTrigger(SoundManager.registerPDASound(SNUtil.tryGetModDLL(), "radio_"+id, soundPath).asset);
			}
			
			public void addRadioTrigger(FMODAsset sound) {
				setStoryGate("radio_"+id);
				radioMessage = SNUtil.addRadioMessage(storyGate, radioText, sound);
			}
			
			public void setStoryGate(string key) {
				storyGate = key;
			}
			
			public void register(string pfb, Vector3 pos, float maxDist = -1) {
				register(pfb, SpriteManager.Get(SpriteManager.Group.Pings, "Signal"), pos, maxDist);
			}
			
			public void register(string pfb, Atlas.Sprite icon, Vector3 pos, float maxDist = -1) {
				if (icon == null || icon == SpriteManager.defaultSprite)
					throw new Exception("Null icon is not allowed");
				signalType = PingHandler.RegisterNewPingType(id, icon);
				SignalManager.types[signalType] = id;
				LanguageHandler.SetLanguageLine(id, "Signal");
				this.icon = icon;
				
				initialPosition = pos;
				maxDistance = maxDist;
				
				signalHolder = new SignalHolder(pfb, this).registerPrefab();
				
				if (pdaEntry != null)
					pdaEntry.register();
				SNUtil.log("Registered signal "+this);
			}
			
			public void addWorldgen(Quaternion? rot = null) {
				GenUtil.registerWorldgen(signalHolder.ClassID, initialPosition, rot);
			}
			
			public void fireRadio() {
				if (radioMessage != null)
					StoryGoal.Execute(storyGate, radioMessage.goalType);//radioMessage.Trigger();
			}
			
			public bool isRadioFired() {
				return !string.IsNullOrEmpty(storyGate) && StoryGoalManager.main.completedGoals.Contains(storyGate);
			}
		
			public void activate(int delay = 0) {					
				signalHolder.signalInstance.displayPingInManager = true;
				signalHolder.signalInstance.enabled = true;
				signalHolder.signalInstance.SetVisible(true);
				
				signalHolder.activate(delay);				
				
				if (pdaEntry != null)
					pdaEntry.unlock(false);
			}
			
			public void deactivate() { //Will not remove the PDA entry!
				//signalInstance.displayPingInManager = false;
				signalHolder.signalInstance.enabled = false;
				signalHolder.signalInstance.SetVisible(false);
			}
			
			public override string ToString()
			{
				return string.Format("[ModSignal Id={0}, Name={1}, LongName={2}, Radio={3}, PdaEntry={4}, Icon={5}, Mod={6}]", id, name, longName, radioText, pdaEntry, icon, ownerMod);
			}			
		}
		
		internal class SignalInitializer : MonoBehaviour {
		  
			internal PingInstance ping;
			
			internal ModSignal signal;
		  
			private void Start() {
				if (ping == null) {
					//SNUtil.log("Ping was null, refetch");
					ping = gameObject.GetComponentInParent<PingInstance>();
					//SNUtil.log("TT is now "+ping.pingType);
				}
				if (ping != null && signal == null) {
					//SNUtil.log("Signal was null, refetch");
					signal = SignalManager.getSignal(SignalManager.types[ping.pingType]);
				}
				SNUtil.log("Starting signal init of "+signal+" / "+ping, SNUtil.diDLL);
				signal.signalHolder.signalInstance = ping;
				signal.signalHolder.initializer = this;
		    	ping.SetLabel(signal.longName);
		    	
				bool available = signal.storyGate == null || StoryGoalManager.main.completedGoals.Contains(signal.storyGate);
				ping.displayPingInManager = available;
				if (!available)
					ping.SetVisible(false);
			}
			
			internal void triggerFX() {
				SNUtil.log("Firing signal unlock FX: "+signal.id);
				SoundManager.playSound("event:/player/signal_upload"); //"signal location uploaded to PDA"
				Subtitles.main.AddRawLong("Signal location uploaded to PDA.", 0, 6);
				//SNUtil.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click	
			}
		}
		
		internal class SignalHolder : GenUtil.CustomPrefabImpl {
			
			private readonly ModSignal signal;
			
			internal PingInstance signalInstance;
			internal SignalInitializer initializer;
	       
			internal SignalHolder(string template, ModSignal s) : base("signalholder_"+s.id, template) {
				signal = s;
		    }
	
			public override void prepareGameObject(GameObject go, Renderer[] r) {
				LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
				lw.cellLevel = LargeWorldEntity.CellLevel.Global;
				
				go.SetActive(false);
				go.transform.position = signal.initialPosition;
				
				signalInstance = go.EnsureComponent<PingInstance>();
				signalInstance.pingType = signal.signalType;
				signalInstance.colorIndex = 0;
				signalInstance.origin = go.transform;
				signalInstance.minDist = 18;
				signalInstance.maxDist = signal.maxDistance >= 0 ? signal.maxDistance : signalInstance.minDist;
				signalInstance.SetLabel(signal.longName);
				
				bool flag = true;
				if (signal.storyGate != null) {
					flag = StoryGoalManager.main.completedGoals.Contains(signal.storyGate);
				}
				signalInstance.displayPingInManager = flag;
				signalInstance.SetVisible(flag);
				
				initializer = go.EnsureComponent<SignalInitializer>();
				initializer.ping = signalInstance;
				initializer.signal = signal;
				
				SNUtil.log("Initialized GO holder for signal "+signal.id+" ["+flag+"]: "+go+" @ "+go.transform.position, SNUtil.diDLL);
				
				go.SetActive(true);
			}
			
			internal SignalHolder registerPrefab() {
				Patch();
				return this;
			}
			
			internal void activate(int delay = 0) {
				if (delay > 0)
					initializer.Invoke("triggerFX", delay);
				else
					initializer.triggerFX();
			}
		}
		
	}
}
