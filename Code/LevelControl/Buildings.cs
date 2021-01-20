using System.Collections.Generic;
using ColossalFramework.IO;


namespace ABLC
{
    /// ABLC dictionary of individual building level settings.
    internal static class BuildingsABLC
    {
        // Master dictionary.
        internal static Dictionary<ushort, LevelRange> levelRanges;


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
                if (minLevel == 0 && BuildingsABLC.levelRanges[buildingID].maxLevel == LevelUtils.GetMaxLevel(buildingID))
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
                if (maxLevel == LevelUtils.GetMaxLevel(buildingID) && BuildingsABLC.levelRanges[buildingID].minLevel == 0)
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
    }


    /// <summary>
    /// Basic level control data class (minimum and maximum).
    /// Class, not struct, as this is mutable, and Mutable Structs Are Evil.
    /// </summary>
    public class LevelRange
    {
        public byte minLevel;
        public byte maxLevel;
    }


    /// <summary>
    ///  Savegame (de)serialisation for building level settings.
    /// </summary>
    public class BuildingSerializer : IDataContainer
    {
        /// <summary>
        /// Serialise to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void Serialize(DataSerializer serializer)
        {
            // Get the number of entries required for our flat (serialisation) data arrays.
            int dataLength = BuildingsABLC.levelRanges.Count;

            // Flat data arrays for serialisation.
            uint[] buildingKeys = new uint[dataLength];
            byte[] minLevels = new byte[dataLength];
            byte[] maxLevels = new byte[dataLength];

            // Iterate through dictionary, populating the serialisation arrays with data from each entry.
            int i = 0;
            foreach (ushort key in BuildingsABLC.levelRanges.Keys)
            {
                LevelRange range = BuildingsABLC.levelRanges[key];

                buildingKeys[i] = key;
                minLevels[i] = range.minLevel;
                maxLevels[i ++] = range.maxLevel;
            }

            Logging.Message("writing building settings");

            // Write serialisation arrays to savegame.
            serializer.WriteUInt32Array(buildingKeys);
            serializer.WriteByteArray(minLevels);
            serializer.WriteByteArray(maxLevels);
        }


        /// <summary>
        /// Deseralise from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.Message("reading building settings");

            // Read data from savegame into flat arrays.
            uint[] buildingKeys = serializer.ReadUInt32Array();
            byte[] minLevels = serializer.ReadByteArray();
            byte[] maxLevels = serializer.ReadByteArray();

            // Populate dictionary with data from the arrays.
            for (int i = 0; i < buildingKeys.Length; ++ i)
            {
                BuildingsABLC.levelRanges.Add((ushort)buildingKeys[i], new LevelRange { minLevel = minLevels[i], maxLevel = maxLevels[i] });
            }
        }


        /// <summary>
        /// Performs post-deserialisation check, removing entries corresponding to non-existent buildings.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
            // Make sure that the building manager has been initialised before proceeding.
            if(BuildingManager.exists)
            {
                // List of buildings with data loaded from savegame but which no longer exist.
                List<ushort> nullBuildings = new List<ushort>();

                // List of building instances from BuildingManager.
                Building[] buildingInstances = BuildingManager.instance.m_buildings.m_buffer;

                // Iterate through each key loaded from the savegame.
                foreach (ushort key in BuildingsABLC.levelRanges.Keys)
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
                    BuildingsABLC.levelRanges.Remove(key);
                }
            }
        }
    }
}