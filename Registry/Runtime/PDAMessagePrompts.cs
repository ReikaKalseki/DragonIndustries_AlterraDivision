using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;

using Story;

namespace ReikaKalseki.DIAlterra {
	public class PDAMessagePrompts {

		public static readonly PDAMessagePrompts instance = new PDAMessagePrompts();

		private readonly Dictionary<string, StoryGoal> mappings = new Dictionary<string, StoryGoal>();

		private PDAMessagePrompts() {

		}

		public StoryGoal addPDAMessage(XMLLocale.LocaleEntry e) {
			return this.addPDAMessage(e.key, e.desc, e.pda);
		}

		public StoryGoal addPDAMessage(string key, string text, string soundFile) {
			SNUtil.log("Constructing PDA message " + key);
			StoryGoal item = new StoryGoal(key, Story.GoalType.PDA, 0);
			SNUtil.addVOLine(item, text, SoundManager.registerPDASound(SNUtil.tryGetModDLL(), "prompt_" + key, soundFile).asset);
			mappings[key] = item;
			return item;
		}

		public StoryGoal getMessage(string key) {
			return mappings[key];
		}

		public bool isTriggered(string m) {
			return StoryGoalManager.main.completedGoals.Contains(this.getMessage(m).key);
		}

		public bool trigger(string m) {
			StoryGoal sg = this.getMessage(m);
			if (!StoryGoalManager.main.completedGoals.Contains(sg.key)) {
				StoryGoal.Execute(sg.key, sg.goalType);
				return true;
			}
			return false;
		}
	}
}
