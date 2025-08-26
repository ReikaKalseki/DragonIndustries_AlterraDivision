using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
	public class ProgressionTrigger {

		public readonly Predicate<Player> isReady;

		public ProgressionTrigger(Predicate<Player> b) {
			isReady = b;
		}

		public override string ToString() {
			return isReady.Method != null ? isReady.Method.Name : "unnamed callback";
		}

	}

	public class ScanTrigger : ProgressionTrigger {

		public readonly TechType tech;

		public ScanTrigger(TechType tt) : base(ep => PDAScanner.complete.Contains(tt)) {
			tech = tt;
		}

		public override string ToString() {
			return "Scan " + tech;
		}

	}

	public class TechTrigger : ProgressionTrigger {

		public readonly TechType tech;

		public TechTrigger(TechType tt) : base(ep => KnownTech.knownTech.Contains(tt)) {
			tech = tt;
		}

		public override string ToString() {
			return "Tech " + tech;
		}

	}

	public class MultiTechTrigger : ProgressionTrigger {

		public readonly IEnumerable<TechType> techs;

		public MultiTechTrigger(params TechType[] tt) : this((IEnumerable<TechType>)tt.ToArray()) {

		}

		public MultiTechTrigger(IEnumerable<TechType> tt) : base(ep => tt.All(KnownTech.knownTech.Contains)) {
			techs = tt;
		}

		public override string ToString() {
			return "Techs " + techs.toDebugString();
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
			return "Ency " + pdaKey;
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
			return "Story " + storyKey;
		}

	}

	public class StoryUntilStoryTrigger : ProgressionTrigger {

		public readonly string storyKeyFrom;
		public readonly string storyKeyTo;

		public StoryUntilStoryTrigger(StoryGoal g1, StoryGoal g2) : this(g1.key, g2.key) {

		}

		public StoryUntilStoryTrigger(string key1, string key2) : base(ep => StoryGoalManager.main.completedGoals.Contains(key1) && !StoryGoalManager.main.completedGoals.Contains(key2)) {
			storyKeyFrom = key1;
			storyKeyTo = key2;
		}

		public override string ToString() {
			return "Story from " + storyKeyFrom + " to " + storyKeyTo;
		}

	}

	public class PositionTrigger : ProgressionTrigger {

		public readonly Vector3 position;
		public readonly float radius;

		public PositionTrigger(Vector3 pos, float r = 10) : base(ep => (ep.transform.position - pos).sqrMagnitude <= r * r) {
			position = pos;
			radius = r;
		}

		public override string ToString() {
			return "Position " + position + " R=" + radius;
		}

	}

	public class BiomeTrigger : ProgressionTrigger {

		public readonly BiomeBase biome;

		public BiomeTrigger(BiomeBase b) : base(ep => ep.GetDepth() >= 2 && BiomeBase.getBiome(ep.transform.position) == b) {
			biome = b;
		}

		public override string ToString() {
			return "Biome " + biome;
		}

	}

	public class DelayedProgressionEffect {

		public readonly Action fire;
		public readonly Func<bool> isFired;
		public readonly float chancePerTick;
		public readonly float minDelay;

		public float time;

		public DelayedProgressionEffect(Action a, Func<bool> b, float f, float mind = 0) {
			fire = a;
			isFired = b;
			chancePerTick = f;
			minDelay = mind;
		}

		public override string ToString() {
			return fire.Method != null ? fire.Method.Name : "unnamed action";
		}

	}

	public class TechUnlockEffect : DelayedProgressionEffect {

		public readonly TechType unlock;

		public TechUnlockEffect(TechType tt, float chance = 1, float mind = 0) : base(() => unlockTech(tt), () => KnownTech.knownTech.Contains(tt), chance, mind) {
			unlock = tt;
		}

		private static void unlockTech(TechType tt) {
			KnownTech.Add(tt);
			SNUtil.triggerTechPopup(tt);
		}

		public override string ToString() {
			return "Unlock tech " + unlock;
		}

	}

	public class DelayedEncyclopediaEffect : DelayedProgressionEffect {

		private readonly PDAManager.PDAPage page;
		public readonly string pageKey;

		public DelayedEncyclopediaEffect(PDAManager.PDAPage g, float f, float mind = 0, bool doSound = true) : base(() => g.unlock(doSound), g.isUnlocked, f, mind) {
			page = g;
			pageKey = g.id;
		}

		public override string ToString() {
			return "PDA Page " + page.id;
		}

	}

	public class DelayedStoryEffect : DelayedProgressionEffect {

		private readonly StoryGoal goal;
		public readonly string goalKey;

		public DelayedStoryEffect(StoryGoal g, float f, float mind = 0) : base(() => StoryGoal.Execute(g.key, g.goalType), () => StoryGoalManager.main.completedGoals.Contains(g.key), f, mind) {
			goal = g;
			goalKey = g.key;
		}

		public override string ToString() {
			return "Story " + goal.key;
		}

	}
}
