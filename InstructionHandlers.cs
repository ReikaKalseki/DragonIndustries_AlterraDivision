using System;
using System.IO;    //For data read/write methods
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.DIAlterra
{
	public class InstructionHandlers
	{		
		internal static long getIntFromOpcode(CodeInstruction ci) {
			switch (ci.opcode.Name) {
				case "ldc.i4.m1":
				return -1;
				case "ldc.i4.0":
				return 0;
				case "ldc.i4.1":
				return 1;
				case "ldc.i4.2":
				return 2;
				case "ldc.i4.3":
				return 3;
				case "ldc.i4.4":
				return 4;
				case "ldc.i4.5":
				return 5;
				case "ldc.i4.6":
				return 6;
				case "ldc.i4.7":
				return 7;
				case "ldc.i4.8":
				return 8;
				case "ldc.i4.s":
				return (int)((sbyte)ci.operand);
				case "ldc.i4":
				return (int)ci.operand;
				case "ldc.i8":
				return (long)ci.operand;
			default:
				return Int64.MaxValue;
			}
		}
		
		internal static void nullInstructions(List<CodeInstruction> li, int begin, int end) {
			for (int i = begin; i <= end; i++) {
				CodeInstruction insn = li[i];
				insn.opcode = OpCodes.Nop;
				insn.operand = null;
			}
		}
		
		internal static CodeInstruction createMethodCall(string owner, string name, bool instance, params string[] args) {
			return new CodeInstruction(OpCodes.Call, convertMethodOperand(owner, name, instance, args));
		}
		
		internal static CodeInstruction createMethodCall(string owner, string name, bool instance, params Type[] args) {
			return new CodeInstruction(OpCodes.Call, convertMethodOperand(owner, name, instance, args));
		}
		
		internal static MethodInfo convertMethodOperand(string owner, string name, bool instance, params string[] args) {
			Type[] types = new Type[args.Length];
			for (int i = 0; i < args.Length; i++) {
				types[i] = AccessTools.TypeByName(args[i]);
			}
			return convertMethodOperand(owner, name, instance, types);
		}
		
		internal static MethodInfo convertMethodOperand(string owner, string name, bool instance, params Type[] args) {
			Type container = AccessTools.TypeByName(owner);
			if (container == null) {
				throw new Exception("Could not find a type matching name '"+owner+"'!");
			}
			MethodInfo ret = AccessTools.Method(container, name, args);
			//ret.IsStatic = !instance;
			if (ret == null) {
				throw new Exception("Could not find a method named '"+name+"' with args "+string.Join(", ", (object[])args)+" in type '"+owner+"'!");
			}
			return ret;
		}
		
		internal static FieldInfo convertFieldOperand(string owner, string name) {
			Type container = AccessTools.TypeByName(owner);
			FieldInfo ret = AccessTools.Field(container, name);
			if (ret == null) {
				throw new Exception("Could not find a method named '"+name+"' in type '"+owner+"'!");
			}
			return ret;
		}
		
		internal static int getInstruction(List<CodeInstruction> li, int start, int index, OpCode opcode, params object[] args) {
			int count = 0;
			for (int i = start; i < li.Count; i++) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					if (match(insn, args)) {
						if (count == index)
							return i;
						else
							count++;
					}
				}
			}
			return -1;
		}
		
		internal static int getFirstOpcode(List<CodeInstruction> li, int after, OpCode opcode) {
			for (int i = after; i < li.Count; i++) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					return i;
				}
			}
			return -1;
		}
		
		internal static int getLastOpcodeBefore(List<CodeInstruction> li, int before, OpCode opcode) {
			if (before > li.Count)
				before = li.Count;
			for (int i = before-1; i >= 0; i--) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					return i;
				}
			}
			return -1;
		}
		
		internal static int getLastInstructionBefore(List<CodeInstruction> li, int before, OpCode opcode, params object[] args) {
			for (int i = before-1; i >= 0; i--) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					if (match(insn, args)) {
						return i;
					}
				}
			}
			return -1;
		}
		
		internal static bool match(CodeInstruction a, CodeInstruction b) {
			return a.opcode == b.opcode && matchOperands(a.operand, b.operand);
		}
		
		internal static bool matchOperands(object o1, object o2) {
			if (o1 == o2)
				return true;
			if (o1 == null || o2 == null)
				return false;
			if (o1 is LocalBuilder && o2 is LocalBuilder) {
				return ((LocalBuilder)o1).LocalIndex == ((LocalBuilder)o2).LocalIndex;
			}
			return o1.Equals(o2);
		}
		
		internal static bool match(CodeInstruction insn, params object[] args) {
			//FileLog.Log("Comparing "+insn.operand.GetType()+" "+insn.operand.ToString()+" against seek of "+String.Join(",", args.Select(p=>p.ToString()).ToArray()));
			if (insn.opcode == OpCodes.Call || insn.opcode == OpCodes.Callvirt) { //string class, string name, bool instance, Type[] args
				MethodInfo info = convertMethodOperand((string)args[0], (string)args[1], (bool)args[2], (Type[])args[3]);
				return ((MethodInfo)insn.operand) == info;
			}
			else if (insn.opcode == OpCodes.Isinst || insn.opcode == OpCodes.Newobj) { //string class
				return ((Type)insn.operand) == AccessTools.TypeByName((string)args[0]);
			}
			else if (insn.opcode == OpCodes.Ldfld || insn.opcode == OpCodes.Stfld || insn.opcode == OpCodes.Ldsfld || insn.opcode == OpCodes.Stsfld) { //string class, string name
				FieldInfo info = convertFieldOperand((string)args[0], (string)args[1]);
				return ((FieldInfo)insn.operand) == info;
			}
			else if (insn.opcode == OpCodes.Ldarg) { //int pos
				return insn.operand == args[0];
			}/*
			else if (insn.opcode == OpCodes.Ldc_I4 || insn.opcode == OpCodes.Ldc_R4 || insn.opcode == OpCodes.Ldc_I8 || insn.opcode == OpCodes.Ldc_R8) { //ldc
				return insn.operand == args[0];
			}*/
			else if (insn.opcode == OpCodes.Ldloc_S || insn.opcode == OpCodes.Stloc_S) { //LocalBuilder contains a pos and type
				LocalBuilder loc = (LocalBuilder)insn.operand;
				return args[0] is int && loc.LocalIndex == (int)args[0]/* && loc.LocalType == args[1]*/;
			}
			else if (insn.opcode == OpCodes.Ldstr) { //string var
				return (string)insn.operand == (string)args[0];
			}
			return true;
		}
		
		internal static string toString(List<CodeInstruction> li) {
			return "\n"+String.Join("\n", li.Select(p=>toString(p)).ToArray());
		}
		
		internal static string toString(List<CodeInstruction> li, int idx) {
			return idx < 0 || idx >= li.Count ? "ERROR: OOB "+idx+"/"+li.Count : "#"+Convert.ToString(idx, 16)+" = "+toString(li[idx]);
		}
		
		internal static string toString(CodeInstruction ci) {
			return ci.opcode.Name+" "+(ci.operand != null ? ci.operand+" ["+ci.operand.GetType()+"]" : "<null>");
		}
		
	    public static Type getTypeBySimpleName(string name) {
	        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
	            Type tt = assembly.GetType(name);
	            if (tt != null)
	                return tt;
	        }	
	        return null;
	    }
		
		public static void patchEveryReturnPre(List<CodeInstruction> codes, List<CodeInstruction> insert) {
			patchEveryReturnPre(codes, (li, idx) => li.InsertRange(idx, insert));
		}
		
		public static void patchEveryReturnPre(List<CodeInstruction> codes, Action<List<CodeInstruction>, int> injectHook) {
			for (int i = codes.Count-1; i >= 0; i--) {
				if (codes[i].opcode == OpCodes.Ret) {
					injectHook(codes, i);
				}
			}
		}
		
		public static void patchInitialHook(List<CodeInstruction> codes, params CodeInstruction[] insert) {
			List<CodeInstruction> li = new List<CodeInstruction>();
			foreach (CodeInstruction c in insert) {
				li.Add(c);
			}
			patchInitialHook(codes, li);
		}
		
		public static void patchInitialHook(List<CodeInstruction> codes, List<CodeInstruction> insert) {
			for (int i = insert.Count-1; i >= 0; i--) {
				codes.Insert(0, insert[i]);
			}
		}
	}
}
