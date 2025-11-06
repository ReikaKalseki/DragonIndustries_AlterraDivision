using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

using Story;

namespace ReikaKalseki.DIAlterra {

	public class PrecursorStoryConsole : Spawnable {

		public readonly XMLLocale.LocaleEntry locale;

		public StoryGoal storyGoal { get; private set; }

		public SNUtil.PopupData popup;

		internal static readonly Dictionary<string, PrecursorStoryConsole> prefabTable = new Dictionary<string, PrecursorStoryConsole>();

		public PrecursorStoryConsole(XMLLocale.LocaleEntry e, string goal = null) : base(e.key, e.name, e.desc) {
			locale = e;
			if (!string.IsNullOrEmpty(goal))
				storyGoal = new StoryGoal(goal, Story.GoalType.Story, 0);
			OnFinishedPatching += () => {
				prefabTable[ClassID] = this;
			};
		}

		public PrecursorStoryConsole setGoal(string goal) {
			storyGoal = new StoryGoal(goal, Story.GoalType.Story, 0);
			return this;
		}

		public PrecursorStoryConsole setPopup(TechType spr) {
			return setPopup(() => SNUtil.getTechPopupSprite(spr)); //use callback, not direct!
		}

		public PrecursorStoryConsole setPopup(Sprite spr) {
			return setPopup(() => spr);
		}

		public PrecursorStoryConsole setPopup(Func<Sprite> spr) {
			popup = new SNUtil.PopupData(locale.getString("popupTitle"), locale.getString("popupSubtitle")) { controlText = locale.getString("popupDescription"), graphic = spr };
			return this;
		}

		public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("81cf2223-455d-4400-bac3-a5bcd02b3638");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			StoryHandTarget sh = world.EnsureComponent<StoryHandTarget>();
			sh.goal = storyGoal;
			sh.primaryTooltip = locale.getString("tooltip");
			sh.secondaryTooltip = locale.getString("tooltipSecondary");
			sh.informGameObject = world;
			sh.isValidHandTarget = false;
			world.EnsureComponent<StoryConsoleTag>();
			foreach (Renderer r in world.GetComponent<PrecursorComputerTerminal>().fx.GetComponentsInChildren<Renderer>()) {
				//r.materials[0].SetColor("_Color", new Color(0.8F, 0.25F, 1F));
				r.materials[0].SetColor("_Color", new Color(0.3F, 0.9F, 1F));
			}
			return world;
		}

		public void register(Vector3 position, float yaw = 0) {
			this.Patch();
			GenUtil.registerWorldgen(new PositionedPrefab(ClassID, position, Quaternion.Euler(0, yaw, 0)));
		}

		public virtual bool isUsable(StoryConsoleTag tag) {
			return true;
		}

	}

	public class StoryConsoleTag : MonoBehaviour {

		protected StoryHandTarget target;
		protected PrecursorComputerTerminal terminal;

		protected PrecursorStoryConsole prefab;

		void Update() {
			if (!target)
				target = gameObject.GetComponent<StoryHandTarget>();
			if (!terminal)
				terminal = gameObject.GetComponent<PrecursorComputerTerminal>();
			if (prefab == null)
				prefab = PrecursorStoryConsole.prefabTable[GetComponent<PrefabIdentifier>().ClassId];

			if (target) {
				bool unlock = StoryGoalManager.main.IsGoalComplete(prefab.storyGoal.key);
				target.isValidHandTarget = !unlock && prefab != null && prefab.isUsable(this);
				target.enabled = target.isValidHandTarget;
				terminal.enabled = target.enabled;
				target.secondaryTooltip = target.enabled ? prefab.locale.getString("tooltipSecondary") : prefab.locale.getString("tooltipDisabled");
			}
		}

		void OnStoryHandTarget() {
			if (prefab != null && prefab.popup != null)
				SNUtil.triggerUnlockPopup(prefab.popup);
		}

	}
}
