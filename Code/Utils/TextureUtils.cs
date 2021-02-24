using System.IO;
using ColossalFramework.UI;
using UnityEngine;


namespace ABLC
{
	class TextureUtils
	{
		/// <summary>
		/// Loads a four-sprite texture atlas from a given .png file.
		/// </summary>
		/// <param name="atlasName">Atlas name (".png" will be appended fto make the filename)</param>
		/// <returns>New texture atlas</returns>
		internal static UITextureAtlas LoadSpriteAtlas(string atlasName)
		{
			// Create new texture atlas for button.
			UITextureAtlas newAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
			newAtlas.name = atlasName;
			newAtlas.material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);

			// Load texture from file.
			Texture2D newTexture = LoadTexture(atlasName + ".png");
			newAtlas.material.mainTexture = newTexture;

			// Setup sprites.
			string[] spriteNames = new string[] { "disabled", "normal", "pressed", "hovered" };
			int numSprites = spriteNames.Length;
			float spriteWidth = 1f / spriteNames.Length;

			// Iterate through each sprite (counter increment is in region setup).
			for (int i = 0; i < numSprites; ++i)
			{
				UITextureAtlas.SpriteInfo sprite = new UITextureAtlas.SpriteInfo
				{
					name = spriteNames[i],
					texture = newTexture,
					// Sprite regions are horizontally arranged, evenly spaced.
					region = new Rect(i * spriteWidth, 0f, spriteWidth, 1f)
				};
				newAtlas.AddSprite(sprite);
			}

			return newAtlas;
		}


		/// <summary>
		/// Loads a 2D texture from file.
		/// </summary>
		/// <param name="fileName">Texture file to load</param>
		/// <returns>New 2D texture</returns>
		private static Texture2D LoadTexture(string fileName)
		{
			using (Stream stream = OpenResourceFile(fileName))
			{
				// New texture.
				Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp
				};

				// Read texture as byte stream from file.
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				texture.LoadImage(array);
				texture.Apply();

				return texture;
			}
		}


		/// <summary>
		/// Opens the named resource file for reading.
		/// </summary>
		/// <param name="fileName">File to open</param>
		/// <returns>Read-only file stream</returns>
		private static Stream OpenResourceFile(string fileName)
		{
			string path = Path.Combine(ModUtils.GetAssemblyPath(), "Resources");
			return File.OpenRead(Path.Combine(path, fileName));
		}
	}
}
