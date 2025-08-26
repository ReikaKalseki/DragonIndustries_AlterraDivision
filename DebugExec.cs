using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using HarmonyLib;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {
	public static class DebugExec {

		public static void run(string opcode, string type, string member) {
			run(opcode, type, member, "main");
		}

		public static void run(string opcode, string type, string member, string instField) {
			try {
				Type t = InstructionHandlers.getTypeBySimpleName(type);
				object inst = null;
				FieldInfo main = t.GetField(instField);
				if (main != null && main.IsStatic && main.FieldType == t)
					inst = main.GetValue(null);
				switch (opcode) {
					case "call": {
						MethodInfo call = AccessTools.Method(t, member);
						object ret = call.Invoke(inst, new object[0]);
						SNUtil.writeToChat("Invoking " + type + "." + member + " returned: " + toString(ret));
					}
					break;
					case "field": {
						FieldInfo field = AccessTools.Field(t, member);
						object ret = field.GetValue(inst);
						SNUtil.writeToChat("Field " + type + "." + member + " contains: " + toString(ret));
					}
					break;
				}
			}
			catch (Exception e) {
				SNUtil.writeToChat("Exec threw exception: " + e.ToString());
			}
		}

		public static string toString(object o) { //TO DO NOT IMPLEMENTED
			if (o == null) {
				return "null";
			}
			else if (o.isDictionary()) {
				return o.ToString();//((IDictionary)o).toDebugString();
			}
			else if (o.isEnumerable()) {
				return o.ToString();//((IEnumerable)o).toDebugString();
			}
			else {
				return o.ToString();
			}
		}

		public static void tempCode() {
			//SNUtil.showPDANotification("I am pda text", "event:/player/story/Goal_BiomeSparseReef");

		}

	}
}
