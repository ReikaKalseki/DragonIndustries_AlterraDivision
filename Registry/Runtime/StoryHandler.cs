using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using Story;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public class StoryHandler : SerializedTracker<StoryHandler.StoryGoalRecord>, IStoryGoalListener {

		public static readonly StoryHandler instance = new StoryHandler();

		private readonly Dictionary<string, StoryGoalRecord> unlocks = new Dictionary<string, StoryGoalRecord>();

		private readonly Dictionary<ProgressionTrigger, DelayedProgressionEffect> triggers = new Dictionary<ProgressionTrigger, DelayedProgressionEffect>();
		private readonly List<IStoryGoalListener> listeners = new List<IStoryGoalListener>();

		private readonly List<StoryGoal> queuedTickedGoals = new List<StoryGoal>();
		private readonly Dictionary<string, OnGoalUnlock> queuedChainedGoalRedirects = new Dictionary<string, OnGoalUnlock>();

		public bool disableStoryHooks = false;

		private StoryHandler() : base("StoryGoals.dat", false, parse, parseLegacy) {
			//load in world load//IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
			IngameMenuHandler.Main.RegisterOnSaveEvent(this.handleSave);
		}

		private static StoryGoalRecord parse(XmlElement s) {
			return new StoryGoalRecord(s.getProperty("goal"), s.getFloat("eventTime", -1));
		}

		private static StoryGoalRecord parseLegacy(string s) {
			string[] parts = s.Split(',');
			return new StoryGoalRecord(parts[0], float.Parse(parts[1]));
		}

		public void addListener(Action<string> call) {
			listeners.Add(new DelegateGoalListener(call));
		}

		public void addListener(IStoryGoalListener ig) {
			listeners.Add(ig);
		}

		public void registerTrigger(ProgressionTrigger pt, DelayedProgressionEffect e) {
			triggers[pt] = e;
		}

		public void registerTickedGoal(StoryGoal g) {
			queuedTickedGoals.Add(g);
		}

		/// <remarks>Accepts null to deregister the hook entirely</remarks>
		public void registerChainedRedirect(string key, OnGoalUnlock redirect) {
			if (queuedChainedGoalRedirects.ContainsKey(key))
				throw new Exception("Story goal '" + key + "' is already being redirected to " + queuedChainedGoalRedirects[key]);
			queuedChainedGoalRedirects[key] = redirect;
		}

		public void onLoad() {
			if (!BiomeGoalTracker.main) {
				SNUtil.log("Story biome goal tracker not initialized yet!", SNUtil.diDLL);
				return;
			}
			LocationGoalTracker lgt = BiomeGoalTracker.main.gameObject.GetComponent<LocationGoalTracker>();
			ConditionalLocationGoalTracker cg = lgt.gameObject.EnsureComponent<ConditionalLocationGoalTracker>();
			foreach (StoryGoal g in queuedTickedGoals) {
				if (g is ConditionalLocationGoal clg) {
					SNUtil.log("Registering conditional location goal '" + g.key + "' for position " + clg.position, SNUtil.diDLL);
					cg.goals.Add(clg);
				}
				else if (g is LocationGoal lg) {
					SNUtil.log("Registering location goal '" + g.key + "' for position " + lg.position + ": " + lg.location, SNUtil.diDLL);
					lgt.goals.Add(lg);
				}
				else if (g is BiomeGoal bg) {
					SNUtil.log("Registering discovery goal '" + g.key + "' for biome " + bg.biome, SNUtil.diDLL);
					BiomeGoalTracker.main.goals.Add(bg);
				}
				else {
					SNUtil.log("Unrecognized ticked goal '" + g.key + "' type: " + g.GetType().FullName + "!");
				}
			}
			OnGoalUnlockTracker ut = lgt.gameObject.GetComponent<OnGoalUnlockTracker>();
			foreach (KeyValuePair<string, OnGoalUnlock> kvp in queuedChainedGoalRedirects) {
				SNUtil.log("Applying redirect for goal '" + kvp.Key + "': " + kvp.Value);
				if (kvp.Value == null) {
					ut.goalUnlocks.Remove(kvp.Key);
				}
				else {
					ut.goalUnlocks[kvp.Key] = kvp.Value;
				}
			}

			this.handleLoad();
			foreach (string goal in StoryGoalManager.main.completedGoals) {
				if (!unlocks.ContainsKey(goal)) {
					this.add(new StoryGoalRecord(goal, -1));
				}
			}
		}

		public LocationGoal createLocationGoal(double x, double y, double z, double r, string key, float minStay = 0) {
			return this.createLocationGoal(new Vector3((float)x, (float)y, (float)z), r, key, minStay);
		}

		public LocationGoal createLocationGoal(Vector3 pos, double r, string key, float minStay = 0) {
			LocationGoal g =  new LocationGoal();
			g.position = pos;
			g.key = key;
			g.range = (float)r;
			g.location = g.key;
			g.goalType = Story.GoalType.Story;
			return g;
		}

		public ConditionalLocationGoal createLocationGoal(double x, double y, double z, double r, string key, Predicate<Vector3> condition, float minStay = 0) {
			return this.createLocationGoal(new Vector3((float)x, (float)y, (float)z), r, key, condition, minStay);
		}

		public ConditionalLocationGoal createLocationGoal(Vector3 pos, double r, string key, Predicate<Vector3> condition, float minStay = 0) {
			ConditionalLocationGoal g = new ConditionalLocationGoal();
			g.position = pos;
			g.key = key;
			g.range = (float)r;
			g.goalType = Story.GoalType.Story;
			g.condition = condition;
			return g;
		}

		public void tick(Player ep) {
			if (disableStoryHooks || !DIHooks.isWorldLoaded())
				return;
			foreach (KeyValuePair<ProgressionTrigger, DelayedProgressionEffect> kvp in triggers) {
				if (kvp.Key.isReady(ep)) {
					DelayedProgressionEffect dt = kvp.Value;
					dt.time += Time.deltaTime;
					//if (!dt.isFired())
					//	SNUtil.writeToChat("Trigger "+kvp.Key+" is ready, T="+dt.time.ToString("0.000")+"/"+dt.minDelay.ToString("0.0"));
					if (!dt.isFired() && dt.time >= dt.minDelay && UnityEngine.Random.Range(0, 1F) <= dt.chancePerTick * Time.timeScale) {
						//SNUtil.writeToChat("Firing "+dt);
						dt.fire();
					}
				}
				else {
					//SNUtil.writeToChat("Trigger "+kvp.Key+" condition is not met");
				}
			}
		}

		protected override void add(StoryGoalRecord e) {
			base.add(e);
			unlocks[e.goal] = e;
		}

		public StoryGoalRecord getRecord(string goal) {
			return unlocks.ContainsKey(goal) ? unlocks[goal] : null;
		}

		public float getTimeSince(string goal) {
			StoryGoalRecord rec = this.getRecord(goal);
			return rec == null ? -1 : DayNightCycle.main.timePassedAsFloat - (float)rec.eventTime;
		}

		protected override void clear() {
			base.clear();
			unlocks.Clear();
		}

		public void NotifyGoalComplete(string key) {
			SNUtil.log("Completed Story Goal '" + key + "' @ " + DayNightCycle.main.timePassedAsFloat, SNUtil.diDLL);
			foreach (IStoryGoalListener ig in listeners) {
				ig.NotifyGoalComplete(key);
			}
			this.add(new StoryGoalRecord(key, DayNightCycle.main.timePassedAsFloat));
		}

		private class DelegateGoalListener : IStoryGoalListener {

			private readonly Action<string> callback;

			internal DelegateGoalListener(Action<string> a) {
				callback = a;
			}

			public void NotifyGoalComplete(string key) {
				callback(key);
			}

		}

		class ConditionalLocationGoalTracker : MonoBehaviour {

			internal readonly List<ConditionalLocationGoal> goals = new List<ConditionalLocationGoal>();

			private void Start() {
				base.InvokeRepeating("TrackLocation", UnityEngine.Random.value, 2);
			}

			private void TrackLocation() {
				Vector3 position = Player.main.transform.position;
				double timePassed = DayNightCycle.main.timePassed;
				for (int i = goals.Count - 1; i >= 0; i--) {
					if (goals[i].Trigger(position, (float)timePassed)) {
						goals.RemoveFast(i);
					}
				}
			}
		}

		public class ConditionalLocationGoal : StoryGoal {

			public Vector3 position;

			public float range;

			public float minStayDuration;

			private float timeEntered = -1f;

			public Predicate<Vector3> condition;

			public new bool Trigger(Vector3 pos, float time) {
				if (Vector3.SqrMagnitude(pos - position) > range * range || !condition.Invoke(pos)) {
					timeEntered = -1f;
					return false;
				}
				if (timeEntered < 0f)
					timeEntered = time;
				if (time - timeEntered < minStayDuration)
					return false;
				base.Trigger();
				return true;
			}

		}

		public class StoryGoalRecord : SerializedTrackedEvent {

			public readonly string goal;

			internal StoryGoalRecord(string tt, double time) : base(time) {
				goal = tt;
			}

			public override void saveToXML(XmlElement e) {
				e.addProperty("goal", goal);
			}

			public override string ToString() {
				return string.Format("[StoryGoal Goal={0}, Time={1}]", goal, eventTime);
			}

		}
	}

}
