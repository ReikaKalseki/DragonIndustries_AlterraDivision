using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.DIAlterra
{
	public abstract class WorldGenerator : ObjectTemplate {
		
		public static readonly string TAGNAME = "generator";
		
		public readonly Vector3 position;
		
		static WorldGenerator() {
			registerType(TAGNAME, e => {
				string typeName = e.getProperty("type");
				Vector3 pos = e.getVector("position").Value;
				Type tt = InstructionHandlers.getTypeBySimpleName(typeName);
				if (tt == null)
					throw new Exception("No class found for '"+typeName+"'!");
				WorldGenerator gen = (WorldGenerator)Activator.CreateInstance(tt, new object[]{pos});
				return gen;
			});
		}
		
		protected WorldGenerator(Vector3 pos) {
			position = pos;
		}
		
		public abstract void generate(List<GameObject> generated);
		
		public override sealed string getTagName() {
			return TAGNAME;
		}
		
		protected bool isColliding(Vector3 vec, List<GameObject> li) {
			foreach (GameObject go in li) {
				if (SBUtil.objectCollidesPosition(go, vec))
					return true;
			}
			return false;
		}
	}
}
