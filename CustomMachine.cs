using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public abstract class CustomMachine : Buildable {
		
		private static readonly string[] texTypes = new string[]{"_MainTex", "_SpecTex", "_BumpMap", "_Illum"};
		
		private readonly Dictionary<TechType, Ingredient> recipe = new Dictionary<TechType, Ingredient>();
		
		public readonly string id;
		private readonly Base.Piece templatePrefab;
		
		public float glowIntensity = -1;
		
		protected CustomMachine(string id, string name, string desc, Base.Piece template) : base(id, name, desc) {
			this.id = id;
			templatePrefab = template;
		}
		
		public CustomMachine addIngredient(ItemDef item, int amt) {
			return addIngredient(item.getTechType(), amt);
		}
		
		public CustomMachine addIngredient(Craftable item, int amt) {
			return addIngredient(item.TechType, amt);
		}
		
		public CustomMachine addIngredient(TechType item, int amt) {
			if (recipe.ContainsKey(item))
				recipe[item].amount += amt;
			else
				recipe[item] = new Ingredient(item, amt);
			return this;
		}

		public override sealed TechGroup GroupForPDA {
			get {
				return TechGroup.InteriorPieces;
			}
		}

		public override sealed TechCategory CategoryForPDA {
			get {
				return TechCategory.InteriorRoom;
			}
		}
		
		public override GameObject GetGameObject() {
			GameObject world = SBUtil.getBasePiece(templatePrefab);
			if (world != null) {
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				world.EnsureComponent<CustomMachineLogic>().tick = onTick;
				world.EnsureComponent<Constructable>().techType = TechType;
				
				Renderer r = world.GetComponentInChildren<Renderer>();
				applyMaterialChanges(r);
				prepareGameObject(world, r);
				//SBUtil.writeToChat("Applying custom texes to "+world+" @ "+world.transform.position);
				return world;
			}
			else {
				SBUtil.writeToChat("Could not fetch template GO for "+this);
				return null;
			}
		}
		
		protected abstract void onTick(GameObject go);
			
		private void applyMaterialChanges(Renderer r) {
			bool flag = false;
			foreach (String type in texTypes) {
				Texture2D newTex = TextureManager.getTexture("Textures/Resources/"+formatFileName()+type);
				if (newTex != null) {
					r.materials[0].SetTexture(type, newTex);
					r.sharedMaterial.SetTexture(type, newTex);
					flag = true;
					//SBUtil.writeToChat("Found "+type+" texture @ "+path);
				}
				else {
					//SBUtil.writeToChat("No texture found at "+path);
				}
			}
			if (!flag) {
				SBUtil.log("NO CUSTOM TEXTURES FOUND: "+this);
			}
			if (glowIntensity >= 0) {
				SBUtil.setEmissivity(r, glowIntensity, "GlowStrength");
				
				r.materials[0].EnableKeyword("MARMO_EMISSION");
				r.sharedMaterial.EnableKeyword("MARMO_EMISSION");
			}
		}
			
		private string formatFileName() {
			string n = ClassID;
			System.Text.StringBuilder ret = new System.Text.StringBuilder();
			for (int i = 0; i < n.Length; i++) {
				char c = n[i];
				if (c == '_')
					continue;
				bool caps = i == 0 || n[i-1] == '_';
				if (caps) {
					c = Char.ToUpperInvariant(c);
				}
				else {
					c = Char.ToLowerInvariant(c);
				}
				ret.Append(c);
			}
			return ret.ToString();
		}
		
		protected virtual void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
		protected override sealed TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = new List<Ingredient>(recipe.Values),
				craftAmount = 1
			};
		}
		
		protected override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite("Textures/Items/"+formatFileName());
		}
		
		class CustomMachineLogic : MonoBehaviour {
			
			internal Action<GameObject> tick;
			
			void Start() {
				
			}
			
			void Update() {
				tick(gameObject);
			}
			
		}
	}
}
