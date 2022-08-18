// <copyright file="ExternalCalls.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    /// <summary>
    /// Methods for use by other mods to interact with Advanced Building Level Control.
    /// </summary>
    public static class ExternalCalls
    {
        /// <summary>
        /// Locks the building level to the specified level.
        /// </summary>
        /// <param name="buildingID">Targeted building.</param>
        /// <param name="level">Level to set.</param>
        public static void LockBuildingLevel(ushort buildingID, ItemClass.Level level)
        {
            // Set max a min levels for this building to the specified levels.
            Buildings.UpdateMinLevel(buildingID, (byte)level);
            Buildings.UpdateMaxLevel(buildingID, (byte)level);
        }
    }
}