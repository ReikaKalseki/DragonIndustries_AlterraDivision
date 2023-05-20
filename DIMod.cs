using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{
  [QModCore]
  public class DIMod
  {
    public const string MOD_KEY = "ReikaKalseki.DIAlterra";
    
    public static readonly XMLLocale locale = new XMLLocale(SNUtil.diDLL, "XML/locale.xml");
    /*
    private static readonly List<SNMod> mods = new List<SNMod>();
    
    public static void addMod(SNMod mod) {
    	mods.Add(mod);
    }
    */
    //public static readonly ModLogger logger = new ModLogger();
    
    public static readonly Config<DIConfig.ConfigEntries> config = new Config<DIConfig.ConfigEntries>(SNUtil.diDLL);
		
	internal static readonly Dictionary<TechType, Buildable> machineList = new Dictionary<TechType, Buildable>();

    [QModPrePatch]
    public static void PreLoad()
    {    	
    	System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PlacedObject).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CustomPrefab).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WorldGenerator).TypeHandle);
        //mods.Insert(0, new SNMod(MOD_KEY));
    }

    [QModPatch]
    public static void Load()
    {   
    	SNUtil.log("Start DI Main Init", SNUtil.diDLL);
        config.load();
    	
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(SNUtil.diDLL.Location), "harmony-log.txt");
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SNUtil.log("Ran mod register, started harmony", SNUtil.diDLL);
        try {
        	harmony.PatchAll(SNUtil.diDLL);
        }
        catch (Exception ex) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(ex.Message);
			FileLog.Log(ex.StackTrace);
			FileLog.Log(ex.ToString());
        }
        
        ModVersionCheck.getFromGitVsInstall("Dragon Industries", SNUtil.diDLL, "DragonIndustries_AlterraDivision").register();
        SNUtil.checkModHash(SNUtil.diDLL);
        
        new ObjectDeleter().Patch();
        
        locale.load();
        
        createEgg(TechType.SpineEel, TechType.BonesharkEgg, 1, "SpineEelDesc", true, 0.16F, 4, 0.5F, BiomeType.BonesField_Ground, BiomeType.LostRiverJunction_Ground);
        createEgg(TechType.GhostRayBlue, TechType.JumperEgg, 1.75F, "GhostRayDesc", true, 0.12F, 2, 1, BiomeType.TreeCove_LakeFloor);
        createEgg(TechType.GhostRayRed, TechType.CrabsnakeEgg, 1.25F, "CrimsonRayDesc", true, 0.6F, 2, 1, BiomeType.InactiveLavaZone_Chamber_Floor_Far);
        createEgg(TechType.Biter, TechType.RabbitrayEgg, 1F, "BiterDesc", false, 0.6F, 2, 1, BiomeType.GrassyPlateaus_CaveFloor, BiomeType.Mountains_CaveFloor);
        createEgg(TechType.Blighter, TechType.RabbitrayEgg, 1F, "BlighterDesc", false, 0.6F, 2, 1, BiomeType.BloodKelp_CaveFloor);
        
        /*
        dispatchLoadPhase("loadConfig");
        dispatchLoadPhase("afterConfig");
        dispatchLoadPhase("doPatches");
        dispatchLoadPhase("addItems");
        dispatchLoadPhase("loadMain");
        dispatchLoadPhase("loadConfig");
        */
        addCommands();
	    
	    SpriteHandler.RegisterSprite(TechType.PDA, TextureManager.getSprite(SNUtil.diDLL, "Textures/ScannerSprites/PDA"));
	    SpriteHandler.RegisterSprite(TechType.Databox, TextureManager.getSprite(SNUtil.diDLL, "Textures/ScannerSprites/Databox"));
	    
    	SNUtil.log("Finish DI Main Init", SNUtil.diDLL);
    }
    
    private static void killSelf() {
    	Vehicle v = Player.main.GetVehicle();
    	if (v)
    		v.GetComponent<LiveMixin>().TakeDamage(99999);
    	if (Player.main.currentSub && Player.main.currentSub.isCyclops)
    		Player.main.currentSub.GetComponent<LiveMixin>().TakeDamage(99999);
    	Player.main.GetComponent<LiveMixin>().TakeDamage(99999);
    }
    
    private static void createEgg(TechType creature, TechType basis, float scale, string locKey, bool isBig, float grownScale, float daysToGrow, float rate, params BiomeType[] spawn) {
    	Action<CustomEgg> a = e => {
    		e.eggProperties.maxSize = grownScale;
    		if (!isBig)
    			e.eggProperties.initialSize = Mathf.Max(e.eggProperties.initialSize, 0.2F);
    		e.eggProperties.growingPeriod = daysToGrow*20*60;
    	};
    	SNUtil.allowDIDLL = true;
    	CustomEgg.createAndRegisterEgg(creature, basis, scale, locale.getEntry(locKey).desc, isBig, a, rate, spawn);
    	SNUtil.allowDIDLL = false;
    }
    
    [QModPostPatch]
    public static void PostLoad()
    { 
        //dispatchLoadPhase("loadModInteract");
        //dispatchLoadPhase("loadFinal");
        BiomeBase.initializeBiomeHoles();
    }
    
    private static void dispatchLoadPhase(string phase) {/*
        foreach (SNMod mod in mods) {
    		
        }*/
    }
    
    private static void addCommands() {
        BuildingHandler.instance.addCommand<string, PlacedObject>("pfb", BuildingHandler.instance.spawnPrefabAtLook);
        //BuildingHandler.instance.addCommand<string>("btt", BuildingHandler.instance.spawnTechTypeAtLook);
        BuildingHandler.instance.addCommand<bool>("bden", BuildingHandler.instance.setEnabled);  
        BuildingHandler.instance.addCommand("bdsa", BuildingHandler.instance.selectAll);
        BuildingHandler.instance.addCommand("bdslp", BuildingHandler.instance.selectLastPlaced);
        BuildingHandler.instance.addCommand<string>("bdexs", BuildingHandler.instance.saveSelection);
        BuildingHandler.instance.addCommand<string>("bdexa", BuildingHandler.instance.saveAll);
        BuildingHandler.instance.addCommand<string>("bdld", BuildingHandler.instance.loadFile);
        BuildingHandler.instance.addCommand("bdinfo", BuildingHandler.instance.selectedInfo);
        BuildingHandler.instance.addCommand("bdtex", BuildingHandler.instance.dumpTextures);
        BuildingHandler.instance.addCommand("bdact", BuildingHandler.instance.activateObject);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, bool>>("sound", SoundManager.playSound);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("biomeAt", printBiomeData);
	    ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("killSelf", killSelf);
	    ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("hideVersions", DIHooks.hideVersions);
        //ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, string, string>>("exec", DebugExec.run);
    }
    
    private static void printBiomeData() {
    	SNUtil.writeToChat("Current native biome: "+WaterBiomeManager.main.GetBiome(Player.main.transform.position, false));
    	SNUtil.writeToChat("Localized DI name: "+BiomeBase.getBiome(Player.main.transform.position).displayName);
    }
  }
}
