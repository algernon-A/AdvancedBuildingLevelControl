// <copyright file="BuildingExtension.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using ColossalFramework;
    using ColossalFramework.Math;
    using ICities;
    using UnityEngine;

    /// <summary>
    /// Building extension method class.  Used to implement level control for new buildings.
    /// </summary>
    public class BuildingExtension : BuildingExtensionBase
    {
        /// <summary>
        /// Adjust building's intial spawn level according to district settings.
        /// Called by the game as part of the building spawn calculation process.
        /// </summary>
        /// <param name="position">Building position.</param>
        /// <param name="spawn">Initial building spawn data from game.</param>
        /// <returns>Modified spawn data.</returns>
        public override SpawnData OnCalculateSpawn(Vector3 position, SpawnData spawn)
        {
            // Don't do anything for specialized industry.
            if (spawn.subService == SubService.IndustrialForestry || spawn.subService == SubService.IndustrialFarming || spawn.subService == SubService.IndustrialOil || spawn.subService == SubService.IndustrialOre)
            {
                return spawn;
            }

            // Get district.
            ushort districtID = Singleton<DistrictManager>.instance.GetDistrict(position);

            // Get our minimum level for this district for this type of building - our default is for this to be the spawn level.
            Level spawnLevel = (Level)Districts.GetDistrictMin(districtID, spawn.service == Service.Residential);

            // See if we have random spawning levels.
            if (Districts.GetFlag(districtID, (byte)Districts.DistrictFlags.RandomSpawnLevels))
            {
                // Yes - get max level then choose a random level between min and max.
                Level maxLevel = (Level)Districts.GetDistrictMax(districtID, spawn.service == Service.Residential);

                // Get our possible range.
                uint levelRange = (uint)(maxLevel - spawnLevel);

                // Only go further if we've got more than one level to work with, otherwise what's the point?
                if (levelRange > 0)
                {
                    // Create new randomizer (seeded with spawn position, because it's nice and pseudo-random and convenient and simple and why not).
                    Randomizer r = new Randomizer((int)(position.x + position.y + position.z));

                    // Use randomizer to generate a random level between min and max (note max has to be +1'd to get full range).
                    spawnLevel = (Level)((uint)spawnLevel + r.UInt32(levelRange + 1));
                }
            }

            // Set our spawning level to our result.
            spawn.level = spawnLevel;
            return spawn;
        }

        /// <summary>
        /// Sets building's initial historical state accoding to district settings.
        /// Called by the game after building has spawned.
        /// </summary>
        /// <param name="id">Building ID.</param>
        public override void OnBuildingCreated(ushort id)
        {
            // Don't do anything if we haven't yet loaded into game or district data hasn't been set.
            if (!Loading.IsLoaded)
            {
                return;
            }

            // Get building record.
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            // Get district.
            ushort districtID = Singleton<DistrictManager>.instance.GetDistrict(buildingBuffer[id].m_position);

            // Set historical state.
            (buildingBuffer[id].Info?.GetAI() as BuildingAI)?.SetHistorical(id, ref buildingBuffer[id], Districts.GetFlag(districtID, (byte)Districts.DistrictFlags.SpawnHistorical));
        }

        /// <summary>
        /// Checks to see if a released building has a custom level settting, and if so, removes that setting.
        /// Called by the game when a building is released.
        /// </summary>
        /// <param name="id">Building ID.</param>
        public override void OnBuildingReleased(ushort id) => Buildings.DeleteEntry(id);
    }
}
