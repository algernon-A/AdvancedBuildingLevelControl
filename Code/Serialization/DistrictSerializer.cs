// <copyright file="DistrictSerializer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using AlgernonCommons;
    using ColossalFramework.IO;

    /// <summary>
    ///  Savegame (de)serialisation for district level settings.
    /// </summary>
    public class DistrictSerializer : IDataContainer
    {
        /// <summary>
        /// Serialise to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Serialize(DataSerializer serializer)
        {
            Logging.Message("writing district settings");

            // Write district level arrays to savegame.
            serializer.WriteByteArray(Districts.MinResLevels);
            serializer.WriteByteArray(Districts.MaxResLevels);
            serializer.WriteByteArray(Districts.MinWorkLevels);
            serializer.WriteByteArray(Districts.MaxWorkLevels);

            // Extended attributes - starting with version flag.
            serializer.WriteInt16(0);
            serializer.WriteByteArray(Districts.Flags);
        }

        /// <summary>
        /// Deseralise from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.Message("reading district settings");

            // Read data from savegame into flat arrays.
            byte[] minResLevel = serializer.ReadByteArray();
            byte[] maxResLevel = serializer.ReadByteArray();
            byte[] minWorkLevel = serializer.ReadByteArray();
            byte[] maxWorkLevel = serializer.ReadByteArray();

            // Try to read extended attributes - original version didn't have these.
            byte[] flags = null;
            try
            {
                // Read version, but ignore it for now.
                int version = serializer.ReadInt16();

                // Read flags.
                flags = serializer.ReadByteArray();
            }
            catch
            {
                // Don't care if we can't read them.
            }

            // Load read data into arrays.
            Districts.Deserialize(minResLevel, maxResLevel, minWorkLevel, maxWorkLevel, flags);
        }

        /// <summary>
        /// Post-deserialisation.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
            Logging.Message("district settings read");
        }
    }
}