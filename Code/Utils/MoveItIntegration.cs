using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using MoveItIntegration;



namespace ABLC
{
    /// <summary>
    /// Lets Move It know that we've got some integration we want to do.
    /// </summary>
    public class MoveItIntegrationFactory : IMoveItIntegrationFactory
    {
        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public MoveItIntegrationBase GetInstance() => new MoveItIntegration();
    }


    /// <summary>
    /// Integration for the Move It mod, to copy ABLC settings when buildings are copied.
    /// </summary>
    public class MoveItIntegration : MoveItIntegrationBase
    {
        // ABLC identifier.
        public override string ID => "ABLC";

        //public override string Name => null;

        //public override string Description => Translations.Translate("ABLC_DESC");

        // Data version.
        public override Version DataVersion => new Version(1, 0);

        // Let Move It know we're interested in building copies.
        public override object Copy(InstanceID sourceInstanceID) => InstanceType.Building;


        /// <summary>
        /// Called by Move It when an object is pasted.
        /// Used here to copy any ABLC building settings to the copied buildings(s).
        /// </summary>
        /// <param name="targetInstanceID">Target instance (unused)</param>
        /// <param name="record">Custom data record (unused)</param>
        /// <param name="sourceMap">Mapping of new building instances to original building instances</param>
        public override void Paste(InstanceID targetInstanceID, object record, Dictionary<InstanceID, InstanceID> sourceMap)
        {
            // Iterate through each mapping entry in the dictionary.
            foreach (KeyValuePair<InstanceID, InstanceID> entry in sourceMap)
            {
                // Check if the original building has an ABLC custom level entry.
                LevelRange levelRange = BuildingsABLC.GetRecord(entry.Key.Building);
                if (levelRange != null)
                {
                    // Original building has an ABLC level entry - apply those same settings to the new building.
                    ushort newBuilding = entry.Value.Building;
                    BuildingsABLC.UpdateMinLevel(newBuilding, levelRange.minLevel);
                    BuildingsABLC.UpdateMaxLevel(newBuilding, levelRange.maxLevel);
                }
            }
        }


        /// <summary>
        /// Called by Move It - encodes custom data records.
        /// Not specifically used by ABLC.
        /// </summary>
        /// <param name="record">Record to encode</param>
        /// <returns>Encoded data as string</returns>
        public override string Encode64(object record) => null;


        /// <summary>
        /// Called by Move It - decodes custom data records.
        /// Not specifically used by ABLC.
        /// </summary>
        /// <param name="record">String to decode</param>
        /// <param name="dataVersion">Data version</param>
        /// <returns>Decoded record</returns>
        public override object Decode64(string record, Version dataVersion) => null;
    }
}
