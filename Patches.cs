using System;
using System.IO;    //For data read/write methods
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {
	
	[HarmonyPatch(typeof(DayNightCycle))]
	[HarmonyPatch("Update")]
	public static class UpdateLoopHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onTick", false, typeof(DayNightCycle)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(StoryGoalCustomEventHandler))]
	[HarmonyPatch("NotifyGoalComplete")]
	public static class StoryHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onStoryGoalCompleted", false, typeof(string)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(CellManager))]
	[HarmonyPatch("RegisterEntity", typeof(LargeWorldEntity))]
	public static class EntityRegisterBypass {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onEntityRegister", false, typeof(CellManager), typeof(LargeWorldEntity)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("Update")]
	public static class PlayerTick {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "tickPlayer", false, typeof(Player)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Pickupable))]
	[HarmonyPatch("Pickup")]
	public static class OnPlayerPickup {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Pickupable", "PlayPickupSound", true, new Type[0]);
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onItemPickedUp", false, typeof(Pickupable)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(DamageSystem))]
	[HarmonyPatch("CalculateDamage")]
	public static class DamageCalcHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ret);
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "recalculateDamage", false, typeof(float), typeof(DamageType), typeof(GameObject), typeof(GameObject)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_3));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_2));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
				//already present//codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(SkyApplier))]
	[HarmonyPatch("Awake")]
	public static class SkyApplierSpawnHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onSkyApplierSpawn", false, typeof(SkyApplier)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(GrowingPlant))]
	[HarmonyPatch("SpawnGrownModel")]
	public static class PlantFinishedGrowingHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchEveryReturnPre(codes, injectCallback);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	
		private static void injectCallback(List<CodeInstruction> codes, int idx) {
			codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFarmedPlantGrowDone", false, typeof(GrowingPlant), typeof(GameObject)));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
		}
	}
	
	[HarmonyPatch(typeof(Plantable))]
	[HarmonyPatch("Spawn")]
	public static class PlantSpawnsGrowingHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stloc_0)+1;
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFarmedPlantGrowingSpawn", false, typeof(Plantable), typeof(GameObject)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	
		private static void injectCallback(List<CodeInstruction> codes, int idx) {
			codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFarmedPlantGrowDone", false, typeof(GrowingPlant), typeof(GameObject)));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
		}
	}
	
	[HarmonyPatch(typeof(Survival))]
	[HarmonyPatch("Use")]
	public static class ItemUseReimplementation {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "useItem", false, typeof(Survival), typeof(GameObject)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(CraftData))]
	[HarmonyPatch("IsInvUseable")]
	public static class ItemUsabilityHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "isItemUsable", false, typeof(TechType)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PDAScanner))]
	[HarmonyPatch("Unlock")]
	public static class ScanHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onScanComplete", false, typeof(PDAScanner.EntryData)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Sealed))]
	[HarmonyPatch("Weld")]
	public static class SealedEventTimingFix {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx2 = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "Sealed", "_sealed");
				int idx1 = idx2-2;
				int idx0 = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldstr, "Trigger opened event");
				List<CodeInstruction> li = InstructionHandlers.extract(codes, idx1, idx2);
				codes.InsertRange(idx0, li);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(BulkheadDoor))]
	[HarmonyPatch("OnHandHover")]
	public static class BulkheadLaserCutterNotice {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {/*
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "HandReticle", "SetInteractText", true, new Type[]{typeof(string)});
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getBulkheadMouseoverText", false, typeof(string), typeof(BulkheadDoor)));
				FileLog.Log("Codes are "+InstructionHandlers.toString(codes));*/
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getBulkheadMouseoverText", false, typeof(BulkheadDoor)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(BulkheadDoor))]
	[HarmonyPatch("OnHandClick")]
	public static class BulkheadDoorClickIntercept {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onBulkheadClick", false, typeof(BulkheadDoor)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(UseableDiveHatch))]
	[HarmonyPatch("IsInside")]
	public static class HatchInsideHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "isInsideForHatch", false, typeof(UseableDiveHatch)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Constructable))]
	[HarmonyPatch("SetState")]
	public static class ConstructionHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Story.ItemGoalTracker", "OnConstruct", false, new Type[]{typeof(TechType)});
				codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "onConstructionComplete", false, new Type[]{typeof(TechType), typeof(Constructable)});
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
}
