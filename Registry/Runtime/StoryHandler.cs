using System;
using System.Collections.Generic;
using System.Linq;

using Story;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public class StoryHandler : IStoryGoalListener {
		
		public static readonly StoryHandler instance = new StoryHandler();
		
		private readonly Dictionary<ProgressionTrigger, DelayedProgressionEffect> triggers = new Dictionary<ProgressionTrigger, DelayedProgressionEffect>();
		private readonly List<IStoryGoalListener> listeners = new List<IStoryGoalListener>();
		
		public bool disableStoryHooks = false;
		
		private StoryHandler() {
			
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
		
		public void NotifyGoalComplete(string key) {
			SNUtil.log("Completed Story Goal '"+key+"' @ "+DayNightCycle.main.timePassedAsFloat, SNUtil.diDLL);
			foreach (IStoryGoalListener ig in listeners) {
				ig.NotifyGoalComplete(key);
			}	
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
	}
		
}
	