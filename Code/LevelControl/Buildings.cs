// <copyright file="Buildings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using System.Collections.Generic;
    using AlgernonCommons;
    using ColossalFramework;

    /// <summary>
    /// Static class to hold the level control data for buildings.
    /// </summary>
    internal static class Buildings
    {
        // Master dictionary.
        private static readonly Dictionary<ushort, LevelRange> LevelRanges = new Dictionary<ushort, LevelRange>();

        /// <summary>
        /// Gets the maximum level set for the given building.
        /// </summary>
        /// <param name="buildingID">Building ID to check.</param>
        /// <returns>Maximum level for building, if set (4 if no value is set).</returns>
        internal static byte GetMaxLevel(ushort buildingID)
        {
            // Attempt to get minimum level from dictionary.
            if (LevelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange.MaxLevel;
            }

            // If we got here, no value was retrieved; return the default.
            return 4;
        }

        /// <summary>
        /// Gets the maximum level set for the given building; first by checking building settings, and then (if none), district settings.
        /// </summary>
        /// <param name="buildingID">Building ID to check.</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false if workplace.</param>
        /// <returns>Maximum level for building, if set (4 if no value is set).</returns>
        internal static byte GetMaxLevel(ushort buildingID, bool isResidential)
        {
            // Attempt to get maximum level from dictionary.
            if (LevelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange.MaxLevel;
            }

            // If we got here, no value was retrieved; return the district setting.
            return Districts.GetMaxLevel(buildingID, isResidential);
        }

        /// <summary>
        /// Gets the minimum level set for the given building.
        /// </summary>
        /// <param name="buildingID">Building ID to check.</param>
        /// <returns>Minimum level for building, if set (0 if no value is set).</returns>
        internal static byte GetMinLevel(ushort buildingID)
        {
            // Attempt to get minimum level from dictionary.
            if (LevelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange.MinLevel;
            }

            // If we got here, no value was retrieved; return the default.
            return 0;
        }

        /// <summary>
        /// Gets the minimum level set for the given building; first by checking building settings, and then (if none), district settings.
        /// </summary>
        /// <param name="buildingID">Building ID to check.</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false if workplace.</param>
        /// <returns>Minimum level for building, if set (0 if no value is set).</returns>
        internal static byte GetMinLevel(ushort buildingID, bool isResidential)
        {
            // Attempt to get maximum level from dictionary.
            if (LevelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange.MinLevel;
            }

            // If we got here, no value was retrieved; return the district setting.
            return Districts.GetMinLevel(buildingID, isResidential);
        }

        /// <summary>
        /// Returns the level range entry (if any) for the specified building.
        /// </summary>
        /// <param name="buildingID">Building ID to check.</param>
        /// <returns>The LevelRange entry for the specified building, or null if none.</returns>
        internal static LevelRange GetRecord(ushort buildingID)
        {
            // Return entry in dictionary, if it exists.
            if (LevelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange;
            }

            // If we got here, there wasn't an entry; return null.
            return null;
        }

        /// <summary>
        /// Updates the buildng's maximum level.
        /// </summary>
        /// <param name="buildingID">Building to set.</param>
        /// <param name="maxLevel">New maximum level.</param>
        internal static void UpdateMaxLevel(ushort buildingID, byte maxLevel)
        {
            // See if we've already got a dictionary entry for this building.
            if (LevelRanges.ContainsKey(buildingID))
            {
                // We do - if this new maximum level is the maximum for this building and the minimum is zero, delete this entry.
                if (maxLevel == LevelUtils.GetMaxLevel(buildingID) && LevelRanges[buildingID].MinLevel == 0)
                {
                    LevelRanges.Remove(buildingID);
                }
                else
                {
                    // Otherwise, just update our entry's maximum target level.
                    LevelRanges[buildingID].MaxLevel = maxLevel;
                }
            }
            else if (maxLevel < LevelUtils.GetMaxLevel(buildingID))
            {
                // If the new maximum level isn't the absolute maximum for this building, create a new dictionary entry with this maximum and default minimum levels.
                LevelRanges.Add(buildingID, new LevelRange { MinLevel = 0, MaxLevel = maxLevel });
            }
        }

        /// <summary>
        /// Updates a buildng's minimum level.
        /// </summary>
        /// <param name="buildingID">Building to set.</param>
        /// <param name="minLevel">New minimum level.</param>
        internal static void UpdateMinLevel(ushort buildingID, byte minLevel)
        {
            // See if we've already got a dictionary entry for this building.
            if (LevelRanges.ContainsKey(buildingID))
            {
                // We do - if this new minimum level is zero and the maximum for this building is set to the building's maximum, delete this entry.
                if (minLevel == 0 && LevelRanges[buildingID].MaxLevel == LevelUtils.GetMaxLevel(buildingID))
                {
                    LevelRanges.Remove(buildingID);
                }
                else
                {
                    // Otherwise, just update our entry's minimum target level.
                    LevelRanges[buildingID].MinLevel = minLevel;
                }
            }
            else if (minLevel > 0)
            {
                // If the new minimum level isn't the absolute minimum, create a new dictionary entry with this minimum and default maximum levels.
                LevelRanges.Add(buildingID, new LevelRange { MinLevel = minLevel, MaxLevel = LevelUtils.GetMaxLevel(buildingID) });
            }
        }

        /// <summary>
        /// Removes a building entry from the dictionary.
        /// </summary>
        /// <param name="buildingID">Building to remove.</param>
        internal static void DeleteEntry(ushort buildingID)
        {
            // Check for valid entry, and if it exists, remove it.
            if (LevelRanges.ContainsKey(buildingID))
            {
                LevelRanges.Remove(buildingID);
            }
        }

        /// <summary>
        /// Clears all district settings for the given district.
        /// </summary>
        /// <param name="districtID">District to remove.</param>
        internal static void ClearDistrict(ushort districtID)
        {
            // List of building IDs to remove.
            List<ushort> buildingIDs = new List<ushort>();

            // Local references.
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            Logging.Message("clearing building settings from district ", districtManager.GetDistrictName(districtID));

            // Iterate though each building with custom settings.
            foreach (ushort buildingID in LevelRanges.Keys)
            {
                // Check if the building is in this district.
                if (districtManager.GetDistrict(buildingBuffer[buildingID].m_position) == districtID)
                {
                    // It is - add to list of custom settings to remove (can't remove them now because we're enumerating).
                    buildingIDs.Add(buildingID);
                }
            }

            // Now remove from our custom settings dictionary all building IDs that we've collected.
            foreach (ushort buildingID in buildingIDs)
            {
                DeleteEntry(buildingID);
            }
        }

        /// <summary>
        /// Checks the building dictionary and removes entries corresponding to non-existent buildings.
        /// </summary>
        internal static void CheckEntries()
        {
            // Make sure that the building manager has been initialised before proceeding.
            if (BuildingManager.exists)
            {
                // List of buildings with data loaded from savegame but which no longer exist.
                List<ushort> nullBuildings = new List<ushort>();

                // List of building instances from BuildingManager.
                Building[] buildingInstances = BuildingManager.instance.m_buildings.m_buffer;

                // Iterate through each key loaded from the savegame.
                foreach (ushort key in LevelRanges.Keys)
                {
                    // If this building instance no longer exists, add it to our list of non-existent buildings.
                    if (buildingInstances[key].m_flags == Building.Flags.None)
                    {
                        nullBuildings.Add(key);
                    }
                }

                // Iterate through each key in our non-existent buildings list and remove it from our building settings dictionary.
                foreach (ushort key in nullBuildings)
                {
                    LevelRanges.Remove(key);
                }
            }
            else
            {
                Logging.Error("builidng manager didn't exist");
            }
        }

        /// <summary>
        /// Serializes savegame data from the dictionary.
        /// </summary>
        /// <param name="buildingKeys">Building key array (to save data).</param>
        /// <param name="minLevels">Minimum level array (to save data).</param>
        /// <param name="maxLevels">Maximum level array (to save data).</param>
        internal static void Serialize(out uint[] buildingKeys, out byte[] minLevels, out byte[] maxLevels)
        {
            // Get the number of entries required for our flat (serialisation) data arrays.
            int dataLength = LevelRanges.Count;

            // Flat data arrays for serialisation.
            buildingKeys = new uint[dataLength];
            minLevels = new byte[dataLength];
            maxLevels = new byte[dataLength];

            // Iterate through dictionary, populating the serialisation arrays with data from each entry.
            int i = 0;
            foreach (ushort key in LevelRanges.Keys)
            {
                LevelRange range = LevelRanges[key];

                buildingKeys[i] = key;
                minLevels[i] = range.MinLevel;
                maxLevels[i++] = range.MaxLevel;
            }
        }

        /// <summary>
        /// Deserializes savegame data ino the dictionary.
        /// </summary>
        /// <param name="buildingKeys">Building key array (from save data).</param>
        /// <param name="minLevels">Minimum level array (from save data).</param>
        /// <param name="maxLevels">Maximum level array (from save data).</param>
        internal static void Deserialize(uint[] buildingKeys, byte[] minLevels, byte[] maxLevels)
        {
            // Populate dictionary with data from the arrays.
            for (int i = 0; i < buildingKeys.Length; ++i)
            {
                Logging.Message("deserializing building ", buildingKeys[i], " with level settings ", minLevels[i], ":", maxLevels[i]);
                LevelRanges.Add((ushort)buildingKeys[i], new LevelRange { MinLevel = minLevels[i], MaxLevel = maxLevels[i] });
            }
        }

        /// <summary>
        /// Basic level control data class (minimum and maximum).
        /// Class, not struct, as this is mutable, and Mutable Structs Are Evil.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Simple data class")]
        public class LevelRange
        {
            /// <summary>
            /// Minimum building level.
            /// </summary>
            public byte MinLevel;

            /// <summary>
            /// Maximum building level.
            /// </summary>
            public byte MaxLevel;
        }
    }
}