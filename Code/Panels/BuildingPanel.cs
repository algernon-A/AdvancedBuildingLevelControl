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
        private static ABLCBuildingPanel _panel;
        internal static ABLCBuildingPanel Panel => _panel;

        // UI components.
        internal static UICheckBox controlBuildingCheckbox;


        /// <summary>
        /// Adds event handler to show/hide building panel as appropriate (in line with ZonedBuildingWorldInfoPanel).
        /// </summary>
        internal static void Hook()
        {
            // Get building info panel instance.
            UIComponent buildingInfoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name)?.component;
            if (buildingInfoPanel == null)
            {
                Debugging.Message("couldn't hook building info panel");
            }
            else
            {
                // Create/destroy our panel as and when the info panel is shown or hidd
                buildingInfoPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    if (isVisible)
                    {
                        Create();
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

                    _panel = uiGameObject.AddComponent<ABLCBuildingPanel>();

                    // Set up and show panel.
                    Panel.Setup(uiGameObject.transform.parent);
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
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


        /// <summary>
        /// Adds the "Control levels" checkbox to private building info panels.
        /// </summary>
        internal static void AddPanelCheckbox()
        {
            // Add button to access building details from building info panels, if it doesn't already exist.
            if (controlBuildingCheckbox == null)
            {
                // Basic setup.
                ZonedBuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);


                controlBuildingCheckbox = infoPanel.component.AddUIComponent<UICheckBox>();

                // Size and position.
                controlBuildingCheckbox.width = infoPanel.component.width;
                controlBuildingCheckbox.height = 20f;
                controlBuildingCheckbox.clipChildren = true;
                controlBuildingCheckbox.relativePosition = new Vector3(23f, 287f);

                // Sprites.
                UISprite sprite = controlBuildingCheckbox.AddUIComponent<UISprite>();
                sprite.spriteName = "check-unchecked";
                sprite.size = new Vector2(16f, 16f);
                sprite.relativePosition = Vector3.zero;

                controlBuildingCheckbox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
                ((UISprite)controlBuildingCheckbox.checkedBoxObject).spriteName = "check-checked";
                controlBuildingCheckbox.checkedBoxObject.size = new Vector2(16f, 16f);
                controlBuildingCheckbox.checkedBoxObject.relativePosition = Vector3.zero;

                // Label.
                controlBuildingCheckbox.label = controlBuildingCheckbox.AddUIComponent<UILabel>();
                controlBuildingCheckbox.label.text = " ";
                controlBuildingCheckbox.label.textScale = 0.8125f;
                controlBuildingCheckbox.label.relativePosition = new Vector3(21f, 2f);
                controlBuildingCheckbox.label.textColor = new Color32(185, 221, 254, 255);

                // Copy the 'make historical' checkboxes' font.
                controlBuildingCheckbox.label.font = infoPanel.Find<UICheckBox>("CheckboxHistorical").label.font;

                controlBuildingCheckbox.name = "ControlLevel";

                // Set the label text based on current language.
                SetText();

                // Event handler.
                controlBuildingCheckbox.eventCheckChanged += (component, isChecked) =>
                {
                    if (Panel != null)
                    {
                        // Add or remove from locked building list, as appropriate.
                        if (isChecked)
                        {
                            Panel.AddBuilding();
                        }
                        else
                        {
                            Panel.RemoveBuilding();
                        }
                    }
                };
            }
        }


        /// <summary>
        /// Adds/updates the 'control levels' checkbox text according to the current language.
        /// </summary>
        internal static void SetText()
        {
            // Don't do anything if it isn't created yet.
            if (controlBuildingCheckbox != null)
            {
                // Set 'control levels' checkbox text.
                controlBuildingCheckbox.text = Translations.Translate("ABLC_CTRL");
            }
        }
    }


    /// <summary>
    /// ABLC district settings info panel.
    /// </summary>
    public class ABLCBuildingPanel : ABLCPanel
    {
        // Constants.
        protected override float panelHeight => 220f;

        // Downgrade target level.
        protected byte downgradeLevel; 
        

        /// <summary>
        /// Adds custom settings for the current building and shows the panel.
        /// </summary>
        public void AddBuilding()
        {
            // Don't do anything if events are disabled.
            if (!disableEvents)
            {
                // Add building to dictionary with default settings, catching any duplicate key exceptions.
                try
                {
                    BuildingsABLC.levelRanges.Add(targetID, new LevelRange { minLevel = 0, maxLevel = LevelUtils.GetMaxLevel(targetID) });
                }
                catch
                {
                    // Something went wrong somewhere, but it's not fatal: just log and continue on with whatever settings were already there.
                    Debugging.Message("duplicate key for building " + targetID);
                }

                // Update the panel.
                BuildingChanged();

                // Show panel (as this building now has custom settings).
                Show();
            }
        }


        /// <summary>
        /// Removes the current building's custom settings and hides the panel.
        /// </summary>
        public void RemoveBuilding()
        {
            // Don't do anything if events are disabled.
            if (!disableEvents)
            {
                // Remove building from dictionary, catching any non-existent key exceptions.
                try
                {
                    BuildingsABLC.levelRanges.Remove(targetID);
                }
                catch
                {
                    // Something went wrong somewhere, but it's not fatal: just log and continue on.
                    Debugging.Message("missing key for building " + targetID);
                }

                // Hide panel (as this building no longer has has custom settings).
                Hide();
            }
        }


        /// <summary>
        /// Called when the selected building has changed.
        /// </summary>
        public void BuildingChanged()
        {
            // Update selected building ID.
            targetID = WorldInfoPanel.GetCurrentInstanceID().Building;

            // Check maximum level for this building type.
            int maxLevel = LevelUtils.GetMaxLevel(targetID);

            // If building doesn't have more than one level, then we don't have any business to do here.
            if (maxLevel == 1)
            {
                BuildingPanelManager.controlBuildingCheckbox.isVisible = false;
                Hide();
                return;
            }

            // Disable events while we make changes to avoid triggering event handler.
            disableEvents = true;

            // This is a valid building with more than one level; ensure checkbox is visible.
            BuildingPanelManager.controlBuildingCheckbox.isVisible = true;

            // Set name.
            nameLabel.text = Singleton<BuildingManager>.instance.GetBuildingName(targetID, InstanceID.Empty);

            // Check to see if we have custom settings for this building.
            bool hasCustom = BuildingsABLC.levelRanges.ContainsKey(targetID);

            // Set checkbox according to whether or not we have custom settings for this building.
            BuildingPanelManager.controlBuildingCheckbox.isChecked = hasCustom;


            if (hasCustom)
            {
                // Build level dropdown ranges.
                minLevelDropDown.items = new string[maxLevel];
                maxLevelDropDown.items = new string[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                {
                    minLevelDropDown.items[i] = (i + 1).ToString();
                    maxLevelDropDown.items[i] = (i + 1).ToString();
                }

                // Update dropdown selection to match building's settings.
                minLevelDropDown.selectedIndex = BuildingsABLC.levelRanges[targetID].minLevel;
                maxLevelDropDown.selectedIndex = BuildingsABLC.levelRanges[targetID].maxLevel;

                // Show panel - custom settings exist.
                Show();
            }
            else
            {
                // No custom settings - hide panel.
                Hide();
            }

            // Initialise panel with correct level settings.
            UpdatePanel();

            // All done: re-enable events.
            disableEvents = false;
        }

        
        /// <summary>
        /// Updates the panel according to building's current level settings.
        /// </summary>
        public void UpdatePanel()
        {
            // Check to see if the building can upgrade.
            if (LevelUtils.CanBuildingUpgrade(targetID))
            {
                // Yep - enable upgrade button.
                upgradeButton.Enable();
            }
            else
            {
                // Nope - disable upgrade button.
                upgradeButton.Disable();
            }

            // Check to see if the building can be downgraded one level.
            downgradeLevel = (byte)(Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].m_level - 1);
            if (LevelUtils.GetDowngradeInfo(targetID, downgradeLevel) == null)
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
        /// Performs initial setup for the panel; we don't use Start() as that's not sufficiently reliable (race conditions), and is not needed with the dynamic create/destroy process.
        /// </summary>
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
                        BuildingsABLC.levelRanges[targetID].minLevel = (byte)index;

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
                        // Set maximum level of building in dictionary.
                        BuildingsABLC.levelRanges[targetID].maxLevel = (byte)index;

                        // If the maximum level is now less than the minimum level, reduce the minimum to match the maximum.
                        if (index < minLevelDropDown.selectedIndex)
                        {
                            minLevelDropDown.selectedIndex = index;
                        }
                    }
                };

                upgradeButton.eventClick += (control, clickEvent) =>
                {
                    LevelUtils.ForceLevelUp(targetID);

                    // Check to see if we should increase this buildings maximum level.
                    byte newLevel = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].m_level;
                    if (BuildingsABLC.levelRanges.ContainsKey(targetID) && BuildingsABLC.levelRanges[targetID].maxLevel < newLevel)
                    {
                        //BuildingsABLC.levelRanges[targetID].maxLevel = newLevel;
                        maxLevelDropDown.selectedIndex = newLevel;
                    }

                    // Update the panel once done.
                    UpdatePanel();
                };

                downgradeButton.eventClick += (control, clickEvent) =>
                {
                    LevelUtils.ForceLevelDown(targetID, downgradeLevel);

                    // Check to see if we should increase this buildings maximum level.
                    byte newLevel = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].m_level;
                    if (BuildingsABLC.levelRanges.ContainsKey(targetID) && BuildingsABLC.levelRanges[targetID].minLevel > newLevel)
                    {
                        //BuildingsABLC.levelRanges[targetID].minLevel = newLevel;
                        minLevelDropDown.selectedIndex = newLevel;
                    }

                    // Update the panel once done.
                    UpdatePanel();
                };

            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }
    }
}