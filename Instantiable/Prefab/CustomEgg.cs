using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {
	
	public sealed class CustomEgg : Spawnable {
		
		public readonly TechType creatureToSpawn;
		private readonly TechType template;
		
		public float eggScale = 1;
		
		private string eggTexture;
		public string creatureHeldDesc = null;
		
		public int eggSize = 2;
		public int creatureSize = 3;
		
		private readonly Assembly ownerMod;
		
		private static readonly Dictionary<TechType, CustomEgg> eggs = new Dictionary<TechType, CustomEgg>();
		
		public CustomEgg(TechType c, TechType t) : base(c+"_Egg", c.AsString()+" Egg", "Hatches a "+c.AsString()) {
			
			ownerMod = SNUtil.tryGetModDLL();
			
			creatureToSpawn = c;
			template = t;
			
			WaterParkCreatureParameters wpp = WaterParkCreature.waterParkCreatureParameters[TechType.BoneShark];
			WaterParkCreature.waterParkCreatureParameters[creatureToSpawn] = new WaterParkCreatureParameters(wpp.initialSize, wpp.maxSize, wpp.outsideSize, wpp.growingPeriod, wpp.isPickupableOutside);
			
			OnFinishedPatching += () => {CraftDataHandler.SetItemSize(creatureToSpawn, new Vector2int(creatureSize, creatureSize));};
			
			eggs[creatureToSpawn] = this;
		}

		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(eggSize, eggSize);
			}
		}
		
		public void setTexture(string tex) {
			eggTexture = tex;
			SpriteHandler.RegisterSprite(creatureToSpawn, TextureManager.getSprite(ownerMod, eggTexture+creatureToSpawn+"_Hatched"));
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/Egg_"+creatureToSpawn);
		}
		
		public override GameObject GetGameObject() {
			GameObject pfb = CraftData.GetPrefabForTechType(template);
			pfb = UnityEngine.Object.Instantiate(pfb);
			pfb.SetActive(true);
			CreatureEgg egg = pfb.EnsureComponent<CreatureEgg>();
			egg.eggType = TechType;
			egg.overrideEggType = TechType;
			egg.hatchingCreature = creatureToSpawn;
			pfb.transform.localScale = Vector3.one*eggScale;
			RenderUtil.swapTextures(ownerMod, pfb.GetComponentInChildren<Renderer>(), eggTexture+creatureToSpawn);
			return pfb;
		}
		
		public static void updateLocale() {
			foreach (CustomEgg e in eggs.Values) {
				string cname = Language.main.strings[e.creatureToSpawn.AsString()];
				Language.main.strings[e.TechType.AsString()] = cname+" Egg";
				Language.main.strings["Tooltip_"+e.TechType.AsString()] = "Hatches a "+cname;
				SNUtil.log("Relocalized "+e+" > "+Language.main.strings[e.TechType.AsString()], e.ownerMod);
				if (!string.IsNullOrEmpty(e.creatureHeldDesc)) {
					Language.main.strings["Tooltip_"+e.creatureToSpawn.AsString()] = e.creatureHeldDesc;
				}
			}
		}
		
	}
}
