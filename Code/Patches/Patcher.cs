// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using System;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using CitiesHarmony.API;
    using HarmonyLib;

    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public class Patcher : PatcherBase
    {
        /// <summary>
        /// Applies or unapplies Building Themees patch.
        /// </summary>
        public void PatchBuildingThemes()
        {
            // Ensure Harmony is ready before patching.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Harmony harmonyInstance = new Harmony(HarmonyID);

                // Patch method.
                MethodInfo patchMethod = typeof(GetUpgradeInfoPatch).GetMethod(nameof(GetUpgradeInfoPatch.Prefix));

                MethodInfo[] targets = ModUtils.BuildingThemesReflection(out MethodInfo randomBuildingInfo);

                if (targets != null && randomBuildingInfo != null)
                {
                    foreach (MethodInfo targetMethod in targets)
                    {
                        Logging.Message("patching ", targetMethod);
                        harmonyInstance.Patch(targetMethod, prefix: new HarmonyMethod(patchMethod));
                    }

                    // Set delegate to Building Themes' random building prefab selection method.
                    GetUpgradeInfoPatch.BuildingTheme = Delegate.CreateDelegate(typeof(GetUpgradeInfoPatch.BuildingThemeDelegate), randomBuildingInfo) as GetUpgradeInfoPatch.BuildingThemeDelegate;
                }
                else
                {
                    Logging.KeyMessage("didn't patch Building Themes");
                }
            }
            else
            {
                Logging.Error("Harmony not ready");
            }
        }
    }
}