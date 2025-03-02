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
using Story;

namespace ReikaKalseki.DIAlterra {
	
	public sealed class HolographicControl : Spawnable {
		
		private readonly System.Reflection.Assembly ownerMod;
		
		private Sprite[] spr = null;
		
		internal static readonly Dictionary<string, HolographicControl> controlTypes = new Dictionary<string, HolographicControl>();
		
		internal readonly Action<HolographicControlTag> actionData;
		internal readonly Func<HolographicControlTag, bool> validityData;
		internal readonly bool isToggleable;
		internal Sprite[] icons;
		internal static readonly Sprite defaultOffIcon = Sprite.Create(TextureManager.getTexture(SNUtil.diDLL, "Textures/HoloButton_false"), new Rect(0, 0, 200, 200), new Vector2(0, 0));
		internal static readonly Sprite defaultOnIcon = Sprite.Create(TextureManager.getTexture(SNUtil.diDLL, "Textures/HoloButton_true"), new Rect(0, 0, 200, 200), new Vector2(0, 0));
	        
		public HolographicControl(string name, string desc, bool tg, Action<HolographicControlTag> a, Func<HolographicControlTag, bool> f) : base("HoloControl_"+name, "Holographic Control - "+name, desc) {
			ownerMod = SNUtil.tryGetModDLL();
			
			isToggleable = tg;
			actionData = a;
			validityData = f;
			if (spr != null) {
				icons = spr;
			}
			else {
				icons = isToggleable ? new Sprite[]{ defaultOffIcon, defaultOnIcon } : new Sprite[]{ defaultOffIcon };
			}
			
			OnFinishedPatching += () => {
				controlTypes[ClassID] = this;
				LanguageHandler.Main.SetLanguageLine("holocontrol_"+ClassID, desc);
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<HolographicControlTag>().addField("isToggled"));
			};
	    }
		
		public HolographicControl setIcons(string pathAndName, int size) {
			if (isToggleable) {
				Sprite off = Sprite.Create(TextureManager.getTexture(ownerMod, pathAndName+"_false"), new Rect(0, 0, size, size), new Vector2(0, 0));
				Sprite on = Sprite.Create(TextureManager.getTexture(ownerMod, pathAndName+"_true"), new Rect(0, 0, size, size), new Vector2(0, 0));
				return setIcons(off, on);
			}
			else {
				icons = new Sprite[]{Sprite.Create(TextureManager.getTexture(ownerMod, pathAndName), new Rect(0, 0, size, size), new Vector2(0, 0))};
				return this;
			}
		}
		
		public HolographicControl setIcons(Sprite off, Sprite on) {
			icons = new Sprite[]{off, on};
			return this;
		}
			
	    public override GameObject GetGameObject() {
			GameObject world = new GameObject(ClassID);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			Canvas c = world.EnsureComponent<Canvas>();
			c.renderMode = RenderMode.WorldSpace;
			c.scaleFactor = 1;
			c.planeDistance = 100;
			c.referencePixelsPerUnit = 100;
			c.normalizedSortingGridSize = 0.1F;
			c.pixelPerfect = false;
			c.overrideSorting = false;
			c.overridePixelPerfect = false;
			world.EnsureComponent<CanvasScaler>().scaleFactor = 1;
			world.EnsureComponent<GraphicRaycaster>();
			GameObject gph = new GameObject("graphic");
			CanvasRenderer cr = gph.EnsureComponent<CanvasRenderer>();
			gph.transform.SetParent(world.transform);
			gph.transform.localScale = new Vector3(0.0025F, 0.0025F, 1F);
			Image img = gph.EnsureComponent<Image>();
			img.sprite = icons[0];
			SphereCollider box = gph.EnsureComponent<SphereCollider>();
			box.center = Vector3.zero;
			box.radius = 0.5F;
			box.isTrigger = true;
			gph.layer = LayerID.Useable;
			world.layer = LayerID.Useable;
			gph.EnsureComponent<HolographicControlTag>();
			return world;
	    }
		
		public override string ToString() {
			return "Button_"+ClassID;
		}

		
		public static HolographicControlTag addButton(GameObject box, HolographicControl control) {
			foreach (PrefabIdentifier pi in box.transform.GetComponentsInChildren<PrefabIdentifier>()) {
				if (pi && pi.classId == control.ClassID) {
					HolographicControlTag tag = pi.GetComponentInChildren<HolographicControlTag>();
					if (tag)
						return tag;
					else
						UnityEngine.Object.Destroy(pi.gameObject);
				}
			}
			GameObject btn = ObjectUtil.createWorldObject(control.ClassID);
			HolographicControlTag com = btn.GetComponentInChildren<HolographicControl.HolographicControlTag>();
			btn.transform.SetParent(box.transform);
			return com;
		}
		
		public static HolographicControlTag[] addButtons(GameObject box, params HolographicControl[] control) {
			HolographicControlTag[] add = new HolographicControlTag[control.Length];
			for (int i = 0; i < add.Length; i++) {
				add[i] = addButton(box, control[i]);
			}
			return add;
		}
	
		public class HolographicControlTag : MonoBehaviour, IHandTarget {
			
			private bool isToggled;
			
			public HolographicControl controlRef { get; private set; }
			
			public void setState(bool toggle) {
				if (!controlRef.isToggleable)
					return;
				if (toggle != isToggled)
					GetComponent<Image>().sprite = controlRef.icons[toggle ? 1 : 0];
				isToggled = toggle;
				base.SendMessageUpwards("SetHolographicControlState", this, SendMessageOptions.DontRequireReceiver);
			}
			
			void Start() {
				controlRef = HolographicControl.controlTypes[GetComponentInParent<PrefabIdentifier>().ClassId];
			}
			
			public bool getState() {
				return isToggled;
			}
			
			public void disable() {
				setState(false);
			}
			
			public void enableForDuration(float time) {
				setState(true);
				Invoke("disable", time);
			}
			
			public void OnHandHover(GUIHand hand) {
				HandReticle.main.SetInteractText("holocontrol_"+controlRef.ClassID);
				HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
			}
		
			public void OnHandClick(GUIHand hand) {
				controlRef.actionData.Invoke(this);
				SoundManager.playSoundAt(SoundManager.buildSound("event:/sub_module/fabricator/fabricator_click"), transform.position);
			}
			
			public bool isStillValid() {
				return controlRef.validityData.Invoke(this);
			}
			
			public void destroy() {
				UnityEngine.Object.DestroyImmediate(GetComponentInParent<PrefabIdentifier>().gameObject);
			}
			
		}
			
	}
}
