namespace ABLC
{
    /// <summary>
    /// Methods for use by other mods to interact with Advanced Building Level Control.
    /// </summary>
    public static class ExternalCalls
    {
        /// <summary>
        /// Locks the building level to the specified level.
        /// </summary>
        /// <param name="buildingID">Targeted building</param>
        /// <param name="level">Level to set</param>
        public static void LockBuildingLevel(ushort buildingID, ItemClass.Level level)
        {
            // Set max a min levels for this building to the specified levels.
            BuildingsABLC.UpdateMinLevel(buildingID, (byte)level);
            BuildingsABLC.UpdateMaxLevel(buildingID, (byte)level);
        }
    }
}