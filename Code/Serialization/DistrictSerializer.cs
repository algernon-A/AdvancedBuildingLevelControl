using ColossalFramework.IO;


namespace ABLC
{
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
            serializer.WriteByteArray(DistrictsABLC.MinResLevels);
            serializer.WriteByteArray(DistrictsABLC.MaxResLevels);
            serializer.WriteByteArray(DistrictsABLC.MinWorkLevels);
            serializer.WriteByteArray(DistrictsABLC.MaxWorkLevels);

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
            byte[] minResLevel = serializer.ReadByteArray();
            byte[] maxResLevel = serializer.ReadByteArray();
            byte[] minWorkLevel = serializer.ReadByteArray();
            byte[] maxWorkLevel = serializer.ReadByteArray();
            DistrictsABLC.Deserialize(minResLevel, maxResLevel, minWorkLevel, maxWorkLevel);

            // Try to read extended attributes - original version didn't have these.
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
        /// Post-deserialisation.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
            Logging.Message("district settings read");
        }
    }
}