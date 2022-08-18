// <copyright file="SetHistoricalPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using HarmonyLib;

    /// <summary>
    /// Harmony Postfix patch to update ABLC building info panel when the building's historical status changes.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.SetHistorical))]
    internal static class SetHistoricalPatch
    {
        /// <summary>
        /// Harmony Postfix patch to update ABLC building info panel if the building's historical status changes.
        /// </summary>
        public static void Postfix()
        {
            if (BuildingPanelManager.Panel != null)
            {
                BuildingPanelManager.Panel.UpdatePanel();
            }
        }
    }
}
