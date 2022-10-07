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
		
		private readonly string creatureID;
		
		public float eggScale = 1;
		
		private string eggTexture;
		public string creatureHeldDesc = null;
		
		public int eggSize = 2;
		public int creatureSize = 3;
		
		public readonly WaterParkCreatureParameters eggProperties;
		
		private readonly Assembly ownerMod;
		
		private static readonly Dictionary<TechType, CustomEgg> eggs = new Dictionary<TechType, CustomEgg>();
		
		public CustomEgg(Spawnable pfb, TechType t) : this(pfb.TechType, t, pfb.ClassID) {
			
		}
		
		public CustomEgg(TechType c, TechType t) : this(c, t, c.AsString()) {
			
		}
		
		private CustomEgg(TechType c, TechType t, string id) : base(id+"_Egg", id+" Egg", "Hatches a "+id) {
			ownerMod = SNUtil.tryGetModDLL();
			
			creatureToSpawn = c;
			template = t;
			
			creatureID = id;
			
			WaterParkCreatureParameters wpp = WaterParkCreature.waterParkCreatureParameters[TechType.BoneShark];
			eggProperties = new WaterParkCreatureParameters(wpp.initialSize, wpp.maxSize, wpp.outsideSize, wpp.growingPeriod, wpp.isPickupableOutside);
			
			OnFinishedPatching += onPatched;
			
			eggs[creatureToSpawn] = this;
		}
		
		private void onPatched() {
			if (ownerMod == null)
				throw new Exception("Egg item "+creatureID+"/"+TechType+" has no source mod!");
			
			CraftDataHandler.SetItemSize(creatureToSpawn, new Vector2int(creatureSize, creatureSize));
			
			WaterParkCreature.waterParkCreatureParameters[creatureToSpawn] = eggProperties;
			
			//WaterParkCreatureData data = ScriptableObject.CreateInstance<WaterParkCreatureData>();
			
			SNUtil.log("Constructed custom egg for "+creatureID+": "+TechType, ownerMod);
		}

		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(eggSize, eggSize);
			}
		}
		
		public void setTexture(string tex) {
			eggTexture = tex;
			SpriteHandler.RegisterSprite(creatureToSpawn, TextureManager.getSprite(ownerMod, eggTexture+creatureID+"_Hatched"));
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/Egg_"+creatureID);
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
			RenderUtil.swapTextures(ownerMod, pfb.GetComponentInChildren<Renderer>(), eggTexture+creatureID);
			return pfb;
		}
		
		public static void updateLocale() {
			foreach (CustomEgg e in eggs.Values) {
				string cname = Language.main.strings[e.creatureToSpawn.AsString()];
				Language.main.strings[e.TechType.AsString()] = cname+" Egg";
				Language.main.strings["Tooltip_"+e.TechType.AsString()] = "Hatches a "+cname;
				SNUtil.log("Relocalized "+e+" > "+Language.main.strings[e.TechType.AsString()], e.ownerMod);
				if (!string.IsNullOrEmpty(e.creatureHeldDesc)) {
					Language.main.strings["Tooltip_"+e.creatureToSpawn.AsString()] = e.creatureHeldDesc+"\nRaised in containment.";
				}
			}
		}
		
		public static CustomEgg getEgg(TechType creature) {
			return eggs.ContainsKey(creature) ? eggs[creature] : null;
		}
		
		public static CustomEgg createAndRegisterEgg(Spawnable creature, TechType basis, float scale, string grownHeldDesc, bool isBig, float eggSpawnRate = 1, params BiomeType[] spawn) {
	    	CustomEgg egg = new CustomEgg(creature,  basis);
	    	registerEgg(egg, scale, grownHeldDesc, isBig, eggSpawnRate, spawn);
	    	return egg;
		}
    
	    public static CustomEgg createAndRegisterEgg(TechType creature, TechType basis, float scale, string grownHeldDesc, bool isBig, float eggSpawnRate = 1, params BiomeType[] spawn) {
	    	CustomEgg egg = new CustomEgg(creature,  basis);
	    	registerEgg(egg, scale, grownHeldDesc, isBig, eggSpawnRate, spawn);
	    	return egg;
	    }
    
	    private static void registerEgg(CustomEgg egg, float scale, string grownHeldDesc, bool isBig, float eggSpawnRate, params BiomeType[] spawn) {
	    	egg.setTexture("Textures/Eggs/");
	    	egg.creatureHeldDesc = grownHeldDesc;
	    	egg.eggScale = scale;
	    	if (!isBig) {
	    		egg.creatureSize = 2;
	    		egg.eggSize = 1;
	    	}
	    	egg.Patch();
	    	foreach (BiomeType b in spawn)
	    		GenUtil.registerSlotWorldgen(egg.ClassID, egg.PrefabFileName, egg.TechType, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, b, 1, 0.2F*eggSpawnRate);
	    }
		
	}
}
