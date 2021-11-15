﻿using ColossalFramework;
using ColossalFramework.IO;


namespace ABLC
{
    // District flags enum.
    internal enum DistrictFlags
    {
        none = 0x0,
        randomSpawnLevels = 0x1,
        spawnHistorical = 0x2
    }


    /// <summary>
    /// Static class to hold the level control data for districts.
    /// </summary>
    internal static class DistrictsABLC
    {
        // Level data arrays.
        internal static byte[] minResLevel, maxResLevel, minWorkLevel, maxWorkLevel;

        // District flags.
        internal static byte[] flags;


        /// <summary>
        /// Gets the maximum level set for the given building's current district.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false if workplace</param>
        /// <returns>Maximum level for building set in the current district, if set (4 (residential) or 2 (workplace) if no value is set)</returns>
        internal static byte GetMaxLevel(ushort buildingID, bool isResidential)
        {
            // Get district from building location.
            ushort districtID = Singleton<DistrictManager>.instance.GetDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_position);

            // Residential or workplace?
            if (isResidential)
            {
                // Residential - check that array has ben initialised, and if so, return the relevant district maximum.
                if (maxResLevel != null)
                {
                    return maxResLevel[districtID];
                }

                // If we got here, no value was retrieved; return the default.
                return 4;
            }
            else
            {
                // Workplace - check that array has ben initialised, and if so, return the relevant district maximum.
                if (maxWorkLevel != null)
                {
                    return maxWorkLevel[districtID];
                }

                // If we got here, no value was retrieved; return the default.
                return 2;
            }
        }


        /// <summary>
        /// Gets the minimum level set for the given building's current district.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false (default) if workplace</param>
        /// <returns>minimum level for building set in the current district, if set (0 if no value is set)</returns>
        internal static byte GetMinLevel(ushort buildingID, bool isResidential)
        {
            // Get district from building location.
            ushort districtID = Singleton<DistrictManager>.instance.GetDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_position);

            // Residential or workplace?
            if (isResidential)
            {
                // Residential - check that array has ben initialised, and if so, return the relevant district maximum.
                if (minResLevel != null)
                {
                    return minResLevel[districtID];
                }

                // If we got here, no value was retrieved; return the default.
                return 0;
            }
            else
            {
                // Workplace - check that array has ben initialised, and if so, return the relevant district maximum.
                if (minWorkLevel != null)
                {
                    return minWorkLevel[districtID];
                }

                // If we got here, no value was retrieved; return the default.
                return 0;
            }
        }
    }


    /// <summary>
    ///  Savegame (de)serialisation for district level settings.
    /// </summary>
    public class DistrictSerializer : IDataContainer
    {
        /// <summary>
        /// Serialise to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void Serialize(DataSerializer serializer)
        {
            Logging.Message("writing district settings");

            // Write district level arrays to savegame.
            serializer.WriteByteArray(DistrictsABLC.minResLevel);
            serializer.WriteByteArray(DistrictsABLC.maxResLevel);
            serializer.WriteByteArray(DistrictsABLC.minWorkLevel);
            serializer.WriteByteArray(DistrictsABLC.maxWorkLevel);

            // Extended attributes - starting with version flag.
            serializer.WriteInt16(0);
            serializer.WriteByteArray(DistrictsABLC.flags);
        }


        /// <summary>
        /// Deseralise from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.Message("reading district settings");

            // Read data from savegame into flat arrays.
            DistrictsABLC.minResLevel = serializer.ReadByteArray();
            DistrictsABLC.maxResLevel = serializer.ReadByteArray();
            DistrictsABLC.minWorkLevel = serializer.ReadByteArray();
            DistrictsABLC.maxWorkLevel = serializer.ReadByteArray();

            // Try to extended attributes - original version didn't have these.
            try
            {
                // Read version, but ignore it for now.
                int version = serializer.ReadInt16();

                // Read flags.
                DistrictsABLC.flags = serializer.ReadByteArray();
            }
            catch
            {
                // Don't care if we can't read them; 
                DistrictsABLC.flags = null;
            }
        }


        /// <summary>
        /// Performs post-deserialisation check, ensuring that arrays are of correct size.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
            Logging.Message("starting district post-deserialization");

            // If any array is less than the required size, we ignore what was read and re-initialise the array with default values.
            if (DistrictsABLC.minResLevel == null || DistrictsABLC.minResLevel.Length < 128)
            {
                DistrictsABLC.minResLevel = ResetLevels(0, "MinResLevel");
            }
            if (DistrictsABLC.maxResLevel == null || DistrictsABLC.maxResLevel.Length < 128)
            {
                DistrictsABLC.maxResLevel = ResetLevels(4, "MaxResLevel");
            }
            if (DistrictsABLC.minWorkLevel == null || DistrictsABLC.minResLevel.Length < 128)
            {
                DistrictsABLC.minWorkLevel = ResetLevels(0, "MinWorkLevel");
            }
            if (DistrictsABLC.maxWorkLevel == null || DistrictsABLC.minResLevel.Length < 128)
            {
                DistrictsABLC.maxWorkLevel = ResetLevels(2, "MaxWorkLevel");
            }
            if (DistrictsABLC.maxWorkLevel == null || DistrictsABLC.minResLevel.Length < 128)
            {
                DistrictsABLC.maxWorkLevel = ResetLevels(2, "MaxWorkLevel");
            }
            if (DistrictsABLC.flags == null || DistrictsABLC.flags.Length < 128)
            {
                DistrictsABLC.flags = ResetLevels(0, "Flags");
            }
            Logging.Message("district settings read");
        }


        /// <summary>
        /// Resets a district level array to 128 bytes with the given default level.
        /// </summary>
        /// <param name="level">Default level to appluy</param>
        /// <param name="name">Name of district level array (for logging)</param>
        /// <returns>New district level array of 128 bytes pre-populated with the given default level</returns>
        private byte[] ResetLevels(byte level, string name)
        {
            // Return array.
            byte[] newArray = new byte[128];

            Logging.Error(name, " district settings incomplete; resetting");

            // Populate return array with given default level.
            for (int i = 0; i < 128; ++i)
            {
                newArray[i] = level;
            }

            return newArray;
        }
    }
}