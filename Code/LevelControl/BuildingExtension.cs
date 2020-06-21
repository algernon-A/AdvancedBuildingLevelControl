using UnityEngine;
using ICities;
using ColossalFramework;


namespace ABLC
{
    /// <summary>
    /// Building extension method class.  Used to implement level control for new buildings.
    /// </summary>
    public class BuildingExtension : BuildingExtensionBase
    {
        /// <summary>
        /// Adjust building's intial spawn level according to district settings.
        /// Called by the game as part of the building spawn calculation process.
        /// </summary>
        /// <param name="position">Building position</param>
        /// <param name="spawn">Initial building spawn data from game</param>
        /// <returns>Modified spawn data</returns>
        public override SpawnData OnCalculateSpawn(Vector3 position, SpawnData spawn)
        {
            // Get district.
            ushort districtID = Singleton<DistrictManager>.instance.GetDistrict(position);

            // Get our minimum level for this district for this type of building.
            spawn.level = spawn.service == Service.Residential ? (Level)DistrictsABLC.minResLevel[districtID] : (Level)DistrictsABLC.minWorkLevel[districtID];

            return spawn;
        }


        /// <summary>
        /// Checks to see if a released building has a custom level settting, and if so, removes that setting.
        /// Called by the game when a building instance is released.
        /// </summary>
        /// <param name="id">Building instance ID</param>
        public override void OnBuildingReleased(ushort id)
        {
            // Check to see if our dictionary is ready, and if it contains this building ID.
            if (BuildingsABLC.levelRanges != null && BuildingsABLC.levelRanges.ContainsKey(id))
            {
                // It does; delete the entry.
                BuildingsABLC.levelRanges.Remove(id);
            }
        }
    }
}
