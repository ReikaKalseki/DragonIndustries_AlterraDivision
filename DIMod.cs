using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;

namespace ReikaKalseki.DIAlterra
{
  [QModCore]
  public class DIMod
  {
    public const string MOD_KEY = "ReikaKalseki.DIAlterra";
    /*
    private static readonly List<SNMod> mods = new List<SNMod>();
    
    public static void addMod(SNMod mod) {
    	mods.Add(mod);
    }
    */
    //public static readonly ModLogger logger = new ModLogger();

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
    
    [QModPostPatch]
    public static void PostLoad()
    { 
        //dispatchLoadPhase("loadModInteract");
        //dispatchLoadPhase("loadFinal");
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
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, bool>>("sound", SNUtil.playSound);
        //ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, string, string>>("exec", DebugExec.run);
    }
  }
}
