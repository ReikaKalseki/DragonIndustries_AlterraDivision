using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
  [QModCore]
  public class DIMod
  {
    public const string MOD_KEY = "ReikaKalseki.DIAlterra";
    
    //public static readonly ModLogger logger = new ModLogger();

    [QModPatch]
    public static void Load()
    {        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "harmony-log.txt");
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SNUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }
        catch (Exception ex) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(ex.Message);
			FileLog.Log(ex.StackTrace);
			FileLog.Log(ex.ToString());
        }
		
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WorldGenerator).TypeHandle);
    }
  }
}
