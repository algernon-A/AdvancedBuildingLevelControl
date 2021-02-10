using ICities;
using ColossalFramework;


namespace ABLC
{
    /// <summary>
    /// Adds ABLC level up monitoring to game levelling up process.
    /// </summary>
    public class LevelUpExtension : LevelUpExtensionBase
    {
        /// <summary>
        /// Residential level up control - game extension method.
        /// </summary>
        /// <param name="levelUp">Original upgrade struct (target level and progress)</param>
        /// <param name="averageEducation">Average education for the building (ignored)</param>
        /// <param name="landValue">Land value for the building (ignored)</param>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="service">Building service (ignored)</param>
        /// <param name="subService">Building subservice (ignored)</param>
        /// <param name="currentLevel">Existing building level (ignored)</param>
        /// <returns>Revised target level (level and progress)</returns>
        public override ResidentialLevelUp OnCalculateResidentialLevelUp(ResidentialLevelUp levelUp, int averageEducation, int landValue,
            ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
            levelUp.targetLevel = GetTargetLevel(buildingID, levelUp.targetLevel, true);
            return levelUp;
        }


        /// <summary>
        /// Commercial level up control - game extension method.
        /// </summary>
        /// <param name="levelUp">Original upgrade struct (target level and progress)</param>
        /// <param name="averageWealth">Building average wealth (ignored)</param>
        /// <param name="landValue">Land value for the building (ignored)</param>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="service">Building service (ignored)</param>
        /// <param name="subService">Building subservice (ignored)</param>
        /// <param name="currentLevel">Existing building level (ignored)</param>
        /// <returns>Revised target level (level and progress)</returns>
        /// <returns></returns>
        public override CommercialLevelUp OnCalculateCommercialLevelUp(CommercialLevelUp levelUp, int averageWealth, int landValue,
            ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
            levelUp.targetLevel = GetTargetLevel(buildingID, levelUp.targetLevel);
            return levelUp;
        }


        /// <summary>
        /// Industrial level up control - game extension method.
        /// </summary>
        /// <param name="levelUp">Original upgrade struct (target level and progress)</param>
        /// <param name="averageEducation">Average education for the building (ignored)</param>
        /// <param name="serviceScore">Building service score (ignored)</param>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="service">Building service (ignored)</param>
        /// <param name="subService">Building subservice (ignored)</param>
        /// <param name="currentLevel">Existing building level (ignored)</param>
        /// <returns>Revised target level (level and progress)</returns>
        /// <returns></returns>
        public override IndustrialLevelUp OnCalculateIndustrialLevelUp(IndustrialLevelUp levelUp, int averageEducation, int serviceScore,
            ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
            levelUp.targetLevel = GetTargetLevel(buildingID, levelUp.targetLevel);
            return levelUp;
        }


        /// <summary>
        /// Office level up control - game extension method.
        /// </summary>
        /// <param name="levelUp">Original upgrade struct (target level and progress)</param>
        /// <param name="averageEducation">Average education for the building (ignored)</param>
        /// <param name="serviceScore">Building service score (ignored)</param>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="service">Building service (ignored)</param>
        /// <param name="subService">Building subservice (ignored)</param>
        /// <param name="currentLevel">Existing building level (ignored)</param>
        /// <returns>Revised target level (level and progress)</returns>
        /// <returns></returns>
        public override OfficeLevelUp OnCalculateOfficeLevelUp(OfficeLevelUp levelUp, int averageEducation, int serviceScore, ushort buildingID,
            Service service, SubService subService, Level currentLevel)
        {
            levelUp.targetLevel = GetTargetLevel(buildingID, levelUp.targetLevel);
            return levelUp;
        }


        /// <summary>
        /// Returns the maximum permissible level for this building; building settings (if any) taking priority over district settings.
        /// </summary>
        /// <param name="buildingID">Instance ID of this building</param>
        /// <param name="targetLevel">Level that the building is trying to level up to</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false (default) if workplace</param>
        /// <returns>Building maximum level</returns>
        private Level GetTargetLevel(ushort buildingID, Level targetLevel, bool isResidential = false)
        {
            Level maxLevel;
            
            // Check for individual building restrictions, as they have highest priority.
            if (BuildingsABLC.levelRanges.ContainsKey(buildingID))
            {
                // Get individual building maximum level.
                maxLevel = (Level)BuildingsABLC.levelRanges[buildingID].maxLevel;
            }
            else
            {
                // No building restrictions; check district for restrictions.
                ushort districtID = Singleton<DistrictManager>.instance.GetDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_position);

                // Get our maximum level for this district for this type of building.
                maxLevel = isResidential ? (Level)DistrictsABLC.maxResLevel[districtID] : (Level)DistrictsABLC.maxWorkLevel[districtID];
            }

            // If the maximum permissible level is less than the original target level, return the maximum level; otherwise, return original target level.
            return maxLevel < targetLevel ? maxLevel : targetLevel;
        }
    }
}