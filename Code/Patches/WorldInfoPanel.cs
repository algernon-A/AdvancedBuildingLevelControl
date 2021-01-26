using HarmonyLib;


namespace ABLC
{
    /// <summary>
    /// Harmony Postfix patch to update ABLC district info panel when district selection changes.
    /// </summary>
    [HarmonyPatch(typeof(DistrictWorldInfoPanel))]
    [HarmonyPatch("OnSetTarget")]
    internal static class DistrictPanelPatch
    {
        /// <summary>
        /// Harmony Postfix patch to update ABLC district info panel when district selection changes.
        /// </summary>
        public static void Postfix()
        {
            DistrictPanelManager.Panel?.DistrictChanged();
        }
    }


    /// <summary>
    /// Harmony Postfix patch to update ABLC building info panel when building selection changes.
    /// </summary>
    [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel))]
    [HarmonyPatch("OnSetTarget")]
    internal static class BuildingPanelPatch
    {
        /// <summary>
        /// Harmony Postfix patch to update ABLC building info panel when building selection changes.
        /// </summary>
        public static void Postfix()
        {
            BuildingPanelManager.TargetChanged();
        }
    }
}