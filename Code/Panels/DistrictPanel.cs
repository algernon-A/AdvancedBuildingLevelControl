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


        /// <summary>
        /// Adds an ABLC button to a building info panel to open the ABLC panel for that building.
        /// The button will be added to the right of the panel with a small margin from the panel edge, at the relative Y position specified.
        /// </summary>
        /*internal static void AddInfoPanelButton()
        {
            DistrictWorldInfoPanel infoPanel = UIView.library.Get<DistrictWorldInfoPanel>(typeof(DistrictWorldInfoPanel).Name);
            UIButton panelButton = infoPanel.component.AddUIComponent<UIButton>();

            // Create new texture atlas for button.
            UITextureAtlas buttonAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            buttonAtlas.name = "ABLCButton";
            buttonAtlas.material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);

            // Load texture from file.
            Texture2D buttonTexture = FileUtils.LoadTexture("ablc_buttons.png");
            buttonAtlas.material.mainTexture = buttonTexture;

            // Setup sprites.
            string[] spriteNames = new string[] { "disabled", "normal", "pressed", "hovered" };
            int numSprites = spriteNames.Length;
            float spriteWidth = 1f / spriteNames.Length;

            // Iterate through each sprite (counter increment is in region setup).
            for (int i = 0; i < numSprites; ++i)
            {
                UITextureAtlas.SpriteInfo sprite = new UITextureAtlas.SpriteInfo
                {
                    name = spriteNames[i],
                    texture = buttonTexture,
                    // Sprite regions are horizontally arranged, evenly spaced.
                    region = new Rect(i * spriteWidth, 0f, spriteWidth, 1f)
                };
                buttonAtlas.AddSprite(sprite);
            }

            // Basic button setup.
            panelButton.atlas = buttonAtlas;
            panelButton.size = new Vector2(36, 36);
            panelButton.normalFgSprite = "normal";
            panelButton.focusedFgSprite = "hovered";
            panelButton.hoveredFgSprite = "hovered";
            panelButton.pressedFgSprite = "pressed";
            panelButton.disabledFgSprite = "disabled";
            panelButton.name = "ABLCbutton";
            panelButton.tooltip = Translations.Translate("ABLC_NAME");

            // Set position.
            panelButton.relativePosition = new Vector3(4f, 78f, 0f);

            // Event handler.
            panelButton.eventClick += (control, clickEvent) =>
            {
                // Toggle panel visibility.
                if (uiGameObject == null)
                {
                    Create(UIView.library.Get<DistrictWorldInfoPanel>(typeof(DistrictWorldInfoPanel).Name)?.component.transform);
                }
                else
                {
                    Close();
                }

                // Manually unfocus control, otherwise it can stay focused until next UI event (looks untidy).
                control.Unfocus();
            };
        }*/
    }


    /// <summary>
    /// ABLC district settings info panel.
    /// </summary>
    public class ABLCDistrictPanel : ABLCPanel
    {
        // Panel components.
        protected UIDropDown minWorkLevelDropDown;
        protected UIDropDown maxWorkLevelDropDown;
        private UICheckBox randomSpawnCheck;


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
                minWorkLevelDropDown = UIUtils.CreateDropDown(this, Translations.Translate("ABLC_LVL_MIN"), yPos: 160f);
                minWorkLevelDropDown.items = new string[] { "1", "2", "3" };

                maxWorkLevelDropDown = UIUtils.CreateDropDown(this, Translations.Translate("ABLC_LVL_MAX"), yPos: 190f);
                maxWorkLevelDropDown.items = new string[] { "1", "2", "3" };

                // Add random level checkbox.
                randomSpawnCheck = UIUtils.AddCheckBox(this, Translations.Translate("ABLC_RAN_SPN"), yPos: 235f);

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
        /// Attempts to force all buildings in a district to meet the district's minimum or maximum target level, as specified.
        /// </summary>
        /// <param name="districtID">Target district</param>
        /// <param name="upgrade">True if we want to upgrade all buildings (below the minimum) to the minimum for this district, false if we want to downgrade (all buildings above the maximum) to the maximum</param>
        public void LevelDistrict(ushort districtID, bool upgrade)
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
                        // It's in our district.
                        // Are we upgrading or downgrading?
                        if (upgrade)
                        {
                            // Upgrading.
                            byte minLevel = (thisBuilding.Info?.GetService() == ItemClass.Service.Residential ? DistrictsABLC.minResLevel[districtID] : DistrictsABLC.minWorkLevel[districtID]);

                            // It's in our district; check if its level is less than the relevant minimum.
                            if (buildings.m_buffer[i].m_level < minLevel)
                            {
                                // It needs to be upgraded; store copy of current index for action queue.
                                ushort buildingID = i;

                                // Upgrade.
                                LevelUtils.ForceLevel(buildingID, minLevel);
                            }
                        }
                        else
                        {
                            // Downgrading.
                            byte maxLevel = (thisBuilding.Info?.GetService() == ItemClass.Service.Residential ? DistrictsABLC.maxResLevel[districtID] : DistrictsABLC.maxWorkLevel[districtID]);

                            // It's in our district; check if its level is less than the relevant minimum.
                            if (buildings.m_buffer[i].m_level > maxLevel)
                            {
                                // It needs to be upgraded; store copy of current index for action queue.
                                ushort buildingID = i;

                                // Downgrade.
                                LevelUtils.ForceLevel(buildingID, maxLevel);
                            }
                        }
                    }
                }
            }
        }
    }
}