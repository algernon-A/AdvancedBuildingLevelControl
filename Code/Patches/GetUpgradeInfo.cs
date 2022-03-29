using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;


namespace ABLC
{
    /// <summary>
    /// Harmony Prefix patch for PrivateBuildingAI.GetUpgradeInfo.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.GetUpgradeInfo))]
    public static class GetUpgradeInfo
    {
        // Delegate to Building Themes GetRandomBuildingInfo_Upgrade.
        public delegate BuildingInfo BuildingThemeDelegate(Vector3 position, ushort prefabIndex, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style);
        internal static BuildingThemeDelegate buildingTheme;


        /// <summary>
        /// Harmony Prefix patch to randomize levels of potential upgrade targets (+/- 1 level).
        /// </summary>
        /// <param name="__instance">Instance reference</param>
        /// <param name="buildingID">Building instance ID</param>
        /// <param name="data">Building data</param>
        /// <returns>False (don't execute original method) if mod settings are set to randomize building levels, true (exectue original method) otherwise</returns>
        public static bool Prefix(PrivateBuildingAI __instance, ref BuildingInfo __result, ushort buildingID, ref Building data)
        {
            // If we're not randomizing building levels, continue on to original method.
            if (!ModSettings.randomLevels)
            {
                return true;
            }

            // Randomize building level.
            Randomizer r = new Randomizer(buildingID);
            for (int i = 0; i <= data.m_level; i++)
            {
                r.Int32(1000u);
            }
            // Randomize level to +/- 1 level from original.
            int maxLevel = LevelUtils.GetMaxLevel(data.Info.GetSubService()) - 1;   // GetMaxLevel is 1-based, convert to zero-based.
            int finalLevel = data.m_level + 2 - r.Int32(3);

            // Clamp level.
            if (finalLevel < 0)
            {
                finalLevel = 0;
            }
            else if (finalLevel > maxLevel)
            {
                finalLevel = maxLevel;
            }

            Logging.Message("Building ", buildingID, " upgrade from ", (ItemClass.Level)data.m_level, " assigned random final level ", (ItemClass.Level)finalLevel);

            // Get target info.
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(data.m_position);
            ushort style = instance.m_districts.m_buffer[district].m_Style;


            // Are we using Building Themes?
            if (buildingTheme != null)
            {
                // Yes - get result via delegate to Building Themes' method.
                __result = buildingTheme(data.m_position, data.m_infoIndex, ref r, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, (ItemClass.Level)finalLevel, data.Width, data.Length, __instance.m_info.m_zoningMode, style);
            }
            else
            {
                // No - get result via base-game method.
                __result = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref r, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, (ItemClass.Level)finalLevel, data.Width, data.Length, __instance.m_info.m_zoningMode, style);
            }

            // Don't execute original method.
            return false;
        }
    }
}
