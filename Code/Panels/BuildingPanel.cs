// <copyright file="BuildingPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.Math;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// ABLC building settings info panel.
    /// </summary>
    internal class BuildingPanel : ABLCPanel
    {
        // Upgrade and downgrade target levels.
        private byte _upgradeLevel;
        private byte _downgradeLevel;

        /// <summary>
        /// Gets or sets a value indicating whether an update is ready.
        /// </summary>
        internal bool UpdateReady { get; set; } = false;

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        protected override float PanelHeight => 180f;

        /// <summary>
        /// Gets the upgrade button tooltip.
        /// </summary>
        protected override string UpgradeTip => Translations.Translate("BUILDING_UPGRADE");

        /// <summary>
        /// Gets the downgrade button tooltip.
        /// </summary>
        protected override string DowngradeTip => Translations.Translate("BUILDING_DOWNGRADE");

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Add randomize appearance button (no foreground sprite).
            UIButton randomButton = AddIconButton(Margin + 80f, PanelHeight - 40f, string.Empty, Translations.Translate("BUILDING_RANDOM"));
            randomButton.eventClick += (c, p) => Singleton<SimulationManager>.instance.AddAction(() => RandomizeAppearance(m_targetID));

            // Add foreground sprite for randomize appearance button.
            UISprite sprite = AddUIComponent<UISprite>();
            sprite.atlas = UITextures.InGameAtlas;
            sprite.spriteName = "Random";
            sprite.relativePosition = randomButton.relativePosition - new Vector3(33f, 11f);
            sprite.width = 93f;
            sprite.height = 55f;
            sprite.isInteractive = false;

            // Set initial building.
            BuildingChanged();

            // Add event handlers.
            m_minLevelDropDown.eventSelectedIndexChanged += (c, index) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Set minimum level of building in dictionary.
                    UpdateMinLevel((byte)index);

                    // If the minimum level is now greater than the maximum level, increase the maximum to match the minimum.
                    if (index > m_maxLevelDropDown.selectedIndex)
                    {
                        m_maxLevelDropDown.selectedIndex = index;
                    }
                }
            };

            m_maxLevelDropDown.eventSelectedIndexChanged += (c, index) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Update maximum level.
                    UpdateMaxLevel((byte)index);

                    // If the maximum level is now less than the minimum level, reduce the minimum to match the maximum.
                    if (index < m_minLevelDropDown.selectedIndex)
                    {
                        m_minLevelDropDown.selectedIndex = index;
                    }
                }
            };

            m_upgradeButton.eventClick += (c, p) =>
            {
                // Local references for SimulationManager action.
                ushort buildingID = m_targetID;
                byte targetLevel = _upgradeLevel;
                Logging.KeyMessage("upgrading building to level ", _upgradeLevel);
                Singleton<SimulationManager>.instance.AddAction(() =>
                {
                    LevelUtils.ForceLevel(m_targetID, targetLevel);
                });

                // Check to see if we should increase this buildings maximum level.
                if (Buildings.GetMaxLevel(m_targetID) < _upgradeLevel)
                {
                    m_maxLevelDropDown.selectedIndex = _upgradeLevel;
                }
            };

            m_downgradeButton.eventClick += (c, p) =>
            {
                // Local references for SimulationManager action.
                ushort buildingID = m_targetID;
                byte targetLevel = _downgradeLevel;
                Logging.KeyMessage("downgrading building to level ", _downgradeLevel);
                Singleton<SimulationManager>.instance.AddAction(() =>
                {
                    LevelUtils.ForceLevel(m_targetID, targetLevel);
                });

                // Check to see if we should increase this buildings maximum level.
                if (Buildings.GetMinLevel(m_targetID) > _downgradeLevel)
                {
                    m_minLevelDropDown.selectedIndex = _downgradeLevel;
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

        /// <summary>
        /// Called by Unity every tick.  Used here to check to see if we need to update the panel after a building has been updated via the simulation thread.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // Check to see if an update is ready; if so, refresh the panel and clear the flag.
            if (UpdateReady)
            {
                UpdatePanel();
                UpdateReady = false;
            }
        }

        /// <summary>
        /// Called when the selected building has changed.
        /// </summary>
        internal void BuildingChanged()
        {
            // Update selected building ID.
            m_targetID = WorldInfoPanel.GetCurrentInstanceID().Building;

            // Check maximum level for this building type.
            int maxLevel = LevelUtils.GetMaxLevel(m_targetID);

            // If building doesn't have more than one level, then we don't have any business to do here.
            if (maxLevel == 1)
            {
                BuildingPanelManager.PanelButton.Disable();
                Hide();
                return;
            }
            else
            {
                // Enable info panel button.
                BuildingPanelManager.PanelButton.Enable();

                // Make sure we're visible if we're not already.
                if (!isVisible)
                {
                    Show();
                }
            }

            // Disable events while we make changes to avoid triggering event handler.
            m_disableEvents = true;

            // Set name.
            m_nameLabel.text = Singleton<BuildingManager>.instance.GetBuildingName(m_targetID, InstanceID.Empty);

            // Build level dropdown ranges.
            m_minLevelDropDown.items = new string[maxLevel];
            m_maxLevelDropDown.items = new string[maxLevel];
            for (int i = 0; i < maxLevel; i++)
            {
                m_minLevelDropDown.items[i] = (i + 1).ToString();
                m_maxLevelDropDown.items[i] = (i + 1).ToString();
            }

            // Update dropdown selection to match building's settings.
            m_minLevelDropDown.selectedIndex = Buildings.GetMinLevel(m_targetID);
            m_maxLevelDropDown.selectedIndex = Mathf.Min(Buildings.GetMaxLevel(m_targetID), m_maxLevelDropDown.items.Length - 1);

            // Initialise panel with correct level settings.
            UpdatePanel();

            // All done: re-enable events.
            m_disableEvents = false;
        }

        /// <summary>
        /// Updates the panel according to building's current level settings.
        /// </summary>
        internal void UpdatePanel()
        {
            // Make sure we have a valid builidng first.
            if (m_targetID == 0 || (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_targetID].m_flags == Building.Flags.None))
            {
                // Invalid target - disable buttons.
                m_upgradeButton.Disable();
                m_downgradeButton.Disable();
                return;
            }

            // Get building level and subservive.
            ref Building building = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_targetID];
            byte level = building.m_level;
            ItemClass.SubService subService = building.Info.m_class.m_subService;

            // Check to see if the building can be upgraded one level.
            _upgradeLevel = (byte)(level + 1);
            if (LevelUtils.GetMaxLevel(subService) <= level || LevelUtils.GetTargetInfo(m_targetID, _upgradeLevel) == null)
            {
                // Nope - disable upgrade button.
                m_upgradeButton.Disable();
            }
            else
            {
                // Yep - enable upgrade button.
                m_upgradeButton.Enable();
            }

            // Check to see if the building can be downgraded one level.
            _downgradeLevel = (byte)(level - 1);
            if (LevelUtils.GetMaxLevel(subService) <= level || LevelUtils.GetTargetInfo(m_targetID, _downgradeLevel) == null)
            {
                // Nope - disable downgrade button.
                m_downgradeButton.Disable();
            }
            else
            {
                // Yep - enable downgrade button.
                m_downgradeButton.Enable();
            }
        }

        /// <summary>
        /// Updates the buildng's minimum level.
        /// </summary>
        /// <param name="minLevel">New minimum level.</param>
        private void UpdateMinLevel(byte minLevel)
        {
            // Don't do anything if events are disabled.
            if (!m_disableEvents)
            {
                // Update minimum level.
                Buildings.UpdateMinLevel(m_targetID, minLevel);

                // Update the panel.
                BuildingChanged();
            }
        }

        /// <summary>
        /// Updates the buildng's maximum level.
        /// </summary>
        /// <param name="maxLevel">New maximum level.</param>
        private void UpdateMaxLevel(byte maxLevel)
        {
            // Don't do anything if events are disabled.
            if (!m_disableEvents)
            {
                // Update maximum level.
                Buildings.UpdateMaxLevel(m_targetID, maxLevel);

                // Update the panel.
                BuildingChanged();
            }
        }

        /// <summary>
        /// Randomizes the appearance of the given building, leaving the level unchanged.
        /// Should only be called via the simulation thread.
        /// </summary>
        /// <param name="buildingID">Building ID.</param>
        private void RandomizeAppearance(ushort buildingID)
        {
            // Local references.
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building[] buildings = buildingManager.m_buildings.m_buffer;
            ref Building building = ref buildings[buildingID];
            BuildingInfo buildingInfo = building.Info;

            // If a private AI building, select a new random prefab.
            if (buildingInfo?.m_buildingAI is PrivateBuildingAI buildingAI)
            {
                // Apply level randomization, if applicable forcing true randomziation regardless of mod settings.
                ItemClass.Level finalLevel;
                Randomizer r;
                if (ModSettings.RandomLevels)
                {
                    // Random target level.
                    finalLevel = LevelUtils.GetRandomLevel(buildingInfo, buildingID, building.m_level, true, out r);
                }
                else
                {
                    // Static target level.
                    finalLevel = (ItemClass.Level)building.m_level;

                    // Get true random randomizer.
                    r = LevelUtils.GetRandomizer(buildingID, true);
                }

                // Select random info, respecting district styles.
                BuildingInfo targetInfo = GetUpgradeInfoPatch.GetRandomInfo(buildingAI, finalLevel, ref building, ref r);

                // Update building info.
                if (targetInfo != null)
                {
                    buildingManager.UpdateBuildingInfo(buildingID, buildingAI.GetUpgradeInfo(buildingID, ref buildings[m_targetID]));
                }

                // Reset building seed.
                LevelUtils.ClearBuildingSeed(buildingID);
            }
        }
    }
}