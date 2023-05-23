// <copyright file="BuildingUpgradedPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.Math;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches (Prefix and Postfix) for PrivateBuildingAI.BuildingUpgraded.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.BuildingUpgraded))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class BuildingUpgradedPatch
    {
        /// <summary>
        /// Harmony pre-emptive Prefix patch to catch any buildings trying to upgrade beyond their maximum permitted level, and to decouple building levels from prefab levels on upgrade (to enable upgrade model randomisation).
        /// </summary>
        /// <param name="__instance">Instance reference.</param>
        /// <param name="buildingID">Building ID.</param>
        /// <param name="data">Building data.</param>
        /// <returns>False (pre-empt game method) if this is a private building AI, true (execute game method) otherwise.</returns>
        public static bool Prefix(PrivateBuildingAI __instance, ushort buildingID, ref Building data)
        {
            byte maxLevel = LevelUtils.GetMaxLevel(buildingID);

            // Reset building seed.
            LevelUtils.ClearBuildingSeed(buildingID);

            // Check against maxLevel (m_level is zero-based, maxLevel is 1-based, so >= to catch overflows).
            if (data.m_level >= maxLevel)
            {
                Logging.Error("prevented building ", buildingID, " (", __instance.m_info.name, ") from upgrading to illegal level ", data.m_level + 1, "; setting to ", maxLevel);
                data.m_level = (byte)(maxLevel - 1);
            }

            // Override original method if we have a private building AI.
            if (data.Info.m_buildingAI is PrivateBuildingAI buildingAI)
            {
                // Historical building target level is already applied in PrivateBuildingAI.StartUpgrading.
                if ((data.m_flags & Building.Flags.Historical) == 0)
                {
                    // Non-historical buildings need their level increased manually here.
                    data.m_level += 1;
                }

                // Update building CitizenUnits to match new state.
                buildingAI.CalculateWorkplaceCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length, out int level, out int level2, out int level3, out int level4);
                buildingAI.AdjustWorkplaceCount(buildingID, ref data, ref level, ref level2, ref level3, ref level4);
                int workCount = level + level2 + level3 + level4;
                int homeCount = buildingAI.CalculateHomeCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length);
                int visitCount = buildingAI.CalculateVisitplaceCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length);
                ReversePatches.EnsureCitizenUnits(buildingAI, buildingID, ref data, homeCount, workCount, visitCount, 0, 0);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Harmony Postfix patch to ensure valid building levels and to keep a building upgrading until it reaches the minimum controlled level.
        /// </summary>
        /// <param name="buildingID">Building ID.</param>
        /// <param name="data">Building data.</param>
        public static void Postfix(ushort buildingID, ref Building data)
        {
            // If the minimum permissible level is greater than the current building level, force another upgrade.
            if (Buildings.GetMinLevel(buildingID, data.Info.GetService() == ItemClass.Service.Residential) > data.m_level)
            {
                Singleton<SimulationManager>.instance.AddAction(() =>
                {
                    ((Action<ushort>)LevelUtils.TriggerLevelUp).Invoke(buildingID);
                });
            }
        }
    }
}
