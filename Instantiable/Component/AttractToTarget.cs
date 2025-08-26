using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

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
			delete = Mathf.Max(delete, DayNightCycle.main.timePassedAsFloat + 20);
		}

		void Update() {
			if (!owner)
				owner = this.GetComponent<Creature>();
			if (!swimmer)
				swimmer = this.GetComponent<SwimBehaviour>();
			if (!leash)
				leash = this.GetComponent<StayAtLeashPosition>();
			if (!cyclopsAttacker)
				cyclopsAttacker = this.GetComponent<AttackCyclops>();
			if (!targeter)
				targeter = this.GetComponent<LastTarget>();
			if (attacks == null)
				attacks = this.GetComponents<MeleeAttack>();
			if (targeting == null)
				targeting = this.GetComponents<AggressiveWhenSeeTarget>();

			float time = DayNightCycle.main.timePassedAsFloat;
			if (time >= delete) {
				this.destroy();
				return;
			}

			if (time - lastTick <= 0.5)
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
			if (owner is CrabSnake cs) {
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