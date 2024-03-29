﻿// <copyright file="ReversePatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using System;
    using System.Runtime.CompilerServices;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Static class for Harmony reverse patches.
    /// </summary>
    [HarmonyPatch]
    internal static class ReversePatches
    {
        /// <summary>
        /// Reverse patch for BuildingAI.EnsureCitizenUnits to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance.</param>
        /// <param name="buildingID">ID of this building (for game method).</param>
        /// <param name="data">Building data (for game method).</param>
        /// <param name="homeCount">Household count (for game method).</param>
        /// <param name="workCount">Workplace count (for game method).</param>
        /// <param name="visitCount">Visitor count (for game method).</param>
        /// <param name="studentCount">Student count (for game method).</param>
        /// <param name="hotelCount">Hotel capacity count (for game method).</param>
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(BuildingAI), "EnsureCitizenUnits")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void EnsureCitizenUnits(object instance, ushort buildingID, ref Building data, int homeCount, int workCount, int visitCount, int studentCount, int hotelCount)
        {
            Logging.Error("EnsureCitizenUnits reverse Harmony patch wasn't applied, params: ", instance, buildingID, data, homeCount, workCount, visitCount, studentCount, hotelCount);
            throw new NotImplementedException("Harmony reverse patch not applied");
        }
    }
}