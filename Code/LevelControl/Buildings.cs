using System.Collections.Generic;
using ColossalFramework.IO;


namespace ABLC
{
    /// ABLC dictionary of individual building level settings.
    public static class BuildingsABLC
    {
        public static Dictionary<ushort, LevelRange> levelRanges; 
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

            Debugging.Message("writing building settings");

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
            Debugging.Message("reading building settings");

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