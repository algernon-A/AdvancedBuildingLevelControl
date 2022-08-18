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
        /// Initializes a new instance of the <see cref="Patcher"/> class.
        /// </summary>
        /// <param name="harmonyID">This mod's unique Harmony identifier.</param>
        public Patcher(string harmonyID)
            : base(harmonyID)
        {
        }

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        public static new Patcher Instance
        {
            get
            {
                // Auto-initializing getter.
                if (s_instance == null)
                {
                    s_instance = new Patcher(PatcherMod.Instance.HarmonyID);
                }

                return s_instance as Patcher;
            }
        }

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
                    Logging.Error("didn't patch Building Themes");
                }
            }
            else
            {
                Logging.Error("Harmony not ready");
            }
        }
    }
}