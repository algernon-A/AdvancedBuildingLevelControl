namespace ABLC
{
    using System;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using HarmonyLib;
    using CitiesHarmony.API;

    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public class Patcher : PatcherBase
    {
        // Unique harmony identifier.
        private const string harmonyID = "com.github.algernon-A.csl.ablc";

        /// <summary>
        /// Initializes a new instance of the <see cref="Patcher"/> class.
        /// </summary>
        /// <param name="harmonyID">This mod's unique Harmony identifier.</param>
        public Patcher(string harmonyID)
            : base(harmonyID)
        {
        }

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        public static new Patcher Instance
        {
            get
            {
                // Auto-initializing getter.
                if (s_instance == null)
                {
                    s_instance = new Patcher(PatcherMod.Instance.HarmonyID);
                }

                return s_instance as Patcher;
            }
        }

        /// <summary>
        /// Applies or unapplies Building Themees patch.
        /// </summary>
        public void PatchBuildingThemes()
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