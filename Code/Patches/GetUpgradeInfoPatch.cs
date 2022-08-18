// <copyright file="GetUpgradeInfoPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using ColossalFramework;
    using ColossalFramework.Math;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony Prefix patch for PrivateBuildingAI.GetUpgradeInfo.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.GetUpgradeInfo))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class GetUpgradeInfoPatch
    {
        /// <summary>
        /// Delegate to Building Themes GetRandomBuildingInfo_Upgrade.
        /// </summary>
        private static BuildingThemeDelegate s_buildingTheme;

        /// <summary>
        /// Delegate to Building Themes GetRandomBuildingInfo_Upgrade.
        /// </summary>
        /// <param name="position">Building position.</param>
        /// <param name="prefabIndex">Building prefab index.</param>
        /// <param name="r">Randomizer.</param>
        /// <param name="service">Building service.</param>
        /// <param name="subService">Building sub-service.</param>
        /// <param name="level">Building level.</param>
        /// <param name="width">Building lot width.</param>
        /// <param name="length">Building lot length.</param>
        /// <param name="zoningMode">Building zoning mode.</param>
        /// <param name="style">District style.</param>
        /// <returns>Building prefab.</returns>
        public delegate BuildingInfo BuildingThemeDelegate(Vector3 position, ushort prefabIndex, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style);

        /// <summary>
        /// Sets the Building Themes delegate.
        /// </summary>
        public static BuildingThemeDelegate BuildingTheme { set => s_buildingTheme = value; }

        /// <summary>
        /// Harmony Prefix patch to randomize levels of potential upgrade targets (+/- 1 level).
        /// </summary>
        /// <param name="__instance">Instance reference.</param>
        /// <param name="__result">Original method result.</param>
        /// <param name="buildingID">Building ID.</param>
        /// <param name="data">Building data.</param>
        /// <returns>False (don't execute original method) if mod settings are set to randomize building levels, true (exectue original method) otherwise.</returns>
        public static bool Prefix(PrivateBuildingAI __instance, ref BuildingInfo __result, ushort buildingID, ref Building data)
        {
            // If we're not randomizing building levels, continue on to original method.
            if (!ModSettings.RandomLevels)
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

            // Get target info.
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(data.m_position);
            ushort style = instance.m_districts.m_buffer[district].m_Style;

            // Are we using Building Themes?
            if (s_buildingTheme != null)
            {
                // Yes - get result via delegate to Building Themes' method.
                __result = s_buildingTheme(
                    data.m_position,
                    data.m_infoIndex,
                    ref r,
                    __instance.m_info.m_class.m_service,
                    __instance.m_info.m_class.m_subService,
                    (ItemClass.Level)finalLevel,
                    data.Width,
                    data.Length,
                    __instance.m_info.m_zoningMode,
                    style);
            }
            else
            {
                // No - get result via base-game method.
                __result = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(
                    ref r,
                    __instance.m_info.m_class.m_service,
                    __instance.m_info.m_class.m_subService,
                    (ItemClass.Level)finalLevel,
                    data.Width,
                    data.Length,
                    __instance.m_info.m_zoningMode,
                    style);
            }

            // Don't execute original method.
            return false;
        }
    }
}
