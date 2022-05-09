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
			}
			
			public void addSubcategory(string s) {
				tree.Add(s);
			}
			
			public void setVoiceover(string path) {
				string sid = VanillaSounds.getID(path);
				if (sid == null) {
					SBUtil.log("Sound path "+path+" did not find an ID. Registering as custom.");
					sid = "pda_vo_"+id;
					SoundManager.registerSound(sid, path);
				}
				pageData.audio = SBUtil.getSound(path);
				pageData.audio.id = sid;
				SBUtil.log("Setting "+this+" sound to "+pageData.audio.id);
			}
			
			public void setHeaderImage(Texture2D img) {
				pageData.image = img;
			}
			
			public void register() {
				pageData.nodes = tree.ToArray();
				pageData.path = string.Join("/", tree);
				PDAEncyclopediaHandler.AddCustomEntry(pageData);
				LanguageHandler.SetLanguageLine("Ency_"+pageData.key, name);
				LanguageHandler.SetLanguageLine("EncyDesc_"+pageData.key, text);
			}
		
			public void unlock(bool doSound = true) {
				pageData.unlocked = true;
				PDAEncyclopedia.Add(pageData.key, true);
				
				if (doSound)
					SBUtil.playSound("event:/tools/scanner/new_PDA_data"); //music + "integrating PDA data"
			}
			
			public override string ToString() {
				return string.Format("[PDAPage Id={0}, Name={1}, Text={2}, Category={3}]", id, name, text.Replace("\n", ""), category);
			}
 
			
		}
		
	}
}
