using System;
using System.Reflection;
using HarmonyLib;
using CitiesHarmony.API;


namespace ABLC
{
    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public static class Patcher
    {
        // Unique harmony identifier.
        private const string harmonyID = "com.github.algernon-A.csl.ablc";

        // Flags.
        internal static bool Patched => _patched;
        private static bool _patched = false;


        /// <summary>
        /// Apply all Harmony patches.
        /// </summary>
        public static void PatchAll()
        {
            // Don't do anything if already patched.
            if (!_patched)
            {
                // Ensure Harmony is ready before patching.
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Logging.KeyMessage("deploying Harmony patches");

                    // Apply all annotated patches and update flag.
                    Harmony harmonyInstance = new Harmony(harmonyID);
                    harmonyInstance.PatchAll();
                    _patched = true;
                }
                else
                {
                    Logging.Error("Harmony not ready");
                }
            }
        }


        /// <summary>
        /// Remove all Harmony patches.
        /// </summary>
        public static void UnpatchAll()
        {
            // Only unapply if patches appplied.
            if (_patched)
            {
                Logging.KeyMessage("reverting Harmony patches");

                // Unapply patches, but only with our HarmonyID.
                Harmony harmonyInstance = new Harmony(harmonyID);
                harmonyInstance.UnpatchAll(harmonyID);
                _patched = false;
            }
        }

        /// <summary>
        /// Applies or unapplies Building Themees patch.
        /// </summary>
        /// <param name="active">True to apply patch, false to unapply</param>
        public static void PatchBuildingThemes()
        {
            // Ensure Harmony is ready before patching.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Harmony harmonyInstance = new Harmony(harmonyID);

                // Patch method.
                MethodInfo patchMethod = typeof(GetUpgradeInfo).GetMethod(nameof(GetUpgradeInfo.Prefix));

                MethodInfo[] targets = ModUtils.BuildingThemesReflection(out MethodInfo randomBuildingInfo);

                if (targets != null && randomBuildingInfo != null)
                {
                    foreach (MethodInfo targetMethod in targets)
                    {
                        Logging.Message("patching ", targetMethod);
                        harmonyInstance.Patch(targetMethod, prefix: new HarmonyMethod(patchMethod));
                    }

                    // Set delegate to Building Themes' random building prefab selection method.
                    GetUpgradeInfo.buildingTheme = Delegate.CreateDelegate(typeof(GetUpgradeInfo.BuildingThemeDelegate), randomBuildingInfo) as GetUpgradeInfo.BuildingThemeDelegate;
                }
                else
                {
                    Logging.Error("didn't patch Building Themes");
                }
            }
            else
            {
                Logging.Error("Harmony not ready");
            }
        }
    }
}