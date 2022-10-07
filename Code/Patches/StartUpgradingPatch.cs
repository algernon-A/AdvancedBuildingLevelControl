// <copyright file="StartUpgradingPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using HarmonyLib;

    /// <summary>
    /// Harmony patches for PrivateBuildingAI.StartUpgrading to handle situations where the target prefab is the same as the existing prefab.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.StartUpgrading))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class StartUpgradingPatch
    {
        /// <summary>
        /// Harmony prefix for PrivateBuildingAI.StartUpgrading to give temporary 'historical' status to buildings upgrading to the same BuildingInfo.
        /// This is a quick and easy method of ensuring proper upgrading when the prefab isn't changing.
        /// </summary>
        /// <param name="__instance">PrivateBuildingAI instance.</param>
        /// <param name="buildingID">Building ID.</param>
        /// <param name="buildingData">Building data reference.</param>
        /// <param name="__state">Flag for postfix to determine if historical flag should be cleared.</param>
        private static void Prefix(PrivateBuildingAI __instance, ushort buildingID, ref Building buildingData, out bool __state)
        {
            __state = false;

            if ((buildingData.m_flags & Building.Flags.Historical) == 0 && __instance.GetUpgradeInfo(buildingID, ref buildingData) == buildingData.Info)
            {
                buildingData.m_flags |= Building.Flags.Historical;
                __state = true;
            }
        }

        /// <summary>
        /// Harmony postfix for PrivateBuildingAI.StartUpgrading to remove temporary 'historical' status applied by prefix./
        /// </summary>
        /// <param name="buildingData">Building data reference.</param>
        /// <param name="__state">Flag from prefix indicating whether 'historical' flag was artificially set.</param>
        private static void Postfix(ref Building buildingData, bool __state)
        {
            // If the building's historical flag was set by the prefix, we clear it here.
            if (__state)
            {
                buildingData.m_flags &= ~Building.Flags.Historical;
            }
        }
    }
}
