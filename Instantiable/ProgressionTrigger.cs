using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using Story;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{	
	public class ProgressionTrigger {
		
		public readonly Func<Player, bool> isReady;
		
		public ProgressionTrigger(Func<Player, bool> b) {
			isReady = b;
		}
		
		public override string ToString() {
			return isReady.Method != null ? isReady.Method.Name : "unnamed callback";
		}
		
	}
	
	public class TechTrigger : ProgressionTrigger {
		
		public readonly TechType tech;
		
		public TechTrigger(TechType tt) : base(ep => KnownTech.knownTech.Contains(tt)) {
			tech = tt;
		}
		
		public override string ToString() {
			return "Tech "+tech;
		}
		
	}
	
	public class EncylopediaTrigger : ProgressionTrigger {
		
		public readonly string pdaKey;
		
		public EncylopediaTrigger(PDAManager.PDAPage g) : this(g.id) {
			
		}
		
		public EncylopediaTrigger(string key) : base(ep => PDAEncyclopedia.entries.ContainsKey(key)) {
			pdaKey = key;
		}
		
		public override string ToString() {
			return "Ency "+pdaKey;
		}
		
	}
	
	public class StoryTrigger : ProgressionTrigger {
		
		public readonly string storyKey;
		
		public StoryTrigger(StoryGoal g) : this(g.key) {
			
		}
		
		public StoryTrigger(string key) : base(ep => StoryGoalManager.main.completedGoals.Contains(key)) {
			storyKey = key;
		}
		
		public override string ToString() {
			return "Story "+storyKey;
		}
		
	}
	
	public class DelayedProgressionEffect {
		
		public readonly Action fire;
		public readonly Func<bool> isFired;
		public readonly float chancePerTick;
		
		public DelayedProgressionEffect(Action a, Func<bool> b, float f) {
			fire = a;
			isFired = b;
			chancePerTick = f;
		}
		
		public override string ToString() {
			return fire.Method != null ? fire.Method.Name : "unnamed action";
		}
		
	}
	
	public class DelayedEncyclopediaEffect : DelayedProgressionEffect {
		
		private readonly PDAManager.PDAPage page;
		public readonly string pageKey;
		
		public DelayedEncyclopediaEffect(PDAManager.PDAPage g, float f, bool doSound = true) : base(() => g.unlock(doSound), g.isUnlocked, f) {
			page = g;
			pageKey = g.id;
		}
		
		public override string ToString() {
			return "PDA Page "+page.id;
		}
		
	}
	
	public class DelayedStoryEffect : DelayedProgressionEffect {
		
		private readonly StoryGoal goal;
		public readonly string goalKey;
		
		public DelayedStoryEffect(StoryGoal g, float f) : base(() => StoryGoal.Execute(g.key, g.goalType), () => StoryGoalManager.main.completedGoals.Contains(g.key), f) {
			goal = g;
			goalKey = g.key;
		}
		
		public override string ToString() {
			return "Story "+goal.key;
		}
		
	}
}
