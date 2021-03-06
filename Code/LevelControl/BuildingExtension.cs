﻿using UnityEngine;
using ICities;
using ColossalFramework;
using ColossalFramework.Math;


namespace ABLC
{
    /// <summary>
    /// Building extension method class.  Used to implement level control for new buildings.
    /// </summary>
    public class BuildingExtension : BuildingExtensionBase
    {
        /// <summary>
        /// Adjust building's intial spawn level according to district settings.
        /// Called by the game as part of the building spawn calculation process.
        /// </summary>
        /// <param name="position">Building position</param>
        /// <param name="spawn">Initial building spawn data from game</param>
        /// <returns>Modified spawn data</returns>
        public override SpawnData OnCalculateSpawn(Vector3 position, SpawnData spawn)
        {
            // Get district.
            ushort districtID = Singleton<DistrictManager>.instance.GetDistrict(position);

            // Get our minimum level for this district for this type of building - our default is for this to be the spawn level.
            Level spawnLevel = spawn.service == Service.Residential ? (Level)DistrictsABLC.minResLevel[districtID] : (Level)DistrictsABLC.minWorkLevel[districtID];

            // See if we have random spawning levels.
            if ((DistrictsABLC.flags[districtID] & (byte)DistrictFlags.randomSpawnLevels) != 0)
            {
                // Yes - get max level then choose a random level between min and max.
                Level maxLevel = spawn.service == Service.Residential ? (Level)DistrictsABLC.maxResLevel[districtID] : (Level)DistrictsABLC.maxWorkLevel[districtID];

                // Get our possible range.
                uint levelRange = (uint)(maxLevel - spawnLevel);

                // Only go further if we've got more than one level to work with, otherwise what's the point?
                if (levelRange > 0)
                {
                    // Create new randomizer (seeded with spawn position, because it's nice and pseudo-random and convenient and simple and why not).
                    Randomizer r = new Randomizer((int)(position.x + position.y + position.z));

                    // Use randomizer to generate a random level between min and max (note max has to be +1'd to get full range).
                    spawnLevel = (Level)((uint)(spawnLevel) + r.UInt32(levelRange + 1));
                }
            }

            // Set our spawning level to our result.
            spawn.level = spawnLevel;
            return spawn;
        }


        /// <summary>
        /// Checks to see if a released building has a custom level settting, and if so, removes that setting.
        /// Called by the game when a building instance is released.
        /// </summary>
        /// <param name="id">Building instance ID</param>
        public override void OnBuildingReleased(ushort id)
        {
            // Check to see if our dictionary is ready, and if it contains this building ID.
            if (BuildingsABLC.levelRanges != null && BuildingsABLC.levelRanges.ContainsKey(id))
            {
                // It does; delete the entry.
                BuildingsABLC.levelRanges.Remove(id);
            }
        }
    }
}
