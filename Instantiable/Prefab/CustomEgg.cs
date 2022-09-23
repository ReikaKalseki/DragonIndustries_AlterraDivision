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
	
	public class CustomEgg : Spawnable {
		
		public readonly TechType creatureToSpawn;
		private readonly TechType template;
		
		private string eggTexture;
		public string creatureHeldDesc = null;
		
		private static readonly Dictionary<TechType, CustomEgg> eggs = new Dictionary<TechType, CustomEgg>();
		
		public CustomEgg(TechType c, TechType t) : base(c+"_Egg", c.AsString()+" Egg", "Hatches a "+c.AsString()) {
			creatureToSpawn = c;
			template = t;
			
			WaterParkCreatureParameters wpp = WaterParkCreature.waterParkCreatureParameters[TechType.BoneShark];
			WaterParkCreature.waterParkCreatureParameters[creatureToSpawn] = new WaterParkCreatureParameters(wpp.initialSize, wpp.maxSize, wpp.outsideSize, wpp.growingPeriod, wpp.isPickupableOutside);
			CraftDataHandler.SetItemSize(creatureToSpawn, new Vector2int(3, 3));
			
			eggs[creatureToSpawn] = this;
		}
		
		public void setTexture(string tex) {
			eggTexture = tex;
			SpriteHandler.RegisterSprite(creatureToSpawn, TextureManager.getSprite(eggTexture+creatureToSpawn+"_Hatched"));
		}
		
		public override GameObject GetGameObject() {
			GameObject pfb = CraftData.GetPrefabForTechType(template);
			pfb = UnityEngine.Object.Instantiate(pfb);
			pfb.SetActive(true);
			CreatureEgg egg = pfb.EnsureComponent<CreatureEgg>();
			egg.eggType = TechType;
			egg.overrideEggType = TechType;
			egg.hatchingCreature = creatureToSpawn;
			RenderUtil.swapTextures(pfb.GetComponentInChildren<Renderer>(), eggTexture);
			return pfb;
		}
		
		public static void updateLocale() {
			foreach (CustomEgg e in eggs.Values) {
				string cname = Language.main.strings[e.creatureToSpawn.AsString()];
				Language.main.strings[e.TechType.AsString()] = cname+" Egg";
				Language.main.strings["Tooltip_"+e.TechType.AsString()] = "Hatches a "+cname;
				SNUtil.log("Relocalized "+e+" > "+Language.main.strings[e.TechType.AsString()]);
				if (!string.IsNullOrEmpty(e.creatureHeldDesc)) {
					Language.main.strings["Tooltip_"+e.creatureToSpawn.AsString()] = e.creatureHeldDesc;
				}
			}
		}
		
	}
}
