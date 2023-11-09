using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public class DIConfig
	{		
		public enum ConfigEntries {
			[ConfigEntry("Prevent large coral tube destruction from overharvesting", true)]INFITUBE,
			[ConfigEntry("Do version checks on another thread", false)]THREADVER, //Whether to offload version checks to another thread in parallel to game load. This is rarely necessary (and in fact often counterproductive) but can help load times if you have very slow internet.
		}
	}
}
