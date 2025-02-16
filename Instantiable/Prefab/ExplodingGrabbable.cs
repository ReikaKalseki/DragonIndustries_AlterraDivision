using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.DIAlterra {
	
	public class ExplodingGrabbable : Spawnable {
		
		public readonly TechType template;
		
		internal static readonly Dictionary<string, ExplodingGrabbable> templates = new Dictionary<string, ExplodingGrabbable>();
		
		public ExplodingGrabbable(string classID, string baseTemplate) : this(classID, CraftData.entClassTechTable[baseTemplate]) {
			
	    }
	        
	    public ExplodingGrabbable(string classID, TechType tt) : base(classID, "", "") {
			template = tt;
			OnFinishedPatching += () => {templates[ClassID] = this;};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(template);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.EnsureComponent<ExplodeOnCollection>();
			ObjectUtil.removeComponent<Pickupable>(world);
			return world;
	    }
		
		class ExplodeOnCollection : MonoBehaviour, IHandTarget {
			
			private ExplodingGrabbable template;
			
			void Update() {
				if (template == null) {
					string id = GetComponent<PrefabIdentifier>().classId;
					template = ExplodingGrabbable.templates.ContainsKey(id) ? ExplodingGrabbable.templates[id] : null;
					if (template == null)
						SNUtil.log("No template for exploding grabbable prefab "+id+" @ "+transform.position);
				}
			}
		
			public void OnHandHover(GUIHand hand) {
				HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
				HandReticle.main.SetInteractText(template == null ? "None" : template.template.AsString());
				HandReticle.main.SetTargetDistance(8);
			}
		
			public void OnHandClick(GUIHand hand) {
				explode();
			}
			
			public void explode() {
				Player.main.liveMixin.TakeDamage(10, transform.position, DamageType.Explosive, gameObject);
				//WorldUtil.spawnParticlesAt(transform.position, "", 1, true);
				//SoundManager.playSound("event:/tools/gravsphere/explode");
				GameObject sm = ObjectUtil.lookupPrefab("1c34945a-656d-4f70-bf86-8bc101a27eee");
				GameObject fx = UnityEngine.Object.Instantiate(sm.GetComponent<SeaMoth>().destructionEffect);
				fx.transform.position = transform.position;
				fx.transform.localScale = Vector3.one*0.5F;
				UnityEngine.Object.Destroy(gameObject);
			}
			
		}
			
	}
}
