using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class RenderUtil {
		
		private static readonly string[] texTypes = new string[]{"_MainTex", "_SpecTex", "_BumpMap", "_Illum"};
		private static readonly HashSet<string> warnNoTextures = new HashSet<string>();
		
		public static void setEmissivity(Renderer r, float amt, HashSet<int> matIndices = null) {
			setEmissivity(r, amt, amt, matIndices);
		}
		
		public static void setEmissivity(Renderer r, float amt, float night, HashSet<int> matIndices = null) {
			Material[] mr = r.materials;
			for (int i = 0; i < mr.Length; i++) {
				if (matIndices != null && !matIndices.Contains(i))
					continue;
				setEmissivity(mr[i], amt, night);
			}
		}
		
		public static void setEmissivity(Material m, float amt) {
			setEmissivity(m, amt, amt);
		}
		
		public static void setEmissivity(Material m, float amt, float night) {
			m.SetFloat("_GlowStrength", amt);
			m.SetFloat("_GlowStrengthNight", night);
			if (amt > 0)
				m.EnableKeyword("MARMO_EMISSION");
			else
				m.DisableKeyword("MARMO_EMISSION");
		}
		
		public static void setGlossiness(Renderer r, float specular, float shininess, float fresnel, HashSet<int> matIndices = null) {
			Material[] mr = r.materials;
			for (int i = 0; i < mr.Length; i++) {
				if (matIndices != null && !matIndices.Contains(i))
					continue;
				setGlossiness(mr[i], specular, shininess, fresnel);
			}
		}
		
		public static void setGlossiness(Material m, float specular, float shininess, float fresnel) {
			m.SetFloat("_Fresnel", fresnel);
			m.SetFloat("_Shininess", shininess);
			m.SetFloat("_SpecInt", specular);
		}
		
		public static void makeTransparent(Renderer r, HashSet<int> matIndices = null) {
			Material[] mr = r.materials;
			for (int i = 0; i < mr.Length; i++) {
				if (matIndices != null && !matIndices.Contains(i))
					continue;
				makeTransparent(mr[i]);
			}
		}
		
		public static void disableTransparency(Material m, int queue = 3100) {
			m.EnableKeyword("_ZWRITE_ON");
	  		m.DisableKeyword("WBOIT");
			m.SetInt("_ZWrite", 1);
			m.SetInt("_Cutoff", 1);
			m.SetInt("_Mode", 0);
			m.SetFloat("_SrcBlend", 1f);
			m.SetFloat("_DstBlend", 0f);
			m.SetFloat("_SrcBlend2", 0f);
			m.SetFloat("_DstBlend2", 10f);
			m.SetFloat("_AddSrcBlend", 1f);
			m.SetFloat("_AddDstBlend", 1f);
			m.SetFloat("_AddSrcBlend2", 0f);
			m.SetFloat("_AddDstBlend2", 10f);
			m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack | MaterialGlobalIlluminationFlags.RealtimeEmissive;
			m.renderQueue = queue;
			//m.enableInstancing = true;
		}
		
		public static void makeTransparent(Material m, int queue = 3101) {
			//m.EnableKeyword("_ZWRITE_ON");
	  		m.EnableKeyword("WBOIT");
			m.SetInt("_ZWrite", 0);
			m.SetInt("_Cutoff", 0);
			m.SetInt("_Mode", 3);
			m.SetFloat("_SrcBlend", 1f);
			m.SetFloat("_DstBlend", 1f);
			m.SetFloat("_SrcBlend2", 0f);
			m.SetFloat("_DstBlend2", 10f);
			m.SetFloat("_AddSrcBlend", 1f);
			m.SetFloat("_AddDstBlend", 1f);
			m.SetFloat("_AddSrcBlend2", 0f);
			m.SetFloat("_AddDstBlend2", 10f);
			m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack | MaterialGlobalIlluminationFlags.RealtimeEmissive;
			m.renderQueue = queue;
			//m.enableInstancing = true;
		}
		
		public static void enableAlpha(Material m, float cutoff = 1) {
			m.EnableKeyword("MARMO_ALPHA_CLIP");
			m.SetFloat("_Cutoff", cutoff);
		}
		
		public static Texture extractTexture(GameObject go, string texType) {
			return go.GetComponentInChildren<Renderer>().materials[0].GetTexture(texType);
		}
		
		public static bool swapTextures(Assembly a, Renderer r, string path, Dictionary<int,string> textureLayers = null)  {
			if (r == null)
				throw new Exception("Tried to retexture a null renderer!");
			bool flag = false;
			foreach (String type in texTypes) {
				for (int i = 0; i < r.materials.Length; i++) {
					if (!r.materials[i])
						continue;
					if ((textureLayers == null && i == 0) || (textureLayers != null && textureLayers.ContainsKey(i))) {
						string suffix = textureLayers != null ? (string.IsNullOrEmpty(textureLayers[i]) ? "" : "_"+textureLayers[i]) : "";
						if (path.EndsWith("/", StringComparison.InvariantCultureIgnoreCase) && suffix.StartsWith("_", StringComparison.InvariantCultureIgnoreCase)) {
							suffix = suffix.Substring(1);
						}
						string name = path+suffix+type;
						Texture2D newTex = TextureManager.getTexture(a, name);
						if (newTex != null) {
							r.materials[i].SetTexture(type, newTex);
							flag = true;
							//if (!skipPrint)
							//	SNUtil.log("Found "+r+"/"+i+" "+type+" texture @ "+name, a);
						}
						else {
							//SNUtil.writeToChat("No texture found at "+path, a);
						}
					}
				}
			}
			return flag;
		}
		
		public static void swapToModdedTextures<T>(Renderer[] r, DIPrefab<T> pfb) where T : PrefabReference {
			foreach (Renderer rr in r) {
				swapToModdedTextures(rr, pfb);
			}
		}
		
		public static void swapToModdedTextures<T>(Renderer r, DIPrefab<T> pfb) where T : PrefabReference {
			string path = "Textures/"+pfb.getTextureFolder()+"/"+ObjectUtil.formatFileName((ModPrefab)pfb);
			//SNUtil.log("Applying custom textures in '"+path+"' to mod prefab "+pfb+" renderer "+r, pfb.getOwnerMod());
			Dictionary<int,string> dict = null;
			if (pfb is MultiTexturePrefab)
				dict = ((MultiTexturePrefab)pfb).getTextureLayers(r);
			if (!swapTextures(pfb.getOwnerMod(), r, path, dict)) {
				if (!warnNoTextures.Contains(pfb.ClassID)) {
					SNUtil.log("NO CUSTOM TEXTURES FOUND in "+path+": "+pfb, pfb.getOwnerMod());
					warnNoTextures.Add(pfb.ClassID);
				}
			}
			
			if (pfb.glowIntensity > 0) {
				setEmissivity(r, pfb.glowIntensity);
			}
			else {
				setEmissivity(r, 0);
				foreach (Material m in r.materials)
					m.DisableKeyword("MARMO_EMISSION");
			}
		}
		
		public static void setPolyanilineColor(Renderer r, Color c, float glow = 0) {
			Material m = r.materials[1]; //liquid
			m.SetColor("_Color", c);
			m.SetColor("_SpecColor", c);
			m.SetColor("_GlowColor", c);
			setEmissivity(r.materials[0], 0);
			setEmissivity(r.materials[2], 0);
			r.materials[0].DisableKeyword("MARMO_EMISSION");
			r.materials[2].DisableKeyword("MARMO_EMISSION");
			setEmissivity(m, glow);
			swapTextures(SNUtil.diDLL, r, "Textures/WhiteAniline", new Dictionary<int, string>(){{1, ""}});
			r.gameObject.FindAncestor<SkyApplier>().SetSky(Skies.Auto);
		}
		
		public static GameObject setModel(Renderer r, GameObject modelObj) {
			return setModel(r.transform.parent.gameObject, r.gameObject.name, modelObj);
		}			
		
		public static GameObject setModel(GameObject go, string localModelName, GameObject modelObj) { //FIXME duplicate models
			ObjectUtil.removeChildObject(go, localModelName);
			modelObj = UnityEngine.Object.Instantiate(modelObj);
			modelObj.name = localModelName;
			modelObj.transform.parent = go.transform;
			modelObj.transform.localPosition = Vector3.zero;
			modelObj.transform.localEulerAngles = Vector3.zero;
			modelObj.transform.localRotation = Quaternion.identity;
			modelObj.transform.localScale = Vector3.one;
			convertToModel(modelObj);
			return modelObj;
		}
		
		public static void convertToModel(GameObject modelObj, params Type[] except) {
			HashSet<Type> li = except.ToSet();
			foreach (Component c in modelObj.GetComponentsInChildren<Component>()) {
				if (c is Transform || c is Renderer || c is MeshFilter || c is Animator || c is Collider || c is VFXFabricating || c is PrefabIdentifier || c is ChildObjectIdentifier || c is AnimatorComponent)
					continue;
				if (li.Contains(c.GetType()))
				    continue;
				UnityEngine.Object.DestroyImmediate(c);
			}
		}
		
		public static void setMesh(GameObject go, Mesh m) {
			if (m == null) {
				SNUtil.writeToChat("Cannot set a GO mesh to null!");
				return;
			}
			Color[] c = m.colors;
			for (int i = 0; i < c.Length; i++) {
				c[i] = new Color(c[i].r, c[i].g, c[i].b, 1);
			}
			m.colors = c;
			foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>(true)) {
				mf.mesh = m;
			}
			foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
				smr.sharedMesh = m;
				smr.enabled = true;
			}
		}
		
		public static Texture2D getSpriteTexture(Sprite s) {
			if (!Mathf.Approximately(s.rect.width, s.texture.width)) {
	        	Texture2D newTex = new Texture2D((int)s.rect.width, (int)s.rect.height);
	        	Rect r = s.textureRect;
	            Color[] newColors = s.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
	            newTex.SetPixels(newColors);
	            newTex.Apply();
	            return newTex;
	       	}
			else {
	             return s.texture;
			}
	    }
		
		public static Atlas.Sprite copySprite(Atlas.Sprite source) {
			return ImageUtils.LoadSpriteFromTexture(source.texture);
		}
		
		public static Texture2D duplicateTexture(Texture2D source) {
		    RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);		
		    Graphics.Blit(source, renderTex);
		    RenderTexture previous = RenderTexture.active;
		    RenderTexture.active = renderTex;
		    Texture2D copy = new Texture2D(source.width, source.height);
		    copy.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		    copy.Apply();
		    RenderTexture.active = previous;
		    RenderTexture.ReleaseTemporary(renderTex);
		    return copy;
		}
		
		public static void dumpTextures(Assembly a, Renderer r) {
			foreach (Material m in r.materials) {
				dumpTextures(a, m, r.gameObject.name+"_$_");
			}
		}
		
		public static void dumpTextures(Assembly a, Material m, string prefix = "") {
			foreach (string tex in m.GetTexturePropertyNames()) {
				string fn = prefix+m.name+"_-_"+tex;
				Texture2D img = (Texture2D)m.GetTexture(tex);
				dumpTexture(a, fn, img);
			}
		}
		
		public static void dumpTexture(Assembly a, string fn, RenderTexture img, string pathOverride = null) {
		    Texture2D copy = new Texture2D(img.width, img.height);
		    copy.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
		    copy.Apply();
		    dumpTexture(a, fn, copy, pathOverride);
		}
		
		public static void dumpTexture(Assembly a, string fn, Texture2D img, string pathOverride = null) {
			if (img != null) {
				byte[] raw = duplicateTexture(img).EncodeToPNG();
				string folder = Path.GetDirectoryName(a.Location);
				string path = Path.Combine(folder, "TextureDump", fn+".png");
				if (!string.IsNullOrEmpty(pathOverride))
					path = Path.Combine(pathOverride, fn+".png");
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes(path, raw);
			}
		}
		/*
		public static Texture2D getVanillaTexture(string tex) {
			return null;//FIXME how
		}*/
		
	}
}
