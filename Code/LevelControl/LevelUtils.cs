﻿using ColossalFramework;
using ColossalFramework.Math;


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
        /// Upgrades/downgrades the selected building to the given level, if possible.
        /// </summary>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="targetLevel">Level to upgrade/downgrade to</param>
        public static void ForceLevel(ushort buildingID, byte targetLevel)
        {
            // BuildingInfo to change to, if this building isn't historical.
            BuildingInfo targetInfo = null;


            // Get an instance reference.
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            // Get building references.
            BuildingInfo buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;
            PrivateBuildingAI buildingAI = buildingInfo?.GetAI() as PrivateBuildingAI;

            if (buildingInfo == null || buildingAI == null)
            {
                // If something went wrong, abort.
                Debugging.Message("couldn't get existing building info");
                return;
            }

            // Check to see if this is historical or not, or is a RICO ploppable.
            bool isHistorical = buildingAI.IsHistorical(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], out bool canSet) || ModUtils.CheckRICOPloppable(buildingInfo);

            // Get target prefab (if needed, i.e. not historical or RICO ploppable).
            if (!isHistorical)
            {
                // Get downgrade building target.
                targetInfo = GetTargetInfo(buildingID, targetLevel);
                if (targetInfo == null)
                {
                    // If we failed, don't do anything more.
                    return;
                }
            }

            // If we have a valid downgrade target, proceed.
            if (isHistorical || targetInfo != null)
            {
                // Apply target level to our building and cancel all level-up progress.
                buildingManager.m_buildings.m_buffer[buildingID].m_level = targetLevel;
                buildingManager.m_buildings.m_buffer[buildingID].m_levelUpProgress = 0;

                // Apply our downgrade target if not historical
                if (!isHistorical)
                {
                    buildingManager.UpdateBuildingInfo(buildingID, targetInfo);
                }

                // Post-downgrade processing to update instance values.
                CustomBuildingUpgraded(buildingID, ref buildingManager.m_buildings.m_buffer[buildingID], buildingAI);
            }
        }


        /// <summary>
        /// The universal equivalent of "BuildingAI.GetUpgradeInfo"; attempts to find a valid upgrade/downgrade target for the provided building.
        /// Will return null if the target level is out of bounds, if the building is currently upgrading, or if no valid replacement can be found.
        /// If the building is historical or a RICO ploppable, will return the existing BuildingInfo for that building.
        /// Replacements follow the same rule as upgrades: same zoning type, same service and subservice, and same size.
        /// </summary>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="targetLevel">Target level of the downgrade</param>
        /// <returns>BuildingInfo reference of the target building, or null if there's no valid target</returns>
        public static BuildingInfo GetTargetInfo (ushort buildingID, byte targetLevel)
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

            // Check to see if this is an historical or RICO Ploppable building; if so, we just return the original building info.
            if (((BuildingAI)thisBuilding.Info.GetAI()).IsHistorical(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], out bool canSet) || ModUtils.CheckRICOPloppable(thisBuilding.Info))
            {
                return thisBuilding.Info;
            }

            // Get our district and its style (for finding suitable random downgrade building).
            byte district = Singleton<DistrictManager>.instance.GetDistrict(thisBuilding.m_position);
            ushort style = Singleton<DistrictManager>.instance.m_districts.m_buffer[district].m_Style;

            // Get new building target, if we can.
            return buildingManager.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, thisBuilding.Info.GetService(), thisBuilding.Info.GetSubService(), (ItemClass.Level)targetLevel, thisBuilding.Width, thisBuilding.Length, thisBuilding.Info.m_zoningMode, style);
        }


        /// <summary>
        /// Custom implementation of PrivateBuildingAI.BuildingUpgraded that takes into account that our levels can be upgraded OR downgraded.
        /// </summary>
        /// <param name="buildingID"></param>
        /// <param name="data"></param>
        /// <param name="buildingAI"></param>
        private static void CustomBuildingUpgraded(ushort buildingID, ref Building data, PrivateBuildingAI buildingAI)
        {
            buildingAI.CalculateWorkplaceCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length, out int level, out int level2, out int level3, out int level4);
            buildingAI.AdjustWorkplaceCount(buildingID, ref data, ref level, ref level2, ref level3, ref level4);
            int workCount = level + level2 + level3 + level4;
            int homeCount = buildingAI.CalculateHomeCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length);
            int visitCount = buildingAI.CalculateVisitplaceCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length);
            ReversePatches.EnsureCitizenUnits(buildingAI, buildingID, ref data, homeCount, workCount, visitCount, 0);
        }
    }
}
