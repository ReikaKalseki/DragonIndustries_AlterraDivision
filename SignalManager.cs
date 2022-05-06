using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
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
			return createSignal(text.key, text.name, text.desc, text.pda);
		}
		
		public static ModSignal createSignal(string id, string name, string desc, string prompt) {
			if (signals.ContainsKey(id))
				throw new Exception("Signal ID '"+id+"' already in use!");
			ModSignal sig = new ModSignal(id, name, desc, prompt);
			signals[sig.id] = sig;
			return sig;
		}
		
		public class ModSignal {
			
			public readonly string id;
			public readonly string name;
			public readonly string longName;
			public readonly string pdaPrompt;			
		
			private PingType signalType;
			private GameObject signalHolder;
			private PingInstance signalInstance;
			
			internal ModSignal(string id, string n, string desc, string prompt) {
				this.id = id;
				name = n;
				longName = desc;
				pdaPrompt = prompt;				
			}
			
			public void register(Atlas.Sprite icon) {
				signalType = PingHandler.RegisterNewPingType(id, icon);
			}
			
			//ONLY CALL THIS WHEN THE WORLD IS LOADED
			public void build(string prefab, Vector3 pos) {
				build(SBUtil.createWorldObject(prefab), pos);
			}
			
			public void build(GameObject holder, Vector3 pos, float maxDist = 9999) {
				signalHolder = SBUtil.createWorldObject("a227d6b6-d64c-4bf0-b919-2db02d67d037");
				signalHolder.transform.position = pos;
				LargeWorldEntity lw = signalHolder.EnsureComponent<LargeWorldEntity>();
				lw.cellLevel = LargeWorldEntity.CellLevel.Global;
				
				signalInstance = signalHolder.EnsureComponent<PingInstance>();
				signalInstance.pingType = signalType;
				signalInstance.origin = signalHolder.transform;
				signalInstance.minDist = 18;
				signalInstance.maxDist = maxDist;
				signalInstance.SetLabel(longName);
			}
		
			public void activate() {
				signalInstance.displayPingInManager = true;
				signalInstance.enabled = true;
				signalInstance.SetVisible(true);
			}
			
		}
		
	}
}
