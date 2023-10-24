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
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra
{
	public static class PDAManager {
		
		private static readonly Dictionary<string, PDAPage> pages = new Dictionary<string, PDAPage>();
		
		static PDAManager() {
			
		}
		
		public static PDAPage getPage(string id) {
			return pages.ContainsKey(id) ? pages[id] : null;
		}
		
		public static PDAPage createPage(XMLLocale.LocaleEntry text) {	
			return createPage(text.key, text.name, text.pda, text.getField<string>("category"));
		}
		
		public static PDAPage createPage(string id, string name, string text, string cat) {
			if (pages.ContainsKey(id))
				throw new Exception("PDA page ID '"+id+"' already in use! "+pages[id]);
			PDAPage sig = new PDAPage(id, name, text, cat);
			pages[sig.id] = sig;
			SNUtil.log("Registered PDA page "+sig);
			return sig;
		}
		
		public class PDAPage {
			
			public readonly string id;
			public readonly string name;
			//public readonly string text;
			public readonly string category;
			
			private string text;
		
			private readonly PDAEncyclopedia.EntryData pageData = new PDAEncyclopedia.EntryData();
			
			public readonly Assembly ownerMod;
			
			private List<string> tree = new List<string>();
			
			private readonly PDAPrefab prefabID;
			
			internal PDAPage(string id, string n, string text, string cat) {
				this.id = id;
				name = n;
				this.text = text;	
				category = cat;
				
				tree.AddRange(cat.Split('/'));
				
				pageData.audio = null;
				pageData.key = id;
				pageData.timeCapsule = false;
				pageData.unlocked = false;		
				
				prefabID = new PDAPrefab(this);
				
				ownerMod = SNUtil.tryGetModDLL();
			}
			
			public PDAPage addSubcategory(string s) {
				tree.Add(s);
				return this;
			}
			
			public PDAPage setVoiceover(string path) {
				string sid = VanillaSounds.getID(path);
				if (sid == null) {
					SNUtil.log("Sound path "+path+" did not find an ID. Registering as custom.");
					pageData.audio = SoundManager.registerPDASound(SNUtil.tryGetModDLL(), "pda_vo_"+id, path).asset;
				}
				else {
					pageData.audio = SoundManager.buildSound(path, sid);
				}
				SNUtil.log("Setting "+this+" sound to "+pageData.audio.id+"="+pageData.audio.path);
				return this;
			}
			
			public PDAPage setHeaderImage(Texture2D img) {
				pageData.image = img;
				return this;
			}
			
			public void register() {
				pageData.nodes = tree.ToArray();
				pageData.path = string.Join("/", tree);
				PDAEncyclopediaHandler.AddCustomEntry(pageData);
				LanguageHandler.SetLanguageLine("Ency_"+pageData.key, name);
				injectString();
				
				prefabID.Patch();
			}
			
			public string getText() {
				return text;
			}
			
			public void append(string s, bool force = false) {
				text = text+s;
				injectString(force);
			}
			
			public void update(string text, bool force = false, bool allowNotification = true) {
				if (this.text == text)
					return;
				this.text = text;
				injectString(force, allowNotification);
			}
			
			private void injectString(bool force = false, bool allowNotification = true) {/*
				if (force && DIHooks.isWorldLoaded())
					Language.main.strings["EncyDesc_"+pageData.key] = text;
				else*/
					LanguageHandler.SetLanguageLine("EncyDesc_"+pageData.key, text);
					if (force && DIHooks.isWorldLoaded()) {
						uGUI_EncyclopediaTab tab = ((uGUI_EncyclopediaTab)Player.main.GetPDA().ui.tabs[PDATab.Encyclopedia]);
						if (tab) {
							if (tab.activeEntry && tab.activeEntry.key == pageData.key)
								tab.DisplayEntry(pageData.key);//.SetText(text);
						}
					}
					if (allowNotification)
						markUpdated(5);
			}
			
			public void markUpdated(float duration = 3F) {
				SNUtil.addEncyNotification(id, duration);
			}
			
			public TreeNode getNode() {
				List<string> li = new List<string>(tree);
				li.Add(id);
				return PDAEncyclopedia.tree.FindNodeByPath(li.ToArray());
			}
			
			public uGUI_ListEntry getListEntry() {
				uGUI_EncyclopediaTab tab = ((uGUI_EncyclopediaTab)Player.main.GetPDA().ui.tabs[PDATab.Encyclopedia]);
				foreach (uGUI_ListEntry e in tab.GetComponentsInChildren<uGUI_ListEntry>()) {
					if (e.GetComponentInChildren<Text>().text == name) {
						return e;
					}
				}
				return null;
			}
		
			public bool unlock(bool doSound = true) {
				if (!isUnlocked()) {
					pageData.unlocked = true;
					PDAEncyclopedia.Add(pageData.key, true);
					
					if (doSound)
						SoundManager.playSound("event:/tools/scanner/new_PDA_data"); //music + "integrating PDA data"
					
					return true;
				}
				return false;
			}
			
			public bool isUnlocked() {
				return pageData.unlocked || PDAEncyclopedia.entries.ContainsKey(pageData.key);
			}
			
			public override string ToString() {
				return string.Format("[PDAPage Id={0}, Name={1}, Text={2}, Category={3}, Header={4}, Mod={5}]", id, name, text.Replace("\n", ""), category, pageData.image, ownerMod.Location);
			}
			
			public string getPDAClassID() {
				return prefabID.ClassID;
			}
			
			public TechType getTechType() {
				return prefabID.TechType;
			}
 
			
		}
		
		sealed class PDAPrefab : Spawnable, DIPrefab<StringPrefabContainer> {
	        
			internal readonly PDAPage page;
		
			public float glowIntensity {get; set;}		
			public StringPrefabContainer baseTemplate {get; set;}
	        
	        internal PDAPrefab(PDAPage p) : base(p.id, p.name, "PDA page "+p.name) {
				page = p;
				baseTemplate = new StringPrefabContainer("0f1dd54e-b36e-40ca-aa85-d01df1e3e426"); //blood kelp PDA
	        }
			
	        public override GameObject GetGameObject() {
				GameObject go = ObjectUtil.getModPrefabBaseObject<StringPrefabContainer>(this);
				StoryHandTarget tgt = go.EnsureComponent<StoryHandTarget>();
				if (tgt.goal == null)
					tgt.goal = new Story.StoryGoal();
				tgt.goal.goalType = Story.GoalType.Encyclopedia;
				tgt.goal.key = page.id;
				return go;
	        }
		
			public Assembly getOwnerMod() {
				return SNUtil.diDLL;
			}
			
			public bool isResource() {
				return false;
			}
			
			public string getTextureFolder() {
				return null;
			}
			
			public void prepareGameObject(GameObject go, Renderer[] r) {
				
			}
		
			public Atlas.Sprite getIcon() {
				return SpriteManager.Get(TechType.PDA);
			}
			
		}
		
	}
}
