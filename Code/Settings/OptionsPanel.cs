// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
    public class OptionsPanel : UIPanel
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
            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);
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
            onRightCheck.isChecked = ModSettings.OnRight;
            onRightCheck.eventCheckChanged += (control, value) => { ModSettings.OnRight = value; };
            currentY += onRightCheck.height + Margin;

            UICheckBox showPanelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_SHO"), checkLabelWidth);
            showPanelCheck.isChecked = ModSettings.ShowPanel;
            showPanelCheck.eventCheckChanged += (control, value) => { ModSettings.ShowPanel = value; };
            currentY += showPanelCheck.height + GroupMargin;

            // Gameplay options.
            UISpacers.AddTitleSpacer(this, Margin, currentY, headerWidth, Translations.Translate("ABLC_OPT_PLY"));
            currentY += TitleMargin;

            UICheckBox abandonHistCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_HNA"), checkLabelWidth);
            abandonHistCheck.isChecked = ModSettings.NoAbandonHistorical;
            abandonHistCheck.eventCheckChanged += (control, value) => { ModSettings.NoAbandonHistorical = value; };
            currentY += abandonHistCheck.height + Margin;

            UICheckBox abandonAnyCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_ANA"), checkLabelWidth);
            abandonAnyCheck.isChecked = ModSettings.NoAbandonAny;
            abandonAnyCheck.eventCheckChanged += (control, value) => { ModSettings.NoAbandonAny = value; };
            currentY += abandonAnyCheck.height + Margin;

            UICheckBox randomLevelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_RND"), checkLabelWidth);
            randomLevelCheck.isChecked = ModSettings.RandomLevels;
            randomLevelCheck.eventCheckChanged += (control, value) => { ModSettings.RandomLevels = value; };
            currentY += randomLevelCheck.height + Margin;

            UICheckBox loadLevelCheck = UICheckBoxes.AddPlainCheckBox(this, Margin, currentY, Translations.Translate("ABLC_OPT_CLL"), checkLabelWidth);
            loadLevelCheck.isChecked = ModSettings.LoadLevelCheck;
            loadLevelCheck.eventCheckChanged += (control, value) => { ModSettings.LoadLevelCheck = value; };
            currentY += loadLevelCheck.height + Margin;
        }
    }
}