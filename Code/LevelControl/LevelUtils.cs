﻿using ColossalFramework;


namespace ABLC
{
    public static class LevelUtils
    {
        /// <summary>
        /// Returns the maximum building level of a given building, based on class and subclass.
        /// </summary>
        public static byte GetMaxLevel(ushort buildingID) => (byte)ZonedBuildingWorldInfoPanel.GetMaxLevel(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.m_class);


        /// <summary>
        /// Forces a building to upgrade.
        /// </summary>
        /// <param name="buildingID">Building instance ID</param>
        public static void TriggerLevelUp(ushort buildingID)
        {
            // Don't force upgrade if building is already upgrading.
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_flags.IsFlagSet(Building.Flags.Upgrading))
            {
                return;
            }

            // Don't force upgrade if building is already at maximum level.
            // Note GetMaxLevel is 1-based, m_level is 0-based, so +1 for difference.
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_level + 1 >= GetMaxLevel(buildingID))
            {
                return;
            }

            // Get building AI and force upgrade.
            PrivateBuildingAI buildingAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info?.GetAI() as PrivateBuildingAI;
            if (buildingAI == null)
            {
                Debugging.Message("couldn't get AI for building " + buildingID);
            }
            else
            {
                // Only upgrade if we've got a valid upgrade target.
                if (buildingAI.GetUpgradeInfo(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID]) != null)
                {
                    buildingAI.StartUpgrading(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID]);
                }
            }
        }


        /// <summary>
        /// Upgrades the selected building one level, if possible.
        /// </summary>
        /// <param name="buildingID">Building instance ID</param>
        public static void ForceLevelUp(ushort buildingID)
        {
            // BuildingInfo to upgrade to, if this building isn't historical.
            BuildingInfo upgradeInfo = null;


            // Get an instance reference.
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            // Get building AI reference.
            PrivateBuildingAI buildingAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info?.GetAI() as PrivateBuildingAI;

            // Check to see if this is historical or not.
            bool isHistorical = buildingAI.IsHistorical(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], out bool canSet);

            // Get upgrade building target if needed.
            if (!isHistorical)
            {
                upgradeInfo = buildingAI.GetUpgradeInfo(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID]);
            }

            // If we have a valid downgrade target, proceed.
            if (isHistorical || upgradeInfo != null)
            {
                // Apply minimum level to our building and cancel all level-up progress.
                ++buildingManager.m_buildings.m_buffer[buildingID].m_level;
                buildingManager.m_buildings.m_buffer[buildingID].m_levelUpProgress = 0;

                // Apply our upgrade targe if not historical.
                if (!isHistorical)
                {
                    buildingManager.UpdateBuildingInfo(buildingID, upgradeInfo);
                }

                // Post-upgrade processing to update instance values.
                ((BuildingAI)Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.GetAI()).BuildingUpgraded(buildingID, ref buildingManager.m_buildings.m_buffer[buildingID]);
            }
        }


        /// <summary>
        /// Downgrades the selected building to the given level, if possible.
        /// </summary>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="targetLevel">Level to downgrade to</param>
        public static void ForceLevelDown(ushort buildingID, byte targetLevel)
        {
            // BuildingInfo to upgrade to, if this building isn't historical.
            BuildingInfo downgradeInfo = null;


            // Get an instance reference.
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            // Get building AI reference.
            PrivateBuildingAI buildingAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info?.GetAI() as PrivateBuildingAI;

            // Check to see if this is historical or not.
            bool isHistorical = buildingAI.IsHistorical(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], out bool canSet);

            // Get downgrade building target if needed.
            if (!isHistorical)
            {
                // Get downgrade building target.
                downgradeInfo = GetDowngradeInfo(buildingID, targetLevel);
            }

            // If we have a valid downgrade target, proceed.
            if (isHistorical || downgradeInfo != null)
            {
                // Apply target level to our building and cancel all level-up progress.
                buildingManager.m_buildings.m_buffer[buildingID].m_level = targetLevel;
                buildingManager.m_buildings.m_buffer[buildingID].m_levelUpProgress = 0;

                // Apply our downgrade target if not historical
                if (!isHistorical)
                {
                    buildingManager.UpdateBuildingInfo(buildingID, downgradeInfo);
                }

                // Post-downgrade processing to update instance values.
                ((BuildingAI)Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.GetAI()).BuildingUpgraded(buildingID, ref buildingManager.m_buildings.m_buffer[buildingID]);
            }
        }


        /// <summary>
        /// The reverse equivalent of "BuildingAI.GetUpgradeInfo"; attempts to find a valid downgrade target for the provided building.
        /// Will return null if the building is already at minimum level, is currently upgrading, or if no valid replacement can be found.
        /// Replacements follow the same rule as upgrades: same zoning type, same service and subservice, and same size.
        /// </summary>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="targetLevel">Target level of the downgrade</param>
        /// <returns>BuildingInfo reference of the target building, or null if there's no valid target</returns>
        public static BuildingInfo GetDowngradeInfo (ushort buildingID, byte targetLevel)
        {
            // Get an instance reference.
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            // Note this is a struct, not a class, so we don't write back to it, but it's handy for reads.
            Building thisBuilding = buildingManager.m_buildings.m_buffer[buildingID];

            // Return null if if target level is out of bounds (remember it's unsigned), or target building is in the middle of ugpgrading.
            if (targetLevel > (byte)ItemClass.Level.Level5 || thisBuilding.m_flags.IsFlagSet(Building.Flags.Upgrading))
            {
                return null;
            }

            // Check to see if this building is controlled by Ploppable RICO Revisited; if it is, we don't downgrade.
            if (ModUtils.CheckRICOPloppable(thisBuilding.Info))
            {
                return null;
            }

            // Check to see if this is an historical building; if so, we just return the original building info.
            if (((BuildingAI)thisBuilding.Info.GetAI()).IsHistorical(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], out bool canSet))
            {
                return thisBuilding.Info;
            }

            // Get our district and its style (for finding suitable random downgrade building).
                byte district = Singleton<DistrictManager>.instance.GetDistrict(thisBuilding.m_position);
            ushort style = Singleton<DistrictManager>.instance.m_districts.m_buffer[district].m_Style;

            // Get downgrade building target, if we can.
            return buildingManager.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, thisBuilding.Info.GetService(), thisBuilding.Info.GetSubService(), (ItemClass.Level)targetLevel, thisBuilding.Width, thisBuilding.Length, thisBuilding.Info.m_zoningMode, style);
        }
    }
}
