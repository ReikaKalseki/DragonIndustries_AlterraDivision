using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra
{
	public abstract class CustomMachine<M> : Buildable, DIPrefab<CustomMachine<M>, StringPrefabContainer> where M : CustomMachineLogic {
		
		private readonly List<PlannedIngredient> recipe = new List<PlannedIngredient>();
		
		public readonly string id;
		
		public float glowIntensity {get; set;}		
		public StringPrefabContainer baseTemplate {get; set;}
		
		protected CustomMachine(string id, string name, string desc, string template) : base(id, name, desc) {
			this.id = id;
			baseTemplate = new StringPrefabContainer(template);
		}
		
		public CustomMachine<M> addIngredient(ItemDef item, int amt) {
			return addIngredient(item.getTechType(), amt);
		}
		
		public CustomMachine<M> addIngredient(ModPrefab item, int amt) {
			return addIngredient(new ModPrefabTechReference(item), amt);
		}
		
		public CustomMachine<M> addIngredient(TechType item, int amt) {
			return addIngredient(new TechTypeContainer(item), amt);
		}
		
		public CustomMachine<M> addIngredient(TechTypeReference item, int amt) {
			recipe.Add(new PlannedIngredient(item, amt));
			return this;
		}

		public override sealed TechGroup GroupForPDA {
			get {
				return TechGroup.InteriorModules;
			}
		}

		public override sealed TechCategory CategoryForPDA {
			get {
				return TechCategory.InteriorModule;
			}
		}
		
		public void addFragments(int needed, float scanTime = 5, params MachineFragment[] fragments) {
			SNUtil.log("Creating "+fragments.Length+" fragments for "+this);
			foreach (MachineFragment m in fragments) {
				m.machine = TechType;
				m.fragmentPrefab = GenUtil.getOrCreateFragment(this, m.template, m.objectModify);
				SNUtil.addPDAEntry(m.fragmentPrefab, scanTime, null, null, null, e => {
					e.blueprint = TechType;
					e.destroyAfterScan = true;
					e.isFragment = true;
					e.totalFragments = needed;
					e.key = m.fragmentPrefab.TechType; //or this?
				});
				SNUtil.log("Registered fragment "+m.fragmentPrefab.ClassID);
			}
		}
		
		//protected abstract OrientedBounds[] GetBounds { get; }
			
		public sealed override GameObject GetGameObject() {
			GameObject world = ObjectUtil.getModPrefabBaseObject(this);
			world.EnsureComponent<M>().prefab = this;
			Constructable ctr = world.EnsureComponent<Constructable>();
			ctr.techType = TechType;
			ctr.allowedInBase = true;
			ctr.allowedInSub = true;
			ctr.allowedOnGround = true;
			ctr.allowedOutside = false;
			ctr.allowedOnCeiling = false;
			ctr.allowedOnWall = false;
			ctr.rotationEnabled = true;
			ctr.surfaceType = VFXSurfaceTypes.metal;
			ctr.forceUpright = true;
			ctr.allowedOnConstructables = false;
			LargeWorldEntity lw = world.EnsureComponent<LargeWorldEntity>();
			lw.cellLevel = LargeWorldEntity.CellLevel.Medium;
			initializeMachine(world);
			world.SetActive(true);
			return world;
		}
		
		public bool isResource() {
			return false;
		}
		
		public string getTextureFolder() {
			return "Machines";
		}
		
		public void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public virtual void initializeMachine(GameObject go) {
			
		}
		
		public sealed override string ToString() {
			return base.ToString()+" ["+TechType+"] / "+ClassID+" / "+PrefabFileName;
		}
		
		protected override sealed TechData GetBlueprintRecipe() {
			return new TechData
			{
				Ingredients = RecipeUtil.buildRecipeList(recipe),
				craftAmount = 1
			};
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite("Textures/Items/"+ObjectUtil.formatFileName(this));
		}
	}
	
	public class MachineFragment {
		
		public readonly string template;
		public readonly Action<GameObject> objectModify;
		
		public TechType machine;
		public GenUtil.ContainerPrefab fragmentPrefab;
		
		public MachineFragment(string pfb, Action<GameObject> a = null) {
			template = pfb;
			objectModify = a;
		}
		
	}
		
	public abstract class CustomMachineLogic : MonoBehaviour {
		
		internal ModPrefab prefab;
		
		private float lastDayTime;
		
		void Start() {
			
		}
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			updateEntity(time-lastDayTime);
			lastDayTime = time;
		}
		
		protected abstract void updateEntity(float seconds);
		
	}
}
