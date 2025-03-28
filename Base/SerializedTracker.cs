using System;
using System.IO;
using System.Reflection;
using System.Xml;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	
	public class SerializedTracker<E> where E : SerializedTrackedEvent {
		
		private readonly string saveFileName;
		private readonly Func<XmlElement, E> parser;
		private readonly Func<string, E> legacyParser;
		
		private static readonly List<E> data = new List<E>();
		
		protected SerializedTracker(string name, bool loadWithGame, Func<XmlElement, E> f, Func<string, E> l) {
			saveFileName = name;
			parser = f;
			legacyParser = l;
			IngameMenuHandler.Main.RegisterOnSaveEvent(handleSave);
			if (loadWithGame)
				IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
		}
		
		public void forAll(Action<E> a) {
			foreach (E e in data)
				a.Invoke(e);
		}
		
		public void forAllNewerThan(float thresh, Action<E> forEach) {
			float time = DayNightCycle.main.timePassedAsFloat;
			foreach (E obj in data) {
				double age = time - obj.eventTime;
				if (age < thresh)
					forEach.Invoke(obj);
			}
		}
		
		public string getData() {
			return data.toDebugString();
		}
		
		public void handleSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			XmlDocument content = new XmlDocument();
			content.AppendChild(content.CreateElement("Root"));
			data.Sort();
			foreach (E tt in data) {
				XmlElement e = content.DocumentElement.addChild("Event");
				tt.saveToXML(e);
				e.addProperty("eventTime", tt.eventTime);
			}
			content.Save(path);
		}
		
		public void handleLoad() {
			string dir = SNUtil.getCurrentSaveDir();
			string path = Path.Combine(dir, saveFileName);
			if (!File.Exists(path))
				return;
			SNUtil.log("Loading saved "+typeof(E).Name+"[] from "+path, SNUtil.diDLL);
			clear();
			IEnumerable<string> ie = File.ReadLines(path);
			if (ie == null)
				return;
			string first = ie.FirstOrDefault(); //ReadLines is lazy-eval so this only actually reads the first line
			if (string.IsNullOrEmpty(first))
				return;
			if (first.Contains("<Root>")) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				if (doc.DocumentElement == null)
					return;
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					try {
						E tt = parser.Invoke(e);
						if (tt != null) {
							add(tt);
						}
						else {
							
						}
					}
					catch (Exception ex) {
						SNUtil.log("Failed to parse " + typeof(E).Name + " from XML '" + e.OuterXml + "': " + ex.ToString(), SNUtil.diDLL);
					}
				}
			}
			else if (legacyParser != null) {
				foreach (string s in File.ReadAllLines(path)) {
					E e = legacyParser.Invoke(s);
					if (e != null) {
						add(e);
					}
				}
			}
		}
		
		protected virtual void add(E e) {
			data.Add(e);
		}
		
		protected virtual void clear() {
			data.Clear();
		}
		
	}
		
	public abstract class SerializedTrackedEvent : IComparable<SerializedTrackedEvent> {
		
		public readonly double eventTime;
		
		public string formatTime { get { return Utils.PrettifyTime((int)eventTime); } }
		
		protected SerializedTrackedEvent(double t) {
			eventTime = t;
		}
			
		public abstract void saveToXML(XmlElement e);
			
		public int CompareTo(SerializedTrackedEvent e) {
			return eventTime.CompareTo(e.eventTime);
		}

			
	}
}
