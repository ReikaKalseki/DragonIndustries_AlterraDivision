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
	
	public sealed class DataChit : Spawnable {
		
		public readonly StoryGoal goal;
		
		public Color renderColor = new Color(229/255F, 133/255F, 0); //avali aerogel color
		public bool showOnScannerRoom = true;
		
		private readonly System.Reflection.Assembly ownerMod;
		
		private static readonly Dictionary<string, SNUtil.PopupData> popupData = new Dictionary<string, SNUtil.PopupData>();
		
		private static bool registeredCommonTechType;
		public static TechType scannerRoomChitType { get; private set; }
		
		public DataChit(string goalKey, string name, string desc, Action<SNUtil.PopupData> a = null) : this(new StoryGoal(goalKey, Story.GoalType.Story, 0), name, desc, a) {
			
		}
	        
		public DataChit(StoryGoal g, string name, string desc, Action<SNUtil.PopupData> a = null) : base("DataChit_"+g.key, "Data Card - "+name, "Unlocks "+g.key) {
			goal = g;
			ownerMod = SNUtil.tryGetModDLL();
			
			if (!registeredCommonTechType) {
				scannerRoomChitType = TechTypeHandler.Main.AddTechType("DataChit", "Data Card", "");
				SpriteHandler.RegisterSprite(scannerRoomChitType, TextureManager.getSprite(SNUtil.diDLL, "Textures/ScannerSprites/DataChit"));
				registeredCommonTechType = true;
			}
			
			OnFinishedPatching += () => {
				SNUtil.PopupData data = new SNUtil.PopupData("Digital Data Downloaded", desc);
				data.sound = "event:/tools/scanner/scan_complete";
				data.onUnlock = () => {SNUtil.triggerUnlockPopup(data);};
				if (a != null)
					a(data);
				popupData[ClassID] = data;
			};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("1bdbad41-adcb-47db-ab2c-0dc4a7180860");
			world.transform.localScale = new Vector3(0.4F, 1, 1F);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			StoryHandTarget tgt = world.EnsureComponent<StoryHandTarget>();
			tgt.goal = goal;
			tgt.primaryTooltip = FriendlyName;
			tgt.informGameObject = world;
			if (showOnScannerRoom)
				ObjectUtil.makeMapRoomScannable(world, scannerRoomChitType);
			else
				ObjectUtil.removeComponent<ResourceTracker>(world);
			world.EnsureComponent<DataChitTag>();
			ObjectUtil.removeChildObject(world, "PDALight");
			Renderer r = world.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SNUtil.diDLL, r, "Textures/DataChit/", new Dictionary<int, string>{{0, ""}, {1, ""}, {2, ""}});
			foreach (Material m in r.materials)
				m.SetColor("_GlowColor", renderColor.WithAlpha(1));
			Light l = ObjectUtil.addLight(world);
			l.color = renderColor;
			l.range = 6;
			l.intensity = 0.5F;
			l.transform.localPosition = new Vector3(0.0F, 0.5F, 0.15F);
			l = ObjectUtil.addLight(world);
			l.color = renderColor;
			l.range = 1.2F;
			l.intensity = 1.5F;
			l.transform.localPosition = new Vector3(0.0F, 0.125F, 0.15F);
			return world;
	    }
	
		class DataChitTag : MonoBehaviour {
			
			void OnStoryHandTarget() {
				SNUtil.PopupData popup = popupData[GetComponent<PrefabIdentifier>().ClassId];
				if (popup.onUnlock != null)
					popup.onUnlock.Invoke();
			}
			
		}
			
	}
}
