using System;
using System.Linq;
using UnityEngine;
using ICities;
using ColossalFramework.UI;
using ColossalFramework.Globalization;


namespace ABLC
{
    /// <summary>
    /// Class to handle the mod's options panel.
    /// </summary>
    internal static class OptionsPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float TitleMargin = Margin * 2f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;


        // Parent UI panel reference.
        internal static UIScrollablePanel optionsPanel;
        private static UIPanel gameOptionsPanel;

        // Instance references.
        private static GameObject optionsGameObject;


        /// <summary>
        /// Options panel setup.
        /// </summary>
        /// <param name="helper">UIHelperBase parent</param>
        internal static void Setup(UIHelperBase helper)
        {
            // Set up tab strip and containers.
            optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
            optionsPanel.autoLayout = false;
        }


        /// <summary>
        /// Attaches an event hook to options panel visibility, to enable/disable mod hokey when the panel is open.
        /// </summary>
        internal static void OptionsEventHook()
        {
            // Get options panel instance.
            gameOptionsPanel = UIView.library.Get<UIPanel>("OptionsPanel");

            if (gameOptionsPanel == null)
            {
                Logging.Error("couldn't find OptionsPanel");
            }
            else
            {
                // Simple event hook to create/destroy GameObject based on appropriate visibility.
                gameOptionsPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    // Create/destroy based on whether or not we're now visible.
                    if (isVisible)
                    {
                        Create();
                    }
                    else
                    {
                        Close();

                        // Save settings on close.
                        ModSettings.Save();
                    }
                };

                // Recreate panel on system locale change.
                LocaleManager.eventLocaleChanged += LocaleChanged;
            }
        }


        /// <summary>
        /// Refreshes the options panel (destroys and rebuilds) on a locale change when the options panel is open.
        /// </summary>
        internal static void LocaleChanged()
        {
            if (gameOptionsPanel != null && gameOptionsPanel.isVisible)
            {
                Logging.KeyMessage("changing locale");

                Close();
                Create();
            }
        }


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        private static void Create()
        {
            try
            {
                Logging.KeyMessage("creating options panel");

                // We're now visible - create our gameobject, and give it a unique name for easy finding with ModTools.
                optionsGameObject = new GameObject("ABLCOptionsPanel");

                // Attach to game options panel.
                optionsGameObject.transform.parent = optionsPanel.transform;

                // Create a base panel attached to our game object, perfectly overlaying the game options panel.
                UIPanel basePanel = optionsGameObject.AddComponent<UIPanel>();
                basePanel.width = optionsPanel.width - 10f;
                basePanel.height = 725f;
                basePanel.clipChildren = false;
                float headerWidth = basePanel.width - (TitleMargin * 2f);
                float checkLabelWidth = headerWidth - 40f;

                // Needed to ensure position is consistent if we regenerate after initial opening (e.g. on language change).
                basePanel.relativePosition = new Vector2(10f, 10f);

                // Y position indicator.
                float currentY = Margin;

                // Get font reference.
                UIFont semiBold = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Semibold");

                // Language choice.
                UIDropDown languageDropDown = UIControls.AddPlainDropDown(basePanel, LeftMargin, currentY, Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index);
                languageDropDown.eventSelectedIndexChanged += (control, index) =>
                {
                    Translations.Index = index;
                    LocaleChanged();
                    ModSettings.Save();
                };
                currentY += languageDropDown.parent.height + GroupMargin;

                // Panel options.
                currentY = AddTitle(basePanel, "ABLC_OPT_PNL", semiBold, headerWidth, currentY);

                UICheckBox onRightCheck = UIControls.AddPlainCheckBox(basePanel, Margin, currentY, Translations.Translate("ABLC_OPT_RT"), checkLabelWidth);
                onRightCheck.isChecked = ModSettings.onRight;
                onRightCheck.eventCheckChanged += (control, value) => { ModSettings.onRight = value; };
                currentY += onRightCheck.height + Margin;

                UICheckBox showPanelCheck = UIControls.AddPlainCheckBox(basePanel, Margin, currentY, Translations.Translate("ABLC_OPT_SHO"), checkLabelWidth);
                showPanelCheck.isChecked = ModSettings.showPanel;
                showPanelCheck.eventCheckChanged += (control, value) => { ModSettings.showPanel = value; };
                currentY += showPanelCheck.height + Margin;

                // Gameplay options.
                currentY = AddTitle(basePanel, "ABLC_OPT_PLY", semiBold, headerWidth, currentY);

                UICheckBox abandonHistCheck = UIControls.AddPlainCheckBox(basePanel, Margin, currentY, Translations.Translate("ABLC_OPT_HNA"), checkLabelWidth);
                abandonHistCheck.isChecked = ModSettings.noAbandonHistorical;
                abandonHistCheck.eventCheckChanged += (control, value) => { ModSettings.noAbandonHistorical = value; };
                currentY += abandonHistCheck.height + Margin;

                UICheckBox abandonAnyCheck = UIControls.AddPlainCheckBox(basePanel, Margin, currentY, Translations.Translate("ABLC_OPT_ANA"), checkLabelWidth);
                abandonAnyCheck.isChecked = ModSettings.noAbandonAny;
                abandonAnyCheck.eventCheckChanged += (control, value) => { ModSettings.noAbandonAny = value; };
                currentY += abandonAnyCheck.height + Margin;

                UICheckBox randomLevelCheck = UIControls.AddPlainCheckBox(basePanel, Margin, currentY, Translations.Translate("ABLC_OPT_RND"), checkLabelWidth);
                randomLevelCheck.isChecked = ModSettings.randomLevels;
                randomLevelCheck.eventCheckChanged += (control, value) => { ModSettings.randomLevels = value; };
                currentY += randomLevelCheck.height + Margin;

                UICheckBox loadLevelCheck = UIControls.AddPlainCheckBox(basePanel, Margin, currentY, Translations.Translate("ABLC_OPT_CLL"), checkLabelWidth);
                loadLevelCheck.isChecked = ModSettings.loadLevelCheck;
                loadLevelCheck.eventCheckChanged += (control, value) => { ModSettings.loadLevelCheck = value; };
                currentY += loadLevelCheck.height + Margin;
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating options panel");
            }
        }


        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        private static void Close()
        {
            // Save settings on close.
            ModSettings.Save();

            // We're no longer visible - destroy our game object.
            if (optionsGameObject != null)
            {
                GameObject.Destroy(optionsGameObject);
                optionsGameObject = null;
            }
        }


        /// <summary>
        /// Adds a spacer and new title to the given panel.
        /// </summary>
        /// <param name="titleKey">Title translation key</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="width">Spacer width</param>
        /// <param name="yPos">Y-position indicator</param>
        /// <returns>Updated Y position indicator</returns>
        private static float AddTitle(UIComponent parent, string titleKey, UIFont titleFont, float maxWidth, float yPos)
        {
            float currentY = yPos + Margin;
            UIControls.OptionsSpacer(parent, Margin, currentY, maxWidth);
            currentY += TitleMargin * 2f;
            UILabel label = UIControls.AddLabel(parent, Margin, currentY, Translations.Translate(titleKey), textScale: 1.2f);
            label.font = titleFont;
            return currentY + label.height + TitleMargin;
        }
    }
}