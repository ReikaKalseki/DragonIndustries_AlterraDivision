using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{		
	public interface ItemDef<E> : ItemDef {
			
		E getItem();
			
		ItemDef<E> addIngredient(TechType item, int amt);
			
		ItemDef<E> addIngredient(ItemDef item, int amt);
		
	}
	
	public interface ItemDef {
			
		TechType getTechType();
		
	}
}
