using HarmonyLib;


namespace ABLC
{
	/// <summary>
	/// Harmony Postfix patch for disabling abandonment for historical buildings.
	/// </summary>
	[HarmonyPatch(typeof(PrivateBuildingAI), "SimulationStepActive")]
	internal static class SimulationStepActivePatch
	{
		/// <summary>
		/// Harmony Postfix patch to stop historical buildings from abandoning.
		/// </summary>
		/// <param name="__instance">Instance reference</param>
		/// <param name="buildingID">Building instance ID</param
		/// <param name="buildingData">Building instance data reference</param>
		public static void Postfix(PrivateBuildingAI __instance, ushort buildingID, ref Building buildingData)
		{
			// Check to see if we have no abandonement for any building set, or no abandonment historical and this is an historical building.
			if (ModSettings.noAbandonAny || (ModSettings.noAbandonHistorical && __instance.IsHistorical(buildingID, ref buildingData, out bool _)))
            {
				// It is - simply reset the major problem timer to avoid the 'abandonment' timeout.
				buildingData.m_majorProblemTimer = 0;

				// Clear abandonment flag just in case.
				buildingData.m_flags &= ~Building.Flags.Abandoned;
			}
		}
	}
}