using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.DIAlterra {
	
	public class TechnologyFragment {
		
		public readonly string template;
		public readonly Action<GameObject> objectModify;
		
		public TechType target;
		public GenUtil.ContainerPrefab fragmentPrefab;
		
		public TechnologyFragment(string pfb, Action<GameObject> a = null) {
			template = pfb;
			objectModify = a;
		}
		
		public static TechnologyFragment createFragment(string pfb, TechType tech, string name, int needed, float scanTime = 2, bool destroyOnScan = true, Action<GameObject> a = null) {
			TechnologyFragment ret = new TechnologyFragment(pfb, a);
			ret.target = tech;
			createFragment(ret, name, needed, scanTime);
			return ret;
		}
		
		public static void createFragment(TechnologyFragment tf, string name, int needed, float scanTime = 2, bool destroyOnScan = true) {
			SNUtil.log("Creating fragments for "+tf.target);
			tf.fragmentPrefab = GenUtil.getOrCreateFragment(tf.target, name, tf.template, tf.objectModify);
			SNUtil.addPDAEntry(tf.fragmentPrefab, scanTime, null, null, null, e => {
				e.blueprint = tf.target;
				e.destroyAfterScan = destroyOnScan;
				e.isFragment = true;
				e.totalFragments = needed;
				e.key = GenUtil.getFragment(tf.target, 0).TechType;
			});
		}
		
	}
}
