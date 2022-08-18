namespace ABLC
{
    using System;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Static class to manage the ABLC building panel.
    /// </summary>
    internal static class BuildingPanelManager
    {
        // Instance references.
        private static GameObject uiGameObject;
        private static ABLCBuildingPanel panel;
        internal static ABLCBuildingPanel Panel => panel;

        // UI components.
        internal static UIButton panelButton;


        /// <summary>
        /// Adds event handler to show/hide building panel as appropriate (in line with ZonedBuildingWorldInfoPanel).
        /// </summary>
        internal static void Hook()
        {
            // Get building info panel instance.
            UIComponent buildingInfoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name)?.component;
            if (buildingInfoPanel == null)
            {
                Logging.Error("couldn't hook building info panel");
            }
            else
            {
                // Toggle button and/or panel visibility when game building info panel visibility changes.
                buildingInfoPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    // Create / destroy our panel as and when the info panel is shown or hidden.
                    if (isVisible)
                    {
                        if (ModSettings.showPanel)
                        {
                            Create();
                        }
                    }
                    else
                    {
                        Close();
                    }
                };
            }
        }


        /// <summary>
        /// Handles a change in target building from the WorldInfoPanel.
        /// Sets the panel button state according to whether or not this building is 'levellable' and communicates changes to the ABLC panel.
        /// </summary>
        internal static void TargetChanged()
        {
            // Get current WorldInfoPanel building instance and determine maximum building level.
            if (LevelUtils.GetMaxLevel(WorldInfoPanel.GetCurrentInstanceID().Building) == 1)
            {
                // Only one building level - not a 'levellable' building, so disable the ABLC button and update the tooltip accordingly.
                panelButton.Disable();
                panelButton.tooltip = Translations.Translate("ABLC_BUT_DIS");
            }
            else
            {
                // Multiple levels available - enable the ABLC button and update the tooltip accordingly.
                panelButton.Enable();
                panelButton.tooltip = Translations.Translate("ABLC_NAME");
            }

            // Communicate target change to the panel (if it's currently instantiated).
            Panel?.BuildingChanged();
        }


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Create()
        {
            try
            {
                // If no instance already set, create one.
                if (uiGameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    uiGameObject = new GameObject("ABLCBuildingPanel");
                    uiGameObject.transform.parent = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name)?.component.transform;

                    panel = uiGameObject.AddComponent<ABLCBuildingPanel>();

                    // Set up and show panel.
                    Panel.transform.parent = uiGameObject.transform.parent;
                    Panel.Setup();
                    Panel.Show();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating ABLCBuildingPanel");
            }
        }


        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            GameObject.Destroy(panel);
            GameObject.Destroy(uiGameObject);

            panel = null;
            uiGameObject = null;
        }


        /// <summary>
        /// Adds an ABLC button to a building info panel to open the ABLC panel for that building.
        /// The button will be added to the right of the panel with a small margin from the panel edge, at the relative Y position specified.
        /// </summary>
        internal static void AddInfoPanelButton()
        {
            const float PanelButtonSize = 36f;

            BuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);
            panelButton = infoPanel.component.AddUIComponent<UIButton>();

            // Basic button setup.
            panelButton.atlas = UITextures.LoadQuadSpriteAtlas("ablc_buttons");
            panelButton.size = new Vector2(PanelButtonSize, PanelButtonSize);
            panelButton.normalFgSprite = "normal";
            panelButton.focusedFgSprite = "hovered";
            panelButton.hoveredFgSprite = "hovered";
            panelButton.pressedFgSprite = "pressed";
            panelButton.disabledFgSprite = "disabled";
            panelButton.name = "ABLCbutton";
            panelButton.tooltip = Translations.Translate("ABLC_NAME");

            // Find ProblemsPanel relative position to position button.
            // We'll use 40f as a default relative Y in case something doesn't work.
            UIComponent problemsPanel;
            float relativeY = 40f;

            // Player info panels have wrappers, zoned ones don't.
            UIComponent wrapper = infoPanel.Find("Wrapper");
            if (wrapper == null)
            {
                problemsPanel = infoPanel.Find("ProblemsPanel");
            }
            else
            {
                problemsPanel = wrapper.Find("ProblemsPanel");
            }

            try
            {
                // Position button vertically in the middle of the problems panel.  If wrapper panel exists, we need to add its offset as well.
                relativeY = (wrapper == null ? 0 : wrapper.relativePosition.y) + problemsPanel.relativePosition.y + ((problemsPanel.height - PanelButtonSize) / 2f);
            }
            catch
            {
                // Don't really care; just use default relative Y.
                Logging.Message("couldn't find ProblemsPanel relative position");
            }

            // Set position.
            panelButton.AlignTo(infoPanel.component, UIAlignAnchor.TopLeft);
            panelButton.relativePosition += new Vector3(infoPanel.component.width - 62f - PanelButtonSize, relativeY, 0f);

            // Event handler.
            panelButton.eventClick += (control, clickEvent) =>
            {
                // Toggle panel visibility.
                if (uiGameObject == null)
                {
                    Create();
                }
                else
                {
                    Close();
                }

                // Manually unfocus control, otherwise it can stay focused until next UI event (looks untidy).
                control.Unfocus();
            };
        }
    }
}