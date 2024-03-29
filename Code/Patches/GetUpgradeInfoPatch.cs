﻿// <copyright file="GetUpgradeInfoPatch.cs" company="algernon (K. Algernon A. Sheppard)">
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
            // Determine target building level.
            ItemClass.Level finalLevel;
            Randomizer r;
            if (ModSettings.RandomLevels)
            {
                finalLevel = LevelUtils.GetRandomLevel(__instance.m_info, buildingID, (byte)(data.m_level + 1), false, out r);
            }
            else
            {
                finalLevel = (ItemClass.Level)data.m_level + 1;
                r = LevelUtils.GetRandomizer(buildingID);
            }

            // Get target info.
            __result = GetRandomInfo(__instance, finalLevel, ref data, ref r);

            // Allow upgrade using existing prefab if the upgrade-without-target setting is enabled.
            if (ModSettings.UpgradeWithoutTarget && __result == null)
            {
                __result = __instance.m_info;
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Selects a random target BuildingInfo for the given building, taking into account whether or not Building Themes is in use.
        /// </summary>
        /// <param name="buildingAI">Private building AI.</param>
        /// <param name="targetLevel">Target building level.</param>
        /// <param name="data">Building data reference.</param>
        /// <param name="r">Randomizer to use.</param>
        /// <returns>Selected target <see cref="BuildingInfo"/>, or <c>null</c> if none.</returns>
        internal static BuildingInfo GetRandomInfo(PrivateBuildingAI buildingAI, ItemClass.Level targetLevel, ref Building data, ref Randomizer r)
        {
            // Get target info.
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(data.m_position);
            ushort style = instance.m_districts.m_buffer[district].m_Style;

            // Are we using Building Themes?
            if (s_buildingTheme != null)
            {
                // Yes - get result via delegate to Building Themes' method.
                return s_buildingTheme(
                    data.m_position,
                    data.m_infoIndex,
                    ref r,
                    buildingAI.m_info.m_class.m_service,
                    buildingAI.m_info.m_class.m_subService,
                    targetLevel,
                    data.Width,
                    data.Length,
                    buildingAI.m_info.m_zoningMode,
                    style);
            }
            else
            {
                // No - get result via base-game method.
                return Singleton<BuildingManager>.instance.GetRandomBuildingInfo(
                    ref r,
                    buildingAI.m_info.m_class.m_service,
                    buildingAI.m_info.m_class.m_subService,
                    targetLevel,
                    data.Width,
                    data.Length,
                    buildingAI.m_info.m_zoningMode,
                    style);
            }
        }
    }
}
