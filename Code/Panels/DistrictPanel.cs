using System;
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
                Debugging.Message("couldn't hook district info panel");
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
                Debug.LogException(e);
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
    public class ABLCDistrictPanel : ABLCPanel
    {
        // Panel components.
        protected UIDropDown minWorkLevelDropDown;
        protected UIDropDown maxWorkLevelDropDown;


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

            minLevelDropDown.selectedIndex = DistrictsABLC.minResLevel[targetID];
            maxLevelDropDown.selectedIndex = DistrictsABLC.maxResLevel[targetID];
            minWorkLevelDropDown.selectedIndex = DistrictsABLC.minWorkLevel[targetID];
            maxWorkLevelDropDown.selectedIndex = DistrictsABLC.maxWorkLevel[targetID];

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
                UILabel resLabel = AddLabel(Translations.Translate("ABLC_CAT_RES"), margin, 50f, hAlign: UIHorizontalAlignment.Left);
                UILabel workLabel = AddLabel(Translations.Translate("ABLC_CAT_WRK"), margin, 140f, hAlign: UIHorizontalAlignment.Left);

                // Add workplace min and max dropdowns.
                minWorkLevelDropDown = UIUtils.CreateDropDown(this, "Minimum level", yPos: 160f);
                minWorkLevelDropDown.items = new string[] { "1", "2", "3" };

                maxWorkLevelDropDown = UIUtils.CreateDropDown(this, "Maximum level", yPos: 190f);
                maxWorkLevelDropDown.items = new string[] { "1", "2", "3" };

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

                applyButton.eventClick += (control, clickEvent) =>
                {
                    UpgradeDistrict(targetID);
                };

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        /// <summary>
        /// Forces an upgrade all buildings in the given district less than the district's specified minimum.
        /// </summary>
        /// <param name="districtID"></param>
        public void UpgradeDistrict(ushort districtID)
        {
            // Instances and arrays.
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            // Iterate through all buildings in map.
            for (ushort i = 0; i < buildings.m_size; ++ i)
            {
                // Skip non-existent buildings.
                if (buildings.m_buffer[i].m_flags != Building.Flags.None)
                {
                    // Building exists; get its district and see if it matches the target district.
                    if (districtManager.GetDistrict(buildings.m_buffer[i].m_position) == districtID)
                    {
                        // It's in our district; check if its level is less than the relevant minimum.
                        if (buildings.m_buffer[i].m_level < (buildings.m_buffer[i].Info?.GetService() == ItemClass.Service.Residential ? DistrictsABLC.minResLevel[districtID] : DistrictsABLC.minWorkLevel[districtID]))
                        {
                            // It needs to be upgraded; store copy of current index for action queue.
                            ushort buildingID = i;

                            // Force upgrade.
                            Singleton<SimulationManager>.instance.AddAction(() =>
                            {
                                ((Action<ushort>)LevelUtils.ForceLevelUp).Invoke(buildingID);
                            });
                        }
                    }
                }
            }
        }
    }
}