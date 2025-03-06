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
			[ConfigEntry("Discard entity ID conflicts when the new entity is at origin", true)]SKIPZEROEDIDOVERWRITE, //Solves the "overwriting id '<id>' (old class '<uid>', new class '<uid>'), used to be '<name>' at <position> now '<name>' at (0.0, 0.0, 0.0)" log errors caused by corrupted entities, at the cost of probably deleting said entities.
		}
	}
}
