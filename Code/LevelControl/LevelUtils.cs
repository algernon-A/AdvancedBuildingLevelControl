using ColossalFramework;


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
        public static void ForceLevelUp(ushort buildingID)
        {
            // Don't force upgrade if building is already upgrading.
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_flags.IsFlagSet(Building.Flags.Upgrading))
            {
                Debugging.Message("building " + buildingID + " is already upgrading");
                return;
            }

            // Don't force upgrade if building is already at maximum level.
            // Note GetMaxLevel is 1-based, m_level is 0-based, so +1 for difference.
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_level + 1 >= GetMaxLevel(buildingID))
            {
                Debugging.Message("building " + buildingID + " is already at maximum level");
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
                Debugging.Message("force upgrading building " + buildingID);
                buildingAI.StartUpgrading(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID]);
            }
        }
    }
}
