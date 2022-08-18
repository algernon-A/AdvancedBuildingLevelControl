
namespace ABLC
{
    using AlgernonCommons;
    using ColossalFramework.IO;

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
            // Get data arrays to serialise.
            BuildingsABLC.Serialize(out uint[] buildingKeys, out byte[] minLevels, out byte[] maxLevels);

            // Write serialisation arrays to savegame.
            Logging.Message("writing building settings");
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

            // Read data from savegame into flat arrays and then pass to building class to deserialize.
            uint[] buildingKeys = serializer.ReadUInt32Array();
            byte[] minLevels = serializer.ReadByteArray();
            byte[] maxLevels = serializer.ReadByteArray();
            BuildingsABLC.Deserialize(buildingKeys, minLevels, maxLevels);
        }


        /// <summary>
        /// Performs post-deserialisation check, removing entries corresponding to non-existent buildings.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void AfterDeserialize(DataSerializer serializer) => BuildingsABLC.CheckEntries();
    }
}