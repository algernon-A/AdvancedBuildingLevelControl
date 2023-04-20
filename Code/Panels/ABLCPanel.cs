﻿// <copyright file="ABLCPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// ABLC info panel base class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Protected fields")]
    internal class ABLCPanel : UIPanel
    {
        /// <summary>
        /// Layout margin.
        /// </summary>
        protected const float Margin = 5f;

        /// <summary>
        /// Menu width.
        /// </summary>
        protected const float MenuWidth = 60f;

        /// <summary>
        /// Minimum level dropdown.
        /// </summary>
        protected UIDropDown m_minLevelDropDown;

        /// <summary>
        /// Maximum level dropdown.
        /// </summary>
        protected UIDropDown m_maxLevelDropDown;

        /// <summary>
        /// Upgrade button.
        /// </summary>
        protected UIButton m_upgradeButton;

        /// <summary>
        /// Downgrade button.
        /// </summary>
        protected UIButton m_downgradeButton;

        /// <summary>
        /// Target ID.
        /// </summary>
        protected ushort m_targetID;

        /// <summary>
        /// Disables event handling if set.
        /// </summary>
        protected bool m_disableEvents = false;

        /// <summary>
        /// Panel name label.
        /// </summary>
        protected UILabel m_nameLabel;

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Basic setup.
            autoLayout = false;
            backgroundSprite = "MenuPanel2";
            opacity = 0.95f;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            size = new Vector2(220f, PanelHeight);

            // Decorative icon (top-left).
            UISprite iconSprite = AddUIComponent<UISprite>();
            iconSprite.relativePosition = new Vector2(0f, 0f);
            iconSprite.height = 36f;
            iconSprite.width = 36f;
            iconSprite.atlas = UITextures.LoadQuadSpriteAtlas("ablc_buttons");
            iconSprite.spriteName = "normal";

            // Category labels.
            m_nameLabel = UILabels.AddLabel(this, 0f, 27f, string.Empty, this.width, 0.6f, UIHorizontalAlignment.Center);
            UILabels.AddLabel(this, 0f, Margin, Translations.Translate("ABLC_SHORT"), this.width, 1.0f, UIHorizontalAlignment.Center);

            // Level dropdowns.
            m_minLevelDropDown = UIDropDowns.AddLabelledDropDown(this, width - Margin - MenuWidth, 70f, Translations.Translate("ABLC_LVL_MIN"), 60f, accomodateLabel: false, tooltip: MinLevelTip);
            m_minLevelDropDown.items = new string[] { "1", "2", "3", "4", "5" };

            m_maxLevelDropDown = UIDropDowns.AddLabelledDropDown(this, width - Margin - MenuWidth, 100f, Translations.Translate("ABLC_LVL_MAX"), 60f, accomodateLabel: false, tooltip: MaxLevelTip);
            m_maxLevelDropDown.items = new string[] { "1", "2", "3", "4", "5" };

            // Upgrade button.
            m_upgradeButton = UIButtons.AddButton(this, Margin, PanelHeight - 80f, Translations.Translate("ABLC_TRIG_UP"), this.width - (Margin * 2), tooltip: UpgradeTip);

            // Downgrade button.
            m_downgradeButton = UIButtons.AddButton(this, Margin, PanelHeight - 40f, Translations.Translate("ABLC_TRIG_DWN"), this.width - (Margin * 2), tooltip: DowngradeTip);
        }

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        protected virtual float PanelHeight => 350f;

        /// <summary>
        /// Gets the minimum level dropdown tooltip.
        /// </summary>
        protected virtual string MinLevelTip => Translations.Translate("ABLC_LVL_MIN_TIP");

        /// <summary>
        /// Gets the maximum level dropdown tooltip.
        /// </summary>
        protected virtual string MaxLevelTip => Translations.Translate("ABLC_LVL_MAX_TIP");

        /// <summary>
        /// Gets the upgrade button tooltip.
        /// </summary>
        protected virtual string UpgradeTip => Translations.Translate("ABLC_TRIG_UPB_TIP");

        /// <summary>
        /// Gets the downgrade button tooltip.
        /// </summary>
        protected virtual string DowngradeTip => Translations.Translate("ABLC_TRIG_DWB_TIP");
    }
}