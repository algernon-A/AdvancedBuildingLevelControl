using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using ICities;
using ColossalFramework.IO;


namespace ABLC
{
    /// <summary>
    /// Handles savegame data saving and loading.
    /// </summary>
    public class Serializer : SerializableDataExtensionBase
    {
        // Unique data ID.
        private readonly string dataID = "ABLC";
        private const uint DataVersion = 0;


        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();


            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Serialise district arrays.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new DistrictSerializer());

                // Serialise building list.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new BuildingSerializer());

                // Write to savegame.
                serializableDataManager.SaveData(dataID, stream.ToArray());

                Logging.Message("wrote " + stream.Length);
            }
        }


        /// <summary>
        /// Deserializes data from a savegame (or initialises new data structures when none available).
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            base.OnLoadData();

            // Initialise building dictionary.
            BuildingsABLC.levelRanges = new Dictionary<ushort, LevelRange>();

            // Read data from savegame.
            byte[] data = serializableDataManager.LoadData(dataID);

            // Check to see if anything was read.
            if (data != null && data.Length != 0)
            {
                // Data was read - go ahead and deserialise.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Deserialise district arrays.
                    DataSerializer.Deserialize<DistrictSerializer>(stream, DataSerializer.Mode.Memory);

                    // Deserialise building list.
                    DataSerializer.Deserialize<BuildingSerializer>(stream, DataSerializer.Mode.Memory);
                }
            }
            else
            {
                // No data read - initialise empty data structures.
                Logging.Message("no data read");

                // Use the post-deserialisation method of the district data serialiser to populate arrays with defaults.
                new DistrictSerializer().AfterDeserialize(null);
            }
        }
    }
}