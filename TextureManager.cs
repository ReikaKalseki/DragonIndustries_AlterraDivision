using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.DIAlterra
{
	public static class TextureManager {
		
		private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		private static readonly Dictionary<string, Atlas.Sprite> sprites = new Dictionary<string, Atlas.Sprite>();
		//private static readonly Texture2D NOT_FOUND = ImageUtils.LoadTextureFromFile(path); 
		
		static TextureManager() {
			
		}
		
		public static Texture2D getTexture(string path) {
			if (!textures.ContainsKey(path)) {
				textures[path] = loadTexture(path);
			}
			return textures[path];
		}
		
		private static Texture2D loadTexture(string relative) {
			string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = Path.Combine(folder, relative+".png");
			Texture2D newTex = ImageUtils.LoadTextureFromFile(path);
			if (newTex == null) {
				//newTex = NOT_FOUND;
			}
			return newTex;
		}
		
		public static Atlas.Sprite getSprite(string path) {
			if (!sprites.ContainsKey(path)) {
				sprites[path] = loadSprite(path);
			}
			return sprites[path];
		}
		
		private static Atlas.Sprite loadSprite(string relative) {
			string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = Path.Combine(folder, relative+".png");
			Atlas.Sprite newTex = ImageUtils.LoadSpriteFromFile(path);
			if (newTex == null) {
				//newTex = NOT_FOUND;
			}
			return newTex;
		}
		
	}
}
