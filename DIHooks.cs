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
	    public static event Action<StringBuilder, TechType, GameObject> itemTooltipEvent;
	    public static event Action<WaterFogValues> fogCalculateEvent;
	    public static event Action<BuildabilityCheck> constructabilityEvent;
	    public static event Action<StoryHandCheck> storyHandEvent;
	    public static event Action<RadiationCheck> radiationCheckEvent;
	    public static event Action<BulkheadLaserCutterHoverCheck> bulkheadLaserHoverEvent;
	    public static event Action<KnifeHarvest> knifeHarvestEvent;
	    //public static event Action<MusicSelectionCheck> musicBiomeChoiceEvent;
	    
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
	    	
	    	public readonly Vector4 originalColor;
	    	public readonly float originalDensity;
	    	
	    	public Vector4 color;
	    	public float density;
	    	
	    	internal WaterFogValues(Vector4 c, float d) {
	    		originalColor = c;
	    		originalDensity = d;
	    		density = d;
	    		color = c;
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
	    	public readonly float originalValue;
	    	
	    	public float value;
	    	
	    	internal RadiationCheck(Vector3 pos, float orig) {
	    		originalValue = orig;
	    		value = orig;
	    		position = pos;
	    	}
	    	
	    }
	    
	    public class BulkheadLaserCutterHoverCheck {
	    	
	    	public readonly Sealed obj;
	    	
	    	public string refusalLocaleKey = null;
	    	
	    	internal BulkheadLaserCutterHoverCheck(Sealed s) {
	    		obj = s;
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
	    	
	    	Biome.tickMusic(cyc);
	    	
	    	if (onDayNightTickEvent != null)
	    		onDayNightTickEvent.Invoke(cyc);
	    }
	    
	    public static void onWorldLoaded() {
	    	worldLoadTime = Time.time;
	    	SNUtil.log("Intercepted world load", SNUtil.diDLL);
	    	DuplicateRecipeDelegate.updateLocale();
	    	CustomEgg.updateLocale();
	    	PickedUpAsOtherItem.updateLocale();
	    	SeamothModule.updateLocale();
	    	
	    	LanguageHandler.SetLanguageLine("BulkheadInoperable", "Bulkhead is inoperable");
	    	
	    	if (onWorldLoadedEvent != null)
	    		onWorldLoadedEvent.Invoke();
	    }
	    
	    public static float getWorldAge() {
	    	return worldLoadTime < 0 ? -1 : Time.time-worldLoadTime;
	    }
	    
	    public static bool isWorldLoaded() {
	    	return worldLoadTime > 0;
	    }
	    
	    public static void tickPlayer(Player ep) {
	    	if (Time.timeScale <= 0)
	    		return;
	    	
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
		    		GameObject go = UnityEngine.Object.Instantiate(CraftData.GetPrefabForTechType(tt2));
		    		go.SetActive(true);
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
	    
	    public static void onSkyApplierSpawn(SkyApplier pk) {
	    	if (pk.GetComponent<Vehicle>()) {
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
	    	if (onSkyApplierSpawnEvent != null)
	    		onSkyApplierSpawnEvent.Invoke(pk);
	    }
	    
	    //private static bool needsLavaDump = true;
	    
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
	   
		public static void onScanComplete(PDAScanner.EntryData data) {
		   	if (data != null)
	   			TechnologyUnlockSystem.instance.triggerDirectUnlock(data.key);
		}
	    
	    public static void tickLaserCutting(Sealed s, float amt) {
			if (s._sealed && s.maxOpenedAmount >= 0) {
	    		string key = null;
	    		if (bulkheadLaserHoverEvent != null) {
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
	    
	    public static void appendItemTooltip(StringBuilder sb, TechType tt, GameObject obj) {
	    	InfectedMixin mix = obj.GetComponent<InfectedMixin>();
	    	if (mix) {
	    		TooltipFactory.WriteDescription(sb, getInfectionTooltip(mix));//TooltipFactory.WriteDescription(sb, "Infected: "+((int)(mix.infectedAmount*100))+"%");
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
		   		return "Status: Healthy.";
	    	}
	    }
	    
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
				Color lightColor = vol.sky.GetLightColor();
				fogColor = lightColor;
				fogColor.w = vol.sunLightAmount * vol.GetTransmission();
				float value3 = lightColor.r * 0.3f + lightColor.g * 0.59f + lightColor.b * 0.11f;
				Shader.SetGlobalVector(ShaderPropertyID._UweFogVsLightDirection, v);
				Shader.SetGlobalVector(ShaderPropertyID._UweFogWsLightDirection, lightDirection);
				Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightGreyscaleColor, value3);
			}
			Biome b = BiomeBase.getBiome(cam.transform.position) as Biome;
			if (b != null) {
				fogColor.setXYZ(b.getFogColor(fogColor.getXYZ()));
				fogColor.w = b.getSunIntensity(fogColor.w);
				fogDensity = b.getFogDensity(fogDensity);
			}
			WaterFogValues wf = new WaterFogValues(fogColor, fogDensity);
	    	if (fogCalculateEvent != null)
	    		fogCalculateEvent.Invoke(wf);
			Shader.SetGlobalVector(ShaderPropertyID._UweFogLightColor, wf.color);
			Shader.SetGlobalFloat(ShaderPropertyID._UweExtinctionAndScatteringScale, wf.density);
	    }
	    
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
			if (p.gameObject.GetComponent<Constructable>().constructed) {
	    		float gen = p.GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * 5f;
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
	}
}
