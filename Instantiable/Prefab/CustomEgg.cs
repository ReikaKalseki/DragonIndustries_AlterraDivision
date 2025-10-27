using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public sealed class CustomEgg : Spawnable {

		public readonly TechType creatureToSpawn;
		private readonly TechType template;
		private TechType undiscoveredTechType;

		private readonly string creatureID;

		public float eggScale = 1;

		private string eggTexture;
		public string creatureHeldDesc = null;
		private Action<GameObject> objectModify = null;

		public int eggSize = 2;
		public int creatureSize = 3;

		public readonly WaterParkCreatureParameters eggProperties;

		private readonly Assembly ownerMod;

		private static readonly Dictionary<TechType, CustomEgg> eggs = new Dictionary<TechType, CustomEgg>();

		public CustomEgg(Spawnable pfb, TechType t) : this(pfb.TechType, t, pfb.ClassID) {

		}

		public CustomEgg(TechType c, TechType t) : this(c, t, c.AsString()) {

		}

		private CustomEgg(TechType c, TechType t, string id, Assembly a = null) : base(id + "_Egg", id + " Egg", "Hatches a " + id) {
			ownerMod = a != null ? a : SNUtil.tryGetModDLL();

			creatureToSpawn = c;
			template = t.getEgg();
			if (template == TechType.None)
				throw new Exception("Failed to find egg for creature techtype " + t.AsString());

			creatureID = id;

			WaterParkCreatureParameters wpp = WaterParkCreature.waterParkCreatureParameters[t];
			eggProperties = new WaterParkCreatureParameters(wpp.initialSize, wpp.maxSize, wpp.outsideSize, wpp.growingPeriod/1200F, wpp.isPickupableOutside);

			OnFinishedPatching += this.onPatched;

			eggs[creatureToSpawn] = this;
		}

		private void onPatched() {
			if (ownerMod == null)
				throw new Exception("Egg item " + creatureID + "/" + TechType + " has no source mod!");

			CraftDataHandler.SetItemSize(creatureToSpawn, new Vector2int(creatureSize, creatureSize));

			WaterParkCreature.waterParkCreatureParameters[creatureToSpawn] = eggProperties;

			undiscoveredTechType = TechTypeHandler.AddTechType(ownerMod, ClassID + "_undiscovered", "", "");
			SpriteHandler.RegisterSprite(undiscoveredTechType, this.GetItemSprite());
			CraftDataHandler.SetItemSize(undiscoveredTechType, SizeInInventory);

			//WaterParkCreatureData data = ScriptableObject.CreateInstance<WaterParkCreatureData>();

			SNUtil.log("Constructed custom egg for " + creatureID + ": " + TechType.AsString(), ownerMod);
		}

		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(eggSize, eggSize);
			}
		}

		public CustomEgg setTexture(string tex) {
			eggTexture = tex;
			SpriteHandler.RegisterSprite(creatureToSpawn, TextureManager.getSprite(ownerMod, eggTexture + creatureID + "_Hatched"));
			return this;
		}

		public CustomEgg modifyGO(Action<GameObject> a) {
			objectModify = a;
			return this;
		}

		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/Egg_" + creatureID);
		}

		public override GameObject GetGameObject() {
			GameObject pfb = ObjectUtil.createWorldObject(template);
			CreatureEgg egg = pfb.EnsureComponent<CreatureEgg>();
			egg.eggType = TechType;
			egg.overrideEggType = undiscoveredTechType; //undiscovered
			egg.hatchingCreature = creatureToSpawn;
			egg.explodeOnHatch = false;
			pfb.fullyEnable();
			pfb.transform.localScale = Vector3.one * eggScale;
			RenderUtil.swapTextures(ownerMod, pfb.GetComponentInChildren<Renderer>(), eggTexture + creatureID);
			if (objectModify != null)
				objectModify.Invoke(pfb);
			return pfb;
		}

		public static void updateLocale() {
			foreach (CustomEgg e in eggs.Values) {
				string cname = Language.main.Get(e.creatureToSpawn);
				CustomLocaleKeyDatabase.registerKey(e.TechType.AsString(), cname + " Egg");
				CustomLocaleKeyDatabase.registerKey("Tooltip_" + e.TechType.AsString(), "Hatches a " + cname);

				CustomLocaleKeyDatabase.registerKey(e.undiscoveredTechType.AsString(), Language.main.Get(TechType.BonesharkEggUndiscovered));
				CustomLocaleKeyDatabase.registerKey("Tooltip_" + e.undiscoveredTechType.AsString(), Language.main.Get("Tooltip_" + TechType.BonesharkEggUndiscovered.AsString()));

				SNUtil.log("Relocalized " + e + " > " + Language.main.Get(e.TechType), e.ownerMod);
				if (!string.IsNullOrEmpty(e.creatureHeldDesc)) {
					CustomLocaleKeyDatabase.registerKey("Tooltip_" + e.creatureToSpawn.AsString(), e.creatureHeldDesc + "\nRaised in containment.");
				}
			}
		}

		public static CustomEgg getEgg(TechType creature) {
			return eggs.ContainsKey(creature) ? eggs[creature] : null;
		}

		public static CustomEgg createAndRegisterEgg(Spawnable creature, TechType basis, float scale, string grownHeldDesc, bool isBig, Action<CustomEgg> modify, float eggSpawnRate = 1, params BiomeType[] spawn) {
			CustomEgg egg = new CustomEgg(creature,  basis);
			registerEgg(egg, scale, grownHeldDesc, isBig, modify, eggSpawnRate, spawn);
			return egg;
		}

		public static CustomEgg createAndRegisterEgg(TechType creature, TechType basis, float scale, string grownHeldDesc, bool isBig, Action<CustomEgg> modify, float eggSpawnRate = 1, params BiomeType[] spawn) {
			CustomEgg egg = new CustomEgg(creature, basis);
			registerEgg(egg, scale, grownHeldDesc, isBig, modify, eggSpawnRate, spawn);
			return egg;
		}

		private static void registerEgg(CustomEgg egg, float scale, string grownHeldDesc, bool isBig, Action<CustomEgg> modify, float eggSpawnRate, params BiomeType[] spawn) {
			egg.setTexture("Textures/Eggs/");
			egg.creatureHeldDesc = grownHeldDesc;
			egg.eggScale = scale;
			if (!isBig) {
				egg.creatureSize = 2;
				egg.eggSize = 1;
			}
			if (modify != null) {
				modify(egg);
			}
			egg.Patch();
			foreach (BiomeType b in spawn)
				GenUtil.registerSlotWorldgen(egg.ClassID, egg.PrefabFileName, egg.TechType, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, b, 1, 0.2F * eggSpawnRate);
		}

		public bool includes(TechType tt) {
			return tt == TechType || tt == undiscoveredTechType;
		}
	}
}
