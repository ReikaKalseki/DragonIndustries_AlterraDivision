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
	public static class InstructionHandlers
	{		
		public static long getIntFromOpcode(CodeInstruction ci) {
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
		
		public static void nullInstructions(List<CodeInstruction> li, int begin, int end) {
			for (int i = begin; i <= end; i++) {
				CodeInstruction insn = li[i];
				insn.opcode = OpCodes.Nop;
				insn.operand = null;
			}
		}
		
		public static CodeInstruction createMethodCall(string owner, string name, bool instance, params string[] args) {
			return new CodeInstruction(OpCodes.Call, convertMethodOperand(owner, name, instance, args));
		}
		
		public static CodeInstruction createMethodCall(string owner, string name, bool instance, params Type[] args) {
			return new CodeInstruction(OpCodes.Call, convertMethodOperand(owner, name, instance, args));
		}
		
		public static MethodInfo convertMethodOperand(string owner, string name, bool instance, params string[] args) {
			Type[] types = new Type[args.Length];
			for (int i = 0; i < args.Length; i++) {
				types[i] = AccessTools.TypeByName(args[i]);
			}
			return convertMethodOperand(owner, name, instance, types);
		}
		
		public static MethodInfo convertMethodOperand(string owner, string name, bool instance, params Type[] args) {
			Type container = AccessTools.TypeByName(owner);
			if (container == null) {
				throw new Exception("Could not find a type matching name '"+owner+"'!");
			}
			MethodInfo ret = AccessTools.Method(container, name, args);
			//ret.IsStatic = !instance;
			if (ret == null) {
				throw new Exception("Could not find a method named '"+name+"' with args "+args.toDebugString()+" in type '"+owner+"'!");
			}
			return ret;
		}
		
		public static FieldInfo convertFieldOperand(string owner, string name) {
			Type container = AccessTools.TypeByName(owner);
			FieldInfo ret = AccessTools.Field(container, name);
			if (ret == null) {
				throw new Exception("Could not find a method named '"+name+"' in type '"+owner+"'!");
			}
			return ret;
		}
		
		public static int getInstruction(List<CodeInstruction> li, int start, int index, OpCode opcode, params object[] args) {
			int count = 0;
			if (index < 0) {
				index = (-index)-1;
				for (int i = li.Count-1; i >= 0; i--) {
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
			}
			else {
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
			}
			throw new Exception("Instruction not found: "+opcode+" #"+string.Join(",", args)+"\nInstruction list:\n"+toString(li));
		}
		
		public static int getMethodCallByName(List<CodeInstruction> li, int start, int index, string owner, string name) {
			int count = 0;
			if (index < 0) {
				index = (-index)-1;
				for (int i = li.Count-1; i >= 0; i--) {
					CodeInstruction insn = li[i];
					if (isMethodCall(insn, owner, name)) {
						if (count == index)
							return i;
						else
							count++;
					}
				}
			}
			else {
				for (int i = start; i < li.Count; i++) {
					CodeInstruction insn = li[i];
					if (isMethodCall(insn, owner, name)) {
						if (count == index)
							return i;
						else
							count++;
					}
				}
			}
			throw new Exception("Method call not found: "+owner+"::"+name+"\nInstruction list:\n"+toString(li));
		}
		
		public static int getFirstOpcode(List<CodeInstruction> li, int after, OpCode opcode) {
			for (int i = after; i < li.Count; i++) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					return i;
				}
			}
			throw new Exception("Instruction not found: "+opcode+"\nInstruction list:\n"+toString(li));
		}
		
		public static int getLastOpcodeBefore(List<CodeInstruction> li, int before, OpCode opcode) {
			if (before > li.Count)
				before = li.Count;
			for (int i = before-1; i >= 0; i--) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					return i;
				}
			}
			throw new Exception("Instruction not found: "+opcode+"\nInstruction list:\n"+toString(li));
		}
		
		public static int getLastInstructionBefore(List<CodeInstruction> li, int before, OpCode opcode, params object[] args) {
			for (int i = before-1; i >= 0; i--) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					if (match(insn, args)) {
						return i;
					}
				}
			}
			throw new Exception("Instruction not found: "+opcode+" #"+string.Join(",", args)+"\nInstruction list:\n"+toString(li));
		}
		
		public static bool match(CodeInstruction a, CodeInstruction b) {
			return a.opcode == b.opcode && matchOperands(a.operand, b.operand);
		}
		
		public static bool matchOperands(object o1, object o2) {
			if (o1 == o2)
				return true;
			if (o1 == null || o2 == null)
				return false;
			if (o1 is LocalBuilder && o2 is LocalBuilder) {
				return ((LocalBuilder)o1).LocalIndex == ((LocalBuilder)o2).LocalIndex;
			}
			return o1.Equals(o2);
		}
		
		public static bool isMethodCall(CodeInstruction insn, string owner, string name) {
			if (insn.opcode != OpCodes.Call && insn.opcode != OpCodes.Callvirt && insn.opcode != OpCodes.Calli)
				return false;
			MethodInfo mi = (MethodInfo)insn.operand;
			//FileLog.Log("Comparing "+mi.Name+" in "+mi.DeclaringType.FullName+" to "+owner+" & "+name);
			return mi.Name == name && mi.DeclaringType.FullName == owner;
		}
		
		public static bool match(CodeInstruction insn, params object[] args) {
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
			}
			else if (insn.opcode == OpCodes.Ldc_I4) { //ldc
				return insn.LoadsConstant(Convert.ToInt32(args[0]));
			}
			else if (insn.opcode == OpCodes.Ldc_R4) { //ldc
				return insn.LoadsConstant(Convert.ToSingle(args[0]));
			}
			else if (insn.opcode == OpCodes.Ldc_I8) { //ldc
				return insn.LoadsConstant(Convert.ToInt64(args[0]));
			}
			else if (insn.opcode == OpCodes.Ldc_R8) { //ldc
				return insn.LoadsConstant(Convert.ToDouble(args[0]));
			}
			else if (insn.opcode == OpCodes.Ldloc_S || insn.opcode == OpCodes.Stloc_S) { //LocalBuilder contains a pos and type
				LocalBuilder loc = (LocalBuilder)insn.operand;
				return args[0] is int && loc.LocalIndex == (int)args[0]/* && loc.LocalType == args[1]*/;
			}
			else if (insn.opcode == OpCodes.Ldstr) { //string var
				return (string)insn.operand == (string)args[0];
			}
			return true;
		}
		
		public static string toString(List<CodeInstruction> li) {
			return "\n"+String.Join("\n", li.Select(p=>toString(p)).ToArray());
		}
		
		public static string toString(List<CodeInstruction> li, int idx) {
			return idx < 0 || idx >= li.Count ? "ERROR: OOB "+idx+"/"+li.Count : "#"+Convert.ToString(idx, 16)+" = "+toString(li[idx]);
		}
		
		public static string toString(CodeInstruction ci) {
			return ci.opcode.Name+" "+toOperandString(ci.opcode, ci.operand);
		}
			
		private static string toOperandString(OpCode code, object operand) {
			if (operand is MethodInfo) {
				MethodInfo m = (MethodInfo)operand;
				return m.DeclaringType+"."+m.Name+" ("+string.Join(", ", m.GetParameters().Select<ParameterInfo, string>(p => p.ParameterType.Name+" "+p.Name))+") [static="+m.IsStatic+"]";
			}
			if (operand is FieldInfo) {
				FieldInfo m = (FieldInfo)operand;
				return m.DeclaringType+"."+m.Name+" [static="+m.IsStatic+"]";
			}
			if (/*code == OpCodes.Ldloc_S*/ operand is LocalBuilder) {
				return "localvar "+((LocalBuilder)operand).LocalIndex;
			}/*
			if (code == OpCodes.Ldloc_S || code == OpCodes.Stloc_S) {
				return "localvar "+((LocalBuilder)operand).LocalIndex;
			}*/
			if (code == OpCodes.Ldarg_S || code == OpCodes.Ldarg) {
				return "arg "+operand;
			}
			if (/*code == OpCodes.Isinst || code == OpCodes.Newobj*/operand is Type) {
				return "type "+((Type)operand).Name;
			}
			return operand != null ? operand+" ["+operand.GetType()+"]" : "<null>";
		}
		
	    public static Type getTypeBySimpleName(string name) {
	        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
	            Type tt = assembly.GetType(name);
	            if (tt != null)
	                return tt;
	        }	
	        return null;
	    }
		
		public static void patchEveryReturnPre(List<CodeInstruction> codes, params CodeInstruction[] insert) {
			patchEveryReturnPre(codes, insert.ToList<CodeInstruction>());
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
		
		public static List<CodeInstruction> extract(List<CodeInstruction> codes, int from, int to) {
			List<CodeInstruction> li = new List<CodeInstruction>();
			for (int i = from; i <= to; i++) {
				li.Add(codes[i]);
			}
			codes.RemoveRange(from, to-from+1);
			return li;
		}
		
		public static void replaceConstantWithMethodCall(List<CodeInstruction> codes, int val, List<CodeInstruction> put) {
			replaceConstantWithMethodCall(codes, val, c => c.opcode == OpCodes.Ldc_I4 && c.LoadsConstant(Convert.ToInt32(val)), put);
		}
		
		public static void replaceConstantWithMethodCall(List<CodeInstruction> codes, long val, List<CodeInstruction> put) {
			replaceConstantWithMethodCall(codes, val, c => c.opcode == OpCodes.Ldc_I8 && c.LoadsConstant(Convert.ToInt64(val)), put);
		}
		
		public static void replaceConstantWithMethodCall(List<CodeInstruction> codes, float val, List<CodeInstruction> put) {
			replaceConstantWithMethodCall(codes, val, c => c.opcode == OpCodes.Ldc_R4 && c.LoadsConstant(Convert.ToSingle(val)), put);
		}
		
		public static void replaceConstantWithMethodCall(List<CodeInstruction> codes, double val, List<CodeInstruction> put) {
			replaceConstantWithMethodCall(codes, val, c => c.opcode == OpCodes.Ldc_R8 && c.LoadsConstant(Convert.ToDouble(val)), put);
		}
	
		private static void replaceConstantWithMethodCall(List<CodeInstruction> codes, double val, Func<CodeInstruction, bool> f, List<CodeInstruction> put) {
			for (int i = codes.Count-1; i >= 0; i--) {
				CodeInstruction c = codes[i];
				if (f(c)) {
					codes.RemoveAt(i);
					codes.InsertRange(i, put);
				}
			}
		}
		
		public static HarmonyMethod clear() {
        	Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>> dele = codes => new List<CodeInstruction>{new CodeInstruction(OpCodes.Ret)};
        	return new HarmonyMethod(dele.Method);
		}
		
		public static void runPatchesIn(Harmony h, Type parent) {
       		FileLog.logPath = Path.Combine(Path.GetDirectoryName(parent.Assembly.Location), "harmony-log.txt");
			SNUtil.log("Running harmony patches in "+parent.Assembly+"::"+parent.Name);
			FileLog.Log("Running harmony patches in "+parent.Assembly+"::"+parent.Name);
			foreach (Type t in parent.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
				FileLog.Log("Running harmony patches in "+t.Name);
				h.PatchAll(t);
			}
		}
		
		public static void patchMethod(Harmony h, Type methodHolder, string name, Type patchHolder, string patchName) {
       		FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Assembly.Location), "harmony-log.txt");
			FileLog.Log("Running harmony patch in "+patchHolder.AssemblyQualifiedName+"::"+patchName+" on "+methodHolder.AssemblyQualifiedName+"::"+name);
			MethodInfo m = methodHolder.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			if (m == null)
				throw new Exception("Method "+name+" not found in "+methodHolder.AssemblyQualifiedName);
			patchMethod(h, m, new HarmonyMethod(AccessTools.Method(patchHolder, patchName, new Type[]{typeof(IEnumerable<CodeInstruction>)})));
		}
		
		public static void patchMethod(Harmony h, Type methodHolder, string name, Assembly patchHolder, Action<List<CodeInstruction>> patch) {
       		FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Location), "harmony-log.txt");
			FileLog.Log("Running harmony patch from "+patchHolder.FullName+" on "+methodHolder.AssemblyQualifiedName+"::"+name);
			MethodInfo m = methodHolder.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			if (m == null)
				throw new Exception("Method "+name+" not found in "+methodHolder.AssemblyQualifiedName);
			currentPatch = patch;
			patchMethod(h, m, new HarmonyMethod(AccessTools.Method(MethodBase.GetCurrentMethod().DeclaringType, "patchHook", new Type[]{typeof(IEnumerable<CodeInstruction>)})));
			currentPatch = null;
		}
		
		public static void patchMethod(Harmony h, MethodInfo m, Assembly patchHolder, Action<List<CodeInstruction>> patch) {
       		FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Location), "harmony-log.txt");
			FileLog.Log("Running harmony patch from "+patchHolder.FullName+" on "+m.DeclaringType.AssemblyQualifiedName+"::"+m.Name);
			currentPatch = patch;
			patchMethod(h, m, new HarmonyMethod(AccessTools.Method(MethodBase.GetCurrentMethod().DeclaringType, "patchHook", new Type[]{typeof(IEnumerable<CodeInstruction>)})));
			currentPatch = null;
		}
		
		private static Action<List<CodeInstruction>> currentPatch;
		private static IEnumerable<CodeInstruction> patchHook(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			currentPatch.Invoke(codes);
			//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			return codes.AsEnumerable();
		}
		
		private static void patchMethod(Harmony h, MethodInfo m, HarmonyMethod patch) {
			try {
				h.Patch(m, null, null, patch, null, null);
				FileLog.Log("Done patch");
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
		}
	}
}
