using ColossalFramework.UI;


namespace ABLC
{
    /// <summary>
    /// Textures.
    /// </summary>
    internal static class Textures
    {
        // ABLC button icon texture atlas.
        private static UITextureAtlas ablcButtonSprites;
        internal static UITextureAtlas ABLCButtonSprites
        {
            get
            {
                if (ablcButtonSprites == null)
                {
                    ablcButtonSprites = TextureUtils.LoadSpriteAtlas("ablc_buttons");
                }

                return ablcButtonSprites;
            }
        }
    }
}