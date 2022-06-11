using System;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	
	public static class RecipeUtil {
		
		private static readonly Dictionary<TechType, RecipeNode> nodes = new Dictionary<TechType, RecipeNode>();
		
		public static void addIngredient(TechType recipe, TechType add, int amt) {
			TechData rec = getRecipe(recipe);
			rec.Ingredients.Add(new Ingredient(add, amt));
			SBUtil.log("Adding "+add+"x"+amt+" to recipe "+recipe);
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
		
		public static TechData addRecipe(TechType item, TechGroup grp, TechCategory cat, int amt = 1, CraftTree.Type fab = CraftTree.Type.Fabricator, string[] path = null) {
			TechData rec = new TechData
			{
				Ingredients = new List<Ingredient>(),
				craftAmount = amt
			};
			CraftDataHandler.SetTechData(item, rec);
			if (grp != TechGroup.Uncategorized)
	        	CraftDataHandler.AddToGroup(grp, cat, item);
			if (fab != CraftTree.Type.None)
	        	CraftTreeHandler.AddCraftingNode(fab, item, path == null ? new string[0] : path);
			return rec;
		}
		
		public static TechData getRecipe(TechType item) {
			TechData rec = CraftDataHandler.GetTechData(item);
			if (rec == null)
				throw new Exception("No such recipe '"+item+"'!");
			CraftDataHandler.SetTechData(item, rec);
			return rec;
		}
		
		public static TechData removeRecipe(TechType item) {
			TechData rec = CraftDataHandler.GetTechData(item);
			CraftData.techData.Remove(item);
			RecipeNode node = getRecipeNode(item);
			if (node == null)
				throw new Exception("No node found for recipe "+item+"\n\n"+nodes.toDebugString<TechType, RecipeNode>());
			if (node.path == null)
				throw new Exception("Invalid pathless node "+node);
			CraftTreeHandler.Main.RemoveNode(node.recipeType, node.path.Split('\\'));
			nodes.Remove(item);
			//CraftTree.craftableTech.Remove(item);
			SBUtil.log("Removing recipe "+item);
			return rec;
		}
		
		public static RecipeNode getRecipeNode(TechType item) {
			if (nodes.Count == 0) {
				foreach (CraftTree.Type t in Enum.GetValues(typeof(CraftTree.Type))) {
					cacheRecipeNode(getRootNode(t), t);
				}
			}
			return nodes[item];
		}
		
		private static void cacheRecipeNode(CraftNode node, CraftTree.Type type) {
			if (node == null)
				return;
			if (node.techType0 != TechType.None)
				nodes[node.techType0] = new RecipeNode(node.techType0, type, node.GetPathString('\\', true));
			if (node.nodes != null) {
				foreach (CraftNode child in node.nodes) {
					cacheRecipeNode(child, type);
				}
			}
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
		
		public static List<Ingredient> buildRecipeList(List<PlannedIngredient> li) {
			List<Ingredient> ret = new List<Ingredient>();
			foreach (PlannedIngredient p in li) {
				ret.Add(new Ingredient(p.item.getTechType(), p.amount));
			}
			return ret;
		}
		
		public class RecipeNode {
			
			public readonly TechType item;
			public readonly CraftTree.Type recipeType;
			public readonly string path;
			
			internal RecipeNode(TechType tt, CraftTree.Type t, string s) {
				item = tt;
				recipeType = t;
				path = s;
			}
			
			public override string ToString() {
				return item+" @ "+recipeType+" >> "+path;
			}
			
		}
		
	}
}
