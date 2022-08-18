// <copyright file="ModUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using System;
    using System.Reflection;
    using AlgernonCommons;
    using ColossalFramework.Plugins;

    /// <summary>
    /// Class that manages interactions with other mods, including compatibility and functionality checks.
    /// </summary>
    internal static class ModUtils
    {
        // RICO installed and enabled flag.
        private static MethodInfo s_ricoPloppable;

        /// <summary>
        /// Checks to see if another mod is installed, based on a provided assembly name.
        /// Case-sensitive!  PloppableRICO is not the same as ploppablerico.
        /// </summary>
        /// <param name="assemblyName">Name of the mod assembly.</param>
        /// <param name="enabledOnly">True if the mod needs to be enabled for the purposes of this check; false if it doesn't matter.</param>
        /// <returns>True if the mod is installed (and, if enabledOnly is true, is also enabled), false otherwise.</returns>
        internal static bool IsModInstalled(string assemblyName, bool enabledOnly = false)
        {
            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals(assemblyName))
                    {
                        Logging.Message("found mod assembly ", assemblyName);
                        if (enabledOnly)
                        {
                            return plugin.isEnabled;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            // If we've made it here, then we haven't found a matching assembly.
            return false;
        }

        /// <summary>
        /// Checks to see whether the given prefab is currently having its population controlled by Ploppable RICO Revisited.
        /// Here as a separate method on its own to avoid issues with unfound binaries breaking other methods.
        /// </summary>
        /// <param name="prefab">Prefab to check.</param>
        /// <returns>True if Ploppable RICO is managing this prefab, false otherwise.</returns>
        internal static bool CheckRICOPloppable(BuildingInfo prefab)
        {
            // If we haven't got the RICO method by reflection, the answer is always false.
            if (s_ricoPloppable != null)
            {
                if (s_ricoPloppable.Invoke(null, new object[] { prefab }) is bool result)
                {
                    return result;
                }
            }

            // Default result.
            return false;
        }

        /// <summary>
        /// Uses reflection to find the IsRICOPopManaged method of Ploppable RICO Revisited.
        /// </summary>
        internal static void RICOReflection()
        {
            string methodName = "IsRICOPloppable";

            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals("ploppablerico") && plugin.isEnabled)
                    {
                        // Found ploppablerico.dll that's part of an enabled plugin; try to get its Interfaces class.
                        Type ricoModUtils = assembly.GetType("PloppableRICO.Interfaces");

                        if (ricoModUtils != null)
                        {
                            // Try to get IsRICOPopManaged method.
                            s_ricoPloppable = ricoModUtils.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                            if (s_ricoPloppable != null)
                            {
                                // Success!  We're done here.
                                Logging.Message("found ", methodName);
                                return;
                            }
                        }
                    }
                }
            }

            // If we got here, we were unsuccessful.
            Logging.Message("didn't find ", methodName);
        }

        /// <summary>
        /// Uses reflection to find key methods for the Building Themes mod.
        /// </summary>
        /// <param name="randomBuildingInfo">RandomBuildingInfo_Upgrade method info (for delegate creation).</param>
        /// <returns>Array of BuildingThemes GetUpgrdadeInfo method infos, one for each AI type.</returns>
        internal static MethodInfo[] BuildingThemesReflection(out MethodInfo randomBuildingInfo)
        {
            string methodName = "GetUpgradeInfo";

            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals("BuildingThemes") && plugin.isEnabled)
                    {
                        // Found BuildingThemes.dll that's part of an enabled plugin; try to get its Detours class.
                        Logging.Message("found Building Themes");
                        Type themesDetours = assembly.GetType("BuildingThemes.Detour.PrivateBuildingAIDetour`1");
                        if (themesDetours != null)
                        {
                            Logging.Message("found Building Themes Detours");

                            // Get RandomBuildingInfo_Upgrade MethodInfo.
                            randomBuildingInfo = assembly.GetType("BuildingThemes.RandomBuildings").GetMethod("GetRandomBuildingInfo_Upgrade", BindingFlags.Public | BindingFlags.Static);

                            // Return the methodinfo for each of the four types.
                            return new MethodInfo[]
                            {
                                themesDetours.MakeGenericType(typeof(ResidentialBuildingAI)).GetMethod(methodName),
                                themesDetours.MakeGenericType(typeof(IndustrialBuildingAI)).GetMethod(methodName),
                                themesDetours.MakeGenericType(typeof(CommercialBuildingAI)).GetMethod(methodName),
                                themesDetours.MakeGenericType(typeof(OfficeBuildingAI)).GetMethod(methodName),
                            };
                        }
                    }
                }
            }

            // If we got here, we were unsuccessful.
            Logging.Message("didn't find ", methodName);
            randomBuildingInfo = null;
            return null;
        }
    }
}
