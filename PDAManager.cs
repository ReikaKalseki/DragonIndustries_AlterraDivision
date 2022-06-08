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
				throw new Exception("Signal ID '"+id+"' already in use!");
			PDAPage sig = new PDAPage(id, name, text, cat);
			pages[sig.id] = sig;
			SBUtil.log("Registered PDA page "+sig);
			return sig;
		}
		
		public class PDAPage {
			
			public readonly string id;
			public readonly string name;
			public readonly string text;
			public readonly string category;
		
			private readonly PDAEncyclopedia.EntryData pageData = new PDAEncyclopedia.EntryData();
			
			private List<string> tree = new List<string>();
			
			private PDAPrefab prefabID;
			
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
			}
			
			public PDAPage addSubcategory(string s) {
				tree.Add(s);
				return this;
			}
			
			public PDAPage setVoiceover(string path) {
				string sid = VanillaSounds.getID(path);
				if (sid == null) {
					SBUtil.log("Sound path "+path+" did not find an ID. Registering as custom.");
					pageData.audio = SoundManager.registerSound("pda_vo_"+id, path);
				}
				else {
					pageData.audio = SBUtil.getSound(path, sid);
				}
				SBUtil.log("Setting "+this+" sound to "+pageData.audio.id+"="+pageData.audio.path);
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
				LanguageHandler.SetLanguageLine("EncyDesc_"+pageData.key, text);
				
				prefabID.Patch();
			}
		
			public void unlock(bool doSound = true) {
				if (!pageData.unlocked) {
					pageData.unlocked = true;
					PDAEncyclopedia.Add(pageData.key, true);
					
					if (doSound)
						SBUtil.playSound("event:/tools/scanner/new_PDA_data"); //music + "integrating PDA data"
				}
			}
			
			public override string ToString() {
				return string.Format("[PDAPage Id={0}, Name={1}, Text={2}, Category={3}]", id, name, text.Replace("\n", ""), category);
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
				GameObject go = SBUtil.getModPrefabBaseObject<StringPrefabContainer>(this);
				StoryHandTarget tgt = go.EnsureComponent<StoryHandTarget>();
				tgt.goal.goalType = Story.GoalType.Encyclopedia;
				tgt.goal.key = page.id;
				return go;
	        }
			
			public bool isResource() {
				return false;
			}
			
			public string getTextureFolder() {
				return null;
			}
			
			public void prepareGameObject(GameObject go, Renderer r) {
				
			}
			
		}
		
	}
}
