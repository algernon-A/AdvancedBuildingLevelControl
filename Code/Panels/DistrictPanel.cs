﻿using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace ABLC
{
    /// <summary>
    /// Static class to manage the ABLC district panel.
    /// </summary>
    internal static class DistrictPanelManager
    {
        // Instance references.
        private static GameObject uiGameObject;
        private static ABLCDistrictPanel _panel;
        internal static ABLCDistrictPanel Panel => _panel;


        /// <summary>
        /// Adds event handler to show/hide district panel as appropriate (in line with DistrictWorldInfoPanel).
        /// </summary>
        internal static void Hook()
        {

            UIComponent districtInfoPanel = UIView.library.Get<DistrictWorldInfoPanel>(typeof(DistrictWorldInfoPanel).Name)?.component;
            if (districtInfoPanel == null)
            {
                Logging.Error("couldn't hook district info panel");
            }
            else
            {
                districtInfoPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    if (isVisible)
                    {
                        Create(districtInfoPanel.transform);
                    }
                    else
                    {
                        Close();
                    }
                };
            }
        }


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Create(Transform parentTransform)
        {
            try
            {
                // If no instance already set, create one.
                if (uiGameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    uiGameObject = new GameObject("ABLCDistrictPanel");
                    uiGameObject.transform.parent = parentTransform;

                    _panel = uiGameObject.AddComponent<ABLCDistrictPanel>();

                    // Set up and show panel.
                    Panel.Setup(parentTransform);
                    Panel.Show();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating ABLCDistrictPanel");
            }
        }


        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            GameObject.Destroy(_panel);
            GameObject.Destroy(uiGameObject);
        }
    }


    /// <summary>
    /// ABLC district settings info panel.
    /// </summary>
    internal class ABLCDistrictPanel : ABLCPanel
    {
        // Panel components.
        protected UIDropDown minWorkLevelDropDown;
        protected UIDropDown maxWorkLevelDropDown;
        private UICheckBox randomSpawnCheck, spawnHistoricalCheck;
        UIButton makeHistoricalButton, removeHistoricalButton;

        // Text strings.
        protected override string MinLevelTip => Translations.Translate("ABLC_CAT_RMN_TIP");
        protected override string MaxLevelTip => Translations.Translate("ABLC_CAT_RMX_TIP");
        protected override string UpgradeTip => Translations.Translate("ABLC_TRIG_UPD_TIP");
        protected override string DowngradeTip => Translations.Translate("ABLC_TRIG_DWD_TIP");


        /// <summary>
        /// Called when the selected district has changed.
        /// </summary>
        public void DistrictChanged()
        {
            // Disable events while we make changes to avoid triggering event handler.
            disableEvents = true;

            // Update selected district ID> 
            targetID = WorldInfoPanel.GetCurrentInstanceID().District;

            // Set name.
            nameLabel.text = Singleton<DistrictManager>.instance.GetDistrictName(targetID);

            // Set min and max levels.
            minLevelDropDown.selectedIndex = DistrictsABLC.minResLevel[targetID];
            maxLevelDropDown.selectedIndex = DistrictsABLC.maxResLevel[targetID];
            minWorkLevelDropDown.selectedIndex = DistrictsABLC.minWorkLevel[targetID];
            maxWorkLevelDropDown.selectedIndex = DistrictsABLC.maxWorkLevel[targetID];

            // Set flags.
            randomSpawnCheck.isChecked = (DistrictsABLC.flags[targetID] & (byte)DistrictFlags.randomSpawnLevels) != 0;
            spawnHistoricalCheck.isChecked = (DistrictsABLC.flags[targetID] & (byte)DistrictFlags.spawnHistorical) != 0;

            // All done: re-enable events.
            disableEvents = false;
        }


        /// <summary>
        /// Performs initial setup for the panel; we don't use Start() as that's not sufficiently reliable (race conditions), and is not needed with the dynamic create/destroy process.
        /// </summary>
        internal override void Setup(Transform parentTransform)
        {
            try
            {
                base.Setup(parentTransform);

                // Add category labels.
                UILabel resLabel = AddLabel(Translations.Translate("ABLC_CAT_RES"), Margin, 50f, hAlign: UIHorizontalAlignment.Left);
                UILabel workLabel = AddLabel(Translations.Translate("ABLC_CAT_WRK"), Margin, 140f, hAlign: UIHorizontalAlignment.Left);

                // Add workplace min and max dropdowns.
                minWorkLevelDropDown = UIControls.AddLabelledDropDown(this, width - Margin - MenuWidth, 160f, Translations.Translate("ABLC_LVL_MIN"), 60f, accomodateLabel: false, tooltip: Translations.Translate("ABLC_CAT_WMN_TIP"));
                minWorkLevelDropDown.items = new string[] { "1", "2", "3" };

                maxWorkLevelDropDown = UIControls.AddLabelledDropDown(this, width - Margin - MenuWidth, 190f, Translations.Translate("ABLC_LVL_MAX"), 60f, accomodateLabel: false, tooltip: Translations.Translate("ABLC_CAT_WMX_TIP"));
                maxWorkLevelDropDown.items = new string[] { "1", "2", "3" };

                // Add random level checkbox.
                randomSpawnCheck = UIControls.LabelledCheckBox(this, 20f, 235f, Translations.Translate("ABLC_RAN_SPN"), tooltip: Translations.Translate("ABLC_RAN_SPN_TIP"));

                // Extend height to fit 'clear all building settings' button and historical settings section.
                height += 200f;

                // Button to clear all building settings in district.
                UIButton clearBuildingsButton = UIControls.AddButton(this, Margin, height - 200f, Translations.Translate("ABLC_CLR_BLD"), this.width - (Margin * 2), tooltip: Translations.Translate("ABLC_CLR_BLD_TIP"));
                clearBuildingsButton.eventClicked += ClearBuildings;

                // Spacer panel.
                UIControls.OptionsSpacer(this, Margin, height - 155f, width - (Margin * 2));

                // Add historical section label.
                AddLabel(Translations.Translate("ABLC_HIS"), 0f, height - 140f, 1.0f);

                // Add historical spawning checkbox.
                spawnHistoricalCheck = UIControls.LabelledCheckBox(this, 20f, height - 110f, Translations.Translate("ABLC_HIS_SPN"), tooltip: Translations.Translate("ABLC_HIS_SPN_TIP"));

                // Button to make all buildings in district historical.
                makeHistoricalButton = UIControls.AddButton(this, Margin, height - 80f, Translations.Translate("ABLC_TRIG_HIS"), this.width - (Margin * 2), tooltip: Translations.Translate("ABLC_TRIG_HIS_TIP"));
                makeHistoricalButton.eventClicked += MakeHistorical;

                // Button to remove historical status from all buildings in district.
                removeHistoricalButton = UIControls.AddButton(this, Margin, height - 40f, Translations.Translate("ABLC_TRIG_NHS"), this.width - (Margin * 2), tooltip: Translations.Translate("ABLC_TRIG_NHS_TIP"));
                removeHistoricalButton.eventClicked += MakeHistorical;

                // Set initial district.
                DistrictChanged();

                // Add event handlers.

                minLevelDropDown.eventSelectedIndexChanged += (control, index) =>
                {
                    // Don't do anything if events are disabled.
                    if (!disableEvents)
                    {
                        DistrictsABLC.minResLevel[targetID] = (byte)index;

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
                        DistrictsABLC.maxResLevel[targetID] = (byte)index;

                        // If the maximum level is now less than the minimum level, reduce the minimum to match the maximum.
                        if (index < minLevelDropDown.selectedIndex)
                        {
                            minLevelDropDown.selectedIndex = index;
                        }
                    }
                };

                minWorkLevelDropDown.eventSelectedIndexChanged += (control, index) =>
                {
                    // Don't do anything if events are disabled.
                    if (!disableEvents)
                    {
                        DistrictsABLC.minWorkLevel[targetID] = (byte)index;

                        // If the minimum level is now greater than the maximum level, increase the maximum to match the minimum.
                        if (index > maxWorkLevelDropDown.selectedIndex)
                        {
                            maxWorkLevelDropDown.selectedIndex = index;
                        }
                    }
                };

                maxWorkLevelDropDown.eventSelectedIndexChanged += (control, index) =>
                {
                    // Don't do anything if events are disabled.
                    if (!disableEvents)
                    {
                        DistrictsABLC.maxWorkLevel[targetID] = (byte)index;

                        // If the maximum level is now less than the minimum level, reduce the minimum to match the maximum.
                        if (index < minWorkLevelDropDown.selectedIndex)
                        {
                            minWorkLevelDropDown.selectedIndex = index;
                        }
                    }
                };

                randomSpawnCheck.eventCheckChanged += (control, isChecked) =>
                {
                    // Don't do anything if events are disabled.
                    if (!disableEvents)
                    {
                        // XOR relevant flag to toggle.
                        DistrictsABLC.flags[targetID] ^= (byte)DistrictFlags.randomSpawnLevels;
                    }
                };

                spawnHistoricalCheck.eventCheckChanged += (control, isChecked) =>
                {
                    // Don't do anything if events are disabled.
                    if (!disableEvents)
                    {
                        // XOR relevant flag to toggle.
                        DistrictsABLC.flags[targetID] ^= (byte)DistrictFlags.spawnHistorical;
                    }
                };

                upgradeButton.eventClicked += (control, clickEvent) =>
                {
                    LevelDistrict(targetID, true);
                };

                downgradeButton.eventClicked += (control, clickEvent) =>
                {
                    LevelDistrict(targetID, false);
                };

            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception setting up district panel");
            }
        }


        /// <summary>
        /// Clear building settings button event handler.
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        /// </summary>
        private void ClearBuildings(UIComponent control, UIMouseEventParameter mouseEvent) => BuildingsABLC.ClearDistrict(targetID);


        /// <summary>
        /// Clear building settings button event handler.
        /// <param name="control">Calling component</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        /// </summary>
        private void MakeHistorical(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Store copy of current target district ID for simulation thread action.
            ushort districtID = targetID;

            // Set historical target via determining calling component.
            bool setHistorical = control == makeHistoricalButton;

            // Add 'set historical' action to simulation thread.
            Singleton<SimulationManager>.instance.AddAction(delegate
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
        /// <param name="districtID">Target district</param>
        /// <param name="upgrade">True if we want to upgrade all buildings (below the minimum) to the minimum for this district, false if we want to downgrade (all buildings above the maximum) to the maximum</param>
        private void LevelDistrict(ushort districtID, bool upgrade)
        {
            // Instances and arrays.
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            // Iterate through all buildings in map.
            for (ushort i = 0; i < buildings.m_size; ++i)
            {
                // Get local reference.
                Building thisBuilding = buildings.m_buffer[i];

                // Skip non-existent buildings or non-Private AI buildings.
                if (thisBuilding.m_flags != Building.Flags.None || !(thisBuilding.Info?.GetAI() is PrivateBuildingAI))
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
                            byte minLevel = BuildingsABLC.GetMinLevel(i, isResidential);

                            // Check if building level is less than the relevant minimum.
                            if (buildings.m_buffer[i].m_level < minLevel)
                            {
                                // It needs to be upgraded; store copy of current index and minimum level for action queue.
                                ushort buildingID = i;
                                byte thisMinLevel = minLevel;

                                // Upgrade.
                                Singleton<SimulationManager>.instance.AddAction(delegate { LevelUtils.ForceLevel(buildingID, thisMinLevel); });
                            }
                        }
                        else
                        {
                            // Downgrading - get the maximum level for this building..
                            byte maxLevel = BuildingsABLC.GetMaxLevel(i, isResidential);

                            // Check if building level is greater than the relevant maximum.
                            if (buildings.m_buffer[i].m_level > maxLevel)
                            {
                                // It needs to be downgraded; store copy of current index and maximum level for action queue.
                                ushort buildingID = i;
                                byte thisMaxLevel = maxLevel;

                                // Downgrade.
                                Singleton<SimulationManager>.instance.AddAction(delegate { LevelUtils.ForceLevel(buildingID, thisMaxLevel); });
                            }
                        }
                    }
                }
            }
        }
    }
}