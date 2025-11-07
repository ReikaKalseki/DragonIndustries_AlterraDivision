using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public class ScreenFXManager {

		public static readonly ScreenFXManager instance = new ScreenFXManager();

		public GameObject mainCamera { get; private set; }
		public MesmerizedScreenFXController mesmerController;// { get; private set; }
		public MesmerizedScreenFX mesmerShader;// { get; private set; }
		public CyclopsSmokeScreenFXController smokeController;// { get; private set; }
		public CyclopsSmokeScreenFX smokeShader;// { get; private set; }
		public RadiationsScreenFXController radiationController;// { get; private set; }
		public RadiationsScreenFX radiationShader;// { get; private set; }
		public SonarScreenFX sonarShader;// { get; private set; }
		public RadialBlurScreenFXController radialController;// { get; private set; }
		public RadialBlurScreenFX radialShader;// { get; private set; }
		public TelepathyScreenFXController telepathyController;// { get; private set; }
		public TelepathyScreenFX telepathyShader;// { get; private set; }
		public WarpScreenFXController warpController;// { get; private set; }
		public WarpScreenFX warpShader;// { get; private set; }
		public EndSequenceWarpScreenFXController endSequenceController;// { get; private set; }
		public EndSequenceWarpScreenFX endSequenceShader;// { get; private set; }
		public TeleportScreenFXController teleportController;// { get; private set; }
		public TeleportScreenFX teleportShader;// { get; private set; }

		public Vector4 defaultMesmerShaderColors { get; private set; }
		public Color defaultSmokeShaderColors { get; private set; }
		public Color defaultRadiationShaderColors { get; private set; }

		private readonly Dictionary<MonoBehaviour, ShaderPair> shaders = new Dictionary<MonoBehaviour, ShaderPair>();
		private readonly List<ScreenFXOverride> effects = new List<ScreenFXOverride>();

		public static bool disableShaders = false;

		//private readonly Dictionary<MonoBehaviour, List<ScreenFXOverride>> activeOverrides = new Dictionary<MonoBehaviour, List<ScreenFXOverride>>();
		//private readonly HashSet<MonoBehaviour> activeOverrides = new HashSet<MonoBehaviour>();

		private ScreenFXManager() {

		}

		private void addShader<S, C>(out S shader, out C controller) where S : MonoBehaviour where C : MonoBehaviour {
			shader = Camera.main.GetComponent<S>();
			controller = Camera.main.GetComponent<C>();
			shaders[shader] = new ShaderPair(shader, controller);
		}

		public void addOverride(ScreenFXOverride fx) {
			effects.Add(fx);
			effects.Sort();
		}

		public void tick() {
			if (!mainCamera && Camera.main) {
				mainCamera = Camera.main.gameObject;
				if (mainCamera) {
					//SNUtil.log("Load cam");

					this.addShader(out mesmerShader, out mesmerController);
					this.addShader(out smokeShader, out smokeController);
					this.addShader(out radiationShader, out radiationController);
					this.addShader(out telepathyShader, out telepathyController);
					this.addShader(out warpShader, out warpController);
					this.addShader(out endSequenceShader, out endSequenceController);
					this.addShader(out teleportShader, out teleportController);
					this.addShader(out radialShader, out radialController);

					shaders[mesmerShader].onStopOverride = () => { if (mesmerShader.mat) mesmerShader.mat.SetVector("_ColorStrength", defaultMesmerShaderColors); };
					shaders[smokeShader].onStopOverride = () => { if (smokeShader.mat) smokeShader.mat.color = defaultSmokeShaderColors; };
					shaders[radiationShader].onStopOverride = () => { radiationShader.color = defaultRadiationShaderColors; };

					if (mesmerShader && mesmerShader.mat)
						defaultMesmerShaderColors = mesmerShader.mat.GetVector("_ColorStrength");
					else
						mainCamera = null;
					if (!mainCamera) {
						//SNUtil.log("Fail A");
						return;
					}

					if (smokeShader) {
						smokeShader.enabled = true;
						if (smokeShader.mat)
							defaultSmokeShaderColors = smokeShader.mat.color;
						else
							mainCamera = null;
						smokeShader.enabled = false;
					}
					else
						mainCamera = null;
					if (!mainCamera) {
						//SNUtil.log("Fail B");
						return;
					}

					if (radiationShader)
						defaultRadiationShaderColors = radiationShader.color;
					else
						mainCamera = null;

					if (!mainCamera) {
						//SNUtil.log("Fail C");
						return;
					}

					sonarShader = mainCamera.GetComponent<SonarScreenFX>();

				}
				else {
					return;
				}
			}
			if (!mainCamera || shaders.Count == 0 || disableShaders)
				return;
			foreach (ShaderPair sp in shaders.Values)
				sp.overriddenThisTick = false;
			foreach (ScreenFXOverride fx in effects) {
				fx.onTick();
			}
			foreach (ShaderPair s in shaders.Values) {
				//s.controller.enabled = !s.overriddenThisTick;//!activeOverrides.Contains(c.Key);
				if (s.overriddenThisTick) {
					if (s.controller)
						s.controller.enabled = false;
					if (s.shader)
						s.shader.enabled = true;
				}
				else {
					if (s.controller)
						s.controller.enabled = true;
				}
				if (!s.overriddenThisTick && s.onStopOverride != null) {
					s.onStopOverride.Invoke();
				}
			}/*
	    	foreach (KeyValuePair<MonoBehaviour, List<ScreenFXOverride>> kvp in activeOverrides) {
	    		fx.onTick();
	    	}*/
		}
		/*
	    public void registerOverrideThisTick(MonoBehaviour shader, ScreenFXOverride over) {
	    	if (!activeOverrides.ContainsKey(shader)) {
	    		activeOverrides[shader] = new List<ScreenFXOverride>();
	    	}
	    	if (!activeOverrides[shader].Contains(over))
	    		activeOverrides[shader].Add(over);
	    }*/
		public void registerOverrideThisTick(MonoBehaviour shader) {
			//activeOverrides.Add(shader);
			shaders[shader].overriddenThisTick = true;
		}

		public abstract class ScreenFXOverride : IComparable<ScreenFXOverride> {

			public readonly int priority;

			public ScreenFXOverride(int pri) {
				priority = pri;
			}

			/// <remarks>Set is of shaders not controllers</remarks>
			public abstract void onTick();

			public int CompareTo(ScreenFXOverride fx) {
				return priority.CompareTo(fx.priority);
			}

		}

		private class ShaderPair {

			public readonly MonoBehaviour shader;
			public readonly MonoBehaviour controller;
			public Action onStopOverride;

			public bool overriddenThisTick;

			public ShaderPair(MonoBehaviour s, MonoBehaviour c) {
				shader = s;
				controller = c;
			}

		}

	}
}
