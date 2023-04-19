// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// The mod's settings options panel.
    /// </summary>
    public class OptionsPanel : OptionsPanelBase
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float TitleMargin = 50f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;

        /// <summary>
        /// Performs on-demand panel setup.
        /// </summary>
        protected override void Setup()
        {
            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);
            float checkLabelWidth = headerWidth - 40f;

            // Y position indicator.
            float currentY = Margin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(this, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (c, index) =>
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
            onRightCheck.isChecked = ModSettings.OnRight;
            onRightCheck.eventCheckChanged += (c, value) => { ModSettings.OnRight = value; };
            currentY += onRightCheck.height + Margin;

            UICheckBox showPanelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_SHO"), checkLabelWidth);
            showPanelCheck.isChecked = ModSettings.ShowPanel;
            showPanelCheck.eventCheckChanged += (c, value) => { ModSettings.ShowPanel = value; };
            currentY += showPanelCheck.height + GroupMargin;

            // Gameplay options.
            UISpacers.AddTitleSpacer(this, Margin, currentY, headerWidth, Translations.Translate("ABLC_OPT_PLY"));
            currentY += TitleMargin;

            UICheckBox abandonHistCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_HNA"), checkLabelWidth);
            abandonHistCheck.isChecked = ModSettings.NoAbandonHistorical;
            abandonHistCheck.eventCheckChanged += (c, value) => { ModSettings.NoAbandonHistorical = value; };
            currentY += abandonHistCheck.height + Margin;

            UICheckBox abandonAnyCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_ANA"), checkLabelWidth);
            abandonAnyCheck.isChecked = ModSettings.NoAbandonAny;
            abandonAnyCheck.eventCheckChanged += (c, value) => { ModSettings.NoAbandonAny = value; };
            currentY += abandonAnyCheck.height + Margin;

            UICheckBox randomLevelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_RND"), checkLabelWidth);
            randomLevelCheck.isChecked = ModSettings.RandomLevels;
            randomLevelCheck.eventCheckChanged += (c, value) => { ModSettings.RandomLevels = value; };
            currentY += randomLevelCheck.height + Margin;

            UICheckBox trulyRandomCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_TRN"), checkLabelWidth);
            trulyRandomCheck.isChecked = LevelUtils.TrulyRandom;
            trulyRandomCheck.eventCheckChanged += (c, value) => { LevelUtils.TrulyRandom = value; };
            currentY += trulyRandomCheck.height + Margin;

            UICheckBox upgradeWithoutTargetCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_UWT"), checkLabelWidth);
            upgradeWithoutTargetCheck.isChecked = ModSettings.UpgradeWithoutTarget;
            upgradeWithoutTargetCheck.eventCheckChanged += (c, value) => { ModSettings.UpgradeWithoutTarget = value; };
            currentY += upgradeWithoutTargetCheck.height + Margin;

            UICheckBox loadLevelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_CLL"), checkLabelWidth);
            loadLevelCheck.isChecked = ModSettings.LoadLevelCheck;
            loadLevelCheck.eventCheckChanged += (c, value) => { ModSettings.LoadLevelCheck = value; };
            currentY += loadLevelCheck.height + GroupMargin;

            // Logging checkbox.
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
        }
    }
}