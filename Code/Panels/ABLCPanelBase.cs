﻿using System;
using UnityEngine;
using ColossalFramework.UI;


namespace ABLC
{
    /// <summary>
    /// ABLC info panel base class.
    /// </summary>
    internal class ABLCPanel : UIPanel
    {
        // Constants.
        protected const float margin = 5.0f;
        protected virtual float PanelHeight => 360f;

        // Reference variables.
        protected ushort targetID;

        // Event toggler.
        protected bool disableEvents = false;

        // Panel components.
        protected UILabel nameLabel;
        protected UIButton closeButton;
        protected UIDropDown minLevelDropDown;
        protected UIDropDown maxLevelDropDown;
        protected UIButton upgradeButton;
        protected UIButton downgradeButton;


        /// <summary>
        /// Performs initial setup for the panel; we don't use Start() as that's not sufficiently reliable (race conditions), and is not needed with the dynamic create/destroy process.
        /// </summary>
        internal virtual void Setup(Transform parentTransform)
        {
            try
            {
                // Hide while we're setting up.
                isVisible = false;

                // Basic setup.
                autoLayout = false;
                backgroundSprite = "MenuPanel2";
                opacity = 0.8f;
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                size = new Vector3(220f, PanelHeight);

                // Set parent transform to game's district info panel.
                transform.parent = parentTransform;

                // Set position according to setting.
                if (ModSettings.onRight)
                {
                    // On right of info panel.
                    relativePosition = new Vector2(parent.width + 10f, 0f);
                }
                else
                {
                    // On left of info panel.
                    relativePosition = new Vector2(-(width + 10f), 0f);
                }

                // Category labels
                nameLabel = AddLabel("", 0f, 25f);
                UILabel titleLabel = AddLabel(Translations.Translate("ABLC_NAME"), 0f, margin, 1.0f);

                // Level dropdowns.
                minLevelDropDown = UIUtils.CreateDropDown(this, Translations.Translate("ABLC_LVL_MIN"), yPos : 70f);
                minLevelDropDown.items = new string[] { "1", "2", "3", "4", "5" };

                maxLevelDropDown = UIUtils.CreateDropDown(this, Translations.Translate("ABLC_LVL_MAX"), yPos : 100f);
                maxLevelDropDown.items = new string[] { "1", "2", "3", "4", "5" };

                // Apply button.
                upgradeButton = UIUtils.CreateButton(this, Translations.Translate("ABLC_TRIG_UP"), width: this.width - (margin * 2), xPos: margin, yPos: PanelHeight - 80f);

                // Add 'downgrade' button.
                downgradeButton = UIUtils.CreateButton(this, Translations.Translate("ABLC_TRIG_DWN"), width: this.width - (margin * 2), xPos: margin, yPos: PanelHeight - 40f);

                // Close button.
                closeButton = AddUIComponent<UIButton>();
                closeButton.relativePosition = new Vector3(width - 35, 2);
                closeButton.normalBgSprite = "buttonclose";
                closeButton.hoveredBgSprite = "buttonclosehover";
                closeButton.pressedBgSprite = "buttonclosepressed";
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception setting up panel base");
            }
        }


        /// <summary>
        /// Adds a UI text label to the current panel.
        /// </summary>
        /// <param name="text">Label text</param>
        /// <param name="yPos">Relative Y position</param>
        /// <param name="scale">Text scale (default 0.8f)</param>
        /// <returns></returns>
        protected UILabel AddLabel(string text, float xPos, float yPos, float scale = 0.8f, UIHorizontalAlignment hAlign = UIHorizontalAlignment.Center)
        {
            UILabel newLabel = AddUIComponent<UILabel>();

            newLabel.autoSize = false;
            newLabel.textAlignment = hAlign;
            newLabel.size = new Vector2(this.width - xPos, scale * 30f);
            newLabel.textScale = scale;
            newLabel.text = text;
            newLabel.relativePosition = new Vector3(xPos, yPos);

            return newLabel;
        }
    }
}