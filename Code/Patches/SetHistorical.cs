using HarmonyLib;


namespace ABLC
{
    /// <summary>
    /// Harmony Postfix patch to update ABLC building info panel when the building's historical status changes.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.SetHistorical))]
    internal static class SetHistoricalPatch
    {
        /// <summary>
        /// Harmony Postfix patch to update ABLC building info panel if the building's historical status changes.
        /// </summary>
        public static void Postfix()
        {
            if (BuildingPanelManager.Panel != null)
            {
                BuildingPanelManager.Panel.UpdatePanel();
            }
        }
    }
}
