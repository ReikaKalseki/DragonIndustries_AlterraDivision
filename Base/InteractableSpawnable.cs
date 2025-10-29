using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
	public abstract class InteractableSpawnable : Spawnable {

		public readonly XMLLocale.LocaleEntry locale;

		public float scanTime = 1;

		public Action<PDAScanner.EntryData> scanEntryModifier = null;

		public readonly Assembly ownerMod;

		public int fragmentCount {  get; private set; }
		public int scanCount { get; private set; }

		public static TechType fragmentUnlock { get; private set; }

		protected InteractableSpawnable(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
			ownerMod = SNUtil.tryGetModDLL();
		}

		public void countGen<G>(WorldgenDatabase worldgen) where G : WorldGenerator {
			fragmentCount = worldgen.getCount<G>();
			SNUtil.log("Found " + fragmentCount + " " + ClassID + " to use as fragments", ownerMod);
		}

		public void countGen(WorldgenDatabase worldgen, string id = null) {
			if (id == null)
				id = ClassID;
			fragmentCount = worldgen.getCount(id);
			SNUtil.log("Found " + fragmentCount + " " + id + " to use as fragments", ownerMod);
		}

		public void setFragment(TechType unlock, int count, bool delete = false) {
			scanCount = count;
			fragmentUnlock = unlock;
			KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>() { fragmentUnlock });
			Action<PDAScanner.EntryData> old = scanEntryModifier;
			scanEntryModifier = e => {
				if (old != null)
					old.Invoke(e);
				e.isFragment = true;
				e.blueprint = fragmentUnlock;
				e.totalFragments = scanCount;
				e.destroyAfterScan = delete;
			};
		}

		public void registerEncyPage() {
			SNUtil.addPDAEntry(this, scanTime, locale.getString("category"), locale.pda, locale.getString("header"), scanEntryModifier);
			/*
			PDAManager.PDAPage page = PDAManager.createPage("ency_"+ClassID, FriendlyName, locale.pda, locale.getString("category"));
			page.setHeaderImage(TextureManager.getTexture(ownerMod, locale.getString("header")));
			page.register();
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.scanTime = scanTime;
			e.locked = true;
			if (scanEntryModifier != null)
				scanEntryModifier.Invoke(e);
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
			*/
		}
	}
}
