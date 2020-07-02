using System;
using System.Reflection;
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
                        Debugging.Message("found mod assembly " + assemblyName);
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
                object result = ricoPloppable.Invoke(null, new object[] { prefab });

                if (result is bool)
                {
                    return (bool)result;
                }
            }

            // Default result.
            return false;
        }


        /// <summary>
        /// Uses reflection to find the IsRICOPopManaged method of Ploppable RICO Revisited.
        /// If successful, sets ricoPopManaged to 
        /// </summary>
        internal static void RICOReflection()
        {
            string methodName = "IsRICOPloppable";

            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals("ploppablerico"))
                    {
                        // Found ploppablerico.dll; try to get its ModUtils class.
                        Type ricoModUtils = assembly.GetType("PloppableRICO.Interfaces");

                        if (ricoModUtils != null)
                        {
                            // Try to get IsRICOPopManaged method.
                            ricoPloppable = ricoModUtils.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                            if (ricoPloppable != null)
                            {
                                // Success!  We're done here.
                                Debugging.Message("found " + methodName);
                                return;
                            }
                        }
                    }
                }
            }

            // If we got here, we were unsuccessful.
            Debugging.Message("didn't find " + methodName);
        }
    }
}
