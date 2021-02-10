using System;
using System.Runtime.CompilerServices;
using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


namespace ABLC
{
    /// <summary>
    /// Static class for Harmony reverse patches.
    /// </summary>
    [HarmonyPatch]
    internal static class ReversePatches
    {
        /// <summary>
        /// Reverse patch for BuildingAI.EnsureCitizenUnits to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="buildingID">ID of this building (for game method)</param>
        /// <param name="data">Building data (for game method)</param>
        /// <param name="homeCount">Household count (for game method)</param>
        /// <param name="workCount">Workplace count (for game method)</param>
        /// <param name="visitCount">Visitor count (for game method)</param>
        /// <param name="studentCount">Student count (for game method)</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(BuildingAI)), "EnsureCitizenUnits")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void EnsureCitizenUnits(object instance, ushort buildingID, ref Building data, int homeCount, int workCount, int visitCount, int studentCount)
        {
            Logging.Error("EnsureCitizenUnits reverse Harmony patch wasn't applied");
            throw new NotImplementedException("Harmony reverse patch not applied");
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter