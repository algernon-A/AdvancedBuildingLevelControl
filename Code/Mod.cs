using ICities;
using ColossalFramework.UI;
using CitiesHarmony.API;


namespace ABLC
{
    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public class ABLCMod : IUserMod
    {
        public static string ModName => "Advanced Building Level Control";
        public static string Version => "0.3";

        public string Name => ModName + " " + Version;
        public string Description => Translations.Translate("ABLC_DESC");


        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());

            // Load the settings file.
            ABLCSettingsFile.LoadSettings();
        }


        /// <summary>
        /// Called by the game when the mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            // Unapply Harmony patches via Cities Harmony.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }


        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            UIDropDown languageDropDown = (UIDropDown)helper.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index, (value) => { Translations.Index = value; ABLCSettingsFile.SaveSettings();  });
            languageDropDown.autoSize = false;
            languageDropDown.width = 270f;
        }
    }
}
