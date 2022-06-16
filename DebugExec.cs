using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{
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
				switch(opcode) {
					case "call": {
						MethodInfo call = AccessTools.Method(t, member);
						object ret = call.Invoke(inst, new object[0]);
						SBUtil.writeToChat("Invoking "+type+"."+member+" returned: "+toString(ret));
					}
					break;
					case "field": {
						FieldInfo field = AccessTools.Field(t, member);
						object ret = field.GetValue(inst);
						SBUtil.writeToChat("Field "+type+"."+member+" contains: "+toString(ret));
					}
					break;
				}
			}
			catch (Exception e) {
				SBUtil.writeToChat("Exec threw exception: "+e.ToString());
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
			//SBUtil.showPDANotification("I am pda text", "event:/player/story/Goal_BiomeSparseReef");
			SeaToSea.SeaToSeaMod.treaderSignal.build("32e48451-8e81-428e-9011-baca82e9cd32", new Vector3(-1239, -360, -1193));
			SeaToSea.SeaToSeaMod.treaderSignal.fireRadio();
		}
		
	}
}
