using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

		public DataChit(StoryGoal g, string name, string desc, Action<SNUtil.PopupData> a = null) : base("DataChit_" + g.key, "Data Card - " + name, "Unlocks " + g.key) {
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
				data.onUnlock = () => { SNUtil.triggerUnlockPopup(data); };
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
				world.removeComponent<ResourceTracker>();
			world.EnsureComponent<DataChitTag>();
			world.removeChildObject("PDALight");
			Renderer r = world.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SNUtil.diDLL, r, "Textures/DataChit/", new Dictionary<int, string> { { 0, "" }, { 1, "" }, { 2, "" } });
			foreach (Material m in r.materials)
				m.SetColor("_GlowColor", renderColor.WithAlpha(1));
			Light l = world.addLight(0.5F, 6, renderColor);
			l.transform.localPosition = new Vector3(0.0F, 0.5F, 0.15F);
			l = world.addLight(1.5F, 1.2F, renderColor);
			l.transform.localPosition = new Vector3(0.0F, 0.125F, 0.15F);
			return world;
		}

		class DataChitTag : MonoBehaviour {

			void Start() {
				if (this.GetComponent<ResourceTracker>())
					ObjectUtil.makeMapRoomScannable(gameObject, DataChit.scannerRoomChitType).Register();
			}

			void OnStoryHandTarget() {
				SNUtil.PopupData popup = popupData[this.GetComponent<PrefabIdentifier>().ClassId];
				if (popup.onUnlock != null)
					popup.onUnlock.Invoke();
			}

		}

	}
}
