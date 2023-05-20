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
		
		internal static readonly Dictionary<string, Action<HolographicControlTag>> actionData = new Dictionary<string, Action<HolographicControlTag>>();
		internal static readonly Dictionary<string, Func<HolographicControlTag, bool>> validityData = new Dictionary<string, Func<HolographicControlTag, bool>>();
		internal static readonly Dictionary<string, Sprite[]> icons = new Dictionary<string, Sprite[]>();
		internal static readonly Sprite defaultOffIcon = Sprite.Create(TextureManager.getTexture(SNUtil.diDLL, "Textures/HoloButton_false"), new Rect(0, 0, 200, 200), new Vector2(0, 0));
		internal static readonly Sprite defaultOnIcon = Sprite.Create(TextureManager.getTexture(SNUtil.diDLL, "Textures/HoloButton_true"), new Rect(0, 0, 200, 200), new Vector2(0, 0));
	        
		public HolographicControl(string name, string desc, Action<HolographicControlTag> a, Func<HolographicControlTag, bool> f) : base("HoloControl_"+name, "Holographic Control - "+name, desc) {
			ownerMod = SNUtil.tryGetModDLL();
			
			OnFinishedPatching += () => {
				LanguageHandler.Main.SetLanguageLine("holocontrol_"+ClassID, desc);
				actionData[ClassID] = a;
				validityData[ClassID] = f;
				if (spr != null) {
					icons[ClassID] = spr;
				}
				else {
					icons[ClassID] = new Sprite[]{defaultOffIcon, defaultOnIcon};
				}
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<HolographicControlTag>().addField("isToggled"));
			};
	    }
		
		public HolographicControl setIcons(string pathAndName, int size) {
			Sprite off = Sprite.Create(TextureManager.getTexture(ownerMod, pathAndName+"_false"), new Rect(0, 0, size, size), new Vector2(0, 0));
			Sprite on = Sprite.Create(TextureManager.getTexture(ownerMod, pathAndName+"_true"), new Rect(0, 0, size, size), new Vector2(0, 0));
			return setIcons(off, on);
		}
		
		public HolographicControl setIcons(Sprite off, Sprite on) {
			spr = new Sprite[]{off, on};
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
			img.sprite = icons[ClassID][0];
			SphereCollider box = gph.EnsureComponent<SphereCollider>();
			box.center = Vector3.zero;
			box.radius = 0.5F;
			box.isTrigger = true;
			gph.layer = LayerID.Useable;
			world.layer = LayerID.Useable;
			gph.EnsureComponent<HolographicControlTag>();
			return world;
	    }
	
		public class HolographicControlTag : MonoBehaviour, IHandTarget {
			
			private bool isToggled;
			
			public void setState(bool toggle) {
				if (toggle != isToggled)
					GetComponent<Image>().sprite = HolographicControl.icons[GetComponentInParent<PrefabIdentifier>().ClassId][toggle ? 1 : 0];
				isToggled = toggle;
				base.SendMessageUpwards("SetHolographicControlState", this, SendMessageOptions.DontRequireReceiver);
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
				HandReticle.main.SetInteractText("holocontrol_"+GetComponentInParent<PrefabIdentifier>().ClassId);
				HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
			}
		
			public void OnHandClick(GUIHand hand) {
				HolographicControl.actionData[GetComponentInParent<PrefabIdentifier>().ClassId].Invoke(this);
			}
			
			public bool isStillValid() {
				return HolographicControl.validityData[GetComponentInParent<PrefabIdentifier>().ClassId].Invoke(this);
			}
			
			public void destroy() {
				UnityEngine.Object.DestroyImmediate(GetComponentInParent<PrefabIdentifier>().gameObject);
			}
			
		}
			
	}
}
