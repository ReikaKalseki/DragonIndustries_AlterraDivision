using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {
	
	public static class DIHooks {
	    
	    private static bool worldLoaded = false;
	    
	    public static event Action<DayNightCycle> onDayNightTickEvent;
	    public static event Action onWorldLoadedEvent;
	    public static event Action<Player> onPlayerTickEvent;
	    public static event Action<DamageToDeal> onDamageEvent;
	    public static event Action<Pickupable> onItemPickedUpEvent;
	    public static event Action<CellManager, LargeWorldEntity> onEntityRegisterEvent;
	    public static event Action<SkyApplier> onSkyApplierSpawnEvent;
	    public static event Action<TechType, Constructable> onConstructedEvent;
	    public static event Func<string, Vector3, string> getBiomeEvent;
	    public static event Action<GameObject> onKnifedEvent;
	    
	    static DIHooks() {
	    	
	    }
	    
	    public class DamageToDeal {
	    	
	    	public readonly float originalAmount;
	    	public readonly DamageType type;
	    	public readonly GameObject target;
	    	public readonly GameObject dealer;
	    	
	    	public float amount;
	    	
	    	internal DamageToDeal(float amt, DamageType tt, GameObject tgt, GameObject dl) {
	    		originalAmount = amt;
	    		amount = originalAmount;
	    		type = tt;
	    		target = tgt;
	    		dealer = dl;
	    	}
	    	
	    }
    
	    public static void onTick(DayNightCycle cyc) {
	    	if (BuildingHandler.instance.isEnabled) {
		    	if (GameInput.GetButtonDown(GameInput.Button.LeftHand)) {
		    		BuildingHandler.instance.handleClick(KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl));
		    	}
	    		if (GameInput.GetButtonDown(GameInput.Button.RightHand)) {
		    		BuildingHandler.instance.handleRClick(KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl));
		    	}
		    	
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Delete)) {
		    		BuildingHandler.instance.deleteSelected();
		    	}
		    	
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftAlt)) {
		    		BuildingHandler.instance.manipulateSelected();
		    	}
	    	}
	    	
	    	if (onDayNightTickEvent != null)
	    		onDayNightTickEvent.Invoke(cyc);
	    }
	    
	    public static void onWorldLoaded() {
	    	worldLoaded = true;
	    	SNUtil.log("Intercepted world load", SNUtil.diDLL);
	        
	    	DuplicateRecipeDelegate.updateLocale();
	    	CustomEgg.updateLocale();
	    	
	    	if (onWorldLoadedEvent != null)
	    		onWorldLoadedEvent.Invoke();
	    }
	    
	    public static void tickPlayer(Player ep) {
	    	if (Time.timeScale <= 0)
	    		return;
	    	
	    	StoryHandler.instance.tick(ep);
	    	
	    	if (onPlayerTickEvent != null)
	    		onPlayerTickEvent.Invoke(ep);
	    }
   
		public static float recalculateDamage(float damage, DamageType type, GameObject target, GameObject dealer) {
	    	if (onDamageEvent != null) {
	    		DamageToDeal deal = new DamageToDeal(damage, type, target, dealer);
	    		onDamageEvent.Invoke(deal);
	   			return deal.amount;
	    	}
	    	else {
	    		return damage;
	    	}
		}
	    
	    public static string getBiomeAt(string orig, Vector3 pos) {
	    	if (getBiomeEvent != null)
	    		orig = getBiomeEvent.Invoke(orig, pos);
	    	return orig;
	    }	
    
	    public static void onItemPickedUp(Pickupable p) {
	    	TechType tt = TechType.None;
	    	TechTag tag = p.gameObject.GetComponent<TechTag>();
	    	if (tag)
	    		tt = tag.type;
	    	if (tt == TechType.None) {
	    		PrefabIdentifier pi = p.gameObject.GetComponent<PrefabIdentifier>();
	    		if (pi)
	    			tt = CraftData.entClassTechTable[pi.ClassId];
	    	}
	    	if (tt != TechType.None)
	    		TechnologyUnlockSystem.instance.triggerDirectUnlock(tt);
	    	
	    	foreach (Renderer r in p.gameObject.GetComponentsInChildren<Renderer>()) {
				foreach (Material m in r.materials) {
					m.DisableKeyword("FX_BUILDING");
				}
			}
	    	
	    	if (onItemPickedUpEvent != null)
	    		onItemPickedUpEvent.Invoke(p);
	    }
    
	    public static void onEntityRegister(CellManager cm, LargeWorldEntity lw) {
	    	if (!worldLoaded) {
	    		onWorldLoaded();
	    	}/*
	    	if (lw.cellLevel != LargeWorldEntity.CellLevel.Global) {
	    		BatchCells batchCells;
				Int3 block = cm.streamer.GetBlock(lw.transform.position);
				Int3 key = block / cm.streamer.blocksPerBatch;
				if (cm.batch2cells.TryGetValue(key, out batchCells)) {
							Int3 u = block % cm.streamer.blocksPerBatch;
							Int3 cellSize = BatchCells.GetCellSize((int)lw.cellLevel, cm.streamer.blocksPerBatch);
							Int3 cellId = u / cellSize;
							bool flag = cellId.x < 0 || cellId.y < 0 || cellId.z < 0;
					if (!flag) {
			    		try {
							//batchCells.Get(cellId, (int)lw.cellLevel);
							batchCells.GetCells((int)lw.cellLevel).Get(cellId);
			    		}
			    		catch {
							flag = true;
			    		}
					}
					if (flag) {
						SNUtil.log("Moving object "+lw.gameObject+" to global cell, as it is outside the world bounds and was otherwise going to bind to an OOB cell.");
		    			lw.cellLevel = LargeWorldEntity.CellLevel.Global;
					}
				}
	    	}*/
	    	if (onEntityRegisterEvent != null)
	    		onEntityRegisterEvent.Invoke(cm, lw);
	    }
	    
	    public static void onFarmedPlantGrowingSpawn(Plantable p, GameObject plant) {
	    	TechTag tt = p.gameObject.GetComponent<TechTag>();
	    	if (tt) {
	    		BasicCustomPlant plantType = BasicCustomPlant.getPlant(tt.type);
	    		//SNUtil.writeToChat("Planted "+tt+" > "+plantType);
	    		if (plantType != null) {
		    		RenderUtil.swapToModdedTextures(plant.GetComponentInChildren<Renderer>(true), plantType);
		    		plant.gameObject.EnsureComponent<TechTag>().type = tt.type;
	    		}
	    	}
	    }
	    
	    public static void onFarmedPlantGrowDone(GrowingPlant p, GameObject plant) {
	    	TechTag tt = p.gameObject.GetComponent<TechTag>();
	    	if (tt) {
	    		BasicCustomPlant plantType = BasicCustomPlant.getPlant(tt.type);
	    		//SNUtil.writeToChat("Grew "+tt+" > "+plantType);
	    		if (plantType != null) {
	    			ObjectUtil.convertTemplateObject(plant, plantType);
	    		}
	    	}
	    }
	    
	    public static void onSkyApplierSpawn(SkyApplier pk) {
	    	if (onSkyApplierSpawnEvent != null)
	    		onSkyApplierSpawnEvent.Invoke(pk);
	    }
	    
	    public static void onStoryGoalCompleted(string key) {
	    	StoryHandler.instance.NotifyGoalComplete(key);
	    }
	    
	    public static bool isItemUsable(TechType tt) {
	    	return tt == TechType.Bladderfish || UsableItemRegistry.instance.isUsable(tt);
	    }
	    
	    public static bool useItem(Survival s, GameObject useObj) {
			bool flag = false;
			if (useObj != null) {
				TechType tt = CraftData.GetTechType(useObj);
				if (tt == TechType.None) {
					Pickupable component = useObj.GetComponent<Pickupable>();
					if (component)
						tt = component.GetTechType();
				}
				SNUtil.log("Player used item "+tt, SNUtil.diDLL);
				flag = UsableItemRegistry.instance.use(tt, s, useObj);
				if (flag)
					FMODUWE.PlayOneShot(CraftData.GetUseEatSound(tt), Player.main.transform.position, 1f);
			}
			return flag;
	    }
	   
		public static void onScanComplete(PDAScanner.EntryData data) {
		   	if (data != null)
	   			TechnologyUnlockSystem.instance.triggerDirectUnlock(data.key);
		}
	    
	    public static void tickLaserCutting(Sealed s, float amt) {
			if (s._sealed && s.maxOpenedAmount >= 0) {
				s.openedAmount = Mathf.Min(s.openedAmount + amt, s.maxOpenedAmount);
				if (Mathf.Approximately(s.openedAmount, s.maxOpenedAmount)) {
					s._sealed = false;
					s.openedEvent.Trigger(s);
					Debug.Log("Trigger opened event");
				}
			}
	    }
	    
	    public static void getBulkheadMouseoverText(BulkheadDoor bk) {
			if (bk.enabled && bk.state == BulkheadDoor.State.Zero) {
	    		Sealed s = bk.GetComponent<Sealed>();
	    		if (s != null && s.IsSealed()) {
	    			if (s.maxOpenedAmount < 0) {
	    				HandReticle.main.SetInteractText("BulkheadInoperable");
						HandReticle.main.SetIcon(HandReticle.IconType.None, 1f);
	    			}
	    			else {
						HandReticle.main.SetInteractText("SealedInstructions"); //is a locale key
						HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
						HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
	    			}
	    		}
	    		else {
					HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
					HandReticle.main.SetInteractText(bk.targetState ? "Close" : "Open");
	    		}
			}
	    }
	    
	    public static void onBulkheadClick(BulkheadDoor bk) {
			Base componentInParent = bk.GetComponentInParent<Base>();
			Sealed s = bk.GetComponent<Sealed>();
			if (s != null && s.IsSealed()) {
				
			}
			else if (componentInParent != null && !componentInParent.isReady) {
				bk.ToggleImmediately();
			}
			else if (bk.enabled && bk.state == BulkheadDoor.State.Zero) {
				if (GameOptions.GetVrAnimationMode()) {
					bk.ToggleImmediately();
					return;
				}
				bk.SequenceDone();
			}
	    }
	   
	   	public static bool isInsideForHatch(UseableDiveHatch hatch) {
	   		SeabaseReconstruction.WorldgenBaseWaterparkHatch wb = hatch.gameObject.GetComponent<SeabaseReconstruction.WorldgenBaseWaterparkHatch>();
	   		if (wb)
	   			return wb.isPlayerInside();
	   		return Player.main.IsInsideWalkable() && Player.main.currentWaterPark == null;
	   	}
	    
	    public static void onConstructionComplete(TechType tt, Constructable c) {
	    	Story.ItemGoalTracker.OnConstruct(tt);
	    	
	    	TechnologyUnlockSystem.instance.triggerDirectUnlock(tt);
	    	
	    	if (onConstructedEvent != null)
	    		onConstructedEvent.Invoke(tt, c);
	    }
	    
	    public static void onKnifed(GameObject go) {
	    	if (go && onKnifedEvent != null)
	    		onKnifedEvent.Invoke(go);
	    }
	}
}
