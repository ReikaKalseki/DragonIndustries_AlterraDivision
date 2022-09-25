using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;

using Story;

using SMLHelper.V2.Handlers;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public class PDAMessagePrompts {
		
		public static readonly PDAMessagePrompts instance = new PDAMessagePrompts();
		
		private readonly Dictionary<string, StoryGoal> mappings = new Dictionary<string, StoryGoal>();
		
		private PDAMessagePrompts() {
			
		}
		
		public void addPDAMessage(string key, string text, string soundFile) {
			SNUtil.log("Constructing PDA message "+key);
			StoryGoal item = SNUtil.addVOLine(key, Story.GoalType.PDA, text, SoundManager.registerSound(SNUtil.tryGetModDLL(), "prompt_"+key, soundFile, SoundSystem.voiceBus));
			mappings[key] = item;
		}
		
		public StoryGoal getMessage(string key) {
			return mappings[key];
		}
		
		public bool isTriggered(string m) {
			return StoryGoalManager.main.completedGoals.Contains(getMessage(m).key);
		}
		
		public bool trigger(string m) {
			StoryGoal sg = getMessage(m);
	    	if (!StoryGoalManager.main.completedGoals.Contains(sg.key)) {
	    		StoryGoal.Execute(sg.key, sg.goalType);
	    		return true;
	    	}
			return false;
		}
	}
}
