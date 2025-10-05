using System;
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using ReikaKalseki.DIAlterra;

using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.DIAlterra {

	static class DIPatches {

		[HarmonyPatch(typeof(DayNightCycle))]
		[HarmonyPatch("Update")]
		public static class UpdateLoopHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onTick", false, typeof(DayNightCycle)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SNCameraRoot))]
		[HarmonyPatch("SonarPing")]
		public static class SonarHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "pingSonar", false, typeof(SNCameraRoot)));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnUpgradeModuleUse")]
		public static class SeamothSonarHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "SNCameraRoot", "SonarPing", true, new Type[0]);
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "pingSeamothSonar", false, typeof(SeaMoth)));
					codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldarg_0));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(CyclopsSonarButton))]
		[HarmonyPatch("SonarPing")]
		public static class CyclopsSonarHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "SNCameraRoot", "SonarPing", true, new Type[0]);
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "pingCyclopsSonar", false, typeof(CyclopsSonarButton)));
					codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldarg_0));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onStoryGoalCompleted", false, typeof(string)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnUpgradeModuleChange")]
		public static class SeamothModuleHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectSMModuleHook);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectSMModuleHook(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "updateSeamothModules", false, typeof(SeaMoth), typeof(int), typeof(TechType), typeof(bool)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_3));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_2));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnUpgradeModuleUse")]
		public static class SeamothDefenceHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "ElectricalDefense", "chargeScalar");
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "pulseSeamothDefence", false, typeof(SeaMoth)));
					codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldarg_0));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SubRoot))]
		[HarmonyPatch("UpdateSubModules")]
		public static class CyclopsModuleHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectModuleHook);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectModuleHook(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "updateCyclopsModules", false, typeof(SubRoot)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(Exosuit))]
		[HarmonyPatch("OnUpgradeModuleChange")]
		public static class PrawnModuleHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectModuleHook);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectModuleHook(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "updatePrawnModules", false, typeof(Exosuit), typeof(int), typeof(TechType), typeof(bool)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_3));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_2));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnUpgradeModuleUse")]
		public static class SeamothModuleUseHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(injectModuleHook());
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static CodeInstruction[] injectModuleHook() {
				InsnList codes = new InsnList();
				codes.add(OpCodes.Ldarg_0);
				codes.add(OpCodes.Ldarg_1);
				codes.add(OpCodes.Ldarg_2);
				codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "useSeamothModule", false, typeof(SeaMoth), typeof(TechType), typeof(int));
				return codes.ToArray();
			}
		}

		[HarmonyPatch(typeof(SinkingGroundChunk))]
		[HarmonyPatch("Start")]
		public static class TreaderChunkHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onTreaderChunkSpawn", false, typeof(SinkingGroundChunk)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Crash))]
		[HarmonyPatch("Detonate")]
		public static class CrashfishExplodeHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onCrashfishExplode", false, typeof(Crash)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onEntityRegister", false, typeof(CellManager), typeof(LargeWorldEntity)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.injectTickHook(codes, "tickPlayer", typeof(Player));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("Update")]
		public static class SeaMothTick {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.injectTickHook(codes, "tickSeamoth", typeof(SeaMoth));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Exosuit))]
		[HarmonyPatch("Update")]
		public static class ExosuitTick {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.injectTickHook(codes, "tickPrawn", typeof(Exosuit));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SubRoot))]
		[HarmonyPatch("Update")]
		public static class SubTick {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.injectTickHook(codes, "tickSub", typeof(SubRoot));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(WaterTemperatureSimulation), "GetTemperature", new Type[] { typeof(Vector3) })]
		public static class WaterTempOverride {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectHook);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectHook(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getWaterTemperature", false, typeof(float), typeof(WaterTemperatureSimulation), typeof(Vector3)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(Pickupable))]
		[HarmonyPatch("Pickup")]
		public static class OnPlayerPickup {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Pickupable", "PlayPickupSound", true, new Type[0]);
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onItemPickedUp", false, typeof(Pickupable)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(ExosuitClawArm))]
		[HarmonyPatch("OnPickup")]
		public static class OnPrawnPickup {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "ItemsContainer", "UnsafeAdd", true, new Type[]{typeof(InventoryItem)});
					codes.InsertRange(idx + 1, new InsnList { new CodeInstruction(OpCodes.Ldloc_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onPrawnItemPickedUp", false, typeof(Pickupable)) });
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Player))]
		[HarmonyPatch("CanBreathe")]
		public static class PlayerBreathabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectCallback);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectCallback(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "canPlayerBreathe", false, typeof(bool), typeof(Player)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(DamageSystem))]
		[HarmonyPatch("CalculateDamage")]
		public static class DamageCalcHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ret);
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "recalculateDamage", false, typeof(float), typeof(DamageType), typeof(GameObject), typeof(GameObject)));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_3));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_2));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
					//already present//codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onSkyApplierSpawn", false, typeof(SkyApplier)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(VehicleDockingBay))]
		[HarmonyPatch("Start")]
		public static class VehicleDockingBaySpawnHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onDockingBaySpawn", false, typeof(VehicleDockingBay)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectCallback);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectCallback(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFarmedPlantGrowDone", false, typeof(GrowingPlant), typeof(GameObject)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(Plantable))]
		[HarmonyPatch("Spawn")]
		public static class PlantSpawnsGrowingHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stloc_0)+1;
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFarmedPlantGrowingSpawn", false, typeof(Plantable), typeof(GameObject)));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectCallback(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFarmedPlantGrowDone", false, typeof(GrowingPlant), typeof(GameObject)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(Survival))]
		[HarmonyPatch("Use")]
		public static class ItemUseReimplementation {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "useItem", false, typeof(Survival), typeof(GameObject));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "isItemUsable", false, typeof(TechType));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Inventory))]
		[HarmonyPatch("InternalDropItem")]
		public static class ItemDroppabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Inventory", "CanDropItemHere", false, new Type[]{typeof(Pickupable), typeof(bool)});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "isItemDroppable", false, typeof(Pickupable), typeof(bool));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onScanComplete", false, typeof(PDAScanner.EntryData)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Sealed))]
		[HarmonyPatch("Weld")]
		public static class SealedOverhaul {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "tickLaserCutting", false, typeof(Sealed), typeof(float));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {/*
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "HandReticle", "SetInteractText", true, new Type[]{typeof(string)});
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getBulkheadMouseoverText", false, typeof(string), typeof(BulkheadDoor)));
				FileLog.Log("Codes are "+InstructionHandlers.toString(codes));*/
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "getBulkheadMouseoverText", false, typeof(BulkheadDoor));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "onBulkheadClick", false, typeof(BulkheadDoor));
					codes.add(OpCodes.Ret);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnHoverTorpedoStorage")]
		public static class SeamothTorpedoHoverHooks {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "hoverSeamothTorpedoStorage", false, typeof(SeaMoth), typeof(HandTargetEventData));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OpenTorpedoStorage")]
		public static class SeamothTorpedoClickHooks {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "openSeamothTorpedoStorage", false, typeof(SeaMoth), typeof(Transform));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Vehicle))]
		[HarmonyPatch("GetStorageInSlot")]
		public static class SeamothStorageAccessHooks {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.add(OpCodes.Ldarg_2);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "getVehicleStorageInSlot", false, typeof(Vehicle), typeof(int), typeof(TechType));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(TemperatureDamage))]
		[HarmonyPatch("GetTemperature")]
		public static class TemperatureDamageGetOverride {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "getTemperatureForDamage", false, typeof(TemperatureDamage));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "isInsideForHatch", false, typeof(UseableDiveHatch));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(LargeWorld), "GetBiome", new Type[] { typeof(Vector3) })]
		public static class BiomeFetchHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectHook);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectHook(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getBiomeAt", false, typeof(string), typeof(Vector3)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
			}
		}

		[HarmonyPatch(typeof(Constructable))]
		[HarmonyPatch("NotifyConstructedChanged")]
		public static class ConstructionHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onConstructionComplete", false, new Type[] { typeof(Constructable), typeof(bool) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Knife))]
		[HarmonyPatch("OnToolUseAnim")]
		public static class KnifeHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Utils", "PlayFMODAsset", false, new Type[]{typeof(FMODAsset), typeof(Transform), typeof(float)});
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onKnifed", false, typeof(GameObject)));
					codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldloc_1));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Knife))]
		[HarmonyPatch("IsValidTarget")]
		public static class KnifeabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "isObjectKnifeable", false, typeof(LiveMixin));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(uGUI_PopupNotification))]
		[HarmonyPatch("Set")]
		public static class DebugTechPopup {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onPopup", false, typeof(uGUI_PopupNotification)));
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
		*/

		[HarmonyPatch(typeof(WaterParkCreature))]
		[HarmonyPatch("Born")]
		public static class WaterParkFix {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "UnityEngine.GameObject", "SetActive", true, new Type[]{typeof(bool)});
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onEggHatched", false, typeof(GameObject)));
					codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldloc_0));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(TooltipFactory))]
		[HarmonyPatch("ItemCommons")]
		public static class CustomTooltipHooks {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "appendItemTooltip", false, typeof(System.Text.StringBuilder), typeof(TechType), typeof(GameObject)));
					int idx = codes.Count-1;
					codes[idx].MoveLabelsTo(codes[idx - 4]);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(EMPBlast))]
		[HarmonyPatch("OnTouch")]
		public static class EMPBlastHooks {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					for (int i = codes.Count - 1; i >= 0; i--) {
						if (codes[i].opcode == OpCodes.Callvirt) {
							MethodInfo mi = (MethodInfo)codes[i].operand;
							if (mi.Name == "DisableElectronicsForTime") {
								PatchLib.injectEMPHook(codes, i);
							}
						}
					}
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onEMPTouch", false, typeof(EMPBlast), typeof(Collider)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(Builder))]
		[HarmonyPatch("CheckAsSubModule")]
		public static class ConstructableBuildabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Builder", "CheckTag", false, new Type[]{typeof(Collider)});
					codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "interceptConstructability", false, typeof(Collider));
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
		}*/
		[HarmonyPatch(typeof(Builder))]
		[HarmonyPatch("Update")]
		public static class ConstructableBuildabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Builder", "UpdateAllowed", false, new Type[0]);
					codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "interceptConstructability", false, new Type[0]);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(WaterscapeVolume))]
		[HarmonyPatch("PreRender")]
		public static class WaterFogShaderHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					//int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "uSkyManager", "GetLightColor", true, new Type[0]);
					//codes.InsertRange(idx+1, new InsnList{new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "interceptChosenColor", false, typeof(Color), typeof(WaterscapeVolume), typeof(Camera))});
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "interceptChosenFog", false, typeof(WaterscapeVolume), typeof(Camera));
					codes.add(OpCodes.Ret);
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
		}*/
		/*
		[HarmonyPatch(typeof(WaterBiomeManager))]
		[HarmonyPatch("RasterizeAtmosphereVolumes")]
		public static class WaterFogShaderHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {/*
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "WaterBiomeManager", "GetEmissiveTextureValue", true, new Type[]{typeof(WaterscapeVolume.Settings)});
					CodeInstruction settings = codes[idx-1];
					InsnList add = new InsnList{
						new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2), new CodeInstruction(settings.opcode, settings.operand),
						InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "overrideFog", false, typeof(WaterBiomeManager), typeof(Vector3), typeof(bool), typeof(WaterscapeVolume.Settings))
					};
					codes.InsertRange(idx+2, add);*//*
					InsnList add = new InsnList{
						new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2),
						InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFogRasterized", false, typeof(WaterBiomeManager), typeof(Vector3), typeof(bool))
					};
					InstructionHandlers.patchInitialHook(add);
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
		*/
		[HarmonyPatch(typeof(WaterBiomeManager))]
		[HarmonyPatch("GetExtinctionTextureValue")]
		public static class ExtinctionTextureHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "interceptExtinction", false, typeof(Vector4), typeof(WaterscapeVolume.Settings)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(WaterBiomeManager))]
		[HarmonyPatch("GetScatteringTextureValue")]
		public static class ScatterTextureHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "interceptScattering", false, typeof(Vector4), typeof(WaterscapeVolume.Settings)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(WaterBiomeManager))]
		[HarmonyPatch("GetEmissiveTextureValue")]
		public static class EmissiveTextureHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "interceptEmissive", false, typeof(Vector4), typeof(WaterscapeVolume.Settings)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(PowerRelay))]
		[HarmonyPatch("GetMaxPower")]
		public static class SeabasePowerCapacityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ret);
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getPowerRelayCapacity", false, typeof(float), typeof(PowerRelay)));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
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
		*/
		[HarmonyPatch(typeof(SolarPanel))]
		[HarmonyPatch("Update")]
		public static class SolarPanelPowerRedirect {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {/* BZ code
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "PowerRelay", "ModifyPower", true, new Type[]{typeof(float), typeof(float).MakeByRefType()});
				codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "addPowerToSeabaseDelegate", false, typeof(IPowerInterface), typeof(float), typeof(float).MakeByRefType(), typeof(MonoBehaviour));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				*/
					/*
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "PowerSource", "power");
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "addPowerToSeabaseDelegateViaPowerSourceSet", false, typeof(PowerSource), typeof(float), typeof(MonoBehaviour));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
					*/
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "updateSolarPanel", false, typeof(SolarPanel));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(ThermalPlant))]
		[HarmonyPatch("AddPower")]
		public static class ThermalPlantPowerRedirect {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.redirectPowerHook(codes);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(BaseBioReactor))]
		[HarmonyPatch("Update")]
		public static class BioreactorPowerRedirect {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.redirectPowerHook(codes);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(BaseNuclearReactor))]
		[HarmonyPatch("Update")]
		public static class NucReactorPowerRedirect {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				if (codes.Count == 1 && codes[0].opcode == OpCodes.Ret) {
					FileLog.Log("Skipping patch " + MethodBase.GetCurrentMethod().DeclaringType + ", Update() was cleared");
					return codes;
				}
				try {
					PatchLib.redirectPowerHook(codes);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(StoryHandTarget))]
		[HarmonyPatch("OnHandClick")]
		public static class StoryHandIntercept {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "clickStoryHandTarget", false, typeof(StoryHandTarget));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Player))]
		[HarmonyPatch("SetRadiationAmount")]
		public static class RadiationAmountIntercept {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Stfld);
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getRadiationLevel", false, typeof(Player), typeof(float)));
					codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Knife))]
		[HarmonyPatch("OnToolUseAnim")]
		public static class KnifeHarvestingHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Knife", "GiveResourceOnDamage", false, new Type[]{typeof(GameObject), typeof(bool), typeof(bool)});
					codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "doKnifeHarvest", false, typeof(Knife), typeof(GameObject), typeof(bool), typeof(bool));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(uGUI_ItemsContainer))]
		[HarmonyPatch("OnAddItem")]
		public static class ItemVisualSizeHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.patchVisualItemSize(codes);
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

		[HarmonyPatch(typeof(uGUI_ItemsContainerView))]
		[HarmonyPatch("OnAddItem")]
		public static class ItemVisualSizeHookView {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.patchVisualItemSize(codes);
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

		[HarmonyPatch(typeof(ItemsContainer))]
		[HarmonyPatch("UnsafeAdd")]
		public static class ItemFunctionalSizeHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.patchVisualItemSize(codes, true);
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

		[HarmonyPatch(typeof(InventoryItem))]
		[HarmonyPatch("get_height")]
		public static class InvItemHeightHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.patchVisualItemSize(codes, true, false, new Type[]{typeof(TechType), typeof(InventoryItem)});
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

		[HarmonyPatch(typeof(InventoryItem))]
		[HarmonyPatch("get_width")]
		public static class InvItemWidthHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.patchVisualItemSize(codes, true, false, new Type[]{typeof(TechType), typeof(InventoryItem)});
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
		}*/

		[HarmonyPatch(typeof(ReaperLeviathan))]
		[HarmonyPatch("GrabVehicle")]
		public static class ReaperGrabHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onReaperGrabVehicle", false, typeof(ReaperLeviathan), typeof(Vehicle)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SubRoot))]
		[HarmonyPatch("OnTakeDamage")]
		public static class CyclopsDamageHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onCyclopsDamaged", false, typeof(SubRoot), typeof(DamageInfo)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(VehicleDockingBay))]
		[HarmonyPatch("OnTriggerEnter")]
		public static class MoonpoolGrabDetection {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "onDockingTriggerCollided", false, typeof(VehicleDockingBay), typeof(Collider));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(AcidicBrineDamageTrigger))]
		[HarmonyPatch("OnTriggerEnter")]
		public static class BrineTouchDetection {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "onAcidTriggerCollided", false, typeof(AcidicBrineDamageTrigger), typeof(Collider));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PrecursorDoorMotorModeSetter))]
		[HarmonyPatch("OnTriggerEnter")]
		public static class AirlockTouchDetection {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "onAirlockTouched", false, typeof(PrecursorDoorMotorModeSetter), typeof(Collider));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(FMOD_CustomEmitter))]
		[HarmonyPatch("OnPlay")]
		public static class SoundHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					//int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "FMOD_CustomEmitter", "OnPlay", true, new Type[0]);
					CodeInstruction ci = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFModEmitterPlay", false, typeof(FMOD_CustomEmitter));
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), ci);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PropulsionCannon))]
		[HarmonyPatch("ValidateObject")]
		public static class PropulsabilityHookMass {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.patchPropulsability(codes, InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PropulsionCannon", "maxMass", true), true);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PropulsionCannon))]
		[HarmonyPatch("ValidateNewObject")]
		public static class PropulsabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.patchPropulsability(codes, InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PropulsionCannon", "maxAABBVolume", true), false);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PropulsionCannon))]
		[HarmonyPatch("TraceForGrabTarget")]
		public static class PropulsionGrabPositionFix {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "UWE.Utils", "SpherecastIntoSharedBuffer", false, new Type[]{typeof(Vector3), typeof(float), typeof(Vector3), typeof(float), typeof(int), typeof(QueryTriggerInteraction)});
					codes[idx - 1] = new CodeInstruction(OpCodes.Ldc_I4_1);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PropulsionCannon))]
		[HarmonyPatch("GetObjectPosition")]
		public static class PropulsionGrabPositionFix2 {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "PropulsionCannon", "grabbedObjectCenter");
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getPropulsionTargetCenter", false, typeof(Vector3), typeof(GameObject)));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(PropulsionCannon))]
		[HarmonyPatch("UpdateTargetPosition")]
		public static class PropulsionGrabPositionFix3 {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getPropulsionMoveToPoint", false, typeof(Vector3), typeof(PropulsionCannon)));
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
		*/
		[HarmonyPatch(typeof(RepulsionCannon))]
		[HarmonyPatch("OnToolUseAnim")]
		public static class RepulsabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.patchPropulsability(codes, InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 1300F), true, new CodeInstruction(OpCodes.Ldloc_S, 11));
					PatchLib.patchPropulsability(codes, InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 400F), false, new CodeInstruction(OpCodes.Ldloc_S, 11));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Vehicle))]
		[HarmonyPatch("EnterVehicle")]
		public static class VehicleEnterHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					CodeInstruction ci = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onVehicleEnter", false, typeof(Vehicle), typeof(Player));
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), ci);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(uGUI_DepthCompass))]
		[HarmonyPatch("UpdateDepth")]
		public static class OverrideDepthCompass {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {/*
				for (int i = codes.Count-1; i >= 0; i--) {
					if (codes[i].opcode == OpCodes.Call) {
						MethodInfo mi = (MethodInfo)codes[i].operand;
						if (mi.Name == "FloorToInt") {
							codes.Insert(i, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getCompassDepth", false, typeof(float)));
						}
					}
				}*/
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "uGUI_DepthCompass", "GetDepthInfo", true, new Type[]{typeof(int).MakeByRefType(), typeof(int).MakeByRefType()});
					/*
					InsnList li = new InsnList();
					li.add(OpCodes.Ldarg_0);
					li.add(OpCodes.Ldloc_S, 0);
					li.invoke("ReikaKalseki.DIAlterra.DIHooks", "getCompassDepth", false, typeof(uGUI_DepthCompass), typeof(int).MakeByRefType());
					//li.add(OpCodes.Stloc_S, 0);
					codes.InsertRange(idx+2, li);*/

					codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "getCompassDepth", false, typeof(uGUI_DepthCompass), typeof(int).MakeByRefType(), typeof(int).MakeByRefType());
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Survival))]
		[HarmonyPatch("OnRespawn")]
		public static class RespawnHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new InsnList(){
					new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onRespawnPre", false, typeof(Survival), typeof(Player))
				});
					codes.patchEveryReturnPre(new InsnList(){
					new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onRespawnPost", false, typeof(Survival), typeof(Player))
				});
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Inventory))]
		[HarmonyPatch("LoseItems")]
		public static class ItemLossHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onItemsLost", false, new Type[0]));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));

					//this is a bugfix, they stop at second last item for some reason
					int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Sub);
					codes.RemoveAt(idx);
					codes.RemoveAt(idx - 1);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/* DO NOT ENABLE, CAUSES ALL GUI PINGS TO DISAPPEAR WHEN THEIR CAUSAL GO DERENDERS/UNLOADS AT DISTANCE
		[HarmonyPatch(typeof(ResourceTracker))]
		[HarmonyPatch("OnDestroy")]
		public static class ResourceTrackerDestroyUnregisterFix {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new InsnList(){
															new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ResourceTracker", "Unregister", true, new Type[0])
					});
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
		}*/
		/*
		[HarmonyPatch(typeof(Vehicle))]
		[HarmonyPatch("set_docked")]
		public static class DockingDebug {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "logDockingVehicle", false, typeof(Vehicle), typeof(bool)));
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
		*/

		[HarmonyPatch(typeof(Drillable))]
		[HarmonyPatch("OnDrill")]
		public static class DrillableCallHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onDrillableDrilled", false, typeof(Drillable), typeof(Vector3), typeof(Exosuit)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(uGUI_MainMenu))]
		[HarmonyPatch("Awake")]
		public static class MainMenuLoadHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onMainMenuLoaded", false, new Type[0]));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(MapRoomFunctionality))]
		[HarmonyPatch("UpdateBlips")]
		public static class MapRoomUpdateHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new InsnList(){
					new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onMapRoomTick", false, typeof(MapRoomFunctionality))
				});
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(StorageContainer))]
		[HarmonyPatch("OnHandHover")]
		public static class StorageContainerMouseoverHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new InsnList(){
					new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onStorageContainerHover", false, typeof(StorageContainer), typeof(GUIHand))
				});
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Vehicle))]
		[HarmonyPatch("ConsumeEnergy", typeof(TechType))]
		public static class ModuleFireCostHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Vehicle", "ConsumeEnergy", false, new Type[]{typeof(float)});
					codes.InsertRange(idx, new InsnList { new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getModuleFireCost", false, typeof(float), typeof(Vehicle), typeof(TechType)) });
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PDAScanner))]
		[HarmonyPatch("Scan")]
		public static class SelfScanHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "Story.StoryGoal", "Trigger", false, new Type[0]);
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onSelfScan", false, new Type[0]));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(uGUI_MapRoomScanner))]
		[HarmonyPatch("RebuildResourceList")]
		public static class ScannerTypeFilteringHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "filterScannerRoomResourceList", false, typeof(uGUI_MapRoomScanner));
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), call);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(WorldForcesManager))]
		[HarmonyPatch("FixedUpdate")]
		public static class CleanupWorldForcesManager {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "WorldForces", "DoFixedUpdate", true, new Type[0]);
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "tickWorldForces", false, typeof(WorldForces));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SkyApplierUpdater))]
		[HarmonyPatch("Update")]
		public static class CleanupSkyApplierUpdater {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "SkyApplier", "UpdateSkyIfNecessary", true, new Type[0]);
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "updateSkyApplier", false, typeof(SkyApplier));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(ToggleLights))]
		[HarmonyPatch("CheckLightToggle")]
		public static class LightToggleRework {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					//int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "Player", "GetRightHandDown", true, new Type[0]);
					//codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "isRightHandDownForLightToggle", false, typeof(Player));
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 0.25F);
					codes[idx].operand = -1F;
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(StasisSphere))]
		[HarmonyPatch("LateUpdate")]
		public static class StasisRifleHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "StasisSphere", "Freeze", true, new Type[]{typeof(Collider), typeof(Rigidbody).MakeByRefType()});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onStasisFreeze", false, typeof(StasisSphere), typeof(Collider), typeof(Rigidbody).MakeByRefType());

					idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "StasisSphere", "Unfreeze", true, new Type[] { typeof(Rigidbody) });
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onStasisUnfreeze", false, typeof(StasisSphere), typeof(Rigidbody));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(StasisSphere))]
		[HarmonyPatch("UnfreezeAll")]
		public static class StasisRifleHook2 {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "StasisSphere", "Unfreeze", true, new Type[]{typeof(Rigidbody)});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onStasisUnfreeze", false, typeof(StasisSphere), typeof(Rigidbody));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PDAScanner))]
		[HarmonyPatch("Scan")]
		[HarmonyPriority(Priority.Last)]
		public static class RedundantScanHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "CraftData", "AddToInventory", true, new Type[]{typeof(TechType), typeof(int), typeof(bool), typeof(bool)});
					int idx0 = InstructionHandlers.getLastOpcodeBefore(codes, idx-1, OpCodes.Call);
					codes.RemoveRange(idx0 + 1, idx - idx0);
					codes.Insert(idx0 + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onRedundantFragmentScan", false, new Type[0]));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(Equipment))]
		[HarmonyPatch("AllowedToAdd")]
		public static class EquipmentApplicabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.addEquipmentAllowedHook(codes, new CodeInstruction(OpCodes.Ldarg_2));
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

		[HarmonyPatch(typeof(Equipment))]
		[HarmonyPatch("GetCompatibleSlotDefault")]
		public static class EquipmentApplicabilityHook2 {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.addEquipmentAllowedHook(codes);
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

		[HarmonyPatch(typeof(Equipment))]
		[HarmonyPatch("GetFreeSlot")]
		public static class EquipmentApplicabilityHook3 {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.addEquipmentAllowedHook(codes);
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

		[HarmonyPatch(typeof(Equipment))]
		[HarmonyPatch("GetSlots")]
		public static class EquipmentApplicabilityHook4 {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.addEquipmentAllowedHook(codes);
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
		*/

		[HarmonyPatch(typeof(CraftData))]
		[HarmonyPatch("GetEquipmentType")]
		public static class EquipmentTypeHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getOverriddenEquipmentType", false, typeof(EquipmentType), typeof(TechType)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Inventory))]
		[HarmonyPatch("ExecuteItemAction")]
		public static class EatInterception {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "Survival", "Eat", true, new Type[]{typeof(GameObject)});
					codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "tryEat", false, typeof(Survival), typeof(GameObject));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(UnderwaterMotor))]
		[HarmonyPatch("UpdateMove")]
		public static class AffectSwimSpeed {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {/*
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "Inventory", "GetHeldTool", true, new Type[0]);
				while (codes[idx].opcode != OpCodes.Ldloc_1)
					idx--;
				codes.Insert(idx, new CodeInstruction(OpCodes.Stloc_0));
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getSwimSpeed", false, typeof(float)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
				*/
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stloc_S, 9);
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getSwimSpeed", false, typeof(float)));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(GroundMotor))]
		[HarmonyPatch("ApplyInputVelocityChange")]
		public static class AffectWalkSpeed {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getLastInstructionBefore(codes, codes.Count, OpCodes.Ldloc_S, 11);
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getWalkSpeed", false, typeof(float)));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Vehicle))]
		[HarmonyPatch("OnKill")]
		public static class VehicleDeathHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new InsnList(){
					new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onVehicleDestroyed", false, typeof(Vehicle))
				});
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Bed))]
		[HarmonyPatch("EnterInUseMode")]
		public static class SleepHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getLastOpcodeBefore(codes, codes.Count, OpCodes.Ret);
					codes.InsertRange(idx, new InsnList{
					new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onSleep", false, typeof(Bed))
				});
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Survival))]
		[HarmonyPatch("UpdateStats")]
		public static class AffectFoodWaterRate {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getFoodWaterConsumptionRate", false, typeof(float)), new CodeInstruction(OpCodes.Starg, 1));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(BaseRoot))]
		[HarmonyPatch("Start")]
		public static class BaseLoadHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onBaseLoaded", false, new Type[] { typeof(BaseRoot) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(StorageContainer))]
		[HarmonyPatch("Open", typeof(Transform))]
		public static class InvOpenHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onInvOpened", false, new Type[] { typeof(StorageContainer) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(StorageContainer))]
		[HarmonyPatch("OnClosePDA")]
		public static class InvCloseHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onInvClosed", false, new Type[] { typeof(StorageContainer) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(GameInput))]
		[HarmonyPatch("GetMoveDirection")]
		public static class InputDirectionOverrideHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getPlayerMovementControl", false, new Type[] { typeof(Vector3) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(SeamothTorpedo))]
		[HarmonyPatch("Explode")]
		public static class TorpedoExplodeHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Callvirt);
					codes.InsertRange(idx + 1, new InsnList { new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onTorpedoExploded", false, new Type[] { typeof(Transform), typeof(SeamothTorpedo) }) });
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Vehicle))]
		[HarmonyPatch("TorpedoShot")]
		public static class TorpedoFireHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "Bullet", "Shoot", true, new Type[]{typeof(Vector3), typeof(Quaternion), typeof(float), typeof(float)});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "doShootTorpedo", false, new Type[] { typeof(Bullet), typeof(Vector3), typeof(Quaternion), typeof(float), typeof(float), typeof(Vehicle) });
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Gravsphere))]
		[HarmonyPatch("IsValidTarget")]
		public static class GravTrapGrabbabilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "canGravTrapGrab", false, new Type[] { typeof(Gravsphere), typeof(GameObject) });
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(PrecursorTeleporter))]
		[HarmonyPatch("BeginTeleportPlayer")]
		public static class ArchTeleportHookPre {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onArchTeleportPre", false, new Type[]{typeof(PrecursorTeleporter), typeof(GameObject)}));
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

		[HarmonyPatch(typeof(PrecursorTeleporter))]
		[HarmonyPatch("BeginTeleportPlayer")]
		public static class ArchTeleportHookPre {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onArchTeleportPre", false, new Type[]{typeof(PrecursorTeleporter), typeof(GameObject)}));
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
		}*/

		[HarmonyPatch(typeof(Constructor))]
		[HarmonyPatch("OnConstructionDone")]
		public static class VehicleBayCompletionHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onVehicleBayFinish", false, new Type[] { typeof(Constructor), typeof(GameObject) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Creature))]
		[HarmonyPatch("GetCanSeeObject")]
		public static class VisibilityToCreatureHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "canSeeObject", false, new Type[] { typeof(Creature), typeof(GameObject) });
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(AggressiveToPilotingVehicle))]
		[HarmonyPatch("UpdateAggression")]
		public static class VehicleVisibilityToCreatureHook1 {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "tickPilotedVehicleAggression", false, new Type[] { typeof(AggressiveToPilotingVehicle) });
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Base))]
		[HarmonyPatch("RebuildGeometry")]
		public static class BaseRebuildHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onBaseRebuild", false, new Type[] { typeof(Base) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(BaseHullStrength))]
		[HarmonyPatch("OnPostRebuildGeometry")]
		public static class BaseComputeStrengthHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "recomputeBaseHullStrength", false, new Type[] { typeof(BaseHullStrength) });
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(uGUI_ItemsContainer))]
		[HarmonyPatch("OnAddItem")]
		public static class ItemBackgroundHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "uGUI_ItemIcon", "SetBackgroundSprite", true, new Type[]{typeof(Atlas.Sprite)});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "applyItemBackground", false, new Type[] { typeof(uGUI_ItemIcon), typeof(Atlas.Sprite), typeof(InventoryItem) });
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(WaterParkCreature))]
		[HarmonyPatch("GetParameters")]
		public static class GetWPCP {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "GetWPCP", false, new Type[] { typeof(TechType) });
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(Player))]
		[HarmonyPatch("GetOxygenPerBreath")]
		public static class PlayerO2Use {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.add(OpCodes.Ldarg_2);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "getPlayerO2Use", false, typeof(Player), typeof(float), typeof(int));
					codes.add(OpCodes.Ret);
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
		*/
		[HarmonyPatch(typeof(FiltrationMachine))]
		[HarmonyPatch("Spawn")]
		public static class OnWaterFilterSpawn {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onWaterFilterSpawn", false, new Type[] { typeof(FiltrationMachine), typeof(Pickupable) }), new CodeInstruction(OpCodes.Starg, 1));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Builder))]
		[HarmonyPatch("GetSurfaceType")]
		public static class DebugGetSurfaceType {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "debugGetSurfaceType", false, new Type[] { typeof(SurfaceType), typeof(Vector3) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(UniqueIdentifier))]
		[HarmonyPatch("Register")]
		public static class CleanupIdentifierRegister {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "registerUID", false, typeof(UniqueIdentifier));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(InventoryConsoleCommands))]
		[HarmonyPatch("OnConsoleCommand_item")]
		public static class ItemSpawnHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "CraftData", "InstantiateFromPrefab", false, new Type[]{typeof(TechType), typeof(bool)});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "createSpawnedItem", false, typeof(TechType), typeof(bool));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(ExosuitClawArm))]
		[HarmonyPatch("TryUse")]
		public static class PrawnGrabDelayRemoval {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					for (int i = codes.Count - 1; i >= 0; i--) {
						CodeInstruction ci = codes[i];
						if (ci.opcode == OpCodes.Ldstr && (string)ci.operand == "use_tool") {
							codes.InsertRange(i + 2, new InsnList { new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ExosuitClawArm", "OnPickup", true, new Type[0]) });
						}
					}
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(DevConsole))]
		[HarmonyPatch("Submit")]
		public static class OnCommandUse {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onCommandUse", false, new Type[] { typeof(DevConsole), typeof(string) }));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(GrowingPlant))]
		[HarmonyPatch("Update")]
		public static class TickGrowingPlant {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getMethodCallByName(codes, 0, 0, "GrowingPlant", "GetProgress");
					InsnList inject = new InsnList{
					new CodeInstruction(OpCodes.Ldarg_0),
					InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getGrowingPlantProgressInTick", false, new Type[]{typeof(float), typeof(GrowingPlant)})
				};
					codes.InsertRange(idx + 1, inject);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(CuteFishHandTarget))]
		[HarmonyPatch("PrepareCinematicMode")]
		public static class CuddlefishPlayHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onCuddlefishPlayed", false, typeof(CuteFishHandTarget), typeof(Player), typeof(CuteFishHandTarget.CuteFishCinematic)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Rocket))]
		[HarmonyPatch("AdvanceRocketStage")]
		public static class RocketStageCompletionHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onRocketStageCompleted", false, typeof(Rocket)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(CrashedShipExploder))]
		[HarmonyPatch("Start")]
		public static class AuroraSpawnHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onAuroraSpawn", false, typeof(CrashedShipExploder)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(CraftData))]
		[HarmonyPatch("GetCraftTime")]
		public static class CraftingSpeed {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "getFabricatorTime", false, typeof(TechType), typeof(float).MakeByRefType());
					codes.add(OpCodes.Ret);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		*/
		[HarmonyPatch(typeof(Crafter))]
		[HarmonyPatch("Craft")]
		public static class FabSpeedHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "CrafterLogic", "Craft", true, new Type[]{typeof(TechType), typeof(float)});
					codes.InsertRange(idx, new InsnList{
					new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getCrafterTime", false, typeof(float), typeof(Crafter), typeof(TechType))
				});
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(EscapePodCinematicControl))]
		[HarmonyPatch("OnIntroStart")]
		public static class IntroStartHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onAuroraSpawn", false, typeof(CrashedShipExploder)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}*/

		[HarmonyPatch(typeof(Targeting))]
		[HarmonyPatch("Skip")]
		public static class ControllableTargetingBypass {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectHook);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectHook(InsnList codes, int i) {
				codes.Insert(i, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "checkTargetingSkip", false, typeof(bool), typeof(Transform)));
				codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(Constructable))]
		[HarmonyPatch("Deconstruct")]
		public static class DeconstructionRefundHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = codes.getInstruction(0, 0, OpCodes.Stloc_3);
					codes.InsertRange(idx, new List<CodeInstruction> { new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onRefundConstructableIngredient", false, typeof(Pickupable), typeof(Constructable)) });
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		static class PatchLib {

			internal static void addEquipmentAllowedHook(InsnList codes, params CodeInstruction[] getItem) {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Equipment", "IsCompatible", false, new Type[]{typeof(EquipmentType), typeof(EquipmentType)});
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "isEquipmentApplicable", false, typeof(EquipmentType), typeof(EquipmentType), typeof(Equipment), typeof(Pickupable));
				codes.InsertRange(idx, getItem);
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}

			internal static void patchPropulsability(InsnList codes, int idx, bool mass, CodeInstruction go = null) {
				InsnList add = new InsnList {
					go == null ? new CodeInstruction(OpCodes.Ldarg_1) : go
				};
				add.add(OpCodes.Ldarg_0);
				add.add(mass ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
				add.invoke("ReikaKalseki.DIAlterra.DIHooks", "getMaxPropulsible", false, typeof(float), typeof(GameObject), typeof(MonoBehaviour), typeof(bool));
				codes.InsertRange(idx + 1, add);
			}

			internal static void patchVisualItemSize(InsnList codes, bool useSelfContainer = false) {
				patchVisualItemSize(codes, useSelfContainer, true, useSelfContainer ? new Type[] { typeof(TechType), typeof(InventoryItem), typeof(IItemsContainer) } : new Type[] { typeof(TechType), typeof(InventoryItem) });
			}

			internal static void patchVisualItemSize(InsnList codes, bool ldSelf = false, bool ldArg1 = true, params Type[] args) {
				for (int i = codes.Count - 1; i >= 0; i--) {
					if (codes[i].opcode == OpCodes.Call) {
						MethodInfo m = (MethodInfo)codes[i].operand;
						if (m != null && m.DeclaringType.Name == "CraftData" && m.Name == "GetItemSize") {
							MethodInfo call = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "getItemDisplaySize", false, args);
							codes[i].operand = call;
							if (ldSelf)
								codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
							if (ldArg1)
								codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
						}
					}
				}
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				/*
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "CraftData", "GetItemSize", false, new Type[]{typeof(TechType)});
				codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "getItemDisplaySize", false, typeof(TechType), typeof(InventoryItem));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));*/
			}

			internal static void redirectPowerHook(InsnList codes) {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "PowerSystem", "AddEnergy", false, new Type[]{typeof(IPowerInterface), typeof(float), typeof(float).MakeByRefType()});
				codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "addPowerToSeabaseDelegate", false, typeof(IPowerInterface), typeof(float), typeof(float).MakeByRefType(), typeof(MonoBehaviour));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}

			internal static void injectEMPHook(InsnList codes, int idx) {
				CodeInstruction arg = codes[idx-3]; //-1 is getfield time, -2 is loadarg0 to get that field
				idx -= 4;
				codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onEMPHit", false, typeof(EMPBlast), typeof(MonoBehaviour)));
				codes.Insert(idx + 1, new CodeInstruction(arg.opcode, arg.operand));
				codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldarg_0));
			}

			internal static void injectTickHook(InsnList codes, string name, Type arg) {
				codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", name, false, arg));
			}

		}
	}
}
