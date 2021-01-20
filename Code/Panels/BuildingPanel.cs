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
                // Create/destroy our panel as and when the info panel is shown or hidden.
                buildingInfoPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    if (isVisible && ModSettings.showPanel)
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
            GameObject.Destroy(_panel);
            GameObject.Destroy(uiGameObject);
        }


        /// <summary>
        /// Adds an ABLC button to a building info panel to open the ABLC panel for that building.
        /// The button will be added to the right of the panel with a small margin from the panel edge, at the relative Y position specified.
        /// </summary>
        internal static void AddInfoPanelButton()
        {
            BuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);
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
                relativeY = (wrapper == null ? 0 : wrapper.relativePosition.y) + problemsPanel.relativePosition.y + ((problemsPanel.height - 36) / 2);
            }
            catch
            {
                // Don't really care; just use default relative Y.
                Logging.Message("couldn't find ProblemsPanel relative position");
            }

            // Set position.
            panelButton.AlignTo(infoPanel.component, UIAlignAnchor.TopRight);
            panelButton.relativePosition += new Vector3(-62f, relativeY, 0f);

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
    public class ABLCBuildingPanel : ABLCPanel
    {
        // Constants.
        protected override float panelHeight => 220f;

        // Upgrade and downgrade target levels.
        protected byte upgradeLevel, downgradeLevel;


        /// <summary>
        /// Updates the buildng's minimum level.
        /// </summary>
        /// <param name="minLevel">New minimum level</param>
        public void UpdateMinLevel(byte minLevel)
        {
            // Don't do anything if events are disabled.
            if (!disableEvents)
            {
                // See if we've already got a dictionary entry for this building.
                if (BuildingsABLC.levelRanges.ContainsKey(targetID))
                {
                    // We do - if this new minimum level is zero and the maximum for this building is set to the building's maximum, delete this entry.
                    if (minLevel == 0 && BuildingsABLC.levelRanges[targetID].maxLevel == LevelUtils.GetMaxLevel(targetID))
                    {
                        BuildingsABLC.levelRanges.Remove(targetID);
                    }
                    else
                    {
                        // Otherwise, just update our entry's minimum target level.
                        BuildingsABLC.levelRanges[targetID].minLevel = minLevel;
                    }
                }
                else if (minLevel > 0)
                {
                    // If the new minimum level isn't the absolute minimum, create a new dictionary entry with this minimum and default maximum levels.
                    BuildingsABLC.levelRanges.Add(targetID, new LevelRange { minLevel = minLevel, maxLevel = LevelUtils.GetMaxLevel(targetID) });
                }

                // Update the panel.
                BuildingChanged();
            }
        }


        /// <summary>
        /// Updates the buildng's maximum level.
        /// </summary>
        /// <param name="maxLevel">New maximum level</param>
        public void UpdateMaxLevel(byte maxLevel)
        {
            // Don't do anything if events are disabled.
            if (!disableEvents)
            {
                // See if we've already got a dictionary entry for this building.
                if (BuildingsABLC.levelRanges.ContainsKey(targetID))
                {
                    // We do - if this new maximum level is the maximum for this building and the minimum is zero, delete this entry.
                    if (maxLevel == LevelUtils.GetMaxLevel(targetID) && BuildingsABLC.levelRanges[targetID].minLevel == 0)
                    {
                        BuildingsABLC.levelRanges.Remove(targetID);
                    }
                    else
                    {
                        // Otherwise, just update our entry's maximum target level.
                        BuildingsABLC.levelRanges[targetID].maxLevel = maxLevel;
                    }
                }
                else if (maxLevel < LevelUtils.GetMaxLevel(targetID))
                {
                    // If the new maximum level isn't the absolute maximum for this building, create a new dictionary entry with this maximum and default minimum levels.
                    BuildingsABLC.levelRanges.Add(targetID, new LevelRange { minLevel = 0, maxLevel = maxLevel });
                }

                // Update the panel.
                BuildingChanged();
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
                Hide();
                return;
            }
            else if (!isVisible)
            {
                // Otherwise, make sure we're visible if we're not already.
                Show();
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


            // Check to see if we have custom settings for this building.
            if (BuildingsABLC.levelRanges.ContainsKey(targetID))
            {
                // Update dropdown selection to match building's settings.
                minLevelDropDown.selectedIndex = BuildingsABLC.levelRanges[targetID].minLevel;
                maxLevelDropDown.selectedIndex = BuildingsABLC.levelRanges[targetID].maxLevel;
            }
            else
            {
                // Set min and max to default.
                minLevelDropDown.selectedIndex = 0;
                maxLevelDropDown.selectedIndex = maxLevelDropDown.items.Length - 1;
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
                    LevelUtils.ForceLevel(targetID, upgradeLevel);

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
                    LevelUtils.ForceLevel(targetID, downgradeLevel);

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
    }
}