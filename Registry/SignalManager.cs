using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public static class SignalManager {

		private static readonly Dictionary<string, ModSignal> signals = new Dictionary<string, ModSignal>();
		private static readonly Dictionary<PingType, string> types = new Dictionary<PingType, string>();

		static SignalManager() {

		}

		public static ModSignal getSignal(string id) {
			return signals.ContainsKey(id) ? signals[id] : null;
		}

		public static ModSignal createSignal(XMLLocale.LocaleEntry text) {
			return createSignal(text.key, text.name, text.desc, text.pda, text.getString("radio"));
		}

		public static ModSignal createSignal(string id, string name, string desc, string pda, string prompt) {
			if (signals.ContainsKey(id))
				throw new Exception("Signal ID '" + id + "' already in use!");
			ModSignal sig = new ModSignal(id, name, desc, pda, prompt);
			signals[sig.id] = sig;
			SNUtil.log("Constructed signal " + sig);
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

			public string storyGate { get; private set; }

			public Atlas.Sprite icon { get; private set; }

			public Vector3 initialPosition { get; private set; }
			public float maxDistance { get; private set; }

			public PingType signalType { get; private set; }
			internal SignalHolder signalHolder { get; private set; }
			internal GenericSignalHolder genericSignalHolder { get; private set; }

			internal PingInstance signalInstance;
			internal SignalInitializer initializer;

			internal ModSignal(string id, string n, string desc, string pda, string prompt) {
				this.id = "signal_" + id;
				name = n;
				longName = desc;
				radioText = prompt;

				pdaEntry = string.IsNullOrEmpty(pda) ? null : PDAManager.createPage("signal_" + id, longName, pda, "DownloadedData");

				ownerMod = SNUtil.tryGetModDLL();
			}

			public ModSignal addRadioTrigger(string soundPath) {
				return this.addRadioTrigger(SoundManager.registerPDASound(SNUtil.tryGetModDLL(), "radio_" + id, soundPath).asset);
			}

			public ModSignal addRadioTrigger(FMODAsset sound) {
				this.setStoryGate("radio_" + id);
				radioMessage = SNUtil.addRadioMessage(storyGate, radioText, sound);
				return this;
			}

			public ModSignal setStoryGate(string key) {
				storyGate = key;
				return this;
			}

			public ModSignal move(Vector3 pos) {
				initialPosition = pos;
				return this;
			}

			public void register(string pfb, Vector3 pos, float maxDist = -1) {
				this.register(pfb, SpriteManager.Get(SpriteManager.Group.Pings, "Signal"), pos, maxDist);
			}

			public void register(string pfb, Atlas.Sprite icon, Vector3 pos, float maxDist = -1) {
				if (icon == null || icon == SpriteManager.defaultSprite)
					throw new Exception("Null icon is not allowed");
				signalType = PingHandler.RegisterNewPingType(id, icon);
				SignalManager.types[signalType] = id;
				CustomLocaleKeyDatabase.registerKey(id, "Signal");
				this.icon = icon;

				initialPosition = pos;
				maxDistance = maxDist;

				signalHolder = new SignalHolder(pfb, this).registerPrefab();
				genericSignalHolder = new GenericSignalHolder(this);
				genericSignalHolder.Patch();

				if (pdaEntry != null)
					pdaEntry.register();
				SNUtil.log("Registered signal " + this);
			}

			public GameObject spawnGenericSignalHolder(Vector3 pos) {
				GameObject go = genericSignalHolder.GetGameObject();
				go.SetActive(true);

				go.transform.position = pos;
				PingInstance pi = go.GetComponent<PingInstance>();
				pi.origin = go.transform;
				pi.displayPingInManager = true;
				pi.SetVisible(true);
				return go;
			}

			public void addWorldgen(Quaternion? rot = null) {
				GenUtil.registerWorldgen(signalHolder.ClassID, initialPosition, rot);
			}

			public PingInstance attachToObject(GameObject go) {
				LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
				lw.cellLevel = LargeWorldEntity.CellLevel.Global;

				go.SetActive(false);
				go.transform.position = initialPosition;

				signalInstance = go.EnsureComponent<PingInstance>();
				signalInstance.pingType = signalType;
				signalInstance.colorIndex = 0;
				signalInstance.origin = go.transform;
				signalInstance.minDist = 18;
				signalInstance.maxDist = maxDistance >= 0 ? maxDistance : signalInstance.minDist;
				signalInstance.SetLabel(longName);

				bool flag = true;
				if (storyGate != null)
					flag = StoryGoalManager.main.completedGoals.Contains(storyGate);

				signalInstance.displayPingInManager = flag;
				signalInstance.SetVisible(flag);

				initializer = go.EnsureComponent<SignalInitializer>();
				initializer.ping = signalInstance;
				initializer.signal = this;

				SNUtil.log("Initialized GO holder for signal " + id + " [" + flag + "]: " + go + " @ " + go.transform.position, SNUtil.diDLL);

				go.SetActive(true);

				return signalInstance;
			}

			public void fireRadio() {
				if (radioMessage != null)
					StoryGoal.Execute(storyGate, radioMessage.goalType);//radioMessage.Trigger();
			}

			public bool isRadioFired() {
				return !string.IsNullOrEmpty(storyGate) && StoryGoalManager.main.completedGoals.Contains(storyGate);
			}

			public void activate(int delay = 0) {
				if (!signalInstance) {
					SNUtil.log("Cannot disable mod signal " + this + " because it has no object/instance!", ownerMod);
					return;
				}
				bool already = signalInstance.enabled;
				signalInstance.displayPingInManager = true;
				signalInstance.enabled = true;
				signalInstance.SetVisible(true);

				if (already)
					return;

				if (delay > 0)
					initializer.Invoke("triggerFX", delay);
				else
					initializer.triggerFX();

				if (pdaEntry != null)
					pdaEntry.unlock(false);
			}

			public void deactivate() { //Will not remove the PDA entry!
				if (!signalInstance)
					return;
				//signalInstance.displayPingInManager = false;
				signalInstance.enabled = false;
				signalInstance.SetVisible(false);
			}

			public bool isActive() {
				return signalInstance && signalInstance.isActiveAndEnabled;
			}

			public override string ToString() {
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
				SNUtil.log("Starting signal init of " + signal + " / " + ping, SNUtil.diDLL);
				signal.signalInstance = ping;
				signal.initializer = this;
				ping.SetLabel(signal.longName);

				bool available = signal.storyGate == null || StoryGoalManager.main.completedGoals.Contains(signal.storyGate);
				ping.displayPingInManager = available;
				if (!available)
					ping.SetVisible(false);
			}

			internal void triggerFX() {
				SNUtil.log("Firing signal unlock FX: " + signal.id);
				SoundManager.playSound("event:/player/signal_upload"); //"signal location uploaded to PDA"
				Subtitles.main.AddRawLong("Signal location uploaded to PDA.", 0, 6);
				//SNUtil.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click	
			}
		}

		internal class SignalHolder : GenUtil.CustomPrefabImpl {

			private readonly ModSignal signal;

			internal SignalHolder(string template, ModSignal s) : base("signalholder_" + s.id, template) {
				signal = s;
			}

			public override void prepareGameObject(GameObject go, Renderer[] r) {
				signal.attachToObject(go);
			}

			internal SignalHolder registerPrefab() {
				this.Patch();
				return this;
			}
		}

		internal class GenericSignalHolder : Spawnable {

			private readonly ModSignal signal;

			internal GenericSignalHolder(ModSignal s) : base("genericsignalholder_" + s.id, "", "") {
				signal = s;
			}

			public override GameObject GetGameObject() {
				GameObject go = new GameObject("Signal_"+signal.id+"(Clone)");
				go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				go.EnsureComponent<TechTag>().type = TechType;
				go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

				PingInstance ping = go.EnsureComponent<PingInstance>();
				ping.pingType = signal.signalType;//PingType.Beacon;
				PingHandler.RegisterNewPingType("", null);
				//ping.displayPingInManager = false;
				ping.colorIndex = 0;
				ping.origin = go.transform;
				ping.minDist = 18f;
				ping.maxDist = 1;

				GenericSignalInitializer si = go.EnsureComponent<GenericSignalInitializer>();
				si.ping = ping;
				si.signal = signal;

				return go;
			}
		}

		internal class GenericSignalInitializer : MonoBehaviour {

			internal PingInstance ping;

			internal ModSignal signal;

			private void Start() {
				if (ping == null) {
					//SNUtil.log("Ping was null, refetch");
					ping = gameObject.FindAncestor<PingInstance>();
					//SNUtil.log("TT is now "+ping.pingType);
				}
				if (ping != null && signal == null) {
					//SNUtil.log("Signal was null, refetch");
					signal = SignalManager.getSignal(SignalManager.types[ping.pingType]);
				}
				SNUtil.log("Starting signal init of " + signal + " / " + ping, SNUtil.diDLL);
				ping.SetLabel(signal.longName);

				bool available = signal.storyGate == null || StoryGoalManager.main.completedGoals.Contains(signal.storyGate);
				ping.displayPingInManager = available;
				if (!available)
					ping.SetVisible(false);
			}
		}

	}
}
