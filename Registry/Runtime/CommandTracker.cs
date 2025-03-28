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
	
	public class CommandTracker : SerializedTracker<CommandTracker.CommandEvent> {
		
		public static readonly CommandTracker instance = new CommandTracker();
		
		private CommandTracker() : base("Commands.dat", true, parse, null) {
			
		}
		
		public void onCommand(string cmd) {
			add(new CommandEvent(cmd, DayNightCycle.main.timePassedAsFloat));
		}
		
		private static CommandEvent parse(XmlElement s) {
			return new CommandEvent(CommandEvent.buildCommand(s), s.getFloat("eventTime", -1));
		}
		
		public class CommandEvent : SerializedTrackedEvent {
			
			public readonly string command;
				
			internal CommandEvent(string c, double time) : base(time) {
				command = c;
			}
				
			public override void saveToXML(XmlElement e) {
				splitCommand(command, e);
			}
			
			internal static string buildCommand(XmlElement e) {
				string cmd = e.getProperty("command");
				foreach (XmlElement e2 in e.getDirectElementsByTagName("arg")) {
					cmd += " "+e2.InnerText;
				}
				return cmd;
			}
			
			private static void splitCommand(string cmd, XmlElement e) {
				string[] parts = cmd.Split(' ');
				e.addProperty("command", parts[0]);
				for (int i = 1; i < parts.Length; i++) {
					e.addProperty("arg", parts[i]);
				}
			}
				
		}
		
	}
}
