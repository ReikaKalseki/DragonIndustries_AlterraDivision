using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Oculus.Newtonsoft.Json;
//using Oculus.Newtonsoft.Json.Linq;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;

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
		public static event Action<ItemPickup> onItemPickedUpEvent;
		public static event Action<CellManager, LargeWorldEntity> onEntityRegisterEvent;
		public static event Action<SkyApplier> onSkyApplierSpawnEvent;
		public static event Action<Constructable, bool> onConstructedEvent;
		public static event Action<BaseRoot> onBaseLoadedEvent;
		public static event Action<StorageContainer> inventoryOpenedEvent;
		public static event Action<StorageContainer> inventoryClosedEvent;
		public static event Action<BiomeCheck> getBiomeEvent;
		public static event Action<WaterTemperatureCalculation> getTemperatureEvent;
		public static event Action<GameObject> onKnifedEvent;
		public static event Action<KnifeAttempt> knifeAttemptEvent;
		public static event Action<GravTrapGrabAttempt> gravTrapAttemptEvent;
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
		public static event Action<BreathabilityCheck> breathabilityEvent;
		//public static event Action<O2UseCheck> o2UseEvent;
		public static event Action<StoryHandCheck> storyHandEvent;
		public static event Action<RadiationCheck> radiationCheckEvent;
		public static event Action<BulkheadLaserCutterHoverCheck> bulkheadLaserHoverEvent;
		public static event Action<KnifeHarvest> knifeHarvestEvent;
		//public static event Action<MusicSelectionCheck> musicBiomeChoiceEvent;
		public static event Action<FruitPlantTag> onFruitPlantTickEvent;
		public static event Action<ReaperLeviathan, Vehicle> reaperGrabVehicleEvent;
		public static event Action<SubRoot, DamageInfo> cyclopsDamageEvent;
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
		public static event Action<Vehicle> onVehicleDestroyEvent;
		public static event Action<StorageContainer, GUIHand> storageHoverEvent;
		public static event Action<ModuleFireCostCheck> moduleFireCostEvent;
		public static event Action<PDAScanner.EntryData> scanCompleteEvent;
		public static event Action selfScanEvent;
		public static event Action<uGUI_MapRoomScanner> scannerRoomTechTypeListingEvent;
		public static event Action<StasisEffectCheck> onStasisRifleFreezeEvent;
		public static event Action<StasisEffectCheck> onStasisRifleUnfreezeEvent;
		public static event Action<RedundantScanEvent> onRedundantScanEvent;
		// public static event Action<EquipmentCompatibilityCheck> equipmentCompatibilityCheckEvent;
		public static event Action<EquipmentTypeCheck> equipmentTypeCheckEvent;
		public static event Action<EatAttempt> tryEatEvent;
		public static event Action<Survival, GameObject> onEatEvent;
		public static event Action<SwimSpeedCalculation> getSwimSpeedEvent;
		public static event Action<Bed> onSleepEvent;
		public static event Action<FoodRateCalculation> getFoodRateEvent;
		public static event Action<PlayerInput> getPlayerInputEvent;
		public static event Action<Bullet, Vehicle> onTorpedoFireEvent;
		public static event Action<SeamothTorpedo, Transform> onTorpedoExplodeEvent;
		public static event Action<CreatureSeeObjectCheck> canCreatureSeeObjectEvent;
		public static event Action<AggressiveToPilotingVehicleCheck> aggressiveToPilotingEvent;
		public static event Action<Base> baseRebuildEvent;
		public static event Action<BaseStrengthCalculation> baseStrengthComputeEvent;
		public static event Action<WaterFilterSpawn> waterFilterSpawnEvent;
		public static event Action<GrowingPlant, float> growingPlantTickEvent;
		public static event Action<CuteFishHandTarget, Player, CuteFishHandTarget.CuteFishCinematic> onPlayWithCuddlefish;
		public static event Action<Rocket, int, bool> onRocketStageCompletedEvent;
		public static event Action<CrashedShipExploder> auroraSpawnEvent;
		public static event Action<CraftTimeCalculation> craftTimeEvent;
		public static event Action<SeaMoth> seamothDischargeEvent;
		public static event Action<SinkingGroundChunk> spawnTreaderChunk;
		public static event Action<Crash> crashfishExplodeEvent;
		public static event Action<TargetabilityCheck> targetabilityEvent;

		private static BasicText updateNotice = new BasicText(TextAnchor.MiddleCenter);

		private static readonly HashSet<TechType> gravTrapTechSet = new HashSet<TechType>();

		private static bool hasLoadedAWorld = false;
		private static bool outdatedMods = false;

		private static bool isKnifeHarvesting = false;
		private static CustomBiome currentCustomBiome;

		public static bool skipWorldForces = false;
		public static bool skipSkyApplier = false;

		private static bool skipZeroedDeserialization = DIMod.config.getBoolean(DIConfig.ConfigEntries.SKIPZEROEDIDOVERWRITE);

		private static readonly List<Pickupable> collectedItemsToDestroy = new List<Pickupable>();

		static DIHooks() {
			SNUtil.log("Initializing DIHooks");

			PrecursorTeleporter.TeleportEventStart += startTeleport;
			PrecursorTeleporter.TeleportEventEnd += stopTeleport;
		}

		public class PlayerInput {

			public readonly Vector3 originalInput;
			public Vector3 selectedInput;

			internal PlayerInput(Vector3 vec) {
				originalInput = vec;
				selectedInput = vec;
			}

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

		public class ItemPickup {

			public readonly Pickupable item;
			public readonly Exosuit prawn;
			public readonly bool isKnife;

			public bool destroy = false;

			internal ItemPickup(Pickupable pp, Exosuit exo, bool knife) {
				item = pp;
				prawn = exo;
				isKnife = knife;
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

		public class SwimSpeedCalculation {

			public readonly float originalValue;

			private bool disallowFurtherChanges;

			internal float speed;

			internal SwimSpeedCalculation(float amt) {
				originalValue = amt;
				speed = originalValue;
				disallowFurtherChanges = false;
			}

			public void lockValue() {
				disallowFurtherChanges = true;
			}

			public float getValue() {
				return speed;
			}

			public void setValue(float amt) {
				//SNUtil.writeToChat("Setting water temp to "+amt);
				if (disallowFurtherChanges)
					return;
				speed = amt;
			}

		}

		public class FoodRateCalculation {

			public readonly float originalValue;
			public float rate;

			internal FoodRateCalculation(float amt) {
				originalValue = amt;
				rate = originalValue;
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

		public class EatAttempt {

			public readonly Survival survival;
			public readonly GameObject food;

			public bool allowEat = true;

			internal EatAttempt(Survival s, GameObject go) {
				survival = s;
				food = go;
			}

		}

		public class KnifeAttempt {

			public readonly LiveMixin target;
			public readonly bool defaultValue;

			public bool allowKnife = true;

			internal KnifeAttempt(LiveMixin tgt, bool def) {
				target = tgt;
				defaultValue = def;
			}

		}

		public class GravTrapGrabAttempt {

			public readonly Gravsphere gravtrap;
			public readonly GameObject target;
			public readonly bool defaultValue;

			public bool allowGrab;

			internal GravTrapGrabAttempt(Gravsphere s, GameObject tgt, bool def) {
				gravtrap = s;
				target = tgt;
				defaultValue = def;
				allowGrab = def;
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

		public class BreathabilityCheck {

			public readonly bool originalValue;
			public readonly Player player;

			public bool breathable;

			internal BreathabilityCheck(bool orig, Player ep) {
				originalValue = orig;
				breathable = orig;
				player = ep;
			}

		}

		public class O2UseCheck {

			public readonly float originalValue;
			public readonly int depthClass;
			public readonly Player player;

			public float value;

			internal O2UseCheck(float orig, Player ep, int depth) {
				originalValue = orig;
				value = orig;
				player = ep;
				depthClass = depth;
			}

		}
		/*
	    public class EquipmentCompatibilityCheck {
	    	
	    	public readonly bool originalValue;
	    	public readonly Equipment container;
	    	public readonly Pickupable item;
	    	public readonly EquipmentType itemType;
	    	public readonly EquipmentType slotType;
	    	
	    	public bool allow;
	    	
	    	internal EquipmentCompatibilityCheck(Equipment box, Pickupable pp, EquipmentType t1, EquipmentType t2, bool orig) {
	    		originalValue = orig;
	    		allow = orig;
	    		container = box;
	    		item = pp;
	    		itemType = t1;
	    		slotType = t2;
	    	}
	    	
	    }*/

		public class EquipmentTypeCheck {

			public readonly EquipmentType originalValue;
			public readonly TechType item;

			public EquipmentType type;

			internal EquipmentTypeCheck(TechType pp, EquipmentType orig) {
				originalValue = orig;
				type = orig;
				item = pp;
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
			//0-1

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

		public class ModuleFireCostCheck {

			public readonly TechType module;
			public readonly Vehicle vehicle;
			public readonly float originalValue;

			public float value;

			internal ModuleFireCostCheck(Vehicle v, TechType item, float orig) {
				originalValue = orig;
				value = orig;
				module = item;
				vehicle = v;
			}

		}

		public class StasisEffectCheck {

			public readonly StasisSphere sphere;
			public readonly Rigidbody body;

			public bool applyKinematicChange = true;
			public bool addToTargetList = true;
			public bool sendMessage = true;
			public bool doFX = true;

			internal StasisEffectCheck(StasisSphere s, Rigidbody b) {
				sphere = s;
				body = b;
			}

		}

		public class RedundantScanEvent {

			public bool preventNormalDrop = false;

		}

		public class CreatureSeeObjectCheck {

			public readonly Creature creature;
			public readonly GameObject target;
			public readonly bool defaultValue;
			public readonly float atDistance;

			public bool canSee;

			internal CreatureSeeObjectCheck(Creature c, GameObject tgt, bool def, float dist) {
				creature = c;
				target = tgt;
				defaultValue = def;
				atDistance = dist;
				canSee = def;
			}

		}

		public class AggressiveToPilotingVehicleCheck {

			public readonly AggressiveToPilotingVehicle ai;
			public readonly Vehicle vehicle;
			public readonly bool defaultVisiblity;

			public bool canTarget;

			internal AggressiveToPilotingVehicleCheck(AggressiveToPilotingVehicle ai, Vehicle v, bool def) {
				this.ai = ai;
				vehicle = v;
				defaultVisiblity = def;
				canTarget = def;
			}

		}

		public class WaterFilterSpawn {

			public readonly FiltrationMachine filter;
			public readonly Pickupable defaultItem;

			public Pickupable item;

			internal WaterFilterSpawn(FiltrationMachine fm, Pickupable pp) {
				filter = fm;
				defaultItem = pp;
				item = defaultItem;
			}

		}

		public class BaseStrengthCalculation {

			public readonly BaseHullStrength component;
			private readonly Dictionary<Int3, float> cellContributions = new Dictionary<Int3, float>(); //can use Base.GetCellObject to get the BaseCell
			private readonly Dictionary<GameObject, float> bonusStrength = new Dictionary<GameObject, float>();
			public float dynamicStrength { get; private set; }

			public float initialStrength = 10;
			public float finalStrength { get { return initialStrength + dynamicStrength; } }

			internal BaseStrengthCalculation(BaseHullStrength b) {
				component = b;
			}

			public void computeCellStrength(Int3 cell) {
				float amt = component.baseComp.GetHullStrength(cell);
				cellContributions[cell] = amt;
				dynamicStrength += amt;
			}

			public void addBonusStrength(GameObject c, float amt) {
				bonusStrength[c] = amt;
				dynamicStrength += amt;
			}

			public float getStrength(Int3 cell) {
				return cellContributions.ContainsKey(cell) ? cellContributions[cell] : 0;
			}

		}

		public class CraftTimeCalculation {

			public readonly float originalDuration;
			public readonly Crafter crafter;
			public readonly TechType recipe;

			public float craftingDuration;

			internal CraftTimeCalculation(float amt, Crafter c, TechType tt) {
				originalDuration = amt;
				craftingDuration = originalDuration;
				recipe = tt;
				crafter = c;
			}

		}

		public class TargetabilityCheck {

			public readonly bool originalValue;
			public readonly Transform transform;
			public readonly PrefabIdentifier prefab;

			public bool allowTargeting;

			internal TargetabilityCheck(bool orig, Transform obj, PrefabIdentifier pi) {
				originalValue = orig;
				transform = obj;
				allowTargeting = orig;
				prefab = pi;
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

			StoryHandler.instance.onLoad();
			CustomLocaleKeyDatabase.onLoad();

			foreach (CustomBiome cb in BiomeBase.getCustomBiomes())
				cb.onLoad();

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

			//SaveSystem.populateLoad();

			List<ModVersionCheck> vers = ModVersionCheck.getOutdatedVersions();
			updateNotice.SetLocation(0, 250);
			updateNotice.SetSize(24);
			updateNotice.SetColor(Color.yellow);
			List<string> li = new List<string>();
			outdatedMods = vers.Count > 0;
			if (outdatedMods) {
				li.Add("Your versions of the following mods are out of date:");
				foreach (ModVersionCheck mv in vers) {
					li.Add(mv.modName + ": Current version " + mv.currentVersion + ", newest version " + mv.remoteVersion.Invoke());
				}
				li.Add("Update your mods to remove this warning.");
				//li.Add("Run the /autoUpdate command to download and install these updates automatically.");
			}
			vers = ModVersionCheck.getErroredVersions();
			if (vers.Count > 0) {
				li.Add("Several mods failed to fetch version information:");
				foreach (ModVersionCheck mv in vers) {
					li.Add(mv.modName + ": Installed version " + mv.currentVersion + ", remote version " + mv.remoteVersion.Invoke());
				}
				if (SNUtil.checkPiracy()) {
					li.Add("<color=#ff5050ff>This appears to be a result of pirating the game, which cuts its internet connection. There is nothing that can be done without buying Subnautica.</color>");
				}
				else {
					li.Add("You should redownload and reinstall mods with local errors and contact Reika if remote versions are invalid.");
					li.Add("This message can be temporarily hidden with /hideVersions");
				}
			}
			if (warnRestart)
				li.Add("You have reloaded a save without exiting the game. This breaks mod loading and will damage your world. Restart your game when changing/reloading saves.\nExit the game now, and DO NOT SAVE before doing so.");
			if (li.Count > 0)
				updateNotice.ShowMessage(string.Join("\n", li));
			else
				updateNotice.Hide();

			if (onWorldLoadedEvent != null)
				onWorldLoadedEvent.Invoke();
		}

		public static void setWarningText(params string[] s) {
			setWarningText(s.AsEnumerable());
		}

		public static void setWarningText(IEnumerable<string> li) {
			updateNotice.ShowMessage(string.Join("\n", li));
		}
		/*
	    internal static void autoUpdate() { //TODO move to own class, and make msg prep and call its own method
	    	if (outdatedMods | true) {
	    		SNUtil.writeToChat("Downloading new versions of mods...");
	    		string dirpath = Path.Combine(Environment.CurrentDirectory, "DIDownloads");
	    		Directory.CreateDirectory(dirpath);
	    		using(HttpClient client = new HttpClient()) {
	    			HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/ReikaKalseki/Reika_SubnauticaModsShared/releases/latest");
					msg.Headers.Add("User-Agent", "Dragon Industries Autoupdate");
					msg.Headers.Add("Accept", "application/vnd.github+json");
					msg.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
					Task<HttpResponseMessage> resp = client.SendAsync(msg);
					resp.RunSynchronously();
					Task<string> task = resp.Result.Content.ReadAsStringAsync();
					task.RunSynchronously();
					JObject json = JObject.Parse(task.Result);
					int id = (int)json["id"];
					msg = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/ReikaKalseki/Reika_SubnauticaModsShared/releases/"+id+"/assets");
					msg.Headers.Add("User-Agent", "Dragon Industries Autoupdate");
					msg.Headers.Add("Accept", "application/vnd.github+json");
					msg.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
					resp = client.SendAsync(msg);
					resp.RunSynchronously();
					task = resp.Result.Content.ReadAsStringAsync();
					task.RunSynchronously();
					json = JObject.Parse(task.Result);
					foreach (JObject mod in json.Values()) {
						string url = ((string)json["browser_download_url"]).Replace("\"", "");
						SNUtil.writeToChat("Downloading from "+url);
						new System.Net.WebClient().DownloadFile(url, Path.Combine(dirpath, url.Substring(url.LastIndexOf('/')+1)));
					}
	    		}
	    	}
	    	else {
	    		SNUtil.writeToChat("No outdated mods, no download will be performed.");
	    	}
	    		
	    	//https://github.com/ReikaKalseki/Reika_SubnauticaModsShared/releases/download/Downloads/AqueousEngineering.zip
	    	//https://github.com/ReikaKalseki/Reika_SubnauticaModsShared/releases/download/Downloads/Dragon_Industries_-_Alterra_Division.zip
	    }*/

		internal static void hideVersions() {
			updateNotice.Hide();
		}

		public static float getWorldAge() {
			return worldLoadTime < 0 ? -1 : Time.time - worldLoadTime;
		}

		public static bool isWorldLoaded() {
			return worldLoadTime > 0;
		}

		public static bool hasWorldLoadStarted() {
			return hasLoadedAWorld;
		}

		public static void tickPlayer(Player ep) {
			CustomBiome b = BiomeBase.getBiome(Camera.main.transform.position) as CustomBiome;
			if (currentCustomBiome != b)
				recomputeFog();
			currentCustomBiome = b;
			if (Time.timeScale <= 0)
				return;
			updateNotice.SetColor(Color.yellow);

			SpawnedItemTracker.instance.tick();

			if (collectedItemsToDestroy.Count > 0) {
				foreach (Pickupable pp in collectedItemsToDestroy) {
					if (!pp)
						continue;
					Inventory.main.container.RemoveItem(pp, true);
					pp.gameObject.destroy(false);
				}
				collectedItemsToDestroy.Clear();
			}

			StoryHandler.instance.tick(ep);
			ScreenFXManager.instance.tick();

			if (onPlayerTickEvent != null) {
				try {
					onPlayerTickEvent.Invoke(ep);
				}
				catch (Exception ex) {
					SNUtil.writeToChat("Threw exception running player tick hooks: "+ex);
				}
			}
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

		public static int damageDebugLevel = 0;

		public static float recalculateDamage(float damage, DamageType type, GameObject target, GameObject dealer) {
			if (DIMod.config.getBoolean(DIConfig.ConfigEntries.INFITUBE) && target.isCoralTube())
				return Mathf.Min(damage, target.FindAncestor<LiveMixin>().health - 1);
			//if (target.isPlayer()) {
			HealthModifier hm = target.GetComponent<HealthModifier>();
			if (hm)
				damage *= hm.damageFactor;
			//}
			PrefabIdentifier pi = target.GetComponent<PrefabIdentifier>();
			if (pi && pi.ClassId == CustomEgg.getEgg(TechType.SpineEel).ClassID && (type == DamageType.Acid || type == DamageType.Poison))
				return 0;
			if (onDamageEvent != null) {
				DamageToDeal deal = new DamageToDeal(damage, type, target, dealer);
				onDamageEvent.Invoke(deal);
				if (damageDebugLevel > 1 || (damageDebugLevel == 1 && !Mathf.Approximately(deal.originalAmount, deal.amount)))
					SNUtil.writeToChat("Adjusting damage type " + type + " yield from " + deal.originalAmount + " to " + deal.amount);
				return deal.amount;
			}
			else {
				if (damageDebugLevel > 2)
					SNUtil.writeToChat("Applying unchanged damage amount " + damage);
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
			isKnifeHarvesting = true;
			foreach (KeyValuePair<TechType, int> kvp in harv.drops)
				CraftData.AddToInventory(kvp.Key, kvp.Value, false, false);
			isKnifeHarvesting = false;
		}

		public static void fireKnifeHarvest(GameObject target, Dictionary<TechType, int> drops) {
			KnifeHarvest harv = new KnifeHarvest(target, CraftData.GetTechType(target), false, false);
			harv.drops.Clear();
			drops.ForEach(kvp => harv.drops[kvp.Key] = kvp.Value);
			if (knifeHarvestEvent != null) {
				knifeHarvestEvent.Invoke(harv);
			}
			foreach (KeyValuePair<TechType, int> kvp in harv.drops)
				CraftData.AddToInventory(kvp.Key, kvp.Value, false, false);
		}

		public static void onPrawnItemPickedUp(Pickupable pp) {
			if (pp)
				onItemPickedUp(pp, Player.main.GetVehicle() as Exosuit);
		}

		public static void onItemPickedUp(Pickupable p) {
			onItemPickedUp(p, null);
		}

		public static void onItemPickedUp(Pickupable p, Exosuit prawn) {
			List<Pickupable> collected = new List<Pickupable>(){ p };
			TechType tt = p.GetTechType();
			PickedUpAsOtherItem mapTo = PickedUpAsOtherItem.getPickedUpAsOther(tt);
			//SNUtil.writeToChat("Pickup "+tt+" >> "+mapTo);
			if (mapTo != null) {
				if (prawn)
					prawn.storageContainer.container.DestroyItem(tt);
				else
					Inventory.main.container.DestroyItem(tt);

				p.gameObject.destroy(false); //not immediate because prawn is animation
				TechType tt2 = mapTo.getTemplate();
				int n = mapTo.getNumberCollectedAs();
				SNUtil.log("Converting pickup '" + p + "' to '" + tt2 + "' x" + n, SNUtil.diDLL);
				collected.Clear();
				for (int i = 0; i < n; i++) {
					GameObject go = ObjectUtil.createWorldObject(tt2, true, false);
					p = go.GetComponent<Pickupable>();
					if (prawn)
						prawn.storageContainer.container.UnsafeAdd(new InventoryItem(p));
					else
						Inventory.main.Pickup(p, false);
					collected.Add(p);
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
			/*
	    	foreach (Renderer r in p.gameObject.GetComponentsInChildren<Renderer>()) {
				foreach (Material m in r.materials) {
					//m.DisableKeyword("FX_BUILDING"); //breaks items which use it for their appearance
				}
			}
	    	*/
			GenUtil.CrateManagement cc = WorldUtil.getClosest<GenUtil.CrateManagement>(p.gameObject);
			if (cc && Vector3.Distance(p.transform.position, cc.transform.position) < 1.5F)
				cc.onPickup(p);

			if (onItemPickedUpEvent != null) {
				foreach (Pickupable pp in collected) {
					ItemPickup ip = new ItemPickup(pp, prawn, isKnifeHarvesting);
					onItemPickedUpEvent.Invoke(ip);
					if (ip.destroy)
						collectedItemsToDestroy.Add(pp); //need to delegate until later because this is called before it is actually added to the inv
				}
			}
		}

		public static bool canPlayerBreathe(bool orig, Player p) {
			if (p.GetComponent<TemporaryBreathPrevention>())
				return false;
			if (breathabilityEvent != null) {
				BreathabilityCheck deal = new BreathabilityCheck(orig, p);
				breathabilityEvent.Invoke(deal);
				return deal.breathable;
			}
			return orig;
		}
		/*
	    public static float getPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
			if (o2UseEvent != null) {
				O2UseCheck deal = new O2UseCheck(breathingInterval, ep, depthClass);
				o2UseEvent.Invoke(deal);
				return deal.value;
			}
			return 
	   		return EnvironmentalDamageSystem.instance.getPlayerO2Use(ep, breathingInterval, depthClass);
	    }
    */
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
			PrefabIdentifier pi = pk.GetComponent<PrefabIdentifier>();/*
	    	if (pi) {
	    		foreach (Collider c in pi.GetComponentsInChildren<Collider>())
	    			c.gameObject.EnsureComponent<ColliderPrefabLink>().init(pi);
	    	}*/
			if (pk.isPlayer())
				pk.gameObject.EnsureComponent<AoERadiationTracker>();
			if (pk.GetComponent<Vehicle>()) {
				pk.gameObject.EnsureComponent<FixedBounds>()._bounds = new Bounds(Vector3.zero, Vector3.one * 5);
				GameObject go = pk.gameObject.getChildObject("LavaWarningTrigger");
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
			if (pi && pi.ClassId == "bb16d2bf-bc85-4bfa-a90e-ddc7343b0ac2") {
				WreckDoorSwaps.setupRepairableDoor(pk.gameObject);
			}
			if (onSkyApplierSpawnEvent != null) {
				try {
					onSkyApplierSpawnEvent.Invoke(pk);
				}
				catch (Exception ex) {
					SNUtil.log("Threw error when processing SkyApplier spawn of " + pk.gameObject.GetFullHierarchyPath() + ": " + ex.ToString());
				}
			}
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
					if (time - lastTickTime >= 0.5F) {
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
					bay = this.GetComponent<VehicleDockingBay>();

				if (bay.dockedVehicle && DayNightCycle.main.timePassedAsFloat - lastTime >= 0.5F && !bay.dockedVehicle.GetComponentInParent<SubRoot>()) {
					bay.DockVehicle(bay.dockedVehicle, false);
					SNUtil.writeToChat("Re-binding vehicle " + bay.dockedVehicle + " to docking bay " + bay.gameObject.GetFullHierarchyPath());
					lastTime = DayNightCycle.main.timePassedAsFloat;
				}
			}

		}

		public class LavaWarningTriggerDetector : IgnoreTrigger {

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
					float y = 1 - (i / (100 - 1F) * 2);  // y goes from 1 to -1
					float radius = Mathf.Sqrt(1 - (y * y));  // radius at y

					float theta = phi * i;  // golden angle increment

					float x = Mathf.Cos(theta) * radius;
					float z = Mathf.Sin(theta) * radius;

					spherePoints.Add(new Vector3(x, y, z));
				}
				for (int i = 0; i < 150; i++) {
					float ang = UnityEngine.Random.Range(0F, 360F);
					float x = Mathf.Cos(Mathf.Deg2Rad * ang) * NEAR_LAVA_RADIUS;
					float z = Mathf.Sin(Mathf.Deg2Rad * ang) * NEAR_LAVA_RADIUS;
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
				float dT = time - lastCheckTime;
				if (dT >= 0.5) {
					lastCheckTime = time;
					ambientTemperatureMinusLava = WaterTemperatureSimulation.main.GetTemperature(transform.position);
				}
				if (damage && ambientTemperatureMinusLava >= 90)
					this.checkNearbyLava();
			}

			private void checkNearbyLava() {
				for (int i = spherePointIndex; i < Math.Min(spherePointIndex + RAYS_PER_TICK, spherePoints.Count); i++) {
					Vector3 vec = spherePoints[i];
					RaycastHit[] hits = Physics.RaycastAll(transform.position, vec.normalized, NEAR_LAVA_RADIUS, Voxeland.GetTerrainLayerMask());
					//SNUtil.writeToChat(vec+" > "+hits.Length);
					foreach (RaycastHit hit in hits) {
						if (hit.transform && this.checkLava(hit.point, Vector3.zero)) {
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
					this.checkLava(this.getCollisionPoint(other, out Vector3 norm), norm);
				}
			}

			private Vector3 getCollisionPoint(Collider other, out Vector3 norm) {

				Vector3 ctr = transform.position;
				return Physics.ComputePenetration(other, other.transform.position, other.transform.rotation, sphere, ctr, Quaternion.identity, out norm, out float depth)
					? ctr + (norm * (NEAR_LAVA_RADIUS - depth))
					: Vector3.zero;
			}

			private bool checkLava(Vector3 pt, Vector3 norm) {
				//SNUtil.log("Checking lava: "+pt+" @ "+lastLavaTime, SNUtil.diDLL);
				if (norm.magnitude < 0.01F)
					norm = transform.position - pt;
				if (damage.lavaDatabase.IsLava(pt, norm)) {
					this.markLavaDetected();
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
				return Mathf.Abs(DayNightCycle.main.timePassedAsFloat - lastGeyserTime) <= 0.5F;
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
				return Mathf.Abs(DayNightCycle.main.timePassedAsFloat - lastLavaTime) <= 2;
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
				SNUtil.log("Player used item " + tt, SNUtil.diDLL);
				flag = UsableItemRegistry.instance.use(tt, s, useObj);
				if (flag)
					FMODUWE.PlayOneShot(CraftData.GetUseEatSound(tt), Player.main.transform.position, 1f);
			}
			return flag;
		}

		public static bool isItemDroppable(Pickupable pp, bool notify) {
			bool flag = Inventory.CanDropItemHere(pp, notify);
			if (pp) {
				IrreplaceableItemRegistry.IrreplaceableItemData data = IrreplaceableItemRegistry.instance.getEffects(pp.GetTechType());
				if (data != null)
					return data.onAttemptToDrop.Invoke(pp, notify);
			}
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
			if (data != null) {
				TechnologyUnlockSystem.instance.triggerDirectUnlock(data.key);
				TechUnlockTracker.instance.onScan(data);
				if (scanCompleteEvent != null)
					scanCompleteEvent.Invoke(data);
			}
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
			return wb ? wb.isPlayerInside() : Player.main.IsInsideWalkable() && Player.main.currentWaterPark == null;
		}

		public static void onConstructionComplete(Constructable c, bool finished) {
			if (finished)
				TechnologyUnlockSystem.instance.triggerDirectUnlock(c.techType);


			CustomMachineLogic lgc = c.GetComponent<CustomMachineLogic>();
			if (lgc)
				lgc.onConstructedChanged(finished);

			if (onConstructedEvent != null)
				onConstructedEvent.Invoke(c, finished);
		}

		public static void onVehicleBayFinish(Constructor c, GameObject go) {
			TechType tt = CraftData.GetTechType(go);
			if (tt != TechType.None)
				TechnologyUnlockSystem.instance.triggerDirectUnlock(tt);
		}

		public static void onBaseLoaded(BaseRoot root) {
			if (onBaseLoadedEvent != null)
				onBaseLoadedEvent.Invoke(root);
		}

		public static void onInvOpened(StorageContainer sc) {
			if (inventoryOpenedEvent != null)
				inventoryOpenedEvent.Invoke(sc);
		}

		public static void onInvClosed(StorageContainer sc) {
			if (inventoryClosedEvent != null)
				inventoryClosedEvent.Invoke(sc);
		}

		public static void onKnifed(GameObject go) {
			if (go && onKnifedEvent != null)
				onKnifedEvent.Invoke(go);
			if (go && Inventory.main.GetHeld().GetTechType() == TechType.HeatBlade) { //allow thermoblade to cook dead fish
				TechType tt = CraftData.GetTechType(go);
				if (tt != TechType.None && CraftData.cookedCreatureList.ContainsKey(tt)) {
					LiveMixin lv = go.GetComponent<LiveMixin>();
					if (lv && !lv.IsAlive()) {
						GameObject put = ObjectUtil.createWorldObject(CraftData.cookedCreatureList[tt]);
						if (put) {
							put.transform.position = go.transform.position;
							put.transform.rotation = go.transform.rotation;
							put.transform.localScale = go.transform.localScale;
							go.destroy(false);
						}
					}
				}
			}
		}

		public static bool isObjectKnifeable(LiveMixin lv) {
			if (!lv)
				return true;
			KnifeAttempt k = new KnifeAttempt(lv, !lv.weldable && lv.knifeable && !lv.GetComponent<EscapePod>());
			if (knifeAttemptEvent != null)
				knifeAttemptEvent.Invoke(k);
			return k.allowKnife;
		}

		public static bool canGravTrapGrab(Gravsphere s, GameObject go) {
			if (!s || !go)
				return false;

			if (gravTrapTechSet.Count == 0)
				gravTrapTechSet.AddRange(Gravsphere.allowedTechTypes);

			Pickupable pp = go.GetComponent<Pickupable>();
			bool def = (pp == null || !pp.attached) && gravTrapTechSet.Contains(CraftData.GetTechType(go));

			GravTrapGrabAttempt k = new GravTrapGrabAttempt(s, go, def);
			if (gravTrapAttemptEvent != null)
				gravTrapAttemptEvent.Invoke(k);
			//SNUtil.writeToChat("Gravsphre "+s+" tried to grab "+go+": "+def+" > "+k.allowGrab);
			return k.allowGrab;
		}

		public static void hoverSeamothTorpedoStorage(SeaMoth sm, HandTargetEventData data) {
			for (int i = 0; i < sm.slotIDs.Length; i++) {
				InventoryItem ii = sm.GetSlotItem(i);
				if (ii != null && ii.item) {
					SeamothModule.SeamothModuleStorage storage = SeamothModule.getStorageHandler(ii.item.GetTechType());
					if (storage != null && storage.storageType == SeamothModule.StorageAccessType.TORPEDO) {
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
						if (storage != null && storage.storageType == SeamothModule.StorageAccessType.TORPEDO) {
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

		public static ItemsContainer getVehicleStorageInSlot(Vehicle sm, int slotID, TechType seek) {
			InventoryItem slotItem = sm.GetSlotItem(slotID);
			if (slotItem == null)
				return null;
			Pickupable item = slotItem.item;
			if (!item)
				return null;
			TechType tt = item.GetTechType();
			if (tt == seek) {
				SeamothStorageContainer ssc = item.GetComponent<SeamothStorageContainer>();
				return ssc ? ssc.container : null;
			}
			if (sm is SeaMoth) {
				SeamothModule.SeamothModuleStorage storage = SeamothModule.getStorageHandler(tt);
				if (storage != null && storage.storageType == SeamothModule.StorageAccessType.BOX) {
					SeamothStorageContainer ssc = item.GetComponent<SeamothStorageContainer>();
					if (ssc) {
						storage.apply(ssc);
						return ssc.container;
					}
				}
			}
			return null;
		}

		public static void updateSeamothModules(SeaMoth sm, int slotID, TechType techType, bool added) {
			if (added) {
				SeamothModule.SeamothModuleStorage storage = SeamothModule.getStorageHandler(techType);
				if (storage != null) {
					if (storage.storageType == SeamothModule.StorageAccessType.TORPEDO) {
						if (sm.torpedoSilos != null && slotID < sm.torpedoSilos.Length)
							sm.torpedoSilos[slotID].SetActive(true);
					}
					else if (storage.storageType == SeamothModule.StorageAccessType.BOX) {
						if (sm.storageInputs != null && slotID < sm.storageInputs.Length)
							sm.storageInputs[slotID].SetEnabled(true);
					}
				}
			}
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
			if (onCyclopsModulesChangedEvent != null && !sm.isBase)
				onCyclopsModulesChangedEvent.Invoke(sm);
		}

		public static void updatePrawnModules(Exosuit sm, int slotID, TechType techType, bool added) {
			if (onPrawnModulesChangedEvent != null)
				onPrawnModulesChangedEvent.Invoke(sm, slotID, techType, added);
		}

		public static void useSeamothModule(SeaMoth sm, TechType techType, int slotID) {
			Spawnable sp = ItemRegistry.instance.getItem(techType, false);
			if (sp is SeamothModule smm) {
				smm.onFired(sm, slotID, sm.GetSlotCharge(slotID));
				sm.quickSlotTimeUsed[slotID] = Time.time;
				sm.quickSlotCooldown[slotID] = smm.getUsageCooldown();
			}
			if (onSeamothModuleUsedEvent != null)
				onSeamothModuleUsedEvent.Invoke(sm, techType, slotID);
		}

		public static float getTemperatureForDamage(TemperatureDamage dmg) {
			if (Mathf.Abs(Time.time - dmg.timeDamageStarted) <= 2.5F) { //active lava dmg
																		//SNUtil.writeToChat(dmg+" Touch lava: "+dmg.timeDamageStarted+" > "+Mathf.Abs(Time.time-dmg.timeDamageStarted));
				return 1200;
			}
			LavaWarningTriggerDetector warn = dmg.GetComponentInChildren<LavaWarningTriggerDetector>();
			if (warn && warn.isInLava())
				return dmg.gameObject.FindAncestor<Exosuit>() ? 300 : 400;
			if (warn && warn.isInGeyser())
				return 180;
			Vehicle v = dmg.GetComponent<Vehicle>();
			return v ? v.precursorOutOfWater ? 25 : v.GetTemperature() : WaterTemperatureSimulation.main.GetTemperature(dmg.transform.position);
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
				hatched.fullyEnable();
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
			SpawnedItemTracker.SpawnedItemEvent e = SpawnedItemTracker.instance.getSpawnEvent(obj);
			if (e != null)
				TooltipFactory.WriteDescription(sb, e.tooltip);
		}

		private static string getInfectionTooltip(InfectedMixin mix) {
			if (mix.IsInfected()) {
				float amt = mix.infectedAmount;
				//return "Infected: "+((int)(amt*100))+"%";
				return amt >= 0.75
					? "This creature is severely infected."
					: amt >= 0.5
						? "Exhibiting symptoms of significant systemic infection."
						: amt >= 0.25 ? "Showing signs of infection." : "Elevated bacterial levels detected.";
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
			if (at is CustomBiome b) {
				float d = b.getMurkiness(settings.murkiness) / 100f;
				float scatter = b.getScatteringFactor(settings.scattering);
				Vector3 vector = b.getColorFalloff(settings.absorption) + (scatter * Vector3.one);
				Vector4 ret = new Vector4(vector.x, vector.y, vector.z, scatter) * d;
				ret.w = b.getFogStart(settings.startDistance);
				return ret;
			}
			return orig;
		}

		public static Vector4 interceptScattering(Vector4 orig, WaterscapeVolume.Settings settings) {
			BiomeBase at = BiomeBase.getBiome(Camera.main.transform.position);
			if (at is CustomBiome b) {
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
			return at is CustomBiome ? ((CustomBiome)at).getEmissiveVector(orig) : orig;
		}

		public static void recomputeFog() {
			SNUtil.log("Recomputing fog @ " + Camera.main.transform.position, SNUtil.diDLL);
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
					SNUtil.writeToChat("Dumping RenderTexture WaterBiomeManager::" + f.Name);
					RenderUtil.dumpTexture(SNUtil.diDLL, f.Name, (RenderTexture)get);
				}
				else if (get is Texture2D) {
					SNUtil.writeToChat("Dumping Texture2D WaterBiomeManager::" + f.Name);
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
	    	vec4.w = wf.sunValue;*/
		/*Vector4 vec4 = fogColor;
                    //SNUtil.writeToChat("Fog color "+vec4+", with density "+fogDensity.ToString("0.000"));
                    Shader.SetGlobalVector(ShaderPropertyID._UweFogLightColor, vec4);
                    Shader.SetGlobalFloat(ShaderPropertyID._UweExtinctionAndScatteringScale, /*wf.density*/
		/*fogDensity);
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
				Collider target = Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore)
					? hit.collider
					: null;
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
	    }*/
		/*

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
					sub.powerRelay.AddEnergy(gen, out float trash);
				}
				else {
					p.powerSource.power = Mathf.Clamp(p.powerSource.power + gen, 0f, p.powerSource.maxPower);
				}
			}
		}

		public static bool addPowerToSeabaseDelegate(IPowerInterface pi, float amount, out float stored, MonoBehaviour component) {
			SubRoot sub = component.gameObject.FindAncestor<SubRoot>();
			return sub ? sub.powerRelay.AddEnergy(amount, out stored) : pi.AddEnergy(amount, out stored);
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
			tgt.destroyGameObject.destroy(false);
		}

		public static float getRadiationLevel(Player p, float orig) {
			float ret = orig;
			ret = Mathf.Max(ret, p.GetComponent<AoERadiationTracker>().getRadiationIntensity());
			//SNUtil.writeToChat("Rad "+ret.ToString());
			//SNUtil.writeToChat((radiationCheckEvent != null)+" # "+orig);
			if (radiationCheckEvent != null) {
				RadiationCheck ch = new RadiationCheck(p.transform.position, ret);
				radiationCheckEvent.Invoke(ch);
				ret = ch.value;
			}
			//SNUtil.writeToChat("PRad "+ret.ToString());
			return ret;
		}

		public static void onReaperGrabVehicle(ReaperLeviathan r, Vehicle v) {
			if (reaperGrabVehicleEvent != null) {
				reaperGrabVehicleEvent.Invoke(r, v);
			}
		}

		public static void onCyclopsDamaged(SubRoot r, DamageInfo d) {
			if (cyclopsDamageEvent != null) {
				cyclopsDamageEvent.Invoke(r, d);
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
			//SNUtil.writeToChat("Testing "+gun.gameObject.GetFullHierarchyPath()+" grab of "+go.GetFullHierarchyPath());
			if (go.FindAncestor<Constructable>() || go.FindAncestor<SubRoot>() || gun.gameObject.FindAncestor<Vehicle>() == go)
				return -1;
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
					ret += (go.transform.forward * -1.25F) + (go.transform.up * -0.125F);
				return ret;
			}
			return orig;
		}

		public static Vector3 getPropulsionMoveToPoint(Vector3 orig, PropulsionCannon gun) {
			Vehicle v = Player.main.GetVehicle();
			return v is SeaMoth && gun.gameObject.FindAncestor<Vehicle>() == v ? v.transform.position : orig;
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
			//SNUtil.writeToChat("Drilling "+dr+" @ "+pos+" by "+driller);
			if (drillableDrillTickEvent != null && dr)
				drillableDrillTickEvent.Invoke(dr, pos, driller);
		}

		public static void onMapRoomTick(MapRoomFunctionality map) {
			if (scannerRoomTickEvent != null && map)
				scannerRoomTickEvent.Invoke(map);
		}

		public static void onItemsLost() {
			Inventory.main.container.forEach((ii) => {
				IrreplaceableItemRegistry.IrreplaceableItemData eff = IrreplaceableItemRegistry.instance.getEffects(ii.item.GetTechType());
				if (eff != null) {
					eff.onDiedWhileHolding.Invoke(ii);
				}
			});
			if (itemsLostEvent != null)
				itemsLostEvent.Invoke();
		}

		public static void onStorageContainerHover(StorageContainer sc, GUIHand hand) {
			DiscreteOperationalMachineLogic lgc = sc.GetComponentInParent<DiscreteOperationalMachineLogic>();
			if (lgc) {
				if (lgc.isWorking()) {
					HandReticle.main.SetProgress(lgc.getProgressScalar());
					HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
				}
				else {
					string err = lgc.getErrorHover();
					if (string.IsNullOrEmpty(err)) {
						HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
					}
					else {
						HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
						HandReticle.main.SetInteractText(err); //locale key
					}
				}
			}
			if (storageHoverEvent != null)
				storageHoverEvent.Invoke(sc, hand);
		}

		public static float getModuleFireCost(float cost, Vehicle v, TechType module) {
			if (moduleFireCostEvent != null) {
				ModuleFireCostCheck e = new ModuleFireCostCheck(v, module, cost);
				moduleFireCostEvent.Invoke(e);
				cost = e.value;
			}
			return cost;
		}

		public static void onSelfScan() {
			if (selfScanEvent != null)
				selfScanEvent.Invoke();
		}

		public static void filterScannerRoomResourceList(uGUI_MapRoomScanner gui) {
			if (scannerRoomTechTypeListingEvent != null)
				scannerRoomTechTypeListingEvent.Invoke(gui);
		}

		public static void tickWorldForces(WorldForces wf) {
			if (skipWorldForces)
				return;
			if (wf == null || wf.gameObject == null || !wf.gameObject.activeInHierarchy || !wf.enabled) {
				//WorldForcesManager.instance.RemoveWorldForces(wf);
				//SNUtil.log("Disabling invalid WF tick in "+wf);
				return;
			}
			wf.DoFixedUpdate();
		}

		public static void updateSkyApplier(SkyApplier wf) {
			if (skipSkyApplier)
				return;
			if (!wf || !wf.gameObject || !wf.transform) {
				return;
			}
			wf.UpdateSkyIfNecessary();
		}
		/*
	   public static bool isRightHandDownForLightToggle(Player p) {
	   	return p.GetRightHandDown();
	   }*/

		public static bool onStasisFreeze(StasisSphere s, Collider c, ref Rigidbody target) {
			CustomMachineLogic m = c.gameObject.FindAncestor<CustomMachineLogic>();
			//SNUtil.writeToChat("Stasis hit "+c+": "+m);
			if (m)
				m.onStasisHit(s);
			target = c.GetComponentInParent<Rigidbody>();
			if (!target)
				return false;
			if (target.GetComponent<BlueprintHandTarget>())
				return false;
			if (s.targets.Contains(target))
				return true;
			StasisEffectCheck ch = new StasisEffectCheck(s, target);
			if (onStasisRifleFreezeEvent != null)
				onStasisRifleFreezeEvent.Invoke(ch);
			string name = target.name.ToLowerInvariant();
			if (name.StartsWith("explorablewreck", StringComparison.InvariantCultureIgnoreCase))
				return false;
			if (name.Contains("biodome"))
				return false;
			if (name.Contains("life_pod") || name.Contains("lifepod"))
				return false;
			if (name.Contains("precursor") && (name.Contains("room") || name.Contains("base")))
				return false;
			if (target.gameObject.isFossilPrefab())
				return false;
			if (c.GetComponentInParent<Player>() || c.GetComponentInParent<Vehicle>())
				return false;
			if (ch.addToTargetList)
				s.targets.Add(target);
			if (ch.applyKinematicChange)
				target.isKinematic = true;
			if (ch.sendMessage)
				target.SendMessage("OnFreeze", SendMessageOptions.DontRequireReceiver);
			if (ch.doFX) {
				Utils.PlayOneShotPS(s.vfxFreeze, target.GetComponent<Transform>().position, Quaternion.identity, null);
				FMODUWE.PlayOneShot(s.soundEnter, s.tr.position, 1f);
			}
			return !target.isKinematic;
		}

		public static void onStasisUnfreeze(StasisSphere s, Rigidbody target) {
			if (!target)
				return;
			StasisEffectCheck ch = new StasisEffectCheck(s, target);
			if (onStasisRifleUnfreezeEvent != null)
				onStasisRifleUnfreezeEvent.Invoke(ch);
			if (ch.doFX)
				Utils.PlayOneShotPS(s.vfxUnfreeze, target.GetComponent<Transform>().position, Quaternion.identity, null);
			if (ch.applyKinematicChange)
				target.isKinematic = false;
			if (ch.sendMessage)
				target.SendMessage("OnUnfreeze", SendMessageOptions.DontRequireReceiver);
		}

		public static void onRedundantFragmentScan() {
			PDAScanner.ScanTarget tgt = PDAScanner.scanTarget;
			SNUtil.writeToChat(Language.main.Get(PDAScanner.GetEntryData(tgt.techType).blueprint) + " already known"); //Language.main.Get("ScannerRedundantScanned")
			RedundantScanEvent r = new RedundantScanEvent();
			if (onRedundantScanEvent != null)
				onRedundantScanEvent.Invoke(r);
			if (!r.preventNormalDrop)
				CraftData.AddToInventory(TechType.Titanium, 2, false, true);
		}
		/*
	   [Obsolete]
	   public static bool isEquipmentApplicable(EquipmentType itemType, EquipmentType slotType, Equipment box, Pickupable item) {
	   		bool ret = Equipment.IsCompatible(itemType, slotType);
	   		if (equipmentCompatibilityCheckEvent != null) {
	   			EquipmentCompatibilityCheck ch = new EquipmentCompatibilityCheck(box, item, itemType, slotType, ret);
	   			equipmentCompatibilityCheckEvent.Invoke(ch);
	   			ret = ch.allow;
	   		}
	   		return ret;
	   }*/

		public static EquipmentType getOverriddenEquipmentType(EquipmentType ret, TechType item) {
			if (equipmentTypeCheckEvent != null) {
				EquipmentTypeCheck ch = new EquipmentTypeCheck(item, ret);
				equipmentTypeCheckEvent.Invoke(ch);
				ret = ch.type;
			}
			return ret;
		}

		public static bool tryEat(Survival s, GameObject go) {
			EatAttempt ea = new EatAttempt(s, go);
			if (tryEatEvent != null) {
				tryEatEvent.Invoke(ea);
			}

			if (ea.allowEat && s.Eat(go)) {
				ConsumableTracker.instance.onConsume(go, true);
				if (onEatEvent != null)
					onEatEvent.Invoke(s, go);
				return true;
			}
			else {
				SoundManager.playSoundAt(SoundManager.buildSound("event:/interface/select"), Player.main.transform.position, false, -1, 1);
				return false;
			}
		}

		public static float getSwimSpeed(float f) {
			foreach (PlayerMovementSpeedModifier m in Player.main.gameObject.GetComponents<PlayerMovementSpeedModifier>())
				f *= m.speedModifier;
			if (getSwimSpeedEvent != null) {
				SwimSpeedCalculation calc = new SwimSpeedCalculation(f);
				getSwimSpeedEvent.Invoke(calc);
				return calc.speed;
			}
			else {
				return f;
			}
		}

		public static float getWalkSpeed(float f) {
			foreach (PlayerMovementSpeedModifier m in Player.main.gameObject.GetComponents<PlayerMovementSpeedModifier>())
				f *= m.speedModifier;
			//SNUtil.writeToChat("Walk speed is "+f.ToString("0.000"));
			return f;
		}

		public static void onVehicleDestroyed(Vehicle v) {
			if (onVehicleDestroyEvent != null)
				onVehicleDestroyEvent.Invoke(v);
			List<Pickupable> storeInLocker = new List<Pickupable>();
			List<IItemsContainer> li = new List<IItemsContainer>();
			v.GetAllStorages(li);
			foreach (ItemsContainer sc in li) {
				sc.forEach(ii => fireVehicleLoss(v, false, ii, storeInLocker));
			}
			if (v.modules != null) {
				v.modules.equipment.Values.ForEach(ii => fireVehicleLoss(v, true, ii, storeInLocker));
			}
			if (storeInLocker.Count > 0) {
				TemporaryFloatingLocker.createFloatingLocker(v.transform.position, storeInLocker);
			}
		}

		private static void fireVehicleLoss(Vehicle v, bool module, InventoryItem ii, List<Pickupable> locker) {
			if (ii != null && ii.item) {
				IrreplaceableItemRegistry.IrreplaceableItemData eff = IrreplaceableItemRegistry.instance.getEffects(ii.item.GetTechType());
				if (eff != null) {
					eff.onLostWithVehicle.Invoke(v, module, ii, locker);
				}
			}
		}

		public static void onSleep(Bed bed) {
			if (onSleepEvent != null)
				onSleepEvent.Invoke(bed);
		}

		public static float getFoodWaterConsumptionRate(float f) {
			if (getFoodRateEvent != null) {
				FoodRateCalculation calc = new FoodRateCalculation(f);
				getFoodRateEvent.Invoke(calc);
				return calc.rate;
			}
			return f;
		}

		public static Vector3 getPlayerMovementControl(Vector3 orig) { //used to override player controls
			if (getPlayerInputEvent != null) {
				PlayerInput calc = new PlayerInput(orig);
				getPlayerInputEvent.Invoke(calc);
				return calc.selectedInput;
			}
			return orig;
		}

		public static void doShootTorpedo(Bullet b, Vector3 position, Quaternion rotation, float speed, float lifeTime, Vehicle v) {
			b.Shoot(position, rotation, speed, lifeTime);
			if (onTorpedoFireEvent != null)
				onTorpedoFireEvent.Invoke(b, v);
		}

		public static Transform onTorpedoExploded(Transform result, SeamothTorpedo sm) {
			result.position = sm.tr.position;
			result.rotation = sm.tr.rotation;
			if (onTorpedoExplodeEvent != null)
				onTorpedoExplodeEvent.Invoke(sm, result);
			return result;
		}

		public static bool canSeeObject(Creature c, GameObject go) {
			float dist = 0;
			bool ret = c.hasEyes && c.IsInFieldOfView(go, out dist);
			if (canCreatureSeeObjectEvent != null) {
				CreatureSeeObjectCheck e = new CreatureSeeObjectCheck(c, go, ret, dist);
				canCreatureSeeObjectEvent.Invoke(e);
				ret = e.canSee;
			}
			return ret;
		}

		public static void tickPilotedVehicleAggression(AggressiveToPilotingVehicle ai) {
			Player main = Player.main;
			if (main == null || main.GetMode() != Player.Mode.LockedPiloting) {
				return;
			}
			Vehicle vehicle = main.GetVehicle();
			if (vehicle == null) {
				return;
			}
			bool can = Vector3.Distance(vehicle.transform.position, ai.transform.position) <= ai.range;
			if (aggressiveToPilotingEvent != null) {
				AggressiveToPilotingVehicleCheck e = new AggressiveToPilotingVehicleCheck(ai, vehicle, can);
				aggressiveToPilotingEvent.Invoke(e);
				can = e.canTarget;
			}
			if (can) {
				ai.lastTarget.target = vehicle.gameObject;
				ai.creature.Aggression.Add(ai.aggressionPerSecond * ai.updateAggressionInterval);
			}
		}

		public static void onBaseRebuild(Base b) {
			if (b.cellObjects != null && baseRebuildEvent != null)
				baseRebuildEvent.Invoke(b);
		}

		public static void recomputeBaseHullStrength(BaseHullStrength b) {
			if (b.baseComp != null) {
				if (GameModeUtils.RequiresReinforcements()) {
					BaseStrengthCalculation calc = new BaseStrengthCalculation(b);
					b.victims.Clear();
					foreach (Int3 cell in b.baseComp.AllCells) {
						if (b.baseComp.GridToWorld(cell).y < 0f) {
							Transform t = b.baseComp.GetCellObject(cell);
							if (t != null) {
								b.victims.Add(t.GetComponent<LiveMixin>());
								calc.computeCellStrength(cell);
							}
						}
					}
					if (baseStrengthComputeEvent != null)
						baseStrengthComputeEvent.Invoke(calc);
					float total = calc.finalStrength;
					if (!Mathf.Approximately(total, b.totalStrength))
						SNUtil.writeToChat(Language.main.GetFormat<float, float>("BaseHullStrChanged", total - b.totalStrength, total));
					b.totalStrength = total;
				}
			}
		}

		public static void applyItemBackground(uGUI_ItemIcon ico, Atlas.Sprite spr, InventoryItem ii) {
			ico.SetBackgroundSprite(spr);
			if (ii.item.GetTechType() == TechType.Peeper && ii.item.GetComponent<Peeper>().isHero) {
				//ico.background.color = new Color(2F, 1F, 0.2F, 1);
				try {
					ico.background.material.color = new Color(1.8F, 0.85F, 0.3F, 1);
				}
				catch (Exception e) {
					SNUtil.log(e.ToString());
				}
			}
			else {
				InfectedMixin im = ii.item.GetComponent<InfectedMixin>();
				if (im && im.IsInfected()) {
					try {
						ico.background.material.color = new Color(0.2F, 1.5F, 0.2F, 1);
					}
					catch (Exception e) {
						SNUtil.log(e.ToString());
					}
				}
			}
		}

		public static WaterParkCreatureParameters GetWPCP(TechType tt) {
			WaterParkCreatureParameters pp = WaterParkCreature.waterParkCreatureParameters.GetOrDefault(tt, WaterParkCreatureParameters.GetDefaultValue());
			if (pp == null) {
				SNUtil.writeToChat("Creature " + tt.AsString() + " had null ACU data! Using defaults.");
				pp = WaterParkCreatureParameters.GetDefaultValue();
			}
			return pp;
		}

		public static Pickupable onWaterFilterSpawn(FiltrationMachine fm, Pickupable pp) {
			Pickupable ret = pp;
			if (waterFilterSpawnEvent != null) {
				WaterFilterSpawn e = new WaterFilterSpawn(fm, pp);
				waterFilterSpawnEvent.Invoke(e);
				ret = e.item;
			}
			return ret;
		}

		public static int surfaceTypeDebugLevel = 1;

		public static SurfaceType debugGetSurfaceType(SurfaceType s, Vector3 vec) {
			if (surfaceTypeDebugLevel > 1 || (surfaceTypeDebugLevel == 1 && s != SurfaceType.Ceiling && s != SurfaceType.Wall && s != SurfaceType.Ground))
				SNUtil.writeToChat("Returning surface type " + s + " from " + vec);
			return s;
		}

		public static void registerUID(UniqueIdentifier uid) {
			string id = uid.id;
			if (string.IsNullOrEmpty(id)) {
				//SNUtil.log("Skipping register of UID with null ID: "+uid.name+" @ "+uid.transform.position, SNUtil.diDLL);
				return;
			}
			if (UniqueIdentifier.identifiers.TryGetValue(id, out UniqueIdentifier has)) {
				if (has != uid) {
					if (has) {
						if (skipZeroedDeserialization && has.transform.position.sqrMagnitude > 0.01 && uid.transform.position.sqrMagnitude < 0.01) {
							SNUtil.log("Skipping setup of UID at origin: " + uid.name + " in favor of " + has.name + " @ " + has.transform.position, SNUtil.diDLL);
							uid.gameObject.destroy(false);
						}
						else {
							Debug.LogErrorFormat(uid, "Overwriting id '{0}' (old class '{1}', new class '{2}'), used to be '{3}' at {4} now '{5}' at {6}", new object[] {
								id,
								has.classId,
								uid.classId,
								has.name,
								has.transform.position,
								uid.name,
								uid.transform.position
							});
							UniqueIdentifier.identifiers[id] = uid;
						}
						return;
					}
					UniqueIdentifier.identifiers[id] = uid;
					return;
				}
			}
			else {
				UniqueIdentifier.identifiers.Add(id, uid);
			}
		}

		public static GameObject createSpawnedItem(TechType tt, bool customOnly) {
			GameObject ret = CraftData.InstantiateFromPrefab(tt, customOnly);
			if (GameModeUtils.currentEffectiveMode != GameModeOption.Creative) {
				SpawnedItemTracker.SpawnedItemEvent e = SpawnedItemTracker.instance.addSpawn(tt);
				if (ret) {
					PrefabIdentifier pi = ret.GetComponentInChildren<PrefabIdentifier>();
					if (pi)
						e.setObject(pi);
					else
						SNUtil.log("No PrefabIdentifier to attach to spawn event " + e);
				}
				else {
					SNUtil.log("No object at all for spawn event " + e);
				}
			}
			return ret;
		}

		public static void onCommandUse(DevConsole c, string cmd) {
			CommandTracker.instance.onCommand(cmd);
		}

		public static float getGrowingPlantProgressInTick(float progress, GrowingPlant g) {
			if (growingPlantTickEvent != null)
				growingPlantTickEvent.Invoke(g, progress);
			return progress;
		}

		public static void onCuddlefishPlayed(CuteFishHandTarget tgt, Player ep, CuteFishHandTarget.CuteFishCinematic anim) {
			if (onPlayWithCuddlefish != null)
				onPlayWithCuddlefish.Invoke(tgt, ep, anim);
		}

		public static void onRocketStageCompleted(Rocket r) {
			if (onRocketStageCompletedEvent != null)
				onRocketStageCompletedEvent.Invoke(r, r.currentRocketStage, Rocket.IsAnyRocketReady);
			StoryGoal.Execute("RocketStage" + r.currentRocketStage, Story.GoalType.Story);
			if (Rocket.IsAnyRocketReady)
				StoryGoalScheduler.main.Schedule(new StoryGoal("RocketComplete", Story.GoalType.Story, 10F));
		}

		public static float getCrafterTime(float time, Crafter c, TechType recipe) {
			CraftTimeCalculation calc = new CraftTimeCalculation(time, c, recipe);
			if (craftTimeEvent != null)
				craftTimeEvent.Invoke(calc);
			//SNUtil.writeToChat("Crafting time adjusted to "+calc.craftingDuration.ToString("0.0")+"s from original "+calc.originalDuration.ToString("0.0")+"s");
			return calc.craftingDuration;
		}

		public static void pulseSeamothDefence(SeaMoth sm) {
			if (seamothDischargeEvent != null)
				seamothDischargeEvent.Invoke(sm);
		}

		public static void onTreaderChunkSpawn(SinkingGroundChunk chunk) {
			if (spawnTreaderChunk != null)
				spawnTreaderChunk.Invoke(chunk);
		}

		public static void onCrashfishExplode(Crash c) {
			if (crashfishExplodeEvent != null)
				crashfishExplodeEvent.Invoke(c);
		}

		public static bool checkTargetingSkip(bool orig, Transform obj) {
			if (!obj || !obj.gameObject)
				return orig;
			PrefabIdentifier id = obj.gameObject.FindAncestor<PrefabIdentifier>();
			if (!id)
				return orig;
			TargetabilityCheck calc = new TargetabilityCheck(!orig, obj, id);
			if (targetabilityEvent != null)
				targetabilityEvent.Invoke(calc);
			//SNUtil.writeToChat("Crafting time adjusted to "+calc.craftingDuration.ToString("0.0")+"s from original "+calc.originalDuration.ToString("0.0")+"s");
			return !calc.allowTargeting;
		}

		public static Pickupable onRefundConstructableIngredient(Pickupable pp, Constructable c) {
			Battery b = pp.GetComponent<Battery>();
			if (b)
				b.charge = 0;
			return pp;
		}

		public static void onAuroraSpawn(CrashedShipExploder ex) {
			if (auroraSpawnEvent != null)
				auroraSpawnEvent.Invoke(ex);
			ex.gameObject.EnsureComponent<ShipExplosionListener>();
		}

		class ShipExplosionListener : MonoBehaviour {

			private void OnShipExplode() {
				StoryGoalScheduler.main.Schedule(new StoryGoal("AuroraExplode", Story.GoalType.Story, 24F)); //right as the ship explodes
			}

		}

		private static GameObject teleportWithPlayer;
		private static PropulsionCannon activePropulsionGun;
		private static Vector3 relativeGrabPosition;
		private static int selectedSlot;

		private static void startTeleport() {
			if (!Player.main.GetVehicle() && !Player.main.currentSub) {
				Pickupable pp = Inventory.main.GetHeld();
				if (pp) {
					PropulsionCannon pc = pp.GetComponent<PropulsionCannon>();
					if (pc && pc.grabbedObject) {
						selectedSlot = Inventory.main.quickSlots.activeSlot;
						activePropulsionGun = pc;
						teleportWithPlayer = pc.grabbedObject;
						relativeGrabPosition = teleportWithPlayer.transform.position - Player.main.transform.position;
						teleportWithPlayer.transform.position = WorldUtil.getClosest<PrecursorTeleporter>(Player.main.transform.position).warpToPos;
						//SNUtil.writeToChat("Teleporting "+teleportWithPlayer+" with player, pre");
					}
				}
			}
		}

		private static void stopTeleport() {
			if (activePropulsionGun) {
				if (teleportWithPlayer) {
					//InventoryItem ii = Inventory.main.container.GetItems(activePropulsionGun.GetComponent<Pickupable>().GetTechType()).First();
					Inventory.main.quickSlots.SelectImmediate(selectedSlot);
					teleportWithPlayer.transform.position = Player.main.transform.position + relativeGrabPosition;
					activePropulsionGun.GrabObject(teleportWithPlayer);
					//SNUtil.writeToChat("Teleporting "+teleportWithPlayer+" with player, post");
				}
				else {
					//SNUtil.writeToChat("Object to teleport with player does not yet exist");
				}
			}
			teleportWithPlayer = null;
			activePropulsionGun = null;
		}
	}
}
