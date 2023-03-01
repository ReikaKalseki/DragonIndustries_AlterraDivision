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
		}
	}
}
