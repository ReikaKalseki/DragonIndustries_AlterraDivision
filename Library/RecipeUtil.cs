using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public static class RecipeUtil {

		private static readonly Dictionary<TechType, RecipeNode> nodes = new Dictionary<TechType, RecipeNode>();
		private static readonly Dictionary<TechType, TechGroup> techGroupData = new Dictionary<TechType, TechGroup>();
		private static readonly Dictionary<TechType, TechCategory> techCatData = new Dictionary<TechType, TechCategory>();
		private static readonly Dictionary<TechType, Dictionary<TechType, int>> originalRecipes = new Dictionary<TechType, Dictionary<TechType, int>>();
		internal static readonly Dictionary<TechType, ProgressionTrigger> techsToRemoveIf = new Dictionary<TechType, ProgressionTrigger>();

		private static bool shouldLogChanges = false;

		public static void startLoggingRecipeChanges() {
			originalRecipes.Clear();
			shouldLogChanges = true;
		}

		public static void logChangedRecipes() {
			SNUtil.log("Collated recipe changes: ");
			List<string> oldR = new List<string>();
			List<string> newR = new List<string>();
			foreach (KeyValuePair<TechType, Dictionary<TechType, int>> kvp in originalRecipes) {
				TechData rec = getRecipe(kvp.Key);
				SNUtil.log("Recipe for " + kvp.Key + " was changed. Previous recipe:");

				if (kvp.Value.Count > 0) {
					string s = (""+kvp.Key).ToUpper();
					foreach (KeyValuePair<TechType, int> kvp2 in kvp.Value) {
						SNUtil.log(kvp2.Key + " x" + kvp2.Value);
						s = s + ".addIngredient(" + (kvp2.Key + "").ToUpper() + ", " + kvp2.Value + ")";
					}
					s += ";";
					oldR.Add(s);
				}

				if (rec == null) {
					SNUtil.log("Recipe was removed");
				}
				else {
					SNUtil.log("New recipe:");
					string s = (""+kvp.Key).ToUpper();
					foreach (Ingredient i in rec.Ingredients) {
						SNUtil.log(i.techType + " x" + i.amount);
						s = s + ".addIngredient(" + (i.techType + "").ToUpper() + ", " + i.amount + ")";
					}
					s += ";";
					newR.Add(s);
				}
			}
			List<string> lines = new List<string>();
			lines.AddRange(oldR);
			lines.Add("=============");
			lines.AddRange(newR);
			System.IO.File.WriteAllLines("E:/My Documents/Desktop Stuff/Game Stuff/Modding/Minecraft/Mods Website - Generator/exported/snrecipe.txt", lines.ToArray());
			originalRecipes.Clear();
			shouldLogChanges = false;
		}

		private static void cacheOriginalRecipe(TechType item, TechData rec) {
			if (originalRecipes.ContainsKey(item))
				return;
			Dictionary<TechType, int> dict = new Dictionary<TechType, int>();
			foreach (Ingredient i in rec.Ingredients) {
				dict[i.techType] = i.amount;
			}
			originalRecipes[item] = dict;
		}

		public static void addIngredient(TechType recipe, TechType add, int amt) {
			TechData rec = getRecipe(recipe);
			cacheOriginalRecipe(recipe, rec);
			SNUtil.log("Adding " + add + "x" + amt + " to recipe " + recipe);
			foreach (Ingredient i in rec.Ingredients) {
				if (i.techType == add) {
					i.amount += amt;
					return;
				}
			}
			rec.Ingredients.Add(new Ingredient(add, amt));
		}

		public static void addIngredient(TechData rec, TechType add, int amt) {
			SNUtil.log("Adding " + add + "x" + amt + " to recipe " + rec);
			foreach (Ingredient i in rec.Ingredients) {
				if (i.techType == add) {
					i.amount += amt;
					return;
				}
			}
			rec.Ingredients.Add(new Ingredient(add, amt));
		}

		public static void ensureIngredient(TechType recipe, TechType item, int amt) {
			TechData rec = getRecipe(recipe);
			cacheOriginalRecipe(recipe, rec);
			SNUtil.log("Ensuring " + item + "x" + amt + " in recipe " + recipe);
			int has = 0;
			foreach (Ingredient i in rec.Ingredients) {
				if (i.techType == item) {
					has += i.amount;
				}
			}
			if (has < amt)
				addIngredient(recipe, item, amt - has);
		}

		public static Ingredient removeIngredient(TechType recipe, TechType item) {
			Ingredient ret = null;
			modifyIngredients(recipe, i => { if (i.techType == item) { ret = i; return true; } return false; });
			return ret;
		}

		/// <remarks>
		/// Return true in the func to delete the ingredient.
		/// </remarks>
		public static void modifyIngredients(TechType recipe, Func<Ingredient, bool> a) {
			TechData rec = getRecipe(recipe);
			cacheOriginalRecipe(recipe, rec);
			for (int idx = rec.Ingredients.Count - 1; idx >= 0; idx--) {
				Ingredient i = rec.Ingredients[idx];
				if (a(i)) {
					rec.Ingredients.RemoveAt(idx);
				}
			}
		}

		public static void clearIngredients(TechType recipe) {
			TechData rec = getRecipe(recipe);
			cacheOriginalRecipe(recipe, rec);
			rec.Ingredients.Clear();
		}

		public static TechData addRecipe(TechType item, TechGroup grp, TechCategory cat, string[] path = null, int amt = 1, CraftTree.Type fab = CraftTree.Type.Fabricator) {
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

		public static bool recipeExists(TechType item) {
			return CraftDataHandler.GetTechData(item) != null;
		}

		public static TechData getRecipe(TechType item, bool errorIfNone = true) {
			TechData rec = CraftDataHandler.GetTechData(item);
			if (rec == null && errorIfNone)
				throw new Exception("No such recipe '" + item + "'!");
			if (rec != null)
				CraftDataHandler.SetTechData(item, rec);
			return rec;
		}

		public static TechData removeRecipe(TechType item, bool removeCategories = false) {
			TechData rec = CraftDataHandler.GetTechData(item);
			CraftData.techData.Remove(item);
			RecipeNode node = getRecipeNode(item);
			if (node == null)
				buildRecipeNodeCache(); //try rebuild first
			if (node == null)
				throw new Exception("No node found for recipe " + item + "\n\n" + nodes.toDebugString());
			if (node.path == null)
				throw new Exception("Invalid pathless node " + node);
			CraftTreeHandler.Main.RemoveNode(node.recipeType, node.path.Split('\\'));
			nodes.Remove(item);
			//CraftTree.craftableTech.Remove(item);
			if (removeCategories) {
				foreach (TechGroup grp in Enum.GetValues(typeof(TechGroup))) {
					foreach (TechCategory cat in Enum.GetValues(typeof(TechCategory)))
						CraftDataHandler.RemoveFromGroup(grp, cat, item);
				}
				//CraftDataHandler.AddToGroup(TechGroup.Uncategorized, TechCategory.Misc, item);
			}
			SNUtil.log("Removing recipe " + item);
			return rec;
		}

		public static void changeRecipePath(TechType item, params string[] path) {
			changeRecipePath(item, CraftTree.Type.None, path);
		}

		public static void changeRecipePath(TechType item, CraftTree.Type cat, params string[] path) {
			RecipeNode node = getRecipeNode(item);
			if (node == null)
				buildRecipeNodeCache(); //try rebuild first
			if (node == null)
				throw new Exception("No node found for recipe " + item + "\n\n" + nodes.toDebugString());
			if (node.path == null)
				throw new Exception("Invalid pathless node " + node);
			CraftTreeHandler.Main.RemoveNode(node.recipeType, node.path.Split('\\'));
			nodes.Remove(item);
			CraftTreeHandler.AddCraftingNode(cat == CraftTree.Type.None ? node.recipeType : cat, item, path);
			SNUtil.log("Repathing recipe " + item + ": " + node.path + " > " + string.Join("\\", path));
		}

		public static void setItemCategory(TechType item, TechGroup tg, TechCategory tc) {
			foreach (TechGroup grp in Enum.GetValues(typeof(TechGroup))) {
				foreach (TechCategory cat in Enum.GetValues(typeof(TechCategory)))
					CraftDataHandler.RemoveFromGroup(grp, cat, item);
			}
			CraftDataHandler.AddToGroup(tg, tc, item);
		}

		public static RecipeNode getRecipeNode(TechType item) {
			if (nodes.Count == 0)
				buildRecipeNodeCache();
			return nodes.ContainsKey(item) ? nodes[item] : null;
		}

		public static void buildRecipeNodeCache() {
			nodes.Clear();
			foreach (CraftTree.Type t in Enum.GetValues(typeof(CraftTree.Type))) {
				cacheRecipeNode(getRootNode(t), t);
			}
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
			switch (type) {
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
			SNUtil.log("Tree " + type + ":", SNUtil.diDLL);
			CraftNode root = getRootNode(type);
			dumpCraftTreeFromNode(root);
		}

		public static void dumpCraftTreeFromNode(CraftNode root) {
			dumpCraftTreeFromNode(root, new List<string>());
		}

		private static void dumpCraftTreeFromNode(CraftNode root, List<string> prefix) {
			if (root == null) {
				SNUtil.log(string.Join("/", prefix) + " -> null @ root", SNUtil.diDLL);
				return;
			}
			List<TreeNode> nodes = root.nodes;
			for (int i = 0; i < nodes.Count; i++) {
				TreeNode node = nodes[i];
				if (node == null) {
					SNUtil.log(string.Join("/", prefix) + " -> null @ " + i, SNUtil.diDLL);
				}
				else {
					try {
						string s = string.Join("/", prefix)+" -> Node #"+i+": "+node.id;
						if (Language.main)
							s += " (" + Language.main.Get("Ency_" + node.id) + ")";
						SNUtil.log(s, SNUtil.diDLL);
						prefix.Add(node.id);
						dumpCraftTreeFromNode((CraftNode)node, prefix);
						prefix.RemoveAt(prefix.Count - 1);
					}
					catch (Exception e) {
						SNUtil.log(e.ToString());
					}
				}
			}
		}

		public static void dumpPDATree() {
			foreach (KeyValuePair<string, PDAEncyclopedia.Entry> kvp in PDAEncyclopedia.entries) {
				SNUtil.log("PDA entry '" + kvp.Key + "': " + kvp.Value, SNUtil.diDLL);
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

		public static List<TechType> buildLinkedItems(params PlannedIngredient[] li) {
			return buildLinkedItems(li.ToList());
		}

		public static List<TechType> buildLinkedItems(List<PlannedIngredient> li) {
			List<TechType> ret = new List<TechType>();
			foreach (PlannedIngredient p in li) {
				for (int i = 0; i < p.amount; i++)
					ret.Add(p.item.getTechType());
			}
			return ret;
		}

		public static TechData copyRecipe(TechData from) {
			TechData ret = new TechData();
			ret.craftAmount = from.craftAmount;
			ret.LinkedItems.AddRange(from.LinkedItems);
			foreach (Ingredient i in from.Ingredients) {
				ret.Ingredients.Add(new Ingredient(i.techType, i.amount));
			}
			ret.craftAmount = from.craftAmount;
			return ret;
		}

		public static TechData createUncrafting(TechType item, TechType primary = TechType.None) {
			TechData rec = getRecipe(item);
			TechData ret = new TechData();
			foreach (Ingredient ing in rec.Ingredients) {
				if (primary != TechType.None && primary == ing.techType) {
					ret.craftAmount = ing.amount;
				}
				else {
					for (int i = 0; i < ing.amount; i++)
						ret.LinkedItems.Add(ing.techType);
				}
			}
			ret.Ingredients.Add(new Ingredient(item, rec.craftAmount));
			CountMap<TechType> counts = new CountMap<TechType>();
			foreach (TechType tt in rec.LinkedItems) {
				counts.add(tt);
			}
			foreach (TechType tt in counts.getItems()) {
				rec.Ingredients.Add(new Ingredient(tt, counts.getCount(tt)));
			}
			return ret;
		}
		/*
		public static void moveRecipeTab(TechType item) {
			TechData rec = getRecipe(item);
	        CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, "Personal", "Equipment", "PrecursorKey_Purple");
	        CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorKey_Purple, "Machines");
		}*/

		public static string toString(TechData rec) {
			return getTotalIngredientSlotCount(rec) + ":" + string.Join("+", rec.Ingredients.Select<Ingredient, string>(r => "[" + r.techType.AsString() + " x" + r.amount + "]").ToArray()) + " = x" + rec.craftAmount + " & " + string.Join("+", rec.LinkedItems.Select<TechType, string>(tt => tt.AsString()).ToArray());
		}

		public static int getTotalIngredientSlotCount(TechData td) {
			int ret = 0;
			foreach (Ingredient i in td.Ingredients) {
				Vector2int size = CraftData.GetItemSize(i.techType);
				ret += size.x * size.y;
			}
			return ret;
		}

		public static List<Ingredient> combineIngredients(IEnumerable<Ingredient> list, IEnumerable<Ingredient> add) {
			CountMap<TechType> amt = new CountMap<TechType>();
			foreach (Ingredient i in list)
				amt.add(i.techType, i.amount);
			foreach (Ingredient i in add)
				amt.add(i.techType, i.amount);
			List<Ingredient> ret = new List<Ingredient>();
			foreach (TechType tt in amt.getItems()) {
				ret.Add(new Ingredient(tt, amt.getCount(tt)));
			}
			return ret;
		}

		public static Dictionary<TechType, int> getIngredientsDict(TechData td) {
			Dictionary<TechType, int> ret = new Dictionary<TechType, int>();
			foreach (Ingredient i in td.Ingredients) {
				int has = ret.ContainsKey(i.techType) ? ret[i.techType] : 0;
				ret[i.techType] = has + i.amount;
			}
			return ret;
		}

		public static void getRecipeCategory(TechType tt, out TechGroup grp, out TechCategory cat) {
			if (techGroupData.Count == 0) {
				foreach (KeyValuePair<TechGroup, Dictionary<TechCategory, List<TechType>>> kvp in CraftData.groups) {
					foreach (KeyValuePair<TechCategory, List<TechType>> kvp2 in kvp.Value) {
						foreach (TechType tt2 in kvp2.Value) {
							techGroupData[tt2] = kvp.Key;
							techCatData[tt2] = kvp2.Key;
						}
					}
				}
			}
			grp = techGroupData.ContainsKey(tt) ? techGroupData[tt] : TechGroup.Miscellaneous;
			cat = techCatData.ContainsKey(tt) ? techCatData[tt] : TechCategory.Misc;
		}
		/*
		public static bool areAnyRecipesOfTypeKnown(CraftTree.Type tree) {
			return areAnyRecipesUnderNodeKnown(getRootNode(tree));
		}
		
		public static bool areAnyRecipesUnderNodeKnown(CraftNode root) {
			if (root == null)
				return false;
			List<TreeNode> nodes = root.nodes;
			for (int i = 0; i < nodes.Count; i++) {
				TreeNode node = nodes[i];
				if (node != null) {
					if (node.
						SNUtil.log(string.Join("/", prefix)+" -> Node #"+i+": "+node.id, SNUtil.diDLL);
						prefix.Add(node.id);
						dumpCraftTreeFromNode((CraftNode)node, prefix);
						prefix.RemoveAt(prefix.Count-1);
				}
			}
		}*/

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
				return item + " @ " + recipeType + " >> " + path;
			}

		}

	}
}
