// <copyright file="DistrictPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;

    /// <summary>
    /// ABLC district settings info panel.
    /// </summary>
    internal class DistrictPanel : ABLCPanel
    {
        // Panel components.
        private readonly UIDropDown _minWorkLevelDropDown;
        private readonly UIDropDown _maxWorkLevelDropDown;
        private readonly UICheckBox _randomSpawnCheck;
        private readonly UICheckBox _spawnHistoricalCheck;
        private readonly UIButton _makeHistoricalButton;
        private readonly UIButton _removeHistoricalButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistrictPanel"/> class.
        /// </summary>
        internal DistrictPanel()
        {
            // Add category labels.
            UILabels.AddLabel(this, Margin, 50f, Translations.Translate("ABLC_CAT_RES"), this.width, 0.8f, UIHorizontalAlignment.Left);
            UILabels.AddLabel(this, Margin, 140f, Translations.Translate("ABLC_CAT_WRK"), this.width, 0.8f, UIHorizontalAlignment.Left);

            // Add workplace min and max dropdowns.
            _minWorkLevelDropDown = UIDropDowns.AddLabelledDropDown(this, width - Margin - MenuWidth, 160f, Translations.Translate("ABLC_LVL_MIN"), 60f, accomodateLabel: false, tooltip: Translations.Translate("ABLC_CAT_WMN_TIP"));
            _minWorkLevelDropDown.items = new string[] { "1", "2", "3" };

            _maxWorkLevelDropDown = UIDropDowns.AddLabelledDropDown(this, width - Margin - MenuWidth, 190f, Translations.Translate("ABLC_LVL_MAX"), 60f, accomodateLabel: false, tooltip: Translations.Translate("ABLC_CAT_WMX_TIP"));
            _maxWorkLevelDropDown.items = new string[] { "1", "2", "3" };

            // Add random level checkbox.
            _randomSpawnCheck = UICheckBoxes.AddLabelledCheckBox(this, 20f, 235f, Translations.Translate("ABLC_RAN_SPN"), tooltip: Translations.Translate("ABLC_RAN_SPN_TIP"));

            // Extend height to fit 'clear all building settings' button and historical settings section.
            height += 200f;

            // Button to clear all building settings in district.
            UIButton clearBuildingsButton = UIButtons.AddButton(this, Margin, height - 200f, Translations.Translate("ABLC_CLR_BLD"), this.width - (Margin * 2), tooltip: Translations.Translate("ABLC_CLR_BLD_TIP"));
            clearBuildingsButton.eventClicked += ClearBuildings;

            // Spacer panel.
            UISpacers.AddOptionsSpacer(this, Margin, height - 155f, width - (Margin * 2));

            // Add historical section label.
            UILabels.AddLabel(this, Margin, height - 140f, Translations.Translate("ABLC_HIS"), this.width, 1.0f, UIHorizontalAlignment.Center);

            // Add historical spawning checkbox.
            _spawnHistoricalCheck = UICheckBoxes.AddLabelledCheckBox(this, 20f, height - 110f, Translations.Translate("ABLC_HIS_SPN"), tooltip: Translations.Translate("ABLC_HIS_SPN_TIP"));

            // Button to make all buildings in district historical.
            _makeHistoricalButton = UIButtons.AddButton(this, Margin, height - 80f, Translations.Translate("ABLC_TRIG_HIS"), this.width - (Margin * 2), tooltip: Translations.Translate("ABLC_TRIG_HIS_TIP"));
            _makeHistoricalButton.eventClicked += MakeHistorical;

            // Button to remove historical status from all buildings in district.
            _removeHistoricalButton = UIButtons.AddButton(this, Margin, height - 40f, Translations.Translate("ABLC_TRIG_NHS"), this.width - (Margin * 2), tooltip: Translations.Translate("ABLC_TRIG_NHS_TIP"));
            _removeHistoricalButton.eventClicked += MakeHistorical;

            // Set initial district.
            DistrictChanged();

            // Add event handlers.
            m_minLevelDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Set new minimum residential level.
                    Districts.SetDistrictMin(m_targetID, true, (byte)index);

                    // If the minimum level is now greater than the maximum level, increase the maximum to match the minimum.
                    if (index > m_maxLevelDropDown.selectedIndex)
                    {
                        m_maxLevelDropDown.selectedIndex = index;
                    }
                }
            };

            m_maxLevelDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Set new maximum residential level.
                    Districts.SetDistrictMax(m_targetID, true, (byte)index);

                    // If the maximum level is now less than the minimum level, reduce the minimum to match the maximum.
                    if (index < m_minLevelDropDown.selectedIndex)
                    {
                        m_minLevelDropDown.selectedIndex = index;
                    }
                }
            };

            _minWorkLevelDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Set new minimum workplace level.
                    Districts.SetDistrictMin(m_targetID, false, (byte)index);

                    // If the minimum level is now greater than the maximum level, increase the maximum to match the minimum.
                    if (index > _maxWorkLevelDropDown.selectedIndex)
                    {
                        _maxWorkLevelDropDown.selectedIndex = index;
                    }
                }
            };

            _maxWorkLevelDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Set new maximum workplace level.
                    Districts.SetDistrictMax(m_targetID, false, (byte)index);

                    // If the maximum level is now less than the minimum level, reduce the minimum to match the maximum.
                    if (index < _minWorkLevelDropDown.selectedIndex)
                    {
                        _minWorkLevelDropDown.selectedIndex = index;
                    }
                }
            };

            _randomSpawnCheck.eventCheckChanged += (control, isChecked) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Set/clear relevant flag.
                    Districts.SetFlag(m_targetID, (byte)Districts.DistrictFlags.RandomSpawnLevels, isChecked);
                }
            };

            _spawnHistoricalCheck.eventCheckChanged += (control, isChecked) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Set/clear relevant flag.
                    Districts.SetFlag(m_targetID, (byte)Districts.DistrictFlags.SpawnHistorical, isChecked);
                }
            };

            m_upgradeButton.eventClicked += (control, clickEvent) =>
            {
                Singleton<SimulationManager>.instance.AddAction(() =>
                    LevelDistrict(m_targetID, true));
            };

            m_downgradeButton.eventClicked += (control, clickEvent) =>
            {
                Singleton<SimulationManager>.instance.AddAction(() =>
                    LevelDistrict(m_targetID, false));
            };
        }

        /// <summary>
        /// Gets the minimum level dropdown tooltip.
        /// </summary>
        protected override string MinLevelTip => Translations.Translate("ABLC_CAT_RMN_TIP");

        /// <summary>
        /// Gets the maximum level dropdown tooltip.
        /// </summary>
        protected override string MaxLevelTip => Translations.Translate("ABLC_CAT_RMX_TIP");

        /// <summary>
        /// Gets the upgrade button tooltip.
        /// </summary>
        protected override string UpgradeTip => Translations.Translate("ABLC_TRIG_UPD_TIP");

        /// <summary>
        /// Gets the downgrade button tooltip.
        /// </summary>
        protected override string DowngradeTip => Translations.Translate("ABLC_TRIG_DWD_TIP");

        /// <summary>
        /// Called when the selected district has changed.
        /// </summary>
        public void DistrictChanged()
        {
            // Disable events while we make changes to avoid triggering event handler.
            m_disableEvents = true;

            // Update selected district ID.
            m_targetID = WorldInfoPanel.GetCurrentInstanceID().District;

            // Set name.
            m_nameLabel.text = Singleton<DistrictManager>.instance.GetDistrictName(m_targetID);

            // Set min and max levels.
            m_minLevelDropDown.selectedIndex = Districts.GetDistrictMin(m_targetID, true);
            m_maxLevelDropDown.selectedIndex = Districts.GetDistrictMax(m_targetID, true);
            _minWorkLevelDropDown.selectedIndex = Districts.GetDistrictMin(m_targetID, false);
            _maxWorkLevelDropDown.selectedIndex = Districts.GetDistrictMax(m_targetID, false);

            // Set check state based on district flags.
            _randomSpawnCheck.isChecked = Districts.GetFlag(m_targetID, (byte)Districts.DistrictFlags.RandomSpawnLevels);
            _spawnHistoricalCheck.isChecked = Districts.GetFlag(m_targetID, (byte)Districts.DistrictFlags.SpawnHistorical);

            // All done: re-enable events.
            m_disableEvents = false;
        }

        /// <summary>
        /// Clear building settings button event handler.
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        /// </summary>
        private void ClearBuildings(UIComponent c, UIMouseEventParameter p) => Buildings.ClearDistrict(m_targetID);

        /// <summary>
        /// Clear building settings button event handler.
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        /// </summary>
        private void MakeHistorical(UIComponent c, UIMouseEventParameter p)
        {
            // Store copy of current target district ID for simulation thread action.
            ushort districtID = m_targetID;

            // Set historical target via determining calling component.
            bool setHistorical = c == _makeHistoricalButton;

            // Add 'set historical' action to simulation thread.
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                // Local references.
                Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                DistrictManager districtManager = Singleton<DistrictManager>.instance;

                // Iterate through all buildings in building manager.
                for (ushort i = 0; i < buildingBuffer.Length; ++i)
                {
                    // Check to see if this building exists and has a valid AI.
                    if (buildingBuffer[i].m_flags != Building.Flags.None && buildingBuffer[i].Info?.GetAI() is BuildingAI buildingAI)
                    {
                        // Check to see if this building is in the targeted district.
                        if (districtManager.GetDistrict(buildingBuffer[i].m_position) == districtID)
                        {
                            // Building is within district - set it according to required historical state.
                            buildingAI.SetHistorical(i, ref buildingBuffer[i], setHistorical);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Attempts to force all buildings in a district to meet the district's minimum or maximum target level, as specified.
        /// </summary>
        /// <param name="districtID">Target district.</param>
        /// <param name="upgrade">True if we want to upgrade all buildings (below the minimum) to the minimum for this district, false if we want to downgrade (all buildings above the maximum) to the maximum.</param>
        private void LevelDistrict(ushort districtID, bool upgrade)
        {
            // Instances and arrays.
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            SimulationManager simulationManager = Singleton<SimulationManager>.instance;

            // Pause simulation.
            bool originalForcedPause = simulationManager.ForcedSimulationPaused;
            simulationManager.ForcedSimulationPaused = true;

            // Iterate through all buildings in map.
            for (ushort i = 0; i < buildings.m_size; ++i)
            {
                // Get local reference.
                Building thisBuilding = buildings.m_buffer[i];

                // Skip non-existent,abandoned, burned down, or collapsed buildings, or non-Private AI buildings.
                if ((thisBuilding.m_flags & Building.Flags.Created) != 0 && (thisBuilding.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Collapsed)) == 0 && thisBuilding.Info?.GetAI() is PrivateBuildingAI)
                {
                    // Building exists; get its district and see if it matches the target district.
                    if (districtManager.GetDistrict(thisBuilding.m_position) == districtID)
                    {
                        // It's in our district; get service.
                        bool isResidential = false;
                        ItemClass.Service service = thisBuilding.Info.GetService();

                        // Check service type.
                        switch (service)
                        {
                            case ItemClass.Service.Residential:
                                // Residential; set residential flag.
                                isResidential = true;
                                break;
                            case ItemClass.Service.Industrial:
                            case ItemClass.Service.Commercial:
                            case ItemClass.Service.Office:
                                break;
                            default:
                                // Ignore buildings with services other than those specified above.
                                continue;
                        }

                        // Are we upgrading or downgrading?
                        if (upgrade)
                        {
                            // Upgrading - get the minimum level for this building.
                            byte minLevel = Buildings.GetMinLevel(i, isResidential);

                            // Check if building level is less than the relevant minimum.
                            if (buildings.m_buffer[i].m_level < minLevel)
                            {
                                // It needs to be upgraded; store copy of current index and minimum level for action queue.
                                ushort buildingID = i;
                                byte thisMinLevel = minLevel;

                                // Upgrade.
                                LevelUtils.ForceLevel(buildingID, thisMinLevel);
                            }
                        }
                        else
                        {
                            // Downgrading - get the maximum level for this building..
                            byte maxLevel = Buildings.GetMaxLevel(i, isResidential);

                            // Check if building level is greater than the relevant maximum.
                            if (buildings.m_buffer[i].m_level > maxLevel)
                            {
                                // It needs to be downgraded; store copy of current index and maximum level for action queue.
                                ushort buildingID = i;
                                byte thisMaxLevel = maxLevel;

                                // Downgrade.
                                LevelUtils.ForceLevel(buildingID, thisMaxLevel);
                            }
                        }
                    }
                }
            }

            // Resume simulation.
            simulationManager.ForcedSimulationPaused = originalForcedPause;
        }
    }
}