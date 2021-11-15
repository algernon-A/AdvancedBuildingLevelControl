using System.Collections.Generic;
using ColossalFramework;


namespace ABLC
{
    /// <summary>
    /// Basic level control data class (minimum and maximum).
    /// Class, not struct, as this is mutable, and Mutable Structs Are Evil.
    /// </summary>
    public class LevelRange
    {
        public byte minLevel;
        public byte maxLevel;
    }


    /// ABLC dictionary of individual building level settings.
    internal static class BuildingsABLC
    {
        // Master dictionary.
        private readonly static Dictionary<ushort, LevelRange> levelRanges = new Dictionary<ushort, LevelRange>();

        /// <summary>
        /// Gets the maximum level set for the given building.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <returns>Maximum level for building, if set (4 if no value is set)</returns>
        internal static byte GetMaxLevel(ushort buildingID)
        {
            // Attempt to get minimum level from dictionary.
            if (levelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange.maxLevel;
            }

            // If we got here, no value was retrieved; return the default.
            return 4;
        }


        /// <summary>
        /// Gets the maximum level set for the given building; first by checking building settings, and then (if none), district settings.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false if workplace</param>
        /// <returns>Maximum level for building, if set (4 if no value is set)</returns>
        internal static byte GetMaxLevel(ushort buildingID, bool isResidential)
        {
            // Attempt to get maximum level from dictionary.
            if (levelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange.maxLevel;
            }

            // If we got here, no value was retrieved; return the district setting.
            return DistrictsABLC.GetMaxLevel(buildingID, isResidential);
        }


        /// <summary>
        /// Gets the minimum level set for the given building.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <returns>Minimum level for building, if set (0 if no value is set)</returns>
        internal static byte GetMinLevel(ushort buildingID)
        {
            // Attempt to get minimum level from dictionary.
            if (levelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange.minLevel;
            }

            // If we got here, no value was retrieved; return the default.
            return 0;
        }


        /// <summary>
        /// Gets the minimum level set for the given building; first by checking building settings, and then (if none), district settings.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false if workplace</param>
        /// <returns>Minimum level for building, if set (0 if no value is set)</returns>
        internal static byte GetMinLevel(ushort buildingID, bool isResidential)
        {
            // Attempt to get maximum level from dictionary.
            if (levelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange.minLevel;
            }

            // If we got here, no value was retrieved; return the district setting.
            return DistrictsABLC.GetMinLevel(buildingID, isResidential);
        }


        /// <summary>
        /// Returns the level range entry (if any) for the specified building.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <returns>The LevelRange entry for the specified building, or null if none</returns>
        internal static LevelRange GetRecord(ushort buildingID)
        {
            // Return entry in dictionary, if it exists.
            if (levelRanges.TryGetValue(buildingID, out LevelRange levelRange))
            {
                return levelRange;
            }

            // If we got here, there wasn't an entry; return null.
            return null;
        }


        /// <summary>
        /// Updates the buildng's maximum level.
        /// </summary>
        /// <param name="buildingID">Building to set</param>
        /// <param name="maxLevel">New maximum level</param>
        internal static void UpdateMaxLevel(ushort buildingID, byte maxLevel)
        {
            // See if we've already got a dictionary entry for this building.
            if (levelRanges.ContainsKey(buildingID))
            {
                // We do - if this new maximum level is the maximum for this building and the minimum is zero, delete this entry.
                if (maxLevel == LevelUtils.GetMaxLevel(buildingID) && levelRanges[buildingID].minLevel == 0)
                {
                    levelRanges.Remove(buildingID);
                }
                else
                {
                    // Otherwise, just update our entry's maximum target level.
                    levelRanges[buildingID].maxLevel = maxLevel;
                }
            }
            else if (maxLevel < LevelUtils.GetMaxLevel(buildingID))
            {
                // If the new maximum level isn't the absolute maximum for this building, create a new dictionary entry with this maximum and default minimum levels.
                levelRanges.Add(buildingID, new LevelRange { minLevel = 0, maxLevel = maxLevel });
            }
        }


        /// <summary>
        /// Updates a buildng's minimum level.
        /// </summary>
        /// <param name="buildingID">Building to set</param>
        /// <param name="minLevel">New minimum level</param>
        internal static void UpdateMinLevel(ushort buildingID, byte minLevel)
        {
            // See if we've already got a dictionary entry for this building.
            if (levelRanges.ContainsKey(buildingID))
            {
                // We do - if this new minimum level is zero and the maximum for this building is set to the building's maximum, delete this entry.
                if (minLevel == 0 && levelRanges[buildingID].maxLevel == LevelUtils.GetMaxLevel(buildingID))
                {
                    levelRanges.Remove(buildingID);
                }
                else
                {
                    // Otherwise, just update our entry's minimum target level.
                    levelRanges[buildingID].minLevel = minLevel;
                }
            }
            else if (minLevel > 0)
            {
                // If the new minimum level isn't the absolute minimum, create a new dictionary entry with this minimum and default maximum levels.
                levelRanges.Add(buildingID, new LevelRange { minLevel = minLevel, maxLevel = LevelUtils.GetMaxLevel(buildingID) });
            }
        }


        /// <summary>
        /// Removes a building entry from the dictionary.
        /// </summary>
        /// <param name="buildingID">Building to remove</param>
        internal static void DeleteEntry(ushort buildingID)
        {
            // Check for valid entry, and if it exists, remove it.
            if (levelRanges.ContainsKey(buildingID))
            {
                levelRanges.Remove(buildingID);
            }
        }


        /// <summary>
        /// Clears all building settings for the given district.
        /// <param name="buildingID">Building to remove</param>
        /// </summary>
        internal static void ClearDistrict(ushort districtID)
        {
            // List of building IDs to remove.
            List<ushort> buildingIDs = new List<ushort>();

            // Local references.
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            Logging.Message("clearing building settings from district ", districtManager.GetDistrictName(districtID));

            // Iterate though each building with custom settings.
            foreach (ushort buildingID in levelRanges.Keys)
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
                foreach (ushort key in levelRanges.Keys)
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
                    levelRanges.Remove(key);
                }
            }
            else
            {
                Logging.Error("builidng manager didn't exist");
            }
        }


        /// <summary>
        /// Srializes savegame data from the dictionary.
        /// </summary>
        /// <param name="buildingKeys">Building key array (to save data)</param>
        /// <param name="minLevels">Minimum level array (to save data)</param>
        /// <param name="maxLevels">Maximum level array (to save data)</param>
        internal static void Serialize(out uint[] buildingKeys, out byte[] minLevels, out byte[] maxLevels)
        {
            // Get the number of entries required for our flat (serialisation) data arrays.
            int dataLength = levelRanges.Count;

            // Flat data arrays for serialisation.
            buildingKeys = new uint[dataLength];
            minLevels = new byte[dataLength];
            maxLevels = new byte[dataLength];

            // Iterate through dictionary, populating the serialisation arrays with data from each entry.
            int i = 0;
            foreach (ushort key in levelRanges.Keys)
            {
                LevelRange range = levelRanges[key];

                buildingKeys[i] = key;
                minLevels[i] = range.minLevel;
                maxLevels[i++] = range.maxLevel;
            }
        }


        /// <summary>
        /// Deserializes savegame data ino the dictionary.
        /// </summary>
        /// <param name="buildingKeys">Building key array (from save data)</param>
        /// <param name="minLevels">Minimum level array (from save data)</param>
        /// <param name="maxLevels">Maximum level array (from save data)</param>
        internal static void Deserialize(uint[] buildingKeys, byte[] minLevels, byte[] maxLevels)
        {
            // Populate dictionary with data from the arrays.
            for (int i = 0; i < buildingKeys.Length; ++i)
            {
                Logging.Message("deserializing building ", buildingKeys[i], " with level settings ", minLevels[i], ":", maxLevels[i]);
                levelRanges.Add((ushort)buildingKeys[i], new LevelRange { minLevel = minLevels[i], maxLevel = maxLevels[i] });
            }
        }
    }
}