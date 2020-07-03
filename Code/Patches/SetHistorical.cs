using HarmonyLib;


namespace ABLC
{
    /// <summary>
    /// Harmony Postfix patch to update ABLC building info panel when the building's historical status changes.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI))]
    [HarmonyPatch("SetHistorical")]
    internal static class SetHistoricalPatch
    {
        public static void Postfix(PrivateBuildingAI __instance, ushort buildingID, ref Building data, bool historical)
        {
            if (BuildingPanelManager.Panel != null)
            {
                BuildingPanelManager.Panel.UpdatePanel();
            }
        }
    }
}
