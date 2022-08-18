namespace ABLC
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// ABLC building settings info panel.
    /// </summary>
    internal class ABLCBuildingPanel : ABLCPanel
    {
        // Constants.
        protected override float PanelHeight => 220f;

        // Upgrade and downgrade target levels.
        protected byte upgradeLevel, downgradeLevel;

        // Update status flag.
        internal bool updateReady = false;


        /// <summary>
        /// Called by Unity every tick.  Used here to check to see if we need to update the panel after a building has been updated via the simulation thread.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // Check to see if an update is ready; if so, refresh the panel and clear the flag.
            if (updateReady)
            {
                UpdatePanel();
                updateReady = false;
            }
        }

        /// <summary>
        /// Performs initial setup for the panel.
        /// </summary>
        internal override void Setup()
        {
            try
            {
                base.Setup();

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
                    Logging.KeyMessage("upgrading building to level ", upgradeLevel);
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        LevelUtils.ForceLevel(targetID, targetLevel);
                    });

                    // Check to see if we should increase this buildings maximum level.
                    if (BuildingsABLC.GetMaxLevel(targetID) < upgradeLevel)
                    {
                        maxLevelDropDown.selectedIndex = upgradeLevel;
                    }
                };

                downgradeButton.eventClick += (control, clickEvent) =>
                {

                    // Local references for SimulationManager action.
                    ushort buildingID = targetID;
                    byte targetLevel = downgradeLevel;
                    Logging.KeyMessage("downgrading building to level ", downgradeLevel);
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        LevelUtils.ForceLevel(targetID, targetLevel);
                    });

                    // Check to see if we should increase this buildings maximum level.
                    if (BuildingsABLC.GetMinLevel(targetID) > downgradeLevel)
                    {
                        minLevelDropDown.selectedIndex = downgradeLevel;
                    }
                };

                // Close button.
                UIButton closeButton = AddUIComponent<UIButton>();
                closeButton.relativePosition = new Vector2(width - 35, 2);
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