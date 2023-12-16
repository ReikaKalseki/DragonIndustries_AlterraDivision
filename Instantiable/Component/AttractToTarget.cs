using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra {
	
		public class AttractToTarget : MonoBehaviour {
		
			public static AttractToTarget attractCreatureToTarget(Creature c, MonoBehaviour obj, bool isHorn) {
				if (obj is BaseRoot) 
					obj = obj.GetComponentsInChildren<BaseCell>().GetRandom().GetComponent<LiveMixin>();
				AttractToTarget ac = c.gameObject.EnsureComponent<AttractToTarget>();
				//SNUtil.writeToChat("Attracted "+c+" @ "+c.transform.position+" to "+obj+" @ "+obj.transform.position);
				ac.fire(obj, isHorn);
				if (c is Reefback && isHorn)
					SoundManager.playSoundAt(c.GetComponent<FMOD_CustomLoopingEmitter>().asset, c.transform.position, false, -1, 1);
				return ac;
			}
			
			private MonoBehaviour target;
			private bool isHorn;
			
			private Creature owner;
			private SwimBehaviour swimmer;
			private StayAtLeashPosition leash;
			private AttackCyclops cyclopsAttacker;
			private LastTarget targeter;
			private MeleeAttack[] attacks;
			private AggressiveWhenSeeTarget[] targeting;
			
			private float lastTick;
			
			private float delete;
			
			private void fire(MonoBehaviour from, bool horn) {
				target = from;
				isHorn |= horn;
				delete = Mathf.Max(delete, DayNightCycle.main.timePassedAsFloat+20);
			}
			
			void Update() {
				if (!owner)
					owner = GetComponent<Creature>();
				if (!swimmer)
					swimmer = GetComponent<SwimBehaviour>();
				if (!leash)
					leash = GetComponent<StayAtLeashPosition>();
				if (!cyclopsAttacker)
					cyclopsAttacker = GetComponent<AttackCyclops>();
				if (!targeter)
					targeter = GetComponent<LastTarget>();
				if (attacks == null)
					attacks = GetComponents<MeleeAttack>();
				if (targeting == null)
					targeting = GetComponents<AggressiveWhenSeeTarget>();
				
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time >= delete) {
					UnityEngine.Object.DestroyImmediate(this);
					return;
				}
				
				if (time-lastTick <= 0.5)
					return;
				lastTick = time;
				
				if (owner is Reefback && isHorn) {
					Reefback r = (Reefback)owner;
					swimmer.SwimTo(target.transform.position, r.maxMoveSpeed);
					r.friend = target.gameObject;
					return;
				}
				
				if (target is SubRoot && !(cyclopsAttacker && cyclopsAttacker.isActiveAndEnabled))
					return;
				
				if (Vector3.Distance(transform.position, target.transform.position) >= 40)
					swimmer.SwimTo(target.transform.position, 10);
				
				owner.Aggression.Add(isHorn ? 0.5F : 0.05F);
				if (cyclopsAttacker)
					cyclopsAttacker.SetCurrentTarget(target.gameObject, false);
				if (targeter)
					targeter.SetTarget(target.gameObject);
				if (owner is CrabSnake) {
					CrabSnake cs = (CrabSnake)owner;
					if (cs.IsInMushroom()) {
						cs.ExitMushroom(target.transform.position);
					}
				}
				//if (leash)
				//	leash.
		    	foreach (MeleeAttack a in attacks)
		    		a.lastTarget.SetTarget(target.gameObject);
		    	foreach (AggressiveWhenSeeTarget a in targeting)
		    		a.lastTarget.SetTarget(target.gameObject);
			}
			
			public bool isTargeting(GameObject go) {
				return target.gameObject == go;
			}
			
		}
}