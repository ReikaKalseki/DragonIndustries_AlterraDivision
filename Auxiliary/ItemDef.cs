using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

namespace ReikaKalseki.DIAlterra {
	public interface ItemDef<E> : ItemDef {

		E getItem();

		ItemDef<E> addIngredient(TechType item, int amt);

		ItemDef<E> addIngredient(ItemDef item, int amt);

	}

	public interface ItemDef {

		TechType getTechType();

	}
}
