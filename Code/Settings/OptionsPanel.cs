namespace ABLC
{
    using System.Linq;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// The mod's settings options panel.
    /// </summary>
    internal class OptionsPanel : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float TitleMargin = 50f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPanel"/> class.
        /// </summary>
        internal OptionsPanel()
        {



            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - Margin * 2f;
            float checkLabelWidth = headerWidth - 40f;
            // Y position indicator.
            float currentY = Margin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(this, LeftMargin, currentY, Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
                ModSettings.Save();
            };
            currentY += languageDropDown.parent.height + Margin;

            // Panel options.
            UISpacers.AddTitleSpacer(this, Margin, currentY, headerWidth, Translations.Translate("ABLC_OPT_PNL"));
            currentY += TitleMargin;

            UICheckBox onRightCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_RT"), checkLabelWidth);
            onRightCheck.isChecked = ModSettings.onRight;
            onRightCheck.eventCheckChanged += (control, value) => { ModSettings.onRight = value; };
            currentY += onRightCheck.height + Margin;

            UICheckBox showPanelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_SHO"), checkLabelWidth);
            showPanelCheck.isChecked = ModSettings.showPanel;
            showPanelCheck.eventCheckChanged += (control, value) => { ModSettings.showPanel = value; };
            currentY += showPanelCheck.height + GroupMargin;

            // Gameplay options.
            UISpacers.AddTitleSpacer(this, Margin, currentY, headerWidth, Translations.Translate("ABLC_OPT_PLY"));
            currentY += TitleMargin;

            UICheckBox abandonHistCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_HNA"), checkLabelWidth);
            abandonHistCheck.isChecked = ModSettings.noAbandonHistorical;
            abandonHistCheck.eventCheckChanged += (control, value) => { ModSettings.noAbandonHistorical = value; };
            currentY += abandonHistCheck.height + Margin;

            UICheckBox abandonAnyCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_ANA"), checkLabelWidth);
            abandonAnyCheck.isChecked = ModSettings.noAbandonAny;
            abandonAnyCheck.eventCheckChanged += (control, value) => { ModSettings.noAbandonAny = value; };
            currentY += abandonAnyCheck.height + Margin;

            UICheckBox randomLevelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_RND"), checkLabelWidth);
            randomLevelCheck.isChecked = ModSettings.randomLevels;
            randomLevelCheck.eventCheckChanged += (control, value) => { ModSettings.randomLevels = value; };
            currentY += randomLevelCheck.height + Margin;

            UICheckBox loadLevelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_CLL"), checkLabelWidth);
            loadLevelCheck.isChecked = ModSettings.loadLevelCheck;
            loadLevelCheck.eventCheckChanged += (control, value) => { ModSettings.loadLevelCheck = value; };
            currentY += loadLevelCheck.height + Margin;
        }
    }
}