using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class RecipeUtil {
		
		public static void addIngredient(TechType recipe, TechType add, int amt) {
			TechData rec = getRecipe(recipe);
			rec.Ingredients.Add(new Ingredient(add, amt));
		}
		
		public static void removeIngredient(TechType recipe, TechType item) {
			modifyIngredients(recipe, i => i.techType == item);
		}
		
		/** Return true in the func to delete the ingredient. */
		public static void modifyIngredients(TechType recipe, Func<Ingredient, bool> a) {
			TechData rec = getRecipe(recipe);
			for (int idx = rec.Ingredients.Count-1; idx >= 0; idx--) {
				Ingredient i = rec.Ingredients[idx];
				if (a(i)) {
					rec.Ingredients.RemoveAt(idx);
				}
			}
		}
		
		public static TechData getRecipe(TechType item) {
			TechData rec = CraftDataHandler.GetTechData(item);
			if (rec == null)
				throw new Exception("No such recipe '"+item+"'!");
			return rec;
		}
		
		public static CraftNode getRootNode(CraftTree.Type type) {
			switch(type) {
				case CraftTree.Type.Fabricator:
					return CraftTree.FabricatorScheme();
				case CraftTree.Type.Constructor:
					return CraftTree.ConstructorScheme();
				case CraftTree.Type.Workbench:
					return CraftTree.WorkbenchScheme();
				case CraftTree.Type.SeamothUpgrades:
					return CraftTree.SeamothUpgradesScheme();
				case CraftTree.Type.MapRoom:
					return CraftTree.MapRoomSheme();
				case CraftTree.Type.Centrifuge:
					return CraftTree.CentrifugeScheme();
				case CraftTree.Type.CyclopsFabricator:
					return CraftTree.CyclopsFabricatorScheme();
				case CraftTree.Type.Rocket:
					return CraftTree.RocketScheme();
			}
			return null;
		}
		
		public static void dumpCraftTree(CraftTree.Type type) {
			SBUtil.log("Tree "+type+":");
			CraftNode root = getRootNode(type);
			dumpCraftTreeFromNode(root);
		}
		
		public static void dumpCraftTreeFromNode(CraftNode root) {
			dumpCraftTreeFromNode(root, new List<string>());
		}
		
		private static void dumpCraftTreeFromNode(CraftNode root, List<string> prefix) {
			if (root == null) {
				SBUtil.log(string.Join("/", prefix)+" -> null @ root");
				return;
			}
			List<TreeNode> nodes = root.nodes;
			for (int i = 0; i < nodes.Count; i++) {
				TreeNode node = nodes[i];
				if (node == null) {
					SBUtil.log(string.Join("/", prefix)+" -> null @ "+i);
				}
				else {
					try {
						SBUtil.log(string.Join("/", prefix)+" -> Node #"+i+": "+node.id);
						prefix.Add(node.id);
						dumpCraftTreeFromNode((CraftNode)node, prefix);
						prefix.RemoveAt(prefix.Count-1);
					}
					catch (Exception e) {
						SBUtil.log(e.ToString());
					}
				}
			}
		}
		
		public static void dumpPDATree() {
			foreach (var kvp in PDAEncyclopedia.entries) {
				SBUtil.log("PDA entry '"+kvp.Key+"': "+kvp.Value);
			}
			dumpCraftTreeFromNode(PDAEncyclopedia.tree);
		}
		
	}
}
