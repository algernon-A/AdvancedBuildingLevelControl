// <copyright file="WorldInfoPanelPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using HarmonyLib;

    /// <summary>
    /// Harmony Postfix patches to update ABLC panels when WorldInfoPanel target changes.
    /// </summary>
    [HarmonyPatch]
    internal static class WorldInfoPanelPatches
    {
        /// <summary>
        /// Harmony Postfix patch to update ABLC district info panel when district selection changes.
        /// </summary>
        [HarmonyPatch(typeof(DistrictWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void DistrictPostfix()
        {
            DistrictPanelManager.Panel?.DistrictChanged();
        }

        /// <summary>
        /// Harmony Postfix patch to update ABLC building info panel when building selection changes.
        /// </summary>
        [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void BuildingPostfix()
        {
            BuildingPanelManager.TargetChanged();
        }
    }
}