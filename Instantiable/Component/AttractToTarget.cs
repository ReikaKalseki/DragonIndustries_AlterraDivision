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

		public static AttractToTarget attractCreatureToTarget(Creature c, MonoBehaviour obj, bool isHorn, float maxDuration = 20) {
			if (obj is BaseRoot)
				obj = obj.GetComponentsInChildren<BaseCell>().GetRandom().GetComponent<LiveMixin>();
			AttractToTarget ac = c.gameObject.EnsureComponent<AttractToTarget>();
			//SNUtil.writeToChat("Attracted "+c+" @ "+c.transform.position+" to "+obj+" @ "+obj.transform.position);
			ac.fire(obj, isHorn, maxDuration);
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
		private AttackLastTarget attacker;

		private float lastTick;

		public bool deleteOnAttack;

		private float delete;

		private void fire(MonoBehaviour from, bool horn, float maxDuration = 20) {
			target = from;
			isHorn |= horn;
			delete = Mathf.Max(delete, DayNightCycle.main.timePassedAsFloat + maxDuration);
		}

		public void OnMeleeAttack(GameObject target) {
			if (target && target.isAncestorOf(this.target) && deleteOnAttack) {
				this.setTarget(null);
				this.destroy();
			}
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
			if (attacker == null)
				attacker = this.GetComponent<AttackLastTarget>();

			float time = DayNightCycle.main.timePassedAsFloat;
			if (time >= delete) {
				this.destroy();
				return;
			}

			if (target.isPlayer()) {
				if (Player.main.currentSub) {
					target = Player.main.currentSub;
				}
				else {
					Vehicle v = Player.main.GetVehicle();
					if (v)
						target = v;
				}
			}

			if (target is AggroAttractor aa) {
				if (!aa.isAggroable) {
					setTarget(null);
					this.destroy();
					return;
				}
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

			if (target is SubRoot && !(cyclopsAttacker && cyclopsAttacker.isActiveAndEnabled)) {
				this.destroy();
				return;
			}

			if (Vector3.Distance(transform.position, target.transform.position) >= 40)
				swimmer.SwimTo(target.transform.position, 10);

			owner.Aggression.Add(deleteOnAttack && delete-time > 1000 ? 1 : (isHorn ? 0.5F : 0.05F));
			if (owner is CrabSnake cs) {
				if (cs.IsInMushroom()) {
					cs.ExitMushroom(target.transform.position);
				}
			}
			//if (leash)
			//	leash.
			setTarget(target.gameObject);
		}

		private void setTarget(GameObject go) {
			if (cyclopsAttacker)
				cyclopsAttacker.SetCurrentTarget(go, false);
			if (targeter) {
				targeter.SetTarget(go);
				if (delete - DayNightCycle.main.timePassedAsFloat > 1000 && deleteOnAttack)
					targeter.SetLockedTarget(go);
			}
			if (attacker)
				attacker.currentTarget = go;
			foreach (MeleeAttack a in attacks)
				a.lastTarget.SetTarget(go);
			foreach (AggressiveWhenSeeTarget a in targeting)
				a.lastTarget.SetTarget(go);
		}

		public bool isTargeting(GameObject go) {
			return target.gameObject == go;
		}

	}

	public interface AggroAttractor {
		bool isAggroable { get; }
	}
}