using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public abstract class RetexturedFish : Spawnable, DIPrefab<StringPrefabContainer> {
		
		public float glowIntensity {get; set;}	
		public StringPrefabContainer baseTemplate {get; set;}
		
		private readonly Assembly ownerMod;
		
		private readonly List<BiomeBase> nativeBiomesCave = new List<BiomeBase>();
		private readonly List<BiomeBase> nativeBiomesSurface = new List<BiomeBase>();
		
		private static readonly Dictionary<TechType, RetexturedFish> creatures = new Dictionary<TechType, RetexturedFish>();
		private static readonly Dictionary<string, RetexturedFish> creatureIDs = new Dictionary<string, RetexturedFish>();
		
		public float scanTime = 2;
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
					CustomEgg.createAndRegisterEgg(this, eggBase, eggScale, Description, bigEgg, e => {e.eggProperties.growingPeriod = eggMaturationTime;}, eggSpawnRate, eggSpawns.ToArray());
		    
		   		//GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Creature, LargeWorldEntity.CellLevel.Medium, BiomeType.SeaTreaderPath_OpenDeep_CreatureOnly, 1, 0.15F);
		   		//GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.GrandReef_TreaderPath, 1, 0.3F);
		   		
		   		BehaviourData.behaviourTypeList[TechType] = getBehavior();
		   		
		   		BioReactorHandler.SetBioReactorCharge(TechType, BaseBioReactor.charge[CraftData.entClassTechTable[baseTemplate.prefab]]);
			};
	    }
		
		public RetexturedFish addNativeBiome(BiomeBase b, bool caveOnly = false) {
			nativeBiomesCave.Add(b);
			if (!caveOnly)
				nativeBiomesSurface.Add(b);
			return this;
		}
		
		public bool isNativeToBiome(Vector3 vec) {
			return isNativeToBiome(BiomeBase.getBiome(vec), WorldUtil.isInCave(vec));
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
			return GetItemSprite();
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(ownerMod, "Textures/Items/"+ObjectUtil.formatFileName(this));
		}
		
		public Atlas.Sprite getSprite() {
			return GetItemSprite();
		}
		
		public virtual void prepareGameObject(GameObject go, Renderer[] r) {

		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
		public Assembly getOwnerMod() {
			return ownerMod;
		}
		
		public abstract BehaviourType getBehavior();
		
	}
}
