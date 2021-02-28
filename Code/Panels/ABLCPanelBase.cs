using System;
using UnityEngine;
using ColossalFramework.UI;


namespace ABLC
{
    /// <summary>
    /// ABLC info panel base class.
    /// </summary>
    internal class ABLCPanel : UIPanel
    {
        // Layout constants.
        protected const float Margin = 5f;
        protected const float MenuWidth = 60f;
        protected virtual float PanelHeight => 360f;

        // Reference variables.
        protected ushort targetID;

        // Event toggler.
        protected bool disableEvents = false;

        // Panel components.
        protected UILabel nameLabel;
        protected UIDropDown minLevelDropDown;
        protected UIDropDown maxLevelDropDown;
        protected UIButton upgradeButton;
        protected UIButton downgradeButton;

        // Text strings.
        protected virtual string MinLevelTip => Translations.Translate("ABLC_LVL_MIN_TIP");
        protected virtual string MaxLevelTip => Translations.Translate("ABLC_LVL_MAX_TIP");
        protected virtual string UpgradeTip => Translations.Translate("ABLC_TRIG_UPB_TIP");
        protected virtual string DowngradeTip => Translations.Translate("ABLC_TRIG_DWB_TIP");



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
                opacity = 0.95f;
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

                // Decorative icon (top-left).
                UISprite iconSprite = AddUIComponent<UISprite>();
                iconSprite.relativePosition = new Vector2(0f, 0f);
                iconSprite.height = 36f;
                iconSprite.width = 36f;
                iconSprite.atlas = Textures.ABLCButtonSprites;
                iconSprite.spriteName = "normal";

                // Category labels
                nameLabel = AddLabel("", 0f, 27f, 0.6f);
                UILabel titleLabel = AddLabel(Translations.Translate("ABLC_SHORT"), 0f, Margin, 1.0f);

                // Level dropdowns.
                minLevelDropDown = UIControls.AddLabelledDropDown(this, width - Margin - MenuWidth, 70f, Translations.Translate("ABLC_LVL_MIN"), 60f, false, MinLevelTip);
                minLevelDropDown.items = new string[] { "1", "2", "3", "4", "5" };

                maxLevelDropDown = UIControls.AddLabelledDropDown(this, width - Margin - MenuWidth, 100f, Translations.Translate("ABLC_LVL_MAX"), 60f, false, MaxLevelTip);
                maxLevelDropDown.items = new string[] { "1", "2", "3", "4", "5" };

                // Apply button.
                upgradeButton = UIControls.AddButton(this, Margin, PanelHeight - 80f, Translations.Translate("ABLC_TRIG_UP"), this.width - (Margin * 2), tooltip: UpgradeTip);

                // Add 'downgrade' button.
                downgradeButton = UIControls.AddButton(this, Margin, PanelHeight - 40f, Translations.Translate("ABLC_TRIG_DWN"), this.width - (Margin * 2), tooltip: DowngradeTip);
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