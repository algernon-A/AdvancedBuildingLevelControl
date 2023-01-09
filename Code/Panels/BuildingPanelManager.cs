// <copyright file="BuildingPanelManager.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
        private static GameObject s_gameObject;
        private static BuildingPanel s_panel;

        // UI components.
        private static UIButton s_panelButton;

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        internal static BuildingPanel Panel => s_panel;

        /// <summary>
        /// Gets the panel button instance.
        /// </summary>
        internal static UIButton PanelButton => s_panelButton;

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
                buildingInfoPanel.eventVisibilityChanged += (c, isVisible) =>
                {
                    // Create / destroy our panel as and when the info panel is shown or hidden.
                    if (isVisible)
                    {
                        if (ModSettings.ShowPanel)
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
                s_panelButton.Disable();
                s_panelButton.tooltip = Translations.Translate("ABLC_BUT_DIS");
            }
            else
            {
                // Multiple levels available - enable the ABLC button and update the tooltip accordingly.
                s_panelButton.Enable();
                s_panelButton.tooltip = Translations.Translate("ABLC_NAME");
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
                if (s_gameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    s_gameObject = new GameObject("ABLCBuildingPanel");
                    s_gameObject.transform.parent = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name)?.component.transform;

                    s_panel = s_gameObject.AddComponent<BuildingPanel>();

                    // Set up and show panel.
                    s_panel.transform.parent = s_gameObject.transform.parent;

                    // Set position according to setting.
                    if (ModSettings.OnRight)
                    {
                        // On right of info panel.
                        s_panel.relativePosition = new Vector2(s_panel.parent.width + 10f, 0f);
                    }
                    else
                    {
                        // On left of info panel.
                        s_panel.relativePosition = new Vector2(-(s_panel.width + 10f), 0f);
                    }

                    Panel.Show();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating BuildingPanel");
            }
        }

        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            GameObject.Destroy(s_panel);
            GameObject.Destroy(s_gameObject);

            s_panel = null;
            s_gameObject = null;
        }

        /// <summary>
        /// Adds an ABLC button to a building info panel to open the ABLC panel for that building.
        /// The button will be added to the right of the panel with a small margin from the panel edge, at the relative Y position specified.
        /// </summary>
        internal static void AddInfoPanelButton()
        {
            const float PanelButtonSize = 36f;

            BuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);
            s_panelButton = infoPanel.component.AddUIComponent<UIButton>();

            // Basic button setup.
            s_panelButton.atlas = UITextures.LoadQuadSpriteAtlas("ablc_buttons");
            s_panelButton.size = new Vector2(PanelButtonSize, PanelButtonSize);
            s_panelButton.normalFgSprite = "normal";
            s_panelButton.focusedFgSprite = "hovered";
            s_panelButton.hoveredFgSprite = "hovered";
            s_panelButton.pressedFgSprite = "pressed";
            s_panelButton.disabledFgSprite = "disabled";
            s_panelButton.name = "ABLCbutton";
            s_panelButton.tooltip = Translations.Translate("ABLC_NAME");

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
            s_panelButton.AlignTo(infoPanel.component, UIAlignAnchor.TopLeft);
            s_panelButton.relativePosition += new Vector3(infoPanel.component.width - 62f - PanelButtonSize, relativeY, 0f);

            // Event handler.
            s_panelButton.eventClick += (c, p) =>
            {
                // Toggle panel visibility.
                if (s_gameObject == null)
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