using ColossalFramework;
using ColossalFramework.Math;


namespace ABLC
{
    internal static class LevelUtils
    {
        /// <summary>
        /// Returns the maximum building level of a given building, based on subclass (1-based).
        /// <param name="buildingID">Building instance ID</param>
        /// </summary>
        /// <returns>Maximum building level (1-based)</returns>
        internal static byte GetMaxLevel(ushort buildingID) => GetMaxLevel(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.m_class.m_subService);


        /// <summary>
        /// Returns the maximum building level of a given building, based on subclass (1-based).
        /// <param name="subService">Building subservice</param>
        /// </summary>
        /// <param name="subService">Building subservice</param>
        /// <returns>Maximum building level (1-based)</returns>
        internal static byte GetMaxLevel(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.ResidentialLow:
                case ItemClass.SubService.ResidentialHigh:
                case ItemClass.SubService.ResidentialLowEco:
                case ItemClass.SubService.ResidentialHighEco:
                    return 5;
                case ItemClass.SubService.CommercialLow:
                case ItemClass.SubService.CommercialHigh:
                case ItemClass.SubService.OfficeGeneric:
                case ItemClass.SubService.IndustrialGeneric:
                    return 3;
                case ItemClass.SubService.IndustrialFarming:
                case ItemClass.SubService.IndustrialForestry:
                case ItemClass.SubService.IndustrialOil:
                case ItemClass.SubService.IndustrialOre:
                    return 2;
                default:
                    return 1;
            }
        }


        /// <summary>
        /// Forces a building to upgrade.
        /// </summary>
        /// <param name="buildingID">Building instance ID</param>
        internal static void TriggerLevelUp(ushort buildingID)
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
                Logging.Error("couldn't get AI for building ", buildingID);
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
        internal static void ForceLevel(ushort buildingID, byte targetLevel)
        {
            // BuildingInfo to change to, if this building isn't historical.
            BuildingInfo targetInfo;

            // References.
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building[] buildingBuffer = buildingManager.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;
            PrivateBuildingAI buildingAI = buildingInfo?.GetAI() as PrivateBuildingAI;

            if (buildingAI == null)
            {
                // If something went wrong, abort.
                Logging.Error("couldn't get PrivateBuildingAI");
                return;
            }
            // Get upgrade/downgrade building target.
            targetInfo = GetTargetInfo(buildingID, targetLevel);

            // Ensure valid AI before proceeding.
            if (targetInfo?.GetAI() is PrivateBuildingAI newAI)
            {
                // Apply target level to our building and cancel all level-up progress.
                buildingBuffer[buildingID].m_level = targetLevel;
                buildingBuffer[buildingID].m_levelUpProgress = 0;

                // Reset upgrade-related flags.
                buildingBuffer[buildingID].m_flags = buildingBuffer[buildingID].m_flags & ~Building.Flags.LevelUpEducation & ~Building.Flags.LevelUpLandValue;

                // Apply updated info.
                buildingManager.UpdateBuildingInfo(buildingID, targetInfo);

                // Post-downgrade processing via custom method.
                CustomBuildingUpgraded(newAI, buildingID, ref buildingBuffer[buildingID]);

                // Update building render.
                buildingManager.UpdateBuildingRenderer(buildingID, true);

                // Set building panel updated flag if panel is open.
                if (BuildingPanelManager.Panel != null)
                {
                    BuildingPanelManager.Panel.updateReady = true;
                }
            }
        }


        /// <summary>
        /// The universal equivalent of "BuildingAI.GetUpgradeInfo"; attempts to find a valid upgrade/downgrade target for the provided building.
        /// Will return null if the target level is out of bounds, if the building is currently upgrading, or if no valid replacement can be found.
        /// If the building is historical or a RICO ploppable, will return the existing BuildingInfo for that building.
        /// Replacements follow the same rule as upgrades: same zoning type, same service and subservice, and same size.
        /// </summary>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="targetLevel">Target level to upgrade/downgrade to</param>
        /// <returns>BuildingInfo reference of the target building, or null if there's no valid target</returns>
        internal static BuildingInfo GetTargetInfo(ushort buildingID, byte targetLevel)
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
            if (((BuildingAI)thisBuilding.Info.GetAI()).IsHistorical(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], out bool _) || ModUtils.CheckRICOPloppable(thisBuilding.Info))
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
        /// Custom implementation of PrivateBuildingAI.BuildingUpgraded that takes into account that our levels can be upgraded OR downgraded; for use when current building level is below the set prefb leve.
        /// </summary>
        /// <param name="buildingAI">Building AI instance</param>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="data">Building data record</param>
        private static void CustomBuildingUpgraded(PrivateBuildingAI buildingAI, ushort buildingID, ref Building data)
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
