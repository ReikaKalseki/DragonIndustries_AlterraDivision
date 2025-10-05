using System;
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using HarmonyLib;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public static class InstructionHandlers {

		public static bool dumpIL = false;

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
					return (sbyte)ci.operand;
				case "ldc.i4":
					return (int)ci.operand;
				case "ldc.i8":
					return (long)ci.operand;
				default:
					return Int64.MaxValue;
			}
		}

		public static void nullInstructions(InsnList li, int begin, int end) {
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

		public static CodeInstruction createConstructorCall(string owner, params Type[] args) {
			return new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(AccessTools.TypeByName(owner), args));
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
				throw new Exception("Could not find a type matching name '" + owner + "'!");
			}
			MethodInfo ret;
			try {
				ret = AccessTools.Method(container, name, args);
			}
			catch (Exception e) {
				throw new Exception("[Harmony " + typeof(AccessTools).AssemblyQualifiedName + "] Failed to perform search for " + owner + "::" + name, e);
			}
			//ret.IsStatic = !instance;
			if (ret == null) {
				string info = "Harmony version:"+typeof(AccessTools).fullClearName()+"\n";
				info += "Methods:\n" + container.listMethods() + "\nComparison:\n";
				foreach (MethodInfo mi in container.getMethods()) {
					if (mi.Name == name) {
						info += "Name Match: " + mi.Name;
						ParameterInfo[] pp = mi.GetParameters();

						if (pp.Length == args.Length)
							info += "; Arg Len Match: " + pp.Length;
						else
							info += "; Arg Len mismatch: " + args.Length + " vs " + pp.Length;

						if (args.SequenceEqual(pp.Select(p => p.ParameterType))) {
							info += "; Arg Match: " + string.Join(", ", pp.Select(clearString).ToArray());
						}
						else {
							info += "; Arg mismatch:\n";
							for (int i = 0; i < pp.Length; i++) {
								Type pt = pp[i].ParameterType;
								info += i + ": " + args[i].fullClearName() + " vs " + pt.fullClearName() + " (" + (args[i] == pt) + ")\n";
							}
						}

						info += "\n";
					}
				}
				throw new Exception("Could not find a method named '" + name + "' with args " + args.toDebugString() + " in type '" + owner + "'!\n" + info);
			}
			return ret;
		}

		public static FieldInfo convertFieldOperand(string owner, string name) {
			Type container = AccessTools.TypeByName(owner);
			FieldInfo ret = AccessTools.Field(container, name);
			return ret == null ? throw new Exception("Could not find a method named '" + name + "' in type '" + owner + "'!") : ret;
		}

		public static int getInstruction(this InsnList li, int start, int index, OpCode opcode, params object[] args) {
			int count = 0;
			if (index < 0) {
				index = (-index) - 1;
				for (int i = li.Count - 1; i >= 0; i--) {
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
			throw new Exception("Instruction not found: " + opcode + " #" + string.Join(",", args) + "\nInstruction list:\n" + li.clearString());
		}

		public static int getMethodCallByName(this InsnList li, int start, int index, string owner, string name) {
			int count = 0;
			if (index < 0) {
				index = (-index) - 1;
				for (int i = li.Count - 1; i >= 0; i--) {
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
			throw new Exception("Method call not found: " + owner + "::" + name + "\nInstruction list:\n" + li.clearString());
		}

		public static int getFirstOpcode(InsnList li, int after, OpCode opcode) {
			for (int i = after; i < li.Count; i++) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					return i;
				}
			}
			throw new Exception("Instruction not found: " + opcode + "\nInstruction list:\n" + li.clearString());
		}

		public static int getLastOpcodeBefore(this InsnList li, int before, OpCode opcode) {
			if (before > li.Count)
				before = li.Count;
			for (int i = before - 1; i >= 0; i--) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					return i;
				}
			}
			throw new Exception("Instruction not found: " + opcode + "\nInstruction list:\n" + li.clearString());
		}

		public static int getLastInstructionBefore(this InsnList li, int before, OpCode opcode, params object[] args) {
			for (int i = before - 1; i >= 0; i--) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					if (match(insn, args)) {
						return i;
					}
				}
			}
			throw new Exception("Instruction not found: " + opcode + " #" + string.Join(",", args) + "\nInstruction list:\n" + li.clearString());
		}

		public static bool match(CodeInstruction a, CodeInstruction b) {
			return a.opcode == b.opcode && matchOperands(a.operand, b.operand);
		}

		public static bool matchOperands(object o1, object o2) {
			return o1 == o2 || (o1 != null && o2 != null && (o1 is LocalBuilder && o2 is LocalBuilder ? ((LocalBuilder)o1).LocalIndex == ((LocalBuilder)o2).LocalIndex : o1.Equals(o2)));
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

		public static string clearString(this CodeInstruction ci) {
			return ci.opcode.Name + " " + toOperandString(ci.opcode, ci.operand);
		}

		private static string toOperandString(OpCode code, object operand) {
			if (operand is MethodInfo) {
				MethodInfo m = (MethodInfo)operand;
				return m.DeclaringType + "." + m.Name + " (" + string.Join(", ", m.GetParameters().Select<ParameterInfo, string>(p => p.ParameterType.Name + " " + p.Name)) + ") [static=" + m.IsStatic + "]";
			}
			if (operand is FieldInfo) {
				FieldInfo m = (FieldInfo)operand;
				return m.DeclaringType + "." + m.Name + " [static=" + m.IsStatic + "]";
			}
			return operand is LocalBuilder
				? "localvar " + ((LocalBuilder)operand).LocalIndex
				: code == OpCodes.Ldarg_S || code == OpCodes.Ldarg
				? "arg " + operand
				: operand is Type ? "type " + ((Type)operand).Name : operand != null ? operand + " [" + operand.GetType() + "]" : "<null>";/*
			if (code == OpCodes.Ldloc_S || code == OpCodes.Stloc_S) {
				return "localvar "+((LocalBuilder)operand).LocalIndex;
			}*/
		}

		public static Type getTypeBySimpleName(string name) {
			if (string.IsNullOrEmpty(name))
				throw new Exception("You cannot get a type of no name!");
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
				string an = a.GetName().Name;
				if (an.StartsWith("0Harmony") && an != "0Harmony") //skip all other harmony versions
					continue;
				Type tt = a.GetType(name);
				if (tt != null)
					return tt;
			}
			return null;
		}

		public static MethodInfo getAnyMethod(this Type t, string name) { //do not remove, useful for debug!
			try {
				return string.IsNullOrEmpty(name) ? throw new Exception("You cannot get a method of no name!") : t.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			}
			catch (Exception ex) {
				throw new Exception("Failed to find '" + t.Name + "::" + name + "'", ex);
			}
		}

		public static HarmonyMethod clear() {
			Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>> dele = codes => new InsnList{new CodeInstruction(OpCodes.Ret)};
			return new HarmonyMethod(dele.Method);
		}

		public static void runPatchesIn(Harmony h, Type parent) {
			FileLog.logPath = Path.Combine(Path.GetDirectoryName(parent.Assembly.Location), "harmony-log.txt");
			string ilDumpFolder = getILDumpFolder();
			string msg = "Running harmony patches in " + parent.Assembly.GetName().Name + "::" + parent.Name;
			SNUtil.log(msg);
			FileLog.Log(msg);
			foreach (Type t in parent.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
				FileLog.Log("Running harmony patches in " + t.Name);
				h.PatchAll(t);
			}
		}

		public static void patchMethod(Harmony h, Type methodHolder, string name, Type patchHolder, string patchName) {
			FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Assembly.Location), "harmony-log.txt");
			FileLog.Log("Running harmony patch in " + patchHolder.FullName + "::" + patchName + " on " + methodHolder.FullName + "::" + name);
			MethodInfo m = methodHolder.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			if (m == null)
				throw new Exception("Method " + name + " not found in " + methodHolder.AssemblyQualifiedName);
			patchMethod(h, m, new HarmonyMethod(AccessTools.Method(patchHolder, patchName, new Type[] { typeof(IEnumerable<CodeInstruction>) })));
		}

		public static void patchMethod(HarmonySystem h, Type methodHolder, string name, Action<InsnList> patch) {
			patchMethod(h.harmonyInstance, methodHolder, name, h.owner, patch);
		}

		public static void patchMethod(Harmony h, Type methodHolder, string name, Assembly patchHolder, Action<InsnList> patch) {
			FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Location), "harmony-log.txt");
			FileLog.Log("Running harmony patch from " + patchHolder.GetName().Name + " on " + methodHolder.FullName + "::" + name);
			MethodInfo m = methodHolder.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			if (m == null)
				throw new Exception("Method " + name + " not found in " + methodHolder.FullName);
			currentPatch = patch;
			patchMethod(h, m, new HarmonyMethod(AccessTools.Method(MethodBase.GetCurrentMethod().DeclaringType, "patchHook", new Type[] { typeof(IEnumerable<CodeInstruction>) })));
			currentPatch = null;
		}

		public static void patchMethod(Harmony h, MethodInfo m, Assembly patchHolder, Action<InsnList> patch) {
			FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Location), "harmony-log.txt");
			FileLog.Log("Running harmony patch from " + patchHolder.GetName().Name + " on " + m.DeclaringType.FullName + "::" + m.Name);
			currentPatch = patch;
			patchMethod(h, m, new HarmonyMethod(AccessTools.Method(MethodBase.GetCurrentMethod().DeclaringType, "patchHook", new Type[] { typeof(IEnumerable<CodeInstruction>) })));
			currentPatch = null;
		}

		private static Action<InsnList> currentPatch;
		private static IEnumerable<CodeInstruction> patchHook(IEnumerable<CodeInstruction> instructions) {
			InsnList codes = new InsnList(instructions);
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

		public static string getILDumpFolder() {
			return Path.Combine(Path.GetDirectoryName(SNUtil.diDLL.Location), "original-il");
		}

		public static void dumpMethodIL(IEnumerable<CodeInstruction> li, string id) {
			string file = Path.Combine(getILDumpFolder(), id+".txt");
			Directory.CreateDirectory(Path.GetDirectoryName(file));
			File.WriteAllText(file, li.ToList().clearString());
		}

		public static IEnumerable<MethodInfo> getMethods(this Type t) {
			return t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		}

		public static string listMethods(this Type t) {
			return string.Join("\n", t.getMethods().Select(clearString).ToArray());
		}

		public static string clearString(this MethodInfo m) {
			return m.Name + "(" + string.Join(", ", m.GetParameters().Select(clearString).ToArray()) + ") -> " + m.ReturnType.FullName;
		}

		public static string clearString(this ParameterInfo m) {
			return m.ParameterType.FullName + " " + m.Name;
		}

		public static string fullClearName(this Type t) {
			return t == null ? "NULL" : t.FullName + " in " + t.Assembly.GetName().Name + " @ " + t.Assembly.Location;
		}

		public static void logPatchStart(MethodBase patch, IEnumerable<CodeInstruction> orig) {
			if (dumpIL) {
				Attribute[] attrs = System.Attribute.GetCustomAttributes(patch.DeclaringType);
				try {
					Type t = null;
					string name = null;
					foreach (Attribute a in attrs) {
						if (a is HarmonyPatch ha) {
							if (ha.info.declaringType != null)
								t = ha.info.declaringType;
							if (ha.info.methodName != null)
								name = ha.info.methodName;
						}
					}
					if (t == null)
						throw new Exception("No target type");
					if (string.IsNullOrEmpty(name))
						throw new Exception("No target name");
					string id = t.Name+"--"+name;
					dumpMethodIL(orig, id);
				}
				catch (Exception e) {
					SNUtil.log("Threw exception dumping patch '" + patch.DeclaringType.Name + "' [" + attrs.toDebugString("\n") + "] IL: " + e);
				}
			}
			FileLog.Log("Starting patch " + patch.DeclaringType);
		}

		public static void logCompletedPatch(MethodBase patch, IEnumerable<CodeInstruction> orig) {/*
			if (dumpIL) {
				Attribute[] attrs = System.Attribute.GetCustomAttributes(patch.DeclaringType);
				try {
					Type t = null;
					string name = null;
					foreach (Attribute a in attrs) {
						if (a is HarmonyPatch ha) {
							if (ha.info.declaringType != null)
								t = ha.info.declaringType;
							if (ha.info.methodName != null)
								name = ha.info.methodName;
						}
					}
					if (t == null)
						throw new Exception("No target type");
					if (string.IsNullOrEmpty(name))
						throw new Exception("No target name");
					string id = t.Name+"--"+name;
					dumpMethodIL(orig, id);
				}
				catch (Exception e) {
					SNUtil.log("Threw exception dumping patch '"+patch.DeclaringType.Name+"' ["+ attrs.toDebugString("\n") + "] IL: "+e);
				}
			}*/
			FileLog.Log("Done patch " + patch.DeclaringType);
		}

		public static void logErroredPatch(MethodBase patch) {
			FileLog.Log("Caught exception when running patch " + patch.DeclaringType + "!");
		}

		public static string clearString(this List<CodeInstruction> li) {
			return "\n" + String.Join("\n", li.Select(p => p.clearString()).ToArray());
		}

		public static string clearString(this List<CodeInstruction> li, int idx) {
			return idx < 0 || idx >= li.Count ? "ERROR: OOB " + idx + "/" + li.Count : "#" + Convert.ToString(idx, 16) + " = " + li[idx].clearString();
		}
	}

	public class InsnList : List<CodeInstruction> {

		public InsnList() : base() {

		}

		public InsnList(IEnumerable<CodeInstruction> li) : base(li) {

		}

		public InsnList add(OpCode opcode, object operand = null) {
			Add(new CodeInstruction(opcode, operand));
			return this;
		}

		public InsnList invoke(string owner, string name, bool instance, params Type[] args) {
			Add(InstructionHandlers.createMethodCall(owner, name, instance, args));
			return this;
		}

		public InsnList field(OpCode opcode, string owner, string name) {
			return add(opcode, InstructionHandlers.convertFieldOperand(owner, name));
		}

		public InsnList field(string owner, string name, bool inst, bool put) {
			OpCode code = OpCodes.Ldc_I4;
			if (inst)
				code = put ? OpCodes.Stfld : OpCodes.Ldfld;
			else
				code = put ? OpCodes.Stsfld : OpCodes.Ldsfld;
			return add(code, InstructionHandlers.convertFieldOperand(owner, name));
		}

		public InsnList ldc(string val) {
			return add(OpCodes.Ldstr, val);
		}

		public InsnList ldc(int val) {
			OpCode code = OpCodes.Ldc_I4;
			switch (val) {
				case -1:
					code = OpCodes.Ldc_I4_M1;
					break;
				case 0:
					code = OpCodes.Ldc_I4_0;
					break;
				case 1:
					code = OpCodes.Ldc_I4_1;
					break;
				case 2:
					code = OpCodes.Ldc_I4_2;
					break;
				case 3:
					code = OpCodes.Ldc_I4_3;
					break;
				case 4:
					code = OpCodes.Ldc_I4_4;
					break;
				case 5:
					code = OpCodes.Ldc_I4_5;
					break;
				case 6:
					code = OpCodes.Ldc_I4_6;
					break;
				case 7:
					code = OpCodes.Ldc_I4_7;
					break;
				case 8:
					code = OpCodes.Ldc_I4_8;
					break;
				default:
					break;
			}
			if (code == OpCodes.Ldc_I4)
				return add(code, val);
			else
				return add(code);
		}

		public InsnList ldc(long val) {
			return add(OpCodes.Ldc_I8, val);
		}

		public InsnList ldc(float val) {
			return add(OpCodes.Ldc_R4, val);
		}

		public InsnList ldc(double val) {
			return add(OpCodes.Ldc_R8, val);
		}

		public int patchEveryReturnPre(params CodeInstruction[] insert) {
			return patchEveryReturnPre(insert.ToList());
		}

		public int patchEveryReturnPre(IEnumerable<CodeInstruction> insert) {
			int times = patchEveryReturnPre((li, idx) => li.InsertRange(idx, insert));
			//FileLog.Log("Injected "+times+" times, codes are now: " + InstructionHandlers.toString(codes));
			return times;
		}

		public int patchEveryReturnPre(Action<InsnList, int> injectHook) {
			int times = 0;
			for (int i = Count - 1; i >= 0; i--) {
				if (this[i].opcode == OpCodes.Ret) {
					//FileLog.Log("Injected @ "+i+", codes are now: "+InstructionHandlers.toString(codes));
					injectHook(this, i);
					times++;
				}
			}
			return times;
		}

		public InsnList patchInitialHook(params CodeInstruction[] insert) {
			InsnList li = new InsnList();
			foreach (CodeInstruction c in insert) {
				li.Add(c);
			}
			return patchInitialHook(li);
		}

		public InsnList patchInitialHook(InsnList insert) {
			for (int i = insert.Count - 1; i >= 0; i--) {
				Insert(0, insert[i]);
			}
			return this;
		}

		public InsnList extract(int from, int to) {
			InsnList li = new InsnList();
			for (int i = from; i <= to; i++) {
				li.Add(this[i]);
			}
			RemoveRange(from, to - from + 1);
			return li;
		}

		public void replaceConstantWithMethodCall(int val, InsnList put) {
			replaceConstantWithMethodCall(val, c => c.opcode == OpCodes.Ldc_I4 && c.LoadsConstant(Convert.ToInt32(val)), put);
		}

		public void replaceConstantWithMethodCall(long val, InsnList put) {
			replaceConstantWithMethodCall(val, c => c.opcode == OpCodes.Ldc_I8 && c.LoadsConstant(Convert.ToInt64(val)), put);
		}

		public void replaceConstantWithMethodCall(float val, InsnList put) {
			replaceConstantWithMethodCall(val, c => c.opcode == OpCodes.Ldc_R4 && c.LoadsConstant(Convert.ToSingle(val)), put);
		}

		public void replaceConstantWithMethodCall(double val, InsnList put) {
			replaceConstantWithMethodCall(val, c => c.opcode == OpCodes.Ldc_R8 && c.LoadsConstant(Convert.ToDouble(val)), put);
		}

		private void replaceConstantWithMethodCall(double val, Func<CodeInstruction, bool> f, InsnList put) {
			for (int i = Count - 1; i >= 0; i--) {
				CodeInstruction c = this[i];
				if (f(c)) {
					RemoveAt(i);
					InsertRange(i, put);
				}
			}
		}

	}
}
