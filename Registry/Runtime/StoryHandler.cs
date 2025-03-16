using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

using Story;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public class StoryHandler : IStoryGoalListener {
		
		private static readonly string saveFileName = "StoryGoals.dat";
		
		public static readonly StoryHandler instance = new StoryHandler();
		
		private static readonly Dictionary<string, StoryGoalRecord> unlocks = new Dictionary<string, StoryGoalRecord>();
		
		private readonly Dictionary<ProgressionTrigger, DelayedProgressionEffect> triggers = new Dictionary<ProgressionTrigger, DelayedProgressionEffect>();
		private readonly List<IStoryGoalListener> listeners = new List<IStoryGoalListener>();
		
		private readonly List<StoryGoal> queuedTickedGoals = new List<StoryGoal>();
		private readonly Dictionary<string, OnGoalUnlock> queuedChainedGoalRedirects = new Dictionary<string, OnGoalUnlock>();
		
		public bool disableStoryHooks = false;
		
		private StoryHandler() {
			//load in world load//IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
			IngameMenuHandler.Main.RegisterOnSaveEvent(handleSave);
		}
		
		public static void handleSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			/*
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (KeyValuePair<TechType, float> kvp in unlockTimes) {	
				SNUtil.log("Found "+sh+" save handler for "+pi.ClassId, SNUtil.diDLL);
				sh.data = doc.CreateElement("object");
				sh.data.SetAttribute("objectID", pi.Id);
				sh.save(pi);
				doc.DocumentElement.AppendChild(sh.data);
			}
			SNUtil.log("Saving "+doc.DocumentElement.ChildNodes.Count+" objects to disk", SNUtil.diDLL);
			doc.Save(path);
			*/
			List<string> content = new List<string>();
			List<StoryGoalRecord> li = new List<StoryGoalRecord>(unlocks.Values);
			li.Sort();
			foreach (StoryGoalRecord tt in li) {
				content.Add(tt.goal+","+tt.unlockTime.ToString("0.0"));
			}
			File.WriteAllLines(path, content.ToArray());
		}
		
		public static void handleLoad() {
			string dir = SNUtil.getCurrentSaveDir();
			string path = Path.Combine(dir, saveFileName);
			if (File.Exists(path)) {
				/*
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				saveData.Clear();
				foreach (XmlElement e in doc.DocumentElement.ChildNodes)
					saveData[e.Name == "player" ? "player" : e.GetAttribute("objectID")] = e;
				SNUtil.log("Loaded "+saveData.Count+" object entries from disk", SNUtil.diDLL);
				*/
			}
			unlocks.Clear();
			string[] content = File.ReadAllLines(path);
			foreach (string s in content) {
				string[] parts = s.Split(',');
				unlocks[parts[0]] = new StoryGoalRecord(parts[0], float.Parse(parts[1]));
			}
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
		
		/** Accepts null to deregister the hook entirely */
		public void registerChainedRedirect(string key, OnGoalUnlock redirect) {
			if (queuedChainedGoalRedirects.ContainsKey(key))
				throw new Exception("Story goal '"+key+"' is already being redirected to "+queuedChainedGoalRedirects[key]);
			queuedChainedGoalRedirects[key] = redirect;
		}
		
		public void onLoad() {
			if (!BiomeGoalTracker.main) {
				SNUtil.log("Story biome goal tracker not initialized yet!", SNUtil.diDLL);
				return;
			}
			LocationGoalTracker lg = BiomeGoalTracker.main.gameObject.GetComponent<LocationGoalTracker>();
			ConditionalLocationGoalTracker cg = lg.gameObject.EnsureComponent<ConditionalLocationGoalTracker>();
			foreach (StoryGoal g in queuedTickedGoals) {
				if (g is ConditionalLocationGoal) {
					ConditionalLocationGoal bg = (ConditionalLocationGoal)g;
					SNUtil.log("Registering conditional location goal '"+g.key+"' for position "+bg.position, SNUtil.diDLL);
					cg.goals.Add(bg);
				}
				else if (g is LocationGoal) {
					LocationGoal bg = (LocationGoal)g;
					SNUtil.log("Registering location goal '"+g.key+"' for position "+bg.position+": "+bg.location, SNUtil.diDLL);
					lg.goals.Add(bg);
				}
				else if (g is BiomeGoal) {
					BiomeGoal bg = (BiomeGoal)g;
					SNUtil.log("Registering discovery goal '"+g.key+"' for biome "+bg.biome, SNUtil.diDLL);
					BiomeGoalTracker.main.goals.Add(bg);
				}
				else {
					SNUtil.log("Unrecognized ticked goal '"+g.key+"' type: "+g.GetType().FullName+"!");
				}
			}
			OnGoalUnlockTracker ut = lg.gameObject.GetComponent<OnGoalUnlockTracker>();
			foreach (KeyValuePair<string, OnGoalUnlock> kvp in queuedChainedGoalRedirects) {
				SNUtil.log("Applying redirect for goal '"+kvp.Key+"': "+kvp.Value);
				if (kvp.Value == null) {
					ut.goalUnlocks.Remove(kvp.Key);
				}
				else {
					ut.goalUnlocks[kvp.Key] = kvp.Value;
				}
			}
		
			handleLoad();
			foreach (string goal in StoryGoalManager.main.completedGoals) {
				if (!unlocks.ContainsKey(goal)) {
					unlocks[goal] = new StoryGoalRecord(goal, -1);
				}
			}
		}
		
		public LocationGoal createLocationGoal(double x, double y, double z, double r, string key, float minStay = 0) {
			return createLocationGoal(new Vector3((float)x, (float)y, (float)z), r, key, minStay);
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
			return createLocationGoal(new Vector3((float)x, (float)y, (float)z), r, key, condition, minStay);
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
					if (!dt.isFired() && dt.time >= dt.minDelay && UnityEngine.Random.Range(0, 1F) <= dt.chancePerTick*Time.timeScale) {
						//SNUtil.writeToChat("Firing "+dt);
						dt.fire();
					}
				}
				else {
					//SNUtil.writeToChat("Trigger "+kvp.Key+" condition is not met");
				}
			}
		}
		
		public void forAllGoalsNewerThan(float thresh, Action<string, StoryGoalRecord> forEach) {
			float time = DayNightCycle.main.timePassedAsFloat;
			foreach (KeyValuePair<string, StoryGoalRecord> kvp in unlocks) {
				float age = time-kvp.Value.unlockTime;
				if (age < thresh)
					forEach.Invoke(kvp.Key, kvp.Value);
			}
		}
		
		public void NotifyGoalComplete(string key) {
			SNUtil.log("Completed Story Goal '"+key+"' @ "+DayNightCycle.main.timePassedAsFloat, SNUtil.diDLL);
			foreach (IStoryGoalListener ig in listeners) {
				ig.NotifyGoalComplete(key);
			}	
			unlocks[key] = new StoryGoalRecord(key, DayNightCycle.main.timePassedAsFloat);
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
				for (int i = this.goals.Count - 1; i >= 0; i--) {
					if (this.goals[i].Trigger(position, (float)timePassed)) {
						this.goals.RemoveFast(i);
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
				if (Vector3.SqrMagnitude(pos - this.position) > this.range * this.range || !condition.Invoke(pos)) {
					this.timeEntered = -1f;
					return false;
				}
				if (this.timeEntered < 0f)
					this.timeEntered = time;
				if (time - this.timeEntered < this.minStayDuration)
					return false;
				base.Trigger();
				return true;
			}
			
		}
		
		public class StoryGoalRecord : IComparable<StoryGoalRecord> {
			
			public readonly string goal;
			public readonly float unlockTime;
			
			internal StoryGoalRecord(string tt, float time) {
				goal = tt;
				unlockTime = time;
			}
			
	    	public int CompareTo(StoryGoalRecord fx) {
	    		return unlockTime.CompareTo(fx.unlockTime);
	    	}
			
		}
	}
		
}
	