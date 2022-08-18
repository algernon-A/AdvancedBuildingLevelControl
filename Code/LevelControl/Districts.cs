namespace ABLC
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;

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
        // Level constants.
        private const byte MaxResLevel = 4;
        private const byte MaxWorkLevel = 2;

        // District size constant.
        private const byte NumDistricts = DistrictManager.MAX_DISTRICT_COUNT;


        // Level data arrays.
        private static byte[] minResLevels, maxResLevels, minWorkLevels, maxWorkLevels;

        // District flags.
        private static byte[] districtFlags;


        /// <summary>
        /// Accessor - minimum residential level array.
        /// </summary>
        internal static byte[] MinResLevels => minResLevels;


        /// <summary>
        /// Accessor - maximum residential level array.
        /// </summary>
        internal static byte[] MaxResLevels => maxResLevels;


        /// <summary>
        /// Accessor - minimum workplace level array.
        /// </summary>
        internal static byte[] MinWorkLevels => minWorkLevels;


        /// <summary>
        /// Accessor - maximum workplace level array.
        /// </summary>
        internal static byte[] MaxWorkLevels => maxWorkLevels;


        /// <summary>
        /// Accessor - distric flag array.
        /// </summary>
        internal static byte[] Flags => districtFlags;


        /// <summary>
        /// Gets the maximum level set for the given building's current district.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false if workplace</param>
        /// <returns>Maximum level for building set in the current district, if set (4 (residential) or 2 (workplace) if no value is set)</returns>
        internal static byte GetMaxLevel(ushort buildingID, bool isResidential) => GetDistrictMax(Singleton<DistrictManager>.instance.GetDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_position), isResidential);


        /// <summary>
        /// Gets the minimum level set for the given building's current district.
        /// </summary>
        /// <param name="buildingID">Building ID to check</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false (default) if workplace</param>
        /// <returns>minimum level for building set in the current district, if set (0 if no value is set)</returns>
        internal static byte GetMinLevel(ushort buildingID, bool isResidential) => GetDistrictMin(Singleton<DistrictManager>.instance.GetDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_position), isResidential);


        /// <summary>
        /// Gets the maximum level set for the given district.
        /// </summary>
        /// <param name="districtID">District ID</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false (default) if workplace</param>
        /// <returns>Maximum level for this type of building in the given district</returns>
        internal static byte GetDistrictMax(ushort districtID, bool isResidential)
        {
            // Residential or workplace?
            if (isResidential)
            {
                // Residential - check that array has ben initialised, and if so, return the relevant district maximum.
                if (minResLevels != null)
                {
                    return maxResLevels[districtID];
                }

                // If we got here, no value was retrieved; return the default.
                return MaxResLevel;
            }
            else
            {
                // Workplace - check that array has ben initialised, and if so, return the relevant district maximum.
                if (minWorkLevels != null)
                {
                    return maxWorkLevels[districtID];
                }

                // If we got here, no value was retrieved; return the default.
                return MaxWorkLevel;
            }
        }


        /// <summary>
        /// Gets the minimum level set for the given district.
        /// </summary>
        /// <param name="districtID">District ID</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false (default) if workplace</param>
        /// <returns>Minimum level for this type of building in the given district</returns>
        internal static byte GetDistrictMin(ushort districtID, bool isResidential)
        {
            // Residential or workplace?
            if (isResidential)
            {
                // Residential - check that array has ben initialised, and if so, return the relevant district minimum.
                if (minResLevels != null)
                {
                    return minResLevels[districtID];
                }

                // If we got here, no value was retrieved; return the default.
                return 0;
            }
            else
            {
                // Workplace - check that array has ben initialised, and if so, return the relevant district minimum.
                if (minWorkLevels != null)
                {
                    return minWorkLevels[districtID];
                }

                // If we got here, no value was retrieved; return the default.
                return 0;
            }
        }


        /// <summary>
        /// Sets the minimum level for the given district.
        /// </summary>
        /// <param name="districtID">District ID</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false (default) if workplace</param>
        /// <param name="level">New minimum level</param>
        internal static void SetDistrictMin(ushort districtID, bool isResidential, byte level)
        {
            if (districtID > NumDistricts)
            {
                Logging.Error("invalid district passed to SetDistrictMin");
                return;
            }

            if (isResidential)
            {
                minResLevels[districtID] = Math.Min(level, MaxResLevel);
            }
            else
            {
                minWorkLevels[districtID] = Math.Min(level, MaxWorkLevel);
            }
        }


        /// <summary>
        /// Sets the maximum level for the given district.
        /// </summary>
        /// <param name="districtID">District ID</param>
        /// <param name="isResidential">True if this building uses residential level restrictions, false (default) if workplace</param>
        /// <param name="level">New maximum level</param>
        internal static void  SetDistrictMax(ushort districtID, bool isResidential, byte level)
        {
            if (districtID > NumDistricts)
            {
                Logging.Error("invalid district passed to SetDistrictMax");
                return;
            }

            if (isResidential)
            {
                maxResLevels[districtID] = Math.Min(level, MaxResLevel);
            }
            else
            {
                maxWorkLevels[districtID] = Math.Min(level, MaxWorkLevel);
            }
        }


        /// <summary>
        /// Checks the given flag for the given district.
        /// </summary>
        /// <param name="districtID">District ID</param>
        /// <param name="flag">Flag to check</param>
        /// <returns>True if flag set, false otherwise</returns>
        internal static bool GetFlag(ushort districtID, byte flag)
        {
            // Safety checks.
            if (districtID >= NumDistricts)
            {
                Logging.Error("districtID ", districtID, " for GetFlag is above limit of ", NumDistricts);
                return false;
            }
            if (districtFlags == null)
            {
                Logging.Error("attempted to get district flag when districtFlags not initialized");
                return false;
            }

            return (districtFlags[districtID] & flag) != 0;
        }


        /// <summary>
        /// Sets or clears the given flag for the given district.
        /// </summary>
        /// <param name="districtID">District ID</param>
        /// <param name="flag">Flag to set</param>
        /// <param name="flagState">True to set the flag, false to clear</param>
        internal static void SetFlag(ushort districtID, byte flag, bool flagState)
        {
            // Safety checks.
            if (districtID >= NumDistricts)
            {
                Logging.Error("districtID ", districtID, " for SetFlag is above limit of ", NumDistricts);
                return;
            }
            if (districtFlags == null)
            {
                Logging.Error("attempted to get district flag when districtFlags not initialized");
                return;
            }

            // Set/clar flag.
            if (flagState)
            {
                // Set flag by OR.
                districtFlags[districtID] |= flag;
            }
            else
            {
                // Clear flag by AND NOT.
                districtFlags[districtID] &= (byte)~flag;
            }
        }


        /// <summary>
        /// Deserialises savegame data into the arrays.
        /// </summary>
        /// <param name="savedMinResLevels">Minimum residential level array (from save data)</param>
        /// <param name="savedMaxResLevels">Maximum residential level array (from save data)</param>
        /// <param name="savedWorkMinLevels">Minimum workplace level array (from save data)</param>
        /// <param name="savedWorkMaxLevels">Maximum workplace level array (from save data)</param>
        /// <param name="savedFlags">District attribute flags (from save data)</param>
        internal static void Deserialize(byte[] savedMinResLevels, byte[] savedMaxResLevels, byte[] savedWorkMinLevels, byte[] savedWorkMaxLevels, byte[] savedFlags)
        {
            // Populate arrays, checking data validity before we do.
            if (savedMinResLevels != null && savedMinResLevels.Length == NumDistricts)
            {
                minResLevels = savedMinResLevels;
            }
            else
            {
                // Invalid data - reset the district array to default.
                minResLevels = ResetLevels(0, "minResLevels");
            }

            if (savedMaxResLevels != null && savedMaxResLevels.Length == NumDistricts)
            {
                maxResLevels = savedMaxResLevels;
            }
            else
            {
                // Invalid data - reset the district array to default.
                maxResLevels = ResetLevels(MaxResLevel, "maxResLevels");
            }

            if (savedWorkMinLevels != null && savedWorkMinLevels.Length == NumDistricts)
            {
                minWorkLevels = savedWorkMinLevels;
            }
            else
            {
                // Invalid data - reset the district array to default.
                minWorkLevels = ResetLevels(0, "minWorkLevels");
            }

            if (savedWorkMaxLevels != null && savedWorkMaxLevels.Length == NumDistricts)
            {
                maxWorkLevels = savedWorkMaxLevels;
            }
            else
            {
                // Invalid data - reset the district array to default.
                maxWorkLevels = ResetLevels(MaxWorkLevel, "maxWorkLevels");
            }

            if (savedFlags != null && savedFlags.Length == NumDistricts)
            {
                districtFlags = savedFlags;
            }
            else
            {
                // Invalid data - reset the district array to default.
                districtFlags = ResetLevels(0, "flags");
            }
        }


        /// <summary>
        /// Checks that district arrays have been properly initialized; if not, initializes them with default values.
        /// </summary>
        internal static void CheckArrays()
        {
            Logging.KeyMessage("Performing post-load array checks");
            if (districtFlags == null || minResLevels.Length != NumDistricts)
            {
                districtFlags = ResetLevels(0, "flags ");
            }
            minResLevels = ResetLevels(0, "residential minimum ");
            if (minResLevels == null || minResLevels.Length != NumDistricts)
            {
                minResLevels = ResetLevels(0, "residential minimum ");
            }
            if (maxResLevels == null || maxResLevels.Length != NumDistricts)
            {
                maxResLevels = ResetLevels(MaxResLevel, "residential maximum ");
            }
            if (minWorkLevels == null || minWorkLevels.Length != NumDistricts)
            {
                minWorkLevels = ResetLevels(0, "workplace minimum ");
            }
            if (maxWorkLevels == null || maxWorkLevels.Length != NumDistricts)
            {
                maxWorkLevels = ResetLevels(MaxWorkLevel, "workplace maximum ");
            }
        }


        /// <summary>
        /// Resets a district level array to 128 bytes with the given default level.
        /// </summary>
        /// <param name="level">Default level to appluy</param>
        /// <param name="name">Name of district level array (for logging)</param>
        /// <returns>New district level array of 128 bytes pre-populated with the given default level</returns>
        private static byte[] ResetLevels(byte level, string name)
        {
            // Return array.
            byte[] newArray = new byte[128];

            Logging.Message(name, " district settings incomplete; resetting");

            // Populate return array with given default level.
            for (int i = 0; i < 128; ++i)
            {
                newArray[i] = level;
            }

            return newArray;
        }
    }
}