using System;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace ABLC
{
    /// <summary>
    /// Static class to manage the ABLC district panel.
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
                    Panel.Setup(uiGameObject.transform.parent);
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
            panelButton.atlas = Textures.ABLCButtonSprites;
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


    /// <summary>
    /// ABLC district settings info panel.
    /// </summary>
    internal class ABLCBuildingPanel : ABLCPanel
    {
        // Constants.
        protected override float PanelHeight => 220f;

        // Upgrade and downgrade target levels.
        protected byte upgradeLevel, downgradeLevel;


        /// <summary>
        /// Performs initial setup for the panel; we don't use Start() as that's not sufficiently reliable (race conditions), and is not needed with the dynamic create/destroy process.
        /// </summary>
        /// <param name="parentTransform">Transform to attach to</param>
        internal override void Setup(Transform parentTransform)
        {
            try
            {
                base.Setup(parentTransform);

                // Set initial building.
                BuildingChanged();

                // Add event handlers.
                minLevelDropDown.eventSelectedIndexChanged += (control, index) =>
                {
                    // Don't do anything if events are disabled.
                    if (!disableEvents)
                    {
                        // Set minimum level of building in dictionary.
                        UpdateMinLevel((byte)index);

                        // If the minimum level is now greater than the maximum level, increase the maximum to match the minimum.
                        if (index > maxLevelDropDown.selectedIndex)
                        {
                            maxLevelDropDown.selectedIndex = index;
                        }
                    }
                };

                maxLevelDropDown.eventSelectedIndexChanged += (control, index) =>
                {
                    // Don't do anything if events are disabled.
                    if (!disableEvents)
                    {
                        // Update maximum level.
                        UpdateMaxLevel((byte)index);

                        // If the maximum level is now less than the minimum level, reduce the minimum to match the maximum.
                        if (index < minLevelDropDown.selectedIndex)
                        {
                            minLevelDropDown.selectedIndex = index;
                        }
                    }
                };

                upgradeButton.eventClick += (control, clickEvent) =>
                {

                    // Local references for SimulationManager action.
                    ushort buildingID = targetID;
                    byte targetLevel = upgradeLevel;
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        LevelUtils.ForceLevel(targetID, targetLevel);
                    });

                    // Check to see if we should increase this buildings maximum level.
                    byte newLevel = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].m_level;
                    if (BuildingsABLC.GetMaxLevel(targetID) < newLevel)
                    {
                        maxLevelDropDown.selectedIndex = newLevel;
                    }

                    // Update the panel once done.
                    UpdatePanel();
                };

                downgradeButton.eventClick += (control, clickEvent) =>
                {

                    // Local references for SimulationManager action.
                    ushort buildingID = targetID;
                    byte targetLevel = downgradeLevel;
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        LevelUtils.ForceLevel(targetID, targetLevel);
                    });

                    // Check to see if we should increase this buildings maximum level.
                    byte newLevel = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].m_level;
                    if (BuildingsABLC.GetMinLevel(targetID) > newLevel)
                    {
                        minLevelDropDown.selectedIndex = newLevel;
                    }

                    // Update the panel once done.
                    UpdatePanel();
                };

                // Close button.
                UIButton closeButton = AddUIComponent<UIButton>();
                closeButton.relativePosition = new Vector3(width - 35, 2);
                closeButton.normalBgSprite = "buttonclose";
                closeButton.hoveredBgSprite = "buttonclosehover";
                closeButton.pressedBgSprite = "buttonclosepressed";

                // Close button event handler.
                closeButton.eventClick += (component, clickEvent) =>
                {
                    BuildingPanelManager.Close();
                };

            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception setting up building panel");
            }
        }


        /// <summary>
        /// Called when the selected building has changed.
        /// </summary>
        internal void BuildingChanged()
        {
            // Update selected building ID.
            targetID = WorldInfoPanel.GetCurrentInstanceID().Building;

            // Check maximum level for this building type.
            int maxLevel = LevelUtils.GetMaxLevel(targetID);

            // If building doesn't have more than one level, then we don't have any business to do here.
            if (maxLevel == 1)
            {
                BuildingPanelManager.panelButton.Disable();
                Hide();
                return;
            }
            else
            {
                // Enable info panel button.
                BuildingPanelManager.panelButton.Enable();

                // Make sure we're visible if we're not already.
                if (!isVisible)
                {
                    Show();
                }
            }

            // Disable events while we make changes to avoid triggering event handler.
            disableEvents = true;

            // Set name.
            nameLabel.text = Singleton<BuildingManager>.instance.GetBuildingName(targetID, InstanceID.Empty);

            // Build level dropdown ranges.
            minLevelDropDown.items = new string[maxLevel];
            maxLevelDropDown.items = new string[maxLevel];
            for (int i = 0; i < maxLevel; i++)
            {
                minLevelDropDown.items[i] = (i + 1).ToString();
                maxLevelDropDown.items[i] = (i + 1).ToString();
            }

            // Update dropdown selection to match building's settings.
            minLevelDropDown.selectedIndex = BuildingsABLC.GetMinLevel(targetID);
            maxLevelDropDown.selectedIndex = Mathf.Min(BuildingsABLC.GetMaxLevel(targetID), maxLevelDropDown.items.Length - 1);

            // Initialise panel with correct level settings.
            UpdatePanel();

            // All done: re-enable events.
            disableEvents = false;
        }


        /// <summary>
        /// Updates the panel according to building's current level settings.
        /// </summary>
        internal void UpdatePanel()
        {
            // Make sure we have a valid builidng first.
            if (targetID == 0 || (Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].m_flags == Building.Flags.None))
            {
                // Invalid target - disable buttons.
                upgradeButton.Disable();
                downgradeButton.Disable();
                return;
            }

            // Get building level.
            byte level = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].m_level);

            // Check to see if the building can be upgraded one level.
            upgradeLevel = (byte)(level + 1);
            if (LevelUtils.GetTargetInfo(targetID, upgradeLevel) == null)

            {
                // Nope - disable upgrade button.
                upgradeButton.Disable();
            }
            else
            {
                // Yep - enable upgrade button.
                upgradeButton.Enable();
            }
            
            // Check to see if the building can be downgraded one level.
            downgradeLevel = (byte)(level - 1);
            if (LevelUtils.GetTargetInfo(targetID, downgradeLevel) == null)
            {
                // Nope - disable downgrade button.
                downgradeButton.Disable();
            }
            else
            {
                // Yep - enable downgrade button.
                downgradeButton.Enable();
            }
        }


        /// <summary>
        /// Updates the buildng's minimum level.
        /// </summary>
        /// <param name="minLevel">New minimum level</param>
        private void UpdateMinLevel(byte minLevel)
        {
            // Don't do anything if events are disabled.
            if (!disableEvents)
            {
                // Update minimum level.
                BuildingsABLC.UpdateMinLevel(targetID, minLevel);

                // Update the panel.
                BuildingChanged();
            }
        }


        /// <summary>
        /// Updates the buildng's maximum level.
        /// </summary>
        /// <param name="maxLevel">New maximum level</param>
        private void UpdateMaxLevel(byte maxLevel)
        {
            // Don't do anything if events are disabled.
            if (!disableEvents)
            {
                // Update maximum level.
                BuildingsABLC.UpdateMaxLevel(targetID, maxLevel);

                // Update the panel.
                BuildingChanged();
            }
        }
    }
}