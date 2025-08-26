using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {
	public static class TextureManager {

		private static readonly Dictionary<Assembly, Dictionary<string, Texture2D>> textures = new Dictionary<Assembly, Dictionary<string, Texture2D>>();
		private static readonly Dictionary<Assembly, Dictionary<string, Atlas.Sprite>> sprites = new Dictionary<Assembly, Dictionary<string, Atlas.Sprite>>();
		//private static readonly Texture2D NOT_FOUND = ImageUtils.LoadTextureFromFile(path); 

		static TextureManager() {

		}

		public static void refresh() {
			textures.Clear();
		}

		public static Texture2D getTexture(Assembly a, string path) {
			if (a == null)
				throw new Exception("You must specify a mod to load the texture for!");
			if (!textures.ContainsKey(a))
				textures[a] = new Dictionary<string, Texture2D>();
			if (!textures[a].ContainsKey(path)) {
				textures[a][path] = loadTexture(a, path);
			}
			return textures[a][path];
		}

		private static Texture2D loadTexture(Assembly a, string relative) {
			string folder = Path.GetDirectoryName(a.Location);
			string path = Path.Combine(folder, relative+".png");
			SNUtil.log("Loading texture from '" + path + "'", a);
			Texture2D newTex = ImageUtils.LoadTextureFromFile(path);
			if (newTex == null) {
				//newTex = NOT_FOUND;
				SNUtil.log("Texture not found @ " + path, a);
			}
			return newTex;
		}

		public static Atlas.Sprite getSprite(Assembly a, string path) {
			if (a == null)
				throw new Exception("You must specify a mod to load the texture for!");
			if (!sprites.ContainsKey(a))
				sprites[a] = new Dictionary<string, Atlas.Sprite>();
			if (!sprites[a].ContainsKey(path)) {
				sprites[a][path] = loadSprite(a, path);
			}
			return sprites[a][path];
		}

		public static Sprite createSprite(Texture2D tex, int pxPerUnit = 512) {
			return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), pxPerUnit);
		}

		private static Atlas.Sprite loadSprite(Assembly a, string relative) {
			string folder = Path.GetDirectoryName(a.Location);
			string path = Path.Combine(folder, relative+".png");
			SNUtil.log("Loading sprite from '" + path + "'", a);
			Atlas.Sprite newTex = ImageUtils.LoadSpriteFromFile(path);
			if (newTex == null) {
				//newTex = NOT_FOUND;
				SNUtil.log("Sprite not found @ " + path, a);
			}
			return newTex;
		}

	}
}
