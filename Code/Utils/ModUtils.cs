using System;
using System.Reflection;
using System.Collections.Generic;
using ColossalFramework.Plugins;


namespace ABLC
{
    /// <summary>
    /// Class that manages interactions with other mods, including compatibility and functionality checks.
    /// </summary>
    internal static class ModUtils
    {
        // RICO installed and enabled flag.
        internal static MethodInfo ricoPloppable;
        // List of conflcting mod names.
        internal static List<string> conflictingModNames;

        /// <summary>
        ///  Flag to determine whether or not a realistic population mod is installed and enabled.
        /// </summary>
        internal static bool realPopEnabled = false;


        /// <summary>
        /// Checks for any known fatal mod conflicts.
        /// </summary>
        /// <returns>True if a mod conflict was detected, false otherwise</returns>
        internal static bool IsModConflict()
        {
            // Initialise flag and list of conflicting mods.
            bool conflictDetected = false;
            conflictingModNames = new List<string>();

            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        case "VanillaGarbageBinBlocker":
                            // Garbage Bin Controller
                            conflictDetected = true;
                            conflictingModNames.Add("Garbage Bin Controller");
                            break;
                        case "Painter":
                            // Painter - this one is trickier because both Painter and Repaint use Painter.dll (thanks to CO savegame serialization...)
                            if (plugin.userModInstance.GetType().ToString().Equals("Painter.UserMod"))
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Painter");
                            }
                            break;
                    }
                }
            }

            // Was a conflict detected?
            if (conflictDetected)
            {
                // Yes - log each conflict.
                foreach (string conflictingMod in conflictingModNames)
                {
                    Logging.Error("Conflicting mod found: ", conflictingMod);
                }
                Logging.Error("exiting due to mod conflict");
            }

            return conflictDetected;
        }


        /// <summary>
        /// Checks to see if another mod is installed, based on a provided assembly name.
        /// Case-sensitive!  PloppableRICO is not the same as ploppablerico!
        /// </summary>
        /// <param name="assemblyName">Name of the mod assembly</param>
        /// <param name="enabledOnly">True if the mod needs to be enabled for the purposes of this check; false if it doesn't matter</param>
        /// <returns>True if the mod is installed (and, if enabledOnly is true, is also enabled), false otherwise</returns>
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
        /// <param name="prefab">Prefab to check</param>
        /// <returns>True if Ploppable RICO is managing this prefab, false otherwise.</returns>
        internal static bool CheckRICOPloppable(BuildingInfo prefab)
        {
            // If we haven't got the RICO method by reflection, the answer is always false.
            if (ricoPloppable != null)
            {
                if (ricoPloppable.Invoke(null, new object[] { prefab }) is bool result)
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
                            ricoPloppable = ricoModUtils.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                            if (ricoPloppable != null)
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
    }
}
