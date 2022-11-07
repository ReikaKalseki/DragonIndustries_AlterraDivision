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
		
		private StoryHandler() {
			
		}
		
		public void addListener(IStoryGoalListener ig) {
			listeners.Add(ig);
		}
		
		public void registerTrigger(ProgressionTrigger pt, DelayedProgressionEffect e) {
			triggers[pt] = e;
		}
		
		public void tick(Player ep) {
			foreach (KeyValuePair<ProgressionTrigger, DelayedProgressionEffect> kvp in triggers) {
				if (kvp.Key.isReady(ep)) {
					//SNUtil.writeToChat("Trigger "+kvp.Key+" is ready");
					DelayedProgressionEffect dt = kvp.Value;
					if (!dt.isFired() && UnityEngine.Random.Range(0, 1F) <= dt.chancePerTick*Time.timeScale) {
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
			//SNUtil.writeToChat("Story '"+key+"'");
			foreach (IStoryGoalListener ig in listeners) {
				ig.NotifyGoalComplete(key);
			}	
		}
	}
		
}
	