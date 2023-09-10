using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Text;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {
	
	public static class DIHooks {
		
		internal static readonly float NEAR_LAVA_RADIUS = 16;
	    
	    private static float worldLoadTime = -1;
	    
	    public static event Action<DayNightCycle> onDayNightTickEvent;
	    public static event Action onWorldLoadedEvent;
	    public static event Action<Player> onPlayerTickEvent;
	    public static event Action<SeaMoth> onSeamothTickEvent;
	    public static event Action<Exosuit> onPrawnTickEvent;
	    public static event Action<SubRoot> onCyclopsTickEvent;
	    public static event Action<DamageToDeal> onDamageEvent;
	    public static event Action<Pickupable> onItemPickedUpEvent;
	    public static event Action<CellManager, LargeWorldEntity> onEntityRegisterEvent;
	    public static event Action<SkyApplier> onSkyApplierSpawnEvent;
	    public static event Action<TechType, Constructable> onConstructedEvent;
	    public static event Action<BiomeCheck> getBiomeEvent;
	    public static event Action<WaterTemperatureCalculation> getTemperatureEvent;
	    public static event Action<GameObject> onKnifedEvent;
	    public static event Action<SeaMoth, int, TechType, bool> onSeamothModulesChangedEvent;
	    public static event Action<SubRoot> onCyclopsModulesChangedEvent;
	    public static event Action<Exosuit, int, TechType, bool> onPrawnModulesChangedEvent;
	    public static event Action<SeaMoth, TechType, int> onSeamothModuleUsedEvent;
	    public static event Action<SNCameraRoot> onSonarUsedEvent;
	    public static event Action<SeaMoth> onSeamothSonarUsedEvent;
	    public static event Action<SubRoot> onCyclopsSonarUsedEvent;
	    public static event Action<GameObject> onEggHatchedEvent;
	    public static event Action<EMPBlast, GameObject> onEMPHitEvent;
	    public static event Action<EMPBlast, Collider> onEMPTouchEvent;
	    public static event Action<StringBuilder, TechType, GameObject> itemTooltipEvent;
	    //public static event Action<WaterFogValues> fogCalculateEvent;
	    public static event Action<BuildabilityCheck> constructabilityEvent;
	    public static event Action<StoryHandCheck> storyHandEvent;
	    public static event Action<RadiationCheck> radiationCheckEvent;
	    public static event Action<BulkheadLaserCutterHoverCheck> bulkheadLaserHoverEvent;
	    public static event Action<KnifeHarvest> knifeHarvestEvent;
	    //public static event Action<MusicSelectionCheck> musicBiomeChoiceEvent;
	    public static event Action<FruitPlantTag> onFruitPlantTickEvent;
	    public static event Action<ReaperLeviathan, Vehicle> reaperGrabVehicleEvent;
	    public static event Action<FMOD_CustomEmitter> onSoundPlayedEvent;
	    public static event Action<SolarEfficiencyCheck> solarEfficiencyEvent;
	    public static event Action<Vehicle, Player> vehicleEnterEvent;
	    public static event Action<DepthCompassCheck> depthCompassEvent;
	    public static event Action<Survival, Player, bool> respawnEvent;
	    public static event Action<PropulsibilityCheck> propulsibilityEvent;
	    public static event Action<Drillable, Vector3, Exosuit> drillableDrillTickEvent;
	    public static event Action<DroppabilityCheck> droppabilityEvent;
	    public static event Action<MapRoomFunctionality> scannerRoomTickEvent;	
	    public static event Action itemsLostEvent;
	    public static event Action<StorageContainer, GUIHand> storageHoverEvent;	
	
		private static BasicText updateNotice = new BasicText(TextAnchor.MiddleCenter);
		
		private static bool hasLoadedAWorld = false;
		
		private static CustomBiome currentCustomBiome;
	    
	    static DIHooks() {
	    	
	    }
	    
	    public class DamageToDeal {
	    	
	    	public readonly float originalAmount;
	    	public readonly DamageType type;
	    	public readonly GameObject target;
	    	public readonly GameObject dealer;
	    	
	    	private bool disallowFurtherChanges;
	    	
	    	internal float amount;
	    	
	    	internal DamageToDeal(float amt, DamageType tt, GameObject tgt, GameObject dl) {
	    		originalAmount = amt;
	    		amount = originalAmount;
	    		type = tt;
	    		target = tgt;
	    		dealer = dl;
	    		disallowFurtherChanges = false;
	    	}
	    	
	    	public void lockValue() {
	    		disallowFurtherChanges = true;
	    	}
	    	
	    	public void setValue(float amt) {
	    		if (disallowFurtherChanges)
	    			return;
	    		amount = amt;
	    		if (amount < 0)
	    			amount = 0;
	    	}
	    	
	    	public float getAmount() {
	    		return amount;
	    	}
	    	
	    	
	    }
	    
	    public class KnifeHarvest {
	    	
	    	public readonly GameObject hit;
	    	public readonly TechType objectType;
	    	public readonly bool isAlive;
	    	public readonly bool wasAlive;
	    	
	    	public readonly HarvestType harvestType;
	    	public readonly TechType defaultDrop;
	    	
	    	public readonly Dictionary<TechType, int> drops = new Dictionary<TechType, int>();
	    	
	    	internal KnifeHarvest(GameObject go, TechType tt, bool isa, bool was) {
	    		hit = go;
	    		objectType = tt;
	    		isAlive = isa;
	    		wasAlive = was;
				harvestType = CraftData.GetHarvestTypeFromTech(tt);
				defaultDrop = CraftData.GetHarvestOutputData(tt);
	    		
				if ((harvestType == HarvestType.DamageAlive && wasAlive) || (harvestType == HarvestType.DamageDead && !isAlive)) {
					int num = 1;
					if (harvestType == HarvestType.DamageAlive && !isAlive)
						num += CraftData.GetHarvestFinalCutBonus(tt);
					
					if (defaultDrop != TechType.None)
						drops[defaultDrop] = num;
				}
	    	}	    	
	    }
	    
	    public class BiomeCheck {
	    	
	    	public readonly string originalValue;
	    	public readonly Vector3 position;
	    	
	    	private bool disallowFurtherChanges;
	    	
	    	internal string biome;
	    	
	    	internal BiomeCheck(string amt, Vector3 pos) {
	    		originalValue = amt;
	    		biome = originalValue;
	    		position = pos;
	    		disallowFurtherChanges = false;
	    	}
	    	
	    	public void lockValue() {
	    		disallowFurtherChanges = true;
	    	}
	    	
	    	public void setValue(string b) {
	    		if (disallowFurtherChanges)
	    			return;
	    		biome = b;
	    	}
	    	
	    }
	    /*
	    public class MusicSelectionCheck {
	    	
	    	public readonly string originalBiome;
	    	public readonly MusicManager manager;
	    	
	    	private bool disallowFurtherChanges;
	    	
	    	internal string biomeToDelegateTo;
	    	
	    	internal MusicSelectionCheck(string biome, MusicManager mgr) {
	    		originalBiome = biome;
	    		biomeToDelegateTo = originalBiome;
	    		manager = mgr;
	    		disallowFurtherChanges = false;
	    	}
	    	
	    	public void lockValue() {
	    		disallowFurtherChanges = true;
	    	}
	    	
	    	public void setValue(string b) {
	    		if (disallowFurtherChanges)
	    			return;
	    		biomeToDelegateTo = b;
	    	}
	    	
	    }*/
	    
	    public class WaterTemperatureCalculation {
	    	
	    	public readonly float originalValue;
	    	public readonly Vector3 position;
	    	public readonly WaterTemperatureSimulation manager;
	    	
	    	private bool disallowFurtherChanges;
	    	
	    	internal float temperature;
	    	
	    	internal WaterTemperatureCalculation(float amt, WaterTemperatureSimulation sim, Vector3 pos) {
	    		originalValue = amt;
	    		temperature = originalValue;
	    		position = pos;
	    		manager = sim;
	    		disallowFurtherChanges = false;
	    	}
	    	
	    	public void lockValue() {
	    		disallowFurtherChanges = true;
	    	}
	    	
	    	public float getTemperature() {
	    		return temperature;
	    	}
	    	
	    	public void setValue(float amt) {
	    		//SNUtil.writeToChat("Setting water temp to "+amt);
	    		if (disallowFurtherChanges)
	    			return;
	    		temperature = amt;
	    	}
	    	
	    }
	    
	    public class WaterFogValues {
	    	
	    	public readonly Color originalColor;
	    	public readonly float originalDensity;
	    	public readonly float originalSunValue;
	    	
	    	public Color color;
	    	public float density;
	    	public float sunValue;
	    	
	    	internal WaterFogValues(Color c, float d, float s) {
	    		originalColor = c;
	    		originalDensity = d;
	    		originalSunValue = s;
	    		density = d;
	    		color = c;
	    		sunValue = s;
	    	}
	    	
	    }
	    
	    public class BuildabilityCheck {
	    	
	    	public readonly bool originalValue;
	    	public readonly Collider placeOn;
	    	
	    	public bool placeable;
	    	public bool ignoreSpaceRequirements = false;
	    	
	    	internal BuildabilityCheck(bool orig, Collider pos) {
	    		originalValue = orig;
	    		placeable = orig;
	    		placeOn = pos;
	    	}
	    	
	    }
	    
	    public class StoryHandCheck {
	    	
	    	public readonly Story.StoryGoal originalValue;
	    	public readonly StoryHandTarget component;
	    	
	    	public bool usable = true;
	    	public Story.StoryGoal goal;
	    	
	    	internal StoryHandCheck(Story.StoryGoal orig, StoryHandTarget tgt) {
	    		originalValue = orig;
	    		goal = orig;
	    		component = tgt;
	    	}
	    	
	    }
	    
	    public class RadiationCheck {
	    	
	    	public readonly Vector3 position;
	    	public readonly float originalValue; //0-1
	    	
	    	public float value;
	    	
	    	internal RadiationCheck(Vector3 pos, float orig) {
	    		originalValue = orig;
	    		value = orig;
	    		position = pos;
	    	}
	    	
	    }
	    
	    public class PropulsibilityCheck {
	    	
	    	public readonly GameObject obj;
	    	public readonly float originalValue;
	    	public readonly MonoBehaviour gunComponent;
	    	public readonly bool isMass;
	    	
	    	public float value;
	    	
	    	internal PropulsibilityCheck(GameObject go, float orig, MonoBehaviour gun, bool mass) {
	    		originalValue = orig;
	    		value = orig;
	    		obj = go;
	    		isMass = mass;
	    		gunComponent = gun;
	    	}
	    	
	    }
	    
	    public class SolarEfficiencyCheck {
	    	
	    	public readonly SolarPanel panel;
	    	public readonly float originalValue;
	    	
	    	public float value;
	    	
	    	internal SolarEfficiencyCheck(SolarPanel pos, float orig) {
	    		originalValue = orig;
	    		value = orig;
	    		panel = pos;
	    	}
	    	
	    }
	    
	    public class BulkheadLaserCutterHoverCheck {
	    	
	    	public readonly Sealed obj;
	    	
	    	public string refusalLocaleKey = null;
	    	
	    	internal BulkheadLaserCutterHoverCheck(Sealed s) {
	    		obj = s;
	    	}
	    	
	    }
	    
	    public class DepthCompassCheck {
	    	
	    	public readonly int originalValue;
	    	public readonly int originalCrushValue;
	    	
	    	public int value;
	    	public int crushValue;
	    	
	    	internal DepthCompassCheck(int orig, int crush) {
	    		originalValue = orig;
	    		value = orig;
	    		
	    		originalCrushValue = crush;
	    		crushValue = crush;
	    	}
	    }
	    
	    public class DroppabilityCheck {
	    	
	    	public readonly Pickupable item;
	    	public readonly bool notify;
	    	public readonly bool defaultAllow;
	    	
	    	public bool allow;
	    	public string error = null;
	    	
	    	internal DroppabilityCheck(Pickupable pp, bool n, bool a) {
	    		item = pp;
	    		notify = n;
	    		defaultAllow = a;
	    		allow = defaultAllow;
	    	}
	    }
    
	    public static void onTick(DayNightCycle cyc) {
	    	if (BuildingHandler.instance.isEnabled) {
		    	if (GameInput.GetButtonDown(GameInput.Button.LeftHand)) {
		    		BuildingHandler.instance.handleClick(KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl));
		    	}
	    		if (GameInput.GetButtonDown(GameInput.Button.RightHand)) {
		    		BuildingHandler.instance.handleRClick(KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl));
		    	}
		    	
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Delete)) {
		    		BuildingHandler.instance.deleteSelected();
		    	}
		    	
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftAlt)) {
		    		BuildingHandler.instance.manipulateSelected();
		    	}
	    	}
	    	
	    	CustomBiome.tickMusic(cyc);
	    	
	    	if (getWorldAge() > 0.25F) {
	    		SaveSystem.populateLoad();
	    	}
	    	
	    	if (onDayNightTickEvent != null)
	    		onDayNightTickEvent.Invoke(cyc);
	    }
	    
	    public static void onMainMenuLoaded() {
	    	worldLoadTime = -1;
	    }
	    
	    public static void onWorldLoaded() {
	    	bool warnRestart = hasLoadedAWorld;
	    	hasLoadedAWorld = true;
	    	worldLoadTime = Time.time;
	    	SNUtil.log("Intercepted world load", SNUtil.diDLL);
	    	DuplicateRecipeDelegate.updateLocale();
	    	CustomEgg.updateLocale();
	    	PickedUpAsOtherItem.updateLocale();
	    	SeamothModule.updateLocale();
	    	/*
    		SNUtil.log("Item goals:", SNUtil.diDLL);
	    	foreach (Story.ItemGoal g in Story.StoryGoalManager.main.itemGoalTracker.goals)
	    		SNUtil.log(g.key+" from "+g.techType, SNUtil.diDLL);
    		SNUtil.log("Location goals:", SNUtil.diDLL);
	    	foreach (Story.LocationGoal g in Story.StoryGoalManager.main.locationGoalTracker.goals)
	    		SNUtil.log(g.key+" at "+g.location+" ("+g.position+")", SNUtil.diDLL);
    		SNUtil.log("Biome goals:", SNUtil.diDLL);
	    	foreach (Story.BiomeGoal g in Story.StoryGoalManager.main.biomeGoalTracker.goals)
	    		SNUtil.log(g.key+" in "+g.biome, SNUtil.diDLL);
    		SNUtil.log("Compound goals:", SNUtil.diDLL);
	    	foreach (Story.CompoundGoal g in Story.StoryGoalManager.main.compoundGoalTracker.goals)
	    		SNUtil.log(g.key+" of ["+string.Join(", ",g.preconditions)+"]", SNUtil.diDLL);
	    	*/
	    	LanguageHandler.SetLanguageLine("BulkheadInoperable", "Bulkhead is inoperable");
	    	
	    	//SaveSystem.populateLoad();
	    	
	    	List<ModVersionCheck> vers = ModVersionCheck.getOutdatedVersions();
			updateNotice.SetLocation(0, 250);
			updateNotice.SetSize(24);
			updateNotice.SetColor(Color.yellow);
			List<string> li = new List<string>();
	    	if (vers.Count > 0) {
				li.Add("Your versions of the following mods are out of date:");
				foreach (ModVersionCheck mv in vers) {
					li.Add(mv.modName+": Current version "+mv.currentVersion+", newest version "+mv.remoteVersion);
				}
				li.Add("Update your mods to remove this warning.");
	    	}
			vers = ModVersionCheck.getErroredVersions();
	    	if (vers.Count > 0) {
				li.Add("Several mods failed to fetch version information:");
				foreach (ModVersionCheck mv in vers) {
					li.Add(mv.modName+": Installed version "+mv.currentVersion+", remote version "+mv.remoteVersion);
				}
				li.Add("You should redownload and reinstall mods with local errors and contact Reika if remote versions are invalid.");
	    	}
			if (warnRestart)
				li.Add("You have reloaded a save without exiting the game. This breaks mod loading and will damage your world. Restart your game when changing/reloading saves.");
			if (li.Count > 0)
				updateNotice.ShowMessage(string.Join("\n", li));
			else
				updateNotice.Hide();
	    	
	    	if (onWorldLoadedEvent != null)
	    		onWorldLoadedEvent.Invoke();
	    }
	    
	    public static void hideVersions() {
	    	updateNotice.Hide();
	    }
	    
	    public static float getWorldAge() {
	    	return worldLoadTime < 0 ? -1 : Time.time-worldLoadTime;
	    }
	    
	    public static bool isWorldLoaded() {
	    	return worldLoadTime > 0;
	    }
	    
	    public static void tickPlayer(Player ep) {
	    	CustomBiome b = BiomeBase.getBiome(Camera.main.transform.position) as CustomBiome;
	    	if (currentCustomBiome != b)
	    		recomputeFog();
	    	currentCustomBiome = b;
	    	if (Time.timeScale <= 0)
	    		return;
	    	updateNotice.SetColor(Color.yellow);
	    	
	    	StoryHandler.instance.tick(ep);
	    	
	    	if (onPlayerTickEvent != null)
	    		onPlayerTickEvent.Invoke(ep);
	    }
	    
	    public static void tickSeamoth(SeaMoth sm) {
	    	if (Time.timeScale <= 0)
	    		return;
	    	
	    	if (onSeamothTickEvent != null)
	    		onSeamothTickEvent.Invoke(sm);
	    }
	    
	    public static void tickPrawn(Exosuit sm) {
	    	if (Time.timeScale <= 0)
	    		return;
	    	
	    	if (onPrawnTickEvent != null)
	    		onPrawnTickEvent.Invoke(sm);
	    }
	    
	    public static void tickSub(SubRoot sub) {
	    	if (Time.timeScale <= 0)
	    		return;
	    	
	    	if (sub.isCyclops && onCyclopsTickEvent != null)
	    		onCyclopsTickEvent.Invoke(sub);
	    }
	    
	    public static void updateSeamothModules(SeaMoth sm, int slotID, TechType techType, bool added) {
			for (int i = 0; i < sm.slotIDs.Length; i++) {
				string slot = sm.slotIDs[i];
				TechType techTypeInSlot = sm.modules.GetTechTypeInSlot(slot);
				if (techTypeInSlot != TechType.None) {
					Spawnable sp = ItemRegistry.instance.getItem(techTypeInSlot, false);
					if (sp is SeamothDepthModule) {
						sm.crushDamage.SetExtraCrushDepth(Mathf.Max(((SeamothDepthModule)sp).depthBonus, sm.crushDamage.extraCrushDepth));
					}
				}
			}
	    	
	    	if (onSeamothModulesChangedEvent != null)
	    		onSeamothModulesChangedEvent.Invoke(sm, slotID, techType, added);
	    }
	    
	    public static void updateCyclopsModules(SubRoot sm) {	    	
	    	if (onCyclopsModulesChangedEvent != null)
	    		onCyclopsModulesChangedEvent.Invoke(sm);
	    }
	    
	    public static void updatePrawnModules(Exosuit sm, int slotID, TechType techType, bool added) {
	    	if (onPrawnModulesChangedEvent != null)
	    		onPrawnModulesChangedEvent.Invoke(sm, slotID, techType, added);
	    }
	    
	    public static void useSeamothModule(SeaMoth sm, TechType techType, int slotID) {
			Spawnable sp = ItemRegistry.instance.getItem(techType, false);
			if (sp is SeamothModule) {
				SeamothModule smm = (SeamothModule)sp;
				smm.onFired(sm, slotID, sm.GetSlotCharge(slotID));
				sm.quickSlotTimeUsed[slotID] = Time.time;
				sm.quickSlotCooldown[slotID] = smm.getUsageCooldown();
			}
	    	if (onSeamothModuleUsedEvent != null)
	    		onSeamothModuleUsedEvent.Invoke(sm, techType, slotID);
	    }
	    
	    public static float getWaterTemperature(float ret, WaterTemperatureSimulation sim, Vector3 pos) {
	    	if (getTemperatureEvent != null) {
	    		WaterTemperatureCalculation calc = new WaterTemperatureCalculation(ret, sim, pos);
	    		getTemperatureEvent.Invoke(calc);
	   			return calc.temperature;
	    	}
	    	else {
	    		return ret;
	    	}
	    }
   
		public static float recalculateDamage(float damage, DamageType type, GameObject target, GameObject dealer) {
	    	if (DIMod.config.getBoolean(DIConfig.ConfigEntries.INFITUBE) && ObjectUtil.isCoralTube(target))
	    		return Mathf.Min(damage, target.FindAncestor<LiveMixin>().health-1);
	    	if (onDamageEvent != null) {
	    		DamageToDeal deal = new DamageToDeal(damage, type, target, dealer);
	    		onDamageEvent.Invoke(deal);
	   			return deal.amount;
	    	}
	    	else {
	    		return damage;
	    	}
		}
	    
	    public static string getBiomeAt(string orig, Vector3 pos) {
	    	if (getBiomeEvent != null) {
	    		BiomeCheck deal = new BiomeCheck(orig, pos);
	    		getBiomeEvent.Invoke(deal);
	   			return deal.biome;
	    	}
	    	else {
	    		return orig;
	    	}
	    }
	    
	    public static void doKnifeHarvest(Knife caller, GameObject target, bool isAlive, bool wasAlive) {
			TechType tt = CraftData.GetTechType(target);
			if (tt == TechType.Creepvine)
				GoalManager.main.OnCustomGoalEvent("Cut_Creepvine");
			if (tt == TechType.BigCoralTubes && DIMod.config.getBoolean(DIConfig.ConfigEntries.INFITUBE) && target.FindAncestor<LiveMixin>().health <= 2)
				wasAlive = false;
			KnifeHarvest harv = new KnifeHarvest(target, tt, isAlive, wasAlive);
			if (knifeHarvestEvent != null) {
				knifeHarvestEvent.Invoke(harv);
			}
			foreach (KeyValuePair<TechType, int> kvp in harv.drops)
				CraftData.AddToInventory(kvp.Key, kvp.Value, false, false);
	    }
    
	    public static void onItemPickedUp(Pickupable p) {
	    	TechType tt = p.GetTechType();
	    	PickedUpAsOtherItem mapTo = PickedUpAsOtherItem.getPickedUpAsOther(tt);
	    	if (mapTo != null) {
		    	Inventory.main.container.DestroyItem(tt);
		    	UnityEngine.Object.DestroyImmediate(p.gameObject);
	    		TechType tt2 = mapTo.getTemplate();
	    		int n = mapTo.getNumberCollectedAs();
		    	SNUtil.log("Converting pickup '"+p+"' to '"+tt2+"' x"+n, SNUtil.diDLL);
	    		for (int i = 0; i < n; i++) {
		    		GameObject go = ObjectUtil.createWorldObject(tt2);
		    		p = go.GetComponent<Pickupable>();
		    		Inventory.main.Pickup(p, false);
	    		}
		    	SNUtil.log("Conversion complete", SNUtil.diDLL);
	    		tt = tt2;
	    	}
	    	
	    	if (tt == TechType.None) {
		    	TechTag tag = p.gameObject.GetComponent<TechTag>();
		    	if (tag)
		    		tt = tag.type;
	    	}
	    	if (tt == TechType.None) {
	    		PrefabIdentifier pi = p.gameObject.GetComponent<PrefabIdentifier>();
	    		if (pi)
	    			tt = CraftData.entClassTechTable[pi.ClassId];
	    	}
	    	if (tt != TechType.None)
	    		TechnologyUnlockSystem.instance.triggerDirectUnlock(tt);
	    	
	    	foreach (Renderer r in p.gameObject.GetComponentsInChildren<Renderer>()) {
				foreach (Material m in r.materials) {
					m.DisableKeyword("FX_BUILDING");
				}
			}
	    	
	    	GenUtil.CustomCrate cc = WorldUtil.getClosest<GenUtil.CustomCrate>(p.gameObject);
	    	if (cc)
	    		cc.onPickup(p);
	    	
	    	if (onItemPickedUpEvent != null)
	    		onItemPickedUpEvent.Invoke(p);
	    }
    
	    public static void onEntityRegister(CellManager cm, LargeWorldEntity lw) {
	    	if (worldLoadTime < 0) {
	    		onWorldLoaded();
	    	}/*
	    	if (lw.cellLevel != LargeWorldEntity.CellLevel.Global) {
	    		BatchCells batchCells;
				Int3 block = cm.streamer.GetBlock(lw.transform.position);
				Int3 key = block / cm.streamer.blocksPerBatch;
				if (cm.batch2cells.TryGetValue(key, out batchCells)) {
							Int3 u = block % cm.streamer.blocksPerBatch;
							Int3 cellSize = BatchCells.GetCellSize((int)lw.cellLevel, cm.streamer.blocksPerBatch);
							Int3 cellId = u / cellSize;
							bool flag = cellId.x < 0 || cellId.y < 0 || cellId.z < 0;
					if (!flag) {
			    		try {
							//batchCells.Get(cellId, (int)lw.cellLevel);
							batchCells.GetCells((int)lw.cellLevel).Get(cellId);
			    		}
			    		catch {
							flag = true;
			    		}
					}
					if (flag) {
						SNUtil.log("Moving object "+lw.gameObject+" to global cell, as it is outside the world bounds and was otherwise going to bind to an OOB cell.");
		    			lw.cellLevel = LargeWorldEntity.CellLevel.Global;
					}
				}
	    	}*/
	    	if (onEntityRegisterEvent != null)
	    		onEntityRegisterEvent.Invoke(cm, lw);
	    }
	    
	    public static void onPopup(uGUI_PopupNotification gui) {/*
	    	System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
	    	t.ToString();*/
			//SNUtil.log("TRIGGER POPUP UNLOCK "+System.Environment.StackTrace, SNUtil.diDLL);
	    }
	    
	    public static void onFarmedPlantGrowingSpawn(Plantable p, GameObject plant) {
	    	TechTag tt = p.gameObject.GetComponent<TechTag>();
	    	if (tt) {
	    		BasicCustomPlant plantType = BasicCustomPlant.getPlant(tt.type);
	    		//SNUtil.writeToChat("Planted "+tt+" > "+plantType);
	    		if (plantType != null) {
	    			//SNUtil.writeToChat(plant.GetComponentsInChildren<Renderer>(true).Length+" Renderers");
		    		RenderUtil.swapToModdedTextures(plant.GetComponentInChildren<Renderer>(true), plantType);
		    		plant.gameObject.EnsureComponent<TechTag>().type = plantType.TechType;
		    		plant.gameObject.EnsureComponent<PrefabIdentifier>().ClassId = plantType.ClassID;
	    		}
	    	}
	    }
	    
	    public static void onFarmedPlantGrowDone(GrowingPlant p, GameObject plant) {
	    	TechTag tt = p.gameObject.GetComponent<TechTag>();
	    	if (tt) {
	    		BasicCustomPlant plantType = BasicCustomPlant.getPlant(tt.type);
	    		//SNUtil.writeToChat("Grew "+tt+" > "+plantType);
	    		if (plantType != null) {
	    			ObjectUtil.convertTemplateObject(plant, plantType);
	    		}
	    	}
	    }
	    
	    public static void onDockingBaySpawn(VehicleDockingBay b) {
	    	b.gameObject.EnsureComponent<DockLock>();
	    }
	    
	    public static void onSkyApplierSpawn(SkyApplier pk) {
	    	if (pk.GetComponent<Vehicle>()) {
	    		pk.gameObject.EnsureComponent<FixedBounds>()._bounds = new Bounds(Vector3.zero, Vector3.one*5);
	    		GameObject go = ObjectUtil.getChildObject(pk.gameObject, "LavaWarningTrigger");
	    		if (!go) {
	    			go = new GameObject("LavaWarningTrigger");
	    			go.transform.localPosition = Vector3.zero;
	    			go.transform.localRotation = Quaternion.identity;
	    			go.transform.SetParent(pk.transform);
	    		}
	    		SphereCollider sp = go.EnsureComponent<SphereCollider>();
	    		sp.center = Vector3.zero;
	    		sp.radius = NEAR_LAVA_RADIUS;
	    		sp.isTrigger = true;
	    		go.EnsureComponent<LavaWarningTriggerDetector>();
	    	}
	    	FruitPlant fp = pk.GetComponent<FruitPlant>();
	    	if (fp) {
	    		fp.gameObject.EnsureComponent<FruitPlantTag>().setPlant(fp);
	    	}
	    	if (onSkyApplierSpawnEvent != null)
	    		onSkyApplierSpawnEvent.Invoke(pk);
	    }
	    
	    //private static bool needsLavaDump = true;
	    
	    public class FruitPlantTag : MonoBehaviour {
	    	
	    	private FruitPlant plant;
	    	private float baseGrowthTime;
	    	
	    	private float lastTickTime = -1;
	    	
	    	internal void setPlant(FruitPlant fp) {
	    		plant = fp;
	    		baseGrowthTime = fp.fruitSpawnInterval;
	    	}
	    	
	    	public FruitPlant getPlant() {
	    		return plant;
	    	}
	    	
	    	public float getBaseGrowthTime() {
	    		return baseGrowthTime;
	    	}
	    	
	    	void Update() {
	    		if (onFruitPlantTickEvent != null) {
	    			float time = DayNightCycle.main.timePassedAsFloat;
	    			if (time-lastTickTime >= 0.5F) {
	    				lastTickTime = time;
	    				onFruitPlantTickEvent.Invoke(this);
	    			}
	    		}
	    	}
	    	
	    }
	    
	    public class DockLock : MonoBehaviour {
	    	
	    	private VehicleDockingBay bay;
	    	
	    	private float lastTime;
	    	
	    	public void Update() {
	    		if (!bay)
	    			bay = GetComponent<VehicleDockingBay>();
	    		
	    		if (bay.dockedVehicle && DayNightCycle.main.timePassedAsFloat-lastTime >= 0.5F && !bay.dockedVehicle.GetComponentInParent<SubRoot>()) {
	    			bay.DockVehicle(bay.dockedVehicle, false);
	    			SNUtil.writeToChat("Re-binding vehicle "+bay.dockedVehicle+" to docking bay "+bay.gameObject.GetFullHierarchyPath());
	    			lastTime = DayNightCycle.main.timePassedAsFloat;
	    		}
	    	}
	    	
	    }
	    
	    public class LavaWarningTriggerDetector : MonoBehaviour {
	    	
	    	private TemperatureDamage damage;
	    	private Vehicle vehicle;
	    	private Collider sphere;
	    	
	    	private float lastLavaTime = -1;
	    	private float lastGeyserTime = -1;
	    	
	    	private float lastCheckTime = -1;
	    	
	    	private static readonly List<Vector3> spherePoints = new List<Vector3>();
	    	private static readonly int RAYS_PER_TICK = 10;
	    	private static int spherePointIndex = 0;
	    	
	    	private float ambientTemperatureMinusLava;
	    	
	    	static LavaWarningTriggerDetector() {
	    		computePoints();
	    	}
	    	
	    	private static void computePoints() {
	    		float phi = Mathf.PI * (3F - Mathf.Sqrt(5F));  // golden angle in radians
			    for (int i = 0; i < 100; i++) {
	    			float y = 1 - (i / (100 - 1F)) * 2;  // y goes from 1 to -1
			        float radius = Mathf.Sqrt(1 - y * y);  // radius at y
			
			        float theta = phi * i;  // golden angle increment
			
			        float x = Mathf.Cos(theta) * radius;
			        float z = Mathf.Sin(theta) * radius;
			
			        spherePoints.Add(new Vector3(x, y, z));
			    }
	    		for (int i = 0; i < 150; i++) {
	    			float ang = UnityEngine.Random.Range(0F, 360F);
			        float x = Mathf.Cos(Mathf.Deg2Rad*ang) * NEAR_LAVA_RADIUS;
			        float z = Mathf.Sin(Mathf.Deg2Rad*ang) * NEAR_LAVA_RADIUS;
			        spherePoints.Add(new Vector3(x, -UnityEngine.Random.Range(0F, 1F), z));
	    		}
	    		spherePoints.Shuffle();
	    	}
	    	
	    	void Update() {
	    		if (!damage)
	    			damage = gameObject.FindAncestor<TemperatureDamage>();
	    		if (!vehicle)
	    			vehicle = gameObject.FindAncestor<Vehicle>();
	    		if (!sphere)
	    			sphere = gameObject.GetComponent<Collider>();
	    		gameObject.transform.localPosition = Vector3.zero;
	    		
	    		float time = DayNightCycle.main.timePassedAsFloat;
	    		float dT = time-lastCheckTime;
	    		if (dT >= 0.5) {
	    			lastCheckTime = time;
	    			ambientTemperatureMinusLava = WaterTemperatureSimulation.main.GetTemperature(transform.position);
	    		}
	    		if (damage && ambientTemperatureMinusLava >= 90)
	    			checkNearbyLava();
	    	}
	    	
	    	private void checkNearbyLava() {
	    		for (int i = spherePointIndex; i < Math.Min(spherePointIndex+RAYS_PER_TICK, spherePoints.Count); i++) {
	    			Vector3 vec = spherePoints[i];
	    			RaycastHit[] hits = Physics.RaycastAll(transform.position, vec.normalized, NEAR_LAVA_RADIUS, Voxeland.GetTerrainLayerMask());
	    			//SNUtil.writeToChat(vec+" > "+hits.Length);
					foreach (RaycastHit hit in hits) {
	    				if (hit.transform && checkLava(hit.point, Vector3.zero)) {
	    					spherePointIndex = i;
	    					return;
	    				}
					}
	    		}
	    		spherePointIndex += RAYS_PER_TICK;
	    		if (spherePointIndex >= spherePoints.Count)
	    			spherePointIndex = 0;
	    	}
	    	
			private void OnTriggerStay(Collider other) {
	    		if (damage && ambientTemperatureMinusLava >= 90) {
		    		Vector3 norm;
		    		checkLava(getCollisionPoint(other, out norm), norm);
	    		}
			}
	    	
			private Vector3 getCollisionPoint(Collider other, out Vector3 norm) {
			    float depth = 0;
			
			    Vector3 ctr = transform.position;
			    if (Physics.ComputePenetration(other, other.transform.position, other.transform.rotation, sphere, ctr, Quaternion.identity, out norm, out depth))
			        return ctr + (norm * (NEAR_LAVA_RADIUS-depth));
			    
			    return Vector3.zero;
			}
	    	
	    	private bool checkLava(Vector3 pt, Vector3 norm) {
	    		//SNUtil.log("Checking lava: "+pt+" @ "+lastLavaTime, SNUtil.diDLL);
	    		if (norm.magnitude < 0.01F)
	    			norm = transform.position - pt;
				if (damage.lavaDatabase.IsLava(pt, norm)) {
	    			markLavaDetected();
	    			//SNUtil.writeToChat("Wide lava: "+pt+" @ "+lastLavaTime);
	    			//SNUtil.log("Is lava", SNUtil.diDLL);
	    			return true;
	    		}
	    		return false;
	    	}
	    	
	    	public void markLavaDetected() {
	    		lastLavaTime = DayNightCycle.main.timePassedAsFloat;
	    	}
	    	
	    	public void markGeyserDetected() {
	    		lastGeyserTime = DayNightCycle.main.timePassedAsFloat;
	    	}
	    	
	    	public bool isInGeyser() {
	    		return Mathf.Abs(DayNightCycle.main.timePassedAsFloat-lastGeyserTime) <= 0.5F;
	    	}
	    	
	    	public bool isInLava() {/*
	    		if (needsLavaDump) {
	    			dmg.lavaDatabase.LazyInitialize();
	    			needsLavaDump = false;
	    			List<string> li = new List<string>();
	    			Dictionary<byte, List<BlockTypeClassification>> db = dmg.lavaDatabase.lavaBlocks;
	    			foreach (KeyValuePair<byte, List<BlockTypeClassification>> kvp in db) {
	    				List<BlockTypeClassification> li0 = kvp.Value;
	    				li.Add("==========================");
	    				li.Add("Byte "+kvp.Key+": "+li0.Count+" entries: ");
	    				foreach (BlockTypeClassification bb in li0) {
	    					li.Add("Type "+bb.blockType+", inclination ["+bb.minInclination+"-"+bb.maxInclination+"], mat='"+bb.material+"'");
	    				}
	    				li.Add("==========================");
	    				li.Add("");
	    			}
	    			string path = "E:/INet/SNlavadump.txt";
	    			File.WriteAllLines(path, li);
	    		}*/
	    		return Mathf.Abs(DayNightCycle.main.timePassedAsFloat-lastLavaTime) <= 2;
	    	}
	    	
	    }
	    
	    public static void onStoryGoalCompleted(string key) {
	    	StoryHandler.instance.NotifyGoalComplete(key);
	    }
	    
	    public static bool isItemUsable(TechType tt) {
	    	return tt == TechType.Bladderfish || UsableItemRegistry.instance.isUsable(tt);
	    }
	    
	    public static bool useItem(Survival s, GameObject useObj) {
			bool flag = false;
			if (useObj != null) {
				TechType tt = CraftData.GetTechType(useObj);
				if (tt == TechType.None) {
					Pickupable component = useObj.GetComponent<Pickupable>();
					if (component)
						tt = component.GetTechType();
				}
				SNUtil.log("Player used item "+tt, SNUtil.diDLL);
				flag = UsableItemRegistry.instance.use(tt, s, useObj);
				if (flag)
					FMODUWE.PlayOneShot(CraftData.GetUseEatSound(tt), Player.main.transform.position, 1f);
			}
			return flag;
	    }
	    
	    public static bool isItemDroppable(Pickupable pp, bool notify) {
	    	bool flag = Inventory.CanDropItemHere(pp, notify);
	    	if (pp && droppabilityEvent != null) {
	    		DroppabilityCheck dropCheck = new DroppabilityCheck(pp, notify, flag);
	    		droppabilityEvent.Invoke(dropCheck);
	    		flag = dropCheck.allow;
	    		if (notify && !flag && !string.IsNullOrEmpty(dropCheck.error)) {
	    			ErrorMessage.AddError(dropCheck.error);
	    		}
	    	}
	    	return flag;
	    }
	   
		public static void onScanComplete(PDAScanner.EntryData data) {
		   	if (data != null)
	   			TechnologyUnlockSystem.instance.triggerDirectUnlock(data.key);
		}
	    
	    public static void tickLaserCutting(Sealed s, float amt) {
			if (s._sealed && s.maxOpenedAmount >= 0) {
	    		string key = null;
	    		if (s.GetComponent<BulkheadDoor>() && bulkheadLaserHoverEvent != null) {
	    			BulkheadLaserCutterHoverCheck ch = new BulkheadLaserCutterHoverCheck(s);
				   	bulkheadLaserHoverEvent.Invoke(ch);
				   	key = ch.refusalLocaleKey;
	    		}
	    		if (string.IsNullOrEmpty(key)) {
					s.openedAmount = Mathf.Min(s.openedAmount + amt, s.maxOpenedAmount);
					if (Mathf.Approximately(s.openedAmount, s.maxOpenedAmount)) {
						s._sealed = false;
						s.openedEvent.Trigger(s);
						//Debug.Log("Trigger opened event");
					}
	    		}
			}
	    }
	    
	    public static void getBulkheadMouseoverText(BulkheadDoor bk) {
			if (bk.enabled && bk.state == BulkheadDoor.State.Zero) {
	    		Sealed s = bk.GetComponent<Sealed>();
	    		if (s && s.IsSealed()) {
	    			if (s.maxOpenedAmount < 0) {
	    				HandReticle.main.SetInteractText("BulkheadInoperable");
						HandReticle.main.SetIcon(HandReticle.IconType.None, 1f);
	    			}
	    			else {
	    				string key = null;
	    				if (bulkheadLaserHoverEvent != null) {
	    					BulkheadLaserCutterHoverCheck ch = new BulkheadLaserCutterHoverCheck(s);
				    		bulkheadLaserHoverEvent.Invoke(ch);
				    		key = ch.refusalLocaleKey;
							HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
	    				}
	    				if (string.IsNullOrEmpty(key)) {
							HandReticle.main.SetInteractText("SealedInstructions"); //is a locale key
							HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
							HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
	    				}
	    				else {
	    					HandReticle.main.SetInteractText(key);
							HandReticle.main.SetIcon(HandReticle.IconType.None, 1f);
	    				}
	    			}
	    		}
	    		else {
					HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
					HandReticle.main.SetInteractText(bk.targetState ? "Close" : "Open");
	    		}
			}
	    }
	    
	    public static void onBulkheadClick(BulkheadDoor bk) {
			Base componentInParent = bk.GetComponentInParent<Base>();
			//PreventDeconstruction prev = bk.GetComponentInParent<PreventDeconstruction>();
			Sealed s = bk.GetComponent<Sealed>();
			if (s != null && s.IsSealed()) {
				
			}
			else if (componentInParent != null && !componentInParent.isReady) {
				bk.ToggleImmediately();
			}
			else if (bk.enabled && bk.state == BulkheadDoor.State.Zero) {
				if (GameOptions.GetVrAnimationMode()) {
					bk.ToggleImmediately();
					return;
				}
				bk.SequenceDone();
			}
	    }
	   
	   	public static bool isInsideForHatch(UseableDiveHatch hatch) {
	   		SeabaseReconstruction.WorldgenBaseWaterparkHatch wb = hatch.gameObject.GetComponent<SeabaseReconstruction.WorldgenBaseWaterparkHatch>();
	   		if (wb)
	   			return wb.isPlayerInside();
	   		return Player.main.IsInsideWalkable() && Player.main.currentWaterPark == null;
	   	}
	    
	    public static void onConstructionComplete(TechType tt, Constructable c) {
	    	Story.ItemGoalTracker.OnConstruct(tt);
	    	
	    	TechnologyUnlockSystem.instance.triggerDirectUnlock(tt);
	    	
	    	if (onConstructedEvent != null)
	    		onConstructedEvent.Invoke(tt, c);
	    }
	    
	    public static void onKnifed(GameObject go) {
	    	if (go && onKnifedEvent != null)
	    		onKnifedEvent.Invoke(go);
	    }

		public static void hoverSeamothTorpedoStorage(SeaMoth sm, HandTargetEventData data) {
			for (int i = 0; i < sm.slotIDs.Length; i++) {
	    		InventoryItem ii = sm.GetSlotItem(i);
	    		if (ii != null && ii.item) {
	    			SeamothModule.SeamothModuleStorage storage = SeamothModule.getStorageHandler(ii.item.GetTechType());
	    			if (storage != null) {
	    				SeamothStorageContainer component = ii.item.GetComponent<SeamothStorageContainer>();
	    				//SNUtil.writeToChat("Found "+component+" ["+storage.title+"] for "+ii.item.GetTechType());
	    				if (component) {
	    					HandReticle.main.SetInteractText(storage.localeKey);
							HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
	    				}
	    			}
	    		}
			}
		}
	
		public static void openSeamothTorpedoStorage(SeaMoth sm, Transform transf) {
	    	TechType foundMatch = TechType.None;
			Inventory.main.ClearUsedStorage();
			for (int i = 0; i < sm.slotIDs.Length; i++) {
	    		InventoryItem ii = sm.GetSlotItem(i);
	    		if (ii != null && ii.item) {
	    			TechType tt = ii.item.GetTechType();
	    			if (foundMatch == tt || foundMatch == TechType.None) {
		    			SeamothModule.SeamothModuleStorage storage = SeamothModule.getStorageHandler(tt);
		    			if (storage != null) {
		    				SeamothStorageContainer component = ii.item.GetComponent<SeamothStorageContainer>();
		    				if (component) {
	    						foundMatch = tt;
		    					storage.apply(component);
								Inventory.main.SetUsedStorage(component.container, true);
		    				}
		    			}
	    			}
	    		}
			}
			if (foundMatch != TechType.None) {
	    		//SNUtil.writeToChat("Opening "+SeamothModule.getStorageHandler(foundMatch).title+" for "+foundMatch);
				Player.main.GetPDA().Open(PDATab.Inventory, transf, null, -1f);
			}
		}
	    
	    public static float getTemperatureForDamage(TemperatureDamage dmg) {
	    	if (Mathf.Abs(Time.time-dmg.timeDamageStarted) <= 2.5F) { //active lava dmg
	    		//SNUtil.writeToChat(dmg+" Touch lava: "+dmg.timeDamageStarted+" > "+Mathf.Abs(Time.time-dmg.timeDamageStarted));
	    		return 1200;
	    	}
	    	LavaWarningTriggerDetector warn = dmg.GetComponentInChildren<LavaWarningTriggerDetector>();
	    	if (warn && warn.isInLava())
	    		return dmg.gameObject.FindAncestor<Exosuit>() ? 300 : 400;
	    	if (warn && warn.isInGeyser())
	    		return 180;
	    	Vehicle v = dmg.GetComponent<Vehicle>();
	    	if (v)
	    		return v.precursorOutOfWater ? 25 : v.GetTemperature();
	    	return WaterTemperatureSimulation.main.GetTemperature(dmg.transform.position);
	    }
	    
	    public static void pingSonar(SNCameraRoot cam) {
	    	if (cam && onSonarUsedEvent != null)
	    		onSonarUsedEvent.Invoke(cam);
	    }
	    
	    public static void pingSeamothSonar(SeaMoth cam) {
	    	if (cam && onSeamothSonarUsedEvent != null)
	    		onSeamothSonarUsedEvent.Invoke(cam);
	    }
	    
	    public static void pingCyclopsSonar(CyclopsSonarButton cam) {
	    	if (cam && onCyclopsSonarUsedEvent != null) {
	    		SubRoot sb = cam.gameObject.FindAncestor<SubRoot>();
	    		if (sb)
	    			onCyclopsSonarUsedEvent.Invoke(sb);
	    	}
	    }
	    
	    public static void onEggHatched(GameObject hatched) {
	    	if (hatched) {
	    		ObjectUtil.fullyEnable(hatched);
		    	if (onEggHatchedEvent != null)
		    		onEggHatchedEvent.Invoke(hatched);
	    	}
	    }
	    
	    public static void onEMPHit(EMPBlast e, MonoBehaviour com) {
	    	if (com && onEMPHitEvent != null) {
	    		onEMPHitEvent.Invoke(e, com.gameObject);
	    	}
	    }
	    
	    public static void onEMPTouch(EMPBlast e, Collider c) {
	    	if (c && onEMPTouchEvent != null) {
	    		onEMPTouchEvent.Invoke(e, c);
	    	}
	    }
	    
	    public static void appendItemTooltip(StringBuilder sb, TechType tt, GameObject obj) {
	    	InfectedMixin mix = obj.GetComponent<InfectedMixin>();
	    	if (mix) {
	    		string tip = getInfectionTooltip(mix);
	    		if (!string.IsNullOrEmpty(tip))
	    			TooltipFactory.WriteDescription(sb, tip);//TooltipFactory.WriteDescription(sb, "Infected: "+((int)(mix.infectedAmount*100))+"%");
	    	}
	    	Peeper peep = obj.GetComponent<Peeper>();
	    	if (peep && peep.isHero) {
	    		TooltipFactory.WriteDescription(sb, "Contains unusual enzymes.");
	    	}
	    	if (itemTooltipEvent != null) {
	    		itemTooltipEvent.Invoke(sb, tt, obj);
	    	}
	    }
	    
	    private static string getInfectionTooltip(InfectedMixin mix) {
	    	if (mix.IsInfected()) {
	    		float amt = mix.infectedAmount;
	    		//return "Infected: "+((int)(amt*100))+"%";
	    		if (amt >= 0.75) {
	    			return "This creature is severely infected.";
	    		}
	    		else if (amt >= 0.5) {
	    			return "Exhibiting symptoms of significant systemic infection.";
	    		}
	    		else if (amt >= 0.25) {
	    			return "Showing signs of infection.";
	    		}
	    		else {
	    			return "Elevated bacterial levels detected.";
	    		}
	    	}
	    	else {
	    		LiveMixin lv = mix.GetComponent<LiveMixin>();
	    		return !lv || lv.IsAlive() ? "Status: Healthy." : null;
	    	}
	    }
	     /*
	    public static WaterscapeVolume.Settings currentRenderVolume = new WaterscapeVolume.Settings();
	   
	    public static void overrideFog(WaterBiomeManager biomes, Vector3 pos, bool detail, WaterscapeVolume.Settings settings) {
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl)) {
	    		currentRenderVolume.copyObject(settings);
	    	}
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftAlt)) {
	    		settings = currentRenderVolume;
	    	}
	    	biomes.atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value, biomes.GetExtinctionTextureValue(settings));
			biomes.atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value1, biomes.GetScatteringTextureValue(settings));
			biomes.atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value2, biomes.GetEmissiveTextureValue(settings));
	    }
	    
	    public static void onFogRasterized(WaterBiomeManager biomes, Vector3 pos, bool detail) {
	    	SNUtil.writeToChat("Rasterizing fog @ "+pos);
	    }*/
	     
	    public static Vector4 interceptExtinction(Vector4 orig, WaterscapeVolume.Settings settings) {
	     	BiomeBase at = BiomeBase.getBiome(Camera.main.transform.position);
	     	if (at is CustomBiome) {
	     		CustomBiome b = (CustomBiome)at;
	     		float d = b.getMurkiness(settings.murkiness) / 100f;
	     		float scatter = b.getScatteringFactor(settings.scattering);
	     		Vector3 vector = b.getColorFalloff(settings.absorption) + scatter * Vector3.one;
				Vector4 ret = new Vector4(vector.x, vector.y, vector.z, scatter) * d;
				ret.w = b.getFogStart(settings.startDistance);
				return ret;
	     	}
	     	return orig;	
	    }
	     
	    public static Vector4 interceptScattering(Vector4 orig, WaterscapeVolume.Settings settings) {
	     	BiomeBase at = BiomeBase.getBiome(Camera.main.transform.position);
	     	if (at is CustomBiome) {
	     		CustomBiome b = (CustomBiome)at;
	     		float factor = b.getScatterFactor(settings.GetExtinctionAndScatteringCoefficients().w);
				Color linear = b.getWaterColor(settings.scatteringColor.linear);
				Vector4 result;
				result.x = linear.r * factor;
				result.y = linear.g * factor;
				result.z = linear.b * factor;
				result.w = b.getSunScale(settings.sunlightScale) * WaterBiomeManager.main.waterTransmission;
				return result;
	     	}
	     	return orig;
	    }
	     
	    public static Vector4 interceptEmissive(Vector4 orig, WaterscapeVolume.Settings settings) {
	     	BiomeBase at = BiomeBase.getBiome(Camera.main.transform.position);
	     	if (at is CustomBiome)
	     		return ((CustomBiome)at).getEmissiveVector(orig);
	     	return orig;
	    }
	    
	    public static void recomputeFog() {
	    	WaterBiomeManager.main.Rebuild();
	    	WaterBiomeManager.main.BuildSettingsTextures();
	    }
	    
	    public static void dumpWaterscapeTextures() {
	    	WaterBiomeManager wbm = WaterBiomeManager.main;
	    	//string biome = wbm.GetBiome(Camera.main.transform.position);
	    	//SNUtil.writeToChat("Dumping biome textures @ "+biome);
	    	foreach (FieldInfo f in typeof(WaterBiomeManager).GetFields((BindingFlags)0x7fffffff)) {
	    		object get = f.GetValue(wbm);
	    		if (get is RenderTexture) {
	    			SNUtil.writeToChat("Dumping RenderTexture WaterBiomeManager::"+f.Name);
	    			RenderUtil.dumpTexture(SNUtil.diDLL, f.Name, (RenderTexture)get);
	    		}
	    		else if (get is Texture2D) {
	    			SNUtil.writeToChat("Dumping Texture2D WaterBiomeManager::"+f.Name);
	    			RenderUtil.dumpTexture(SNUtil.diDLL, f.Name, (Texture2D)get);
	    		}
	    		else {
	    			//SNUtil.writeToChat("Skipping non-texture object "+get);
	    		}
	    	}
	    }
	    /*
	    public static void interceptChosenFog(WaterscapeVolume vol, Camera cam) {
			vol.SetupWaterPlane(cam, vol.waterPlane);
			vol.biomeManager.SetupConstantsForCamera(cam);
			if (vol.fogEnabled)
				Shader.SetGlobalFloat(ShaderPropertyID._UweFogEnabled, 1f);
			float transmission = vol.GetTransmission();
			Shader.SetGlobalFloat(ShaderPropertyID._UweCausticsScale, vol.causticsScale * vol.surface.GetCausticsWorldToTextureScale());
			Shader.SetGlobalVector(ShaderPropertyID._UweCausticsAmount, new Vector3(vol.causticsAmount, vol.surface.GetCausticsTextureScale() * vol.causticsAmount, vol.surface.GetCausticsTextureScale()));
			Shader.SetGlobalFloat(ShaderPropertyID._UweWaterTransmission, transmission);
			Shader.SetGlobalFloat(ShaderPropertyID._UweWaterEmissionAmbientScale, vol.emissionAmbientScale);
			float depth = (cam.transform.position.y - vol.aboveWaterMinHeight) / (vol.aboveWaterMaxHeight - vol.aboveWaterMinHeight);
			float fogDensity = Mathf.Lerp(1f, vol.aboveWaterDensityScale, depth);
			Shader.SetGlobalFloat(ShaderPropertyID._UweExtinctionAndScatteringScale, fogDensity);
			if (vol.sky != null) {
				Vector3 lightDirection = vol.sky.GetLightDirection();
			if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftAlt))
				lightDirection.x = 1;
			if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl))
				lightDirection.y = 1;
			if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftShift))
				lightDirection.z = 1;
			if (KeyCodeUtils.GetKeyHeld(KeyCode.Tab))
				SNUtil.writeToChat(lightDirection.ToString());
				lightDirection.y = Mathf.Min(lightDirection.y, -0.01f);
				Vector3 camLight = -cam.worldToCameraMatrix.MultiplyVector(lightDirection);
				Color lightColor = vol.sky.GetLightColor();
				Vector4 fogValues = lightColor;
				fogValues.w = vol.sunLightAmount * transmission;
				float brightness = lightColor.r * 0.3f + lightColor.g * 0.59f + lightColor.b * 0.11f;
				Shader.SetGlobalVector(ShaderPropertyID._UweFogVsLightDirection, camLight);
				Shader.SetGlobalVector(ShaderPropertyID._UweFogWsLightDirection, lightDirection);
				Shader.SetGlobalVector(ShaderPropertyID._UweFogLightColor, fogValues);
				Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightGreyscaleColor, brightness);
			}
			else {
				Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightAmount, 0f);
			}
			Shader.SetGlobalVector(ShaderPropertyID._UweColorCastFactor, new Vector2(vol.colorCastDistanceFactor, vol.colorCastDepthFactor));
			Shader.SetGlobalFloat(ShaderPropertyID._UweAboveWaterFogStartDistance, vol.aboveWaterStartDistance);
			Vector3 scatter = default(Vector3);
			scatter.x = (1f - vol.scatteringPhase * vol.scatteringPhase) / 12.566371f;
			scatter.y = 1f + vol.scatteringPhase * vol.scatteringPhase;
			scatter.z = 2f * vol.scatteringPhase;
			Shader.SetGlobalVector(ShaderPropertyID._UweFogMiePhaseConst, scatter);
			Shader.SetGlobalFloat(ShaderPropertyID._UweSunAttenuationFactor, vol.sunAttenuation);
	    }*/
	    /*
	    public static void interceptChosenFog(WaterscapeVolume vol, Camera cam) {
	    	if (!vol || !cam)
	    		return;
			float t = (cam.transform.position.y - vol.aboveWaterMinHeight) / (vol.aboveWaterMaxHeight - vol.aboveWaterMinHeight);
			float fogDensity = Mathf.Lerp(1f, vol.aboveWaterDensityScale, t);
			
	    	Vector4 fogColor = default(Vector4);   	
			if (vol.sky != null)
			{
				Vector3 lightDirection = vol.sky.GetLightDirection();
				lightDirection.y = Mathf.Min(lightDirection.y, -0.01f);
				Vector3 v = -cam.worldToCameraMatrix.MultiplyVector(lightDirection);
				fogColor = vol.sky.GetLightColor();
				fogColor.w = vol.sunLightAmount * vol.GetTransmission();
				Shader.SetGlobalVector(ShaderPropertyID._UweFogVsLightDirection, v);
				Shader.SetGlobalVector(ShaderPropertyID._UweFogWsLightDirection, lightDirection);
			}/*
			CustomBiome b = BiomeBase.getBiome(cam.transform.position) as CustomBiome;
			if (b != null) {
				fogColor = fogColor.setXYZ(b.getFogColor(fogColor.getXYZ()));
				fogColor.w = b.getSunIntensity(fogColor.w);
				fogDensity = b.getFogDensity(fogDensity);
			}
			WaterFogValues wf = new WaterFogValues(fogColor.asColor(), fogDensity, fogColor.w);
	    	if (fogCalculateEvent != null)
	    		fogCalculateEvent.Invoke(wf);
	    	Vector4 vec4 = wf.color.toVectorA();
	    	vec4.w = wf.sunValue;*//*Vector4 vec4 = fogColor;
	    	//SNUtil.writeToChat("Fog color "+vec4+", with density "+fogDensity.ToString("0.000"));
			Shader.SetGlobalVector(ShaderPropertyID._UweFogLightColor, vec4);
			Shader.SetGlobalFloat(ShaderPropertyID._UweExtinctionAndScatteringScale, /*wf.density*//*fogDensity);
			float value3 = fogColor.x * 0.3f + fogColor.y * 0.59f + fogColor.z * 0.11f;
			Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightGreyscaleColor, value3);
			Vector3 v2 = default(Vector3);
			v2.x = (1f - vol.scatteringPhase * vol.scatteringPhase) / 12.566371f;
			v2.y = 1f + vol.scatteringPhase * vol.scatteringPhase;
			v2.z = 2f * vol.scatteringPhase;
			Shader.SetGlobalVector(ShaderPropertyID._UweFogMiePhaseConst, v2);
			Shader.SetGlobalFloat(ShaderPropertyID._UweSunAttenuationFactor, vol.sunAttenuation);
			Shader.SetGlobalVector(ShaderPropertyID._UweColorCastFactor, new Vector2(vol.colorCastDistanceFactor, vol.colorCastDepthFactor));
			Shader.SetGlobalFloat(ShaderPropertyID._UweAboveWaterFogStartDistance, vol.aboveWaterStartDistance);
			//SNUtil.writeToChat("Applying fog of "+vol+" @ "+vol.transform.position);
	    }*/
	    
	    public static bool interceptConstructability(/*Collider c*/) {
	    	bool orig = Builder.UpdateAllowed();
	    	//SNUtil.writeToChat("Testing constructability of "+Builder.constructableTechType+", default value = "+orig);
	    	if (constructabilityEvent != null) {
	    		//SNUtil.writeToChat("Event has listeners");
	    		Transform aimTransform = Builder.GetAimTransform();
				RaycastHit hit;
				Collider target = null;
				if (Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
					target = hit.collider;
				else
					target = null;
				//SNUtil.writeToChat("Placement target: "+target+" "+(target == null ? "" : target.gameObject.GetFullHierarchyPath()));
				//SNUtil.writeToChat("Space check: "+Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, target));
	    		BuildabilityCheck deal = new BuildabilityCheck(orig, target);
	    		constructabilityEvent.Invoke(deal);
	    		//SNUtil.writeToChat("Event state: "+deal.placeable+" / "+deal.ignoreSpaceRequirements);
	    		return deal.placeable;// && (target == null || deal.ignoreSpaceRequirements || Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, target));
	    	}
	    	return orig;
	    }
	    /*
	    public static float getPowerRelayCapacity(float orig, PowerRelay relay) {
	    	SubRoot sub = relay.gameObject.FindAncestor<SubRoot>();
	    	if (sub) {
	    		foreach (CustomMachineLogic lgc in sub.GetComponentsInChildren<CustomMachineLogic>()) {
	    			orig += lgc.getBaseEnergyStorageCapacityBonus();
	    		}
	    	}
	    	return orig;
	    }*//*
	    
	    public static void addPowerToSeabaseDelegateViaPowerSourceSet(PowerSource src, float amt, MonoBehaviour component) {
	    	SubRoot sub = component.gameObject.FindAncestor<SubRoot>();
	    	if (sub) {
	    		sub.powerRelay.AddEnergy(amount, out stored);
	    	}
	    	else {
	    		src.power = amt;
	    	}
	    }*/
	    	
	    public static void updateSolarPanel(SolarPanel p) {
	    	if (!p)
	    		return;
	    	Constructable c = p.gameObject.GetComponent<Constructable>();
			if (c && c.constructed) {
		    	float eff = p.GetRechargeScalar();
		    	if (solarEfficiencyEvent != null) {
		    		SolarEfficiencyCheck ch = new SolarEfficiencyCheck(p, eff);
		    		solarEfficiencyEvent.Invoke(ch);
		    		eff = ch.value;
		    	}
	    		float gen = eff * DayNightCycle.main.deltaTime * 0.25f * 5f;
	    		SubRoot sub = p.gameObject.FindAncestor<SubRoot>();
	    		if (sub) {
	    			float trash;
	    			sub.powerRelay.AddEnergy(gen, out trash);
	    		}
	    		else {
	    			p.powerSource.power = Mathf.Clamp(p.powerSource.power + gen, 0f, p.powerSource.maxPower);	
	    		}
			}
	    }
	    
	    public static bool addPowerToSeabaseDelegate(IPowerInterface pi, float amount, out float stored, MonoBehaviour component) {
	    	SubRoot sub = component.gameObject.FindAncestor<SubRoot>();
	    	if (sub) {
	    		return sub.powerRelay.AddEnergy(amount, out stored);
	    	}
	    	else {
	    		return pi.AddEnergy(amount, out stored);
	    	}
	    }
	    /*
	    public static string getBiomeToUseForMusic(string biome, MusicManager mgr) {
	    	if (musicBiomeChoiceEvent != null) {
	    		MusicSelectionCheck mus = new MusicSelectionCheck(biome);
	    	}
	    	return biome;
	    }*/
	    
	    public static void clickStoryHandTarget(StoryHandTarget tgt) {
	    	if (!tgt.enabled || !tgt.isValidHandTarget)
	    		return;
	    	Story.StoryGoal goal = tgt.goal;
	    	if (storyHandEvent != null) {
	    		StoryHandCheck deal = new StoryHandCheck(goal, tgt);
	    		storyHandEvent.Invoke(deal);
	    		if (!deal.usable)
	    			return;
	    		goal = deal.goal;
	    	}
			goal.Trigger();
			if (tgt.informGameObject)
				tgt.informGameObject.SendMessage("OnStoryHandTarget", SendMessageOptions.DontRequireReceiver);
			UnityEngine.Object.Destroy(tgt.destroyGameObject);
	    }
	    
	    public static float getRadiationLevel(Player p, float orig) {
	    	float ret = orig;
	    	//SNUtil.writeToChat((radiationCheckEvent != null)+" # "+orig);
	    	if (radiationCheckEvent != null) {
	    		RadiationCheck ch = new RadiationCheck(p.transform.position, orig);
	    		radiationCheckEvent.Invoke(ch);
	    		ret = ch.value;
	    	}
	    	return ret;
	    }
	    
	    public static void onReaperGrabVehicle(ReaperLeviathan r, Vehicle v) {
	    	if (reaperGrabVehicleEvent != null) {
	    		reaperGrabVehicleEvent.Invoke(r, v);
	    	}
	    }
	    
	    public static void onDockingTriggerCollided(VehicleDockingBay v, Collider other) {
	    	if (other.isTrigger)
	    		return;
			if (v.GetDockedVehicle())
				return;
			if (GameModeUtils.RequiresPower() && !v.IsPowered())
				return;
			if (v.interpolatingVehicle != null)
				return;
			Vehicle componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Vehicle>(other.gameObject);
			if (componentInHierarchy == null || componentInHierarchy.docked || componentInHierarchy.GetRecentlyUndocked())
				return;
			v.timeDockingStarted = Time.time;
			v.interpolatingVehicle = componentInHierarchy;
			v.startPosition = v.interpolatingVehicle.transform.position;
			v.startRotation = v.interpolatingVehicle.transform.rotation;
	    }
	    
	    public static void onAcidTriggerCollided(AcidicBrineDamageTrigger v, Collider other) {
	    	if (other.isTrigger)
	    		return;
			LiveMixin liveMixin = v.GetLiveMixin(other.gameObject);
			if (v.IsValidTarget(liveMixin)) {
				v.AddTarget(liveMixin.gameObject);
			}
	    }
	    
	    public static void onAirlockTouched(PrecursorDoorMotorModeSetter door, Collider col) {
	    	if (col.isTrigger)
	    		return;
			if (door.setToMotorModeOnEnter == PrecursorDoorMotorMode.None)
				return;
			if (col.gameObject != null && col.gameObject.GetComponentInChildren<IgnoreTrigger>() != null)
				return;
			GameObject gameObject = UWE.Utils.GetEntityRoot(col.gameObject);
			if (!gameObject)
				gameObject = col.gameObject;
			Player componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Player>(gameObject);
			if (componentInHierarchy) {
				PrecursorDoorMotorMode precursorDoorMotorMode = door.setToMotorModeOnEnter;
				if (precursorDoorMotorMode != PrecursorDoorMotorMode.Auto) {
					if (precursorDoorMotorMode == PrecursorDoorMotorMode.ForceWalk) {
						componentInHierarchy.precursorOutOfWater = true;
					}
				}
				else {
					componentInHierarchy.precursorOutOfWater = false;
				}
			}
			Exosuit componentInHierarchy2 = UWE.Utils.GetComponentInHierarchy<Exosuit>(gameObject);
			if (componentInHierarchy2) {
				PrecursorDoorMotorMode precursorDoorMotorMode = door.setToMotorModeOnEnter;
				if (precursorDoorMotorMode == PrecursorDoorMotorMode.Auto) {
					componentInHierarchy2.precursorOutOfWater = false;
					return;
				}
				if (precursorDoorMotorMode != PrecursorDoorMotorMode.ForceWalk) {
					return;
				}
				componentInHierarchy2.precursorOutOfWater = true;
			}
			SeaMoth componentInHierarchy3 = UWE.Utils.GetComponentInHierarchy<SeaMoth>(gameObject);
			if (componentInHierarchy3) {
				PrecursorDoorMotorMode precursorDoorMotorMode = door.setToMotorModeOnEnter;
				if (precursorDoorMotorMode == PrecursorDoorMotorMode.Auto) {
					componentInHierarchy3.precursorOutOfWater = false;
					return;
				}
				if (precursorDoorMotorMode != PrecursorDoorMotorMode.ForceWalk) {
					return;
				}
				componentInHierarchy3.precursorOutOfWater = true;
				componentInHierarchy3.GetComponent<Rigidbody>().useGravity = true;
			}
	    }
	    /*
	    public static Vector2int getItemDisplaySize(TechType tt, InventoryItem ii) {
	    	return getItemDisplaySize(tt, ii, ii.container);
	    }
	    
	    public static Vector2int getItemDisplaySize(InventoryItem ii, TechType tt) {
	    	return getItemDisplaySize(tt, ii, ii.container);
	    }
	    
	    public static Vector2int getItemDisplaySize(TechType tt, InventoryItem ii, IItemsContainer con) {
	    	if (ii != null && ii.item != null && ii.item && ii.item.gameObject != null && ii.item.gameObject) {
			    //SNUtil.writeToChat((con != null ? con.label : "nocontainer")+" for "+tt+" in "+ii.item.gameObject.FindAncestor<Constructable>());
			    BasicCustomPlant plant = BasicCustomPlant.getPlant(tt);
			    if (plant != null && ii.item.gameObject.FindAncestor<Planter>()) {
			    	return plant.getSize() == Plantable.PlantSize.Large ? new Vector2int(2, 2) : new Vector2int(1, 1);
			    }
	    	}
	    	return CraftData.GetItemSize(tt);
	    }*/
	    
	    public static void onFModEmitterPlay(FMOD_CustomEmitter snd) {
	    	if (onSoundPlayedEvent != null)
	    		onSoundPlayedEvent.Invoke(snd);
	    }
	    
	    public static float getMaxPropulsible(float orig, GameObject go, MonoBehaviour gun, bool isMass) {
	    	if (go.FindAncestor<Constructable>() || go.FindAncestor<SubRoot>())
	    		return 0;
	    	float val = orig;
	    	if (propulsibilityEvent != null) {
	    		PropulsibilityCheck e = new PropulsibilityCheck(go, val, gun, isMass);
	    		propulsibilityEvent.Invoke(e);
	    		val = e.value;
	    	}
	    	if (go.GetComponentInChildren<Vehicle>() || go.GetComponentInChildren<AlwaysPropulsible>())
	    		val = 999999999F;
	    	//Bounds aabb = go.GetComponent<FixedBounds>() ? go.GetComponent<FixedBounds>().bounds : UWE.Utils.GetEncapsulatedAABB(go, 20);
	    	//SNUtil.writeToChat("Modifying ["+isMass+"] propulsibility check of "+go+": "+orig+">"+val+"; mass="+go.GetComponent<Rigidbody>().mass+", AABB="+(aabb.size.x * aabb.size.y * aabb.size.z));
	    	return val;
	    }
	    
	    public static Vector3 getPropulsionTargetCenter(Vector3 orig, GameObject go) {
	    	Vehicle v = go.GetComponentInChildren<Vehicle>();
	    	if (v) {
	    		Vector3 ret = go.transform.position;
	    		if (v is SeaMoth)
	    			ret += go.transform.forward*-1.25F+go.transform.up*-0.125F;
	    		return ret;
	    	}
	    	return orig;
	    }
	/*
		public static void logDockingVehicle(Vehicle v, bool dock) {
	    	string s = "Setting vehicle "+v+": dock state (path="+v.gameObject.GetFullHierarchyPath()+")"+" - "+dock;
		    SNUtil.writeToChat(s);
		    SNUtil.log(s, SNUtil.diDLL);
		    SNUtil.log("from trace "+Environment.StackTrace, SNUtil.diDLL);
		}*/
	    
	    public static void onVehicleEnter(Vehicle v, Player ep) {
	    	if (vehicleEnterEvent != null && v && ep) {
	    		vehicleEnterEvent.Invoke(v, ep);
	    	}
	    }
	    /*
	    public static void getCompassDepth(uGUI_DepthCompass gui, ref int depth) {
	    	if (depthCompassEvent != null) {
	    		DepthCompassCheck ch = new DepthCompassCheck(depth);
	    		depthCompassEvent.Invoke(ch);
	    		depth = ch.value;
	    	}
	    }
	    */
	    public static uGUI_DepthCompass.DepthMode getCompassDepth(uGUI_DepthCompass gui, out int depth, out int crush) {
	    	uGUI_DepthCompass.DepthMode ret = gui.GetDepthInfo(out depth, out crush);
	    	if (depthCompassEvent != null) {
	    		DepthCompassCheck ch = new DepthCompassCheck(depth, crush);
	    		depthCompassEvent.Invoke(ch);
	    		depth = ch.value;
	    		crush = ch.crushValue;
	    	}
	    	return ret;
	    }
	   
	   public static void onRespawnPre(Survival s, Player ep) {
	    	if (respawnEvent != null && s && ep)
	    		respawnEvent.Invoke(s, ep, false);
	   }
	   
	   public static void onRespawnPost(Survival s, Player ep) {
	    	if (respawnEvent != null && s && ep)
	    		respawnEvent.Invoke(s, ep, true);
	   }
	   
	   public static void onDrillableDrilled(Drillable dr, Vector3 pos, Exosuit driller) {
	   	if (drillableDrillTickEvent != null && dr)
	   		drillableDrillTickEvent.Invoke(dr, pos, driller);
	   }
	   
	   public static void onMapRoomTick(MapRoomFunctionality map) {
	    	if (scannerRoomTickEvent != null && map)
	    		scannerRoomTickEvent.Invoke(map);
	   }
	   
	   public static void onItemsLost() {
	   		if (itemsLostEvent != null)
	    		itemsLostEvent.Invoke();
	   }
	   
	   public static void onStorageContainerHover(StorageContainer sc, GUIHand hand) {
	   		if (storageHoverEvent != null)
	    		storageHoverEvent.Invoke(sc, hand);
	   }
	}
}
