using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public abstract class RetexturedFish : Spawnable, DIPrefab<StringPrefabContainer> {

		public float glowIntensity { get; set; }
		public StringPrefabContainer baseTemplate { get; set; }

		private readonly Assembly ownerMod;

		private readonly List<BiomeBase> nativeBiomesCave = new List<BiomeBase>();
		private readonly List<BiomeBase> nativeBiomesSurface = new List<BiomeBase>();

		private static readonly Dictionary<TechType, RetexturedFish> creatures = new Dictionary<TechType, RetexturedFish>();
		private static readonly Dictionary<string, RetexturedFish> creatureIDs = new Dictionary<string, RetexturedFish>();

		public float scanTime = 2;
		public int cookableIntoBase = 0;
		private XMLLocale.LocaleEntry locale;
		public TechType eggBase = TechType.None;
		public float eggScale = 1;
		public float eggMaturationTime = 2400;
		public bool bigEgg = true;
		public float eggSpawnRate = 0;
		public readonly List<BiomeType> eggSpawns = new List<BiomeType>();

		protected RetexturedFish(XMLLocale.LocaleEntry e, string pfb) : this(e.key, e.name, e.desc, pfb) {
			locale = e;
		}

		protected RetexturedFish(string id, string name, string desc, string pfb) : base(id, name, desc) {
			baseTemplate = new StringPrefabContainer(pfb);
			ownerMod = SNUtil.tryGetModDLL();
			typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
			OnFinishedPatching += () => {
				creatures[TechType] = this;
				creatureIDs[ClassID] = this;

				if (locale != null && !string.IsNullOrEmpty(locale.pda))
					SNUtil.addPDAEntry(this, scanTime, locale.getField<string>("category"), locale.pda, locale.getField<string>("header"), null);

				if (eggBase != TechType.None)
					CustomEgg.createAndRegisterEgg(this, eggBase, eggScale, Description, bigEgg, e => { e.eggProperties.growingPeriod = eggMaturationTime; }, eggSpawnRate, eggSpawns.ToArray());

				//GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Creature, LargeWorldEntity.CellLevel.Medium, BiomeType.SeaTreaderPath_OpenDeep_CreatureOnly, 1, 0.15F);
				//GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.GrandReef_TreaderPath, 1, 0.3F);

				BehaviourData.behaviourTypeList[TechType] = this.getBehavior();

				TechType basis = CraftData.entClassTechTable.ContainsKey(baseTemplate.prefab) ? CraftData.entClassTechTable[baseTemplate.prefab] : TechType.None;
				if (basis != TechType.None && BaseBioReactor.charge.ContainsKey(basis))
					BioReactorHandler.SetBioReactorCharge(TechType, BaseBioReactor.charge[basis]);

				if (basis != TechType.None && CraftData.equipmentTypes.ContainsKey(basis) && CraftData.equipmentTypes[basis] == EquipmentType.Hand)
					CraftData.equipmentTypes[TechType] = EquipmentType.Hand;

				if (basis != TechType.None && cookableIntoBase > 0 && CraftData.cookedCreatureList.ContainsKey(basis)) {
					TechType cooked = CraftData.cookedCreatureList[basis];
					TechType cured = SNUtil.getTechType(("Cured"+cooked).Replace("Cooked", ""));
					CraftDataHandler.SetCookedVariant(TechType, cooked);
					SNUtil.log("Adding delegate cooking/curing of " + this + " into " + cooked + " & " + cured);

					TechData rec = new TechData();
					rec.Ingredients.Add(new Ingredient(TechType, 1));
					DuplicateRecipeDelegateWithRecipe alt = new DuplicateRecipeDelegateWithRecipe(cooked, rec);
					alt.category = TechCategory.CookedFood;
					alt.group = TechGroup.Survival;
					alt.craftingType = CraftTree.Type.Fabricator;
					alt.craftingMenuTree = new string[] { "Survival", "CookedFood" };
					alt.ownerMod = ownerMod;
					alt.craftTime = 2; //time not fetchable, not in dict(?!)
					alt.setRecipe(cookableIntoBase);
					alt.unlock = TechType;
					alt.allowUnlockPopups = true;
					alt.Patch();
					TechnologyUnlockSystem.instance.addDirectUnlock(TechType, alt.TechType);

					rec = new TechData();
					rec.Ingredients.Add(new Ingredient(TechType, 1));
					rec.Ingredients.Add(new Ingredient(TechType.Salt, 1));
					alt = new DuplicateRecipeDelegateWithRecipe(cured, rec);
					alt.category = TechCategory.CuredFood;
					alt.group = TechGroup.Survival;
					alt.craftingType = CraftTree.Type.Fabricator;
					alt.craftingMenuTree = new string[] { "Survival", "CuredFood" };
					alt.ownerMod = ownerMod;
					alt.craftTime = 2;
					alt.setRecipe(cookableIntoBase);
					alt.unlock = TechType;
					alt.allowUnlockPopups = true;
					alt.Patch();
					TechnologyUnlockSystem.instance.addDirectUnlock(TechType, alt.TechType);
				}
			};
		}

		public RetexturedFish addNativeBiome(BiomeBase b, bool caveOnly = false) {
			nativeBiomesCave.Add(b);
			if (!caveOnly)
				nativeBiomesSurface.Add(b);
			return this;
		}

		public bool isNativeToBiome(Vector3 vec) {
			return this.isNativeToBiome(BiomeBase.getBiome(vec), WorldUtil.isInCave(vec));
		}

		public bool isNativeToBiome(BiomeBase b, bool cave) {
			return (cave ? nativeBiomesCave : nativeBiomesSurface).Contains(b);
		}

		public string getPrefabID() {
			return ClassID;
		}

		public bool isResource() {
			return false;
		}

		public virtual string getTextureFolder() {
			return "Creature";
		}

		public sealed override GameObject GetGameObject() {
			GameObject world = ObjectUtil.getModPrefabBaseObject(this);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.SetActive(true);
			return world;
		}

		public Atlas.Sprite getIcon() {
			return this.GetItemSprite();
		}

		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
		}

		public Atlas.Sprite getSprite() {
			return this.GetItemSprite();
		}

		public virtual void prepareGameObject(GameObject go, Renderer[] r) {

		}

		public sealed override string ToString() {
			return base.ToString() + " [" + TechType + "] / " + ClassID + " / " + PrefabFileName;
		}

		public Assembly getOwnerMod() {
			return ownerMod;
		}

		public abstract BehaviourType getBehavior();

		public static RetexturedFish getFish(string id) {
			return creatureIDs.ContainsKey(id) ? creatureIDs[id] : null;
		}

		public static RetexturedFish getFish(TechType tt) {
			return creatures.ContainsKey(tt) ? creatures[tt] : null;
		}

	}
}
