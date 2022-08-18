// <copyright file="BuildingLoadedPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Harmony Prefix patch for PrivateBuildingAI.BuildingLoaded.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.BuildingLoaded))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class BuildingLoadedPatch
    {
        /// <summary>
        /// Harmony Prefix patch to catch any buildings being loaded with levels above their maximum permitted level.
        /// Note that this won't in itself solve any broken prefabs with illegal levels, as the game's BuildingUpgraded method will set their level to the maximum of the actual or prefab level.
        /// </summary>
        /// <param name="__instance">Instance reference.</param>
        /// <param name="buildingID">Building  ID.</param>
        /// <param name="data">Building data.</param>
        public static void Prefix(PrivateBuildingAI __instance, ushort buildingID, ref Building data)
        {
            // Check for buildings with illegal levels - only do this if settings permit.
            if (ModSettings.LoadLevelCheck)
            {
                byte maxLevel = LevelUtils.GetMaxLevel(buildingID);

                // Check against maxLevel (m_level is zero-based, maxLevel is 1-based, so >= to catch overflows).
                if (data.m_level >= maxLevel)
                {
                    Logging.Error("building ", buildingID, " (", __instance.m_info.name, ") had illegal level ", data.m_level + 1, "; setting to ", maxLevel);
                    data.m_level = (byte)(maxLevel - 1);
                }
            }
        }

        /// <summary>
        /// Harmony transpiler to remove building level check, where building level is set to be minimum of saved level and prefab level;
        /// we want buildings to be able to be below prefab level.
        /// Drops instructions corresponding to the code "data.m_level = (byte)Mathf.Max(data.m_level, (int)m_info.m_class.m_level);".
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Patched ILCode.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Instruction parsing.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            CodeInstruction instruction;

            // Status flags.
            bool completed = false;

            // Iterate through all instructions in original method.
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction.
                instruction = instructionsEnumerator.Current;

                if (!completed)
                {
                    // Look for a call instruction - we start dropping after the first call instruction of the method (call instance void BuildingAI::BuildingLoaded(uint16, valuetype Building&, uint32)
                    if (instruction.opcode == OpCodes.Call)
                    {
                        yield return instruction;

                        // Now just skip over all instructions until stfld (stfld uint8 Building::m_level)
                        while (instructionsEnumerator.MoveNext())
                        {
                            if (instructionsEnumerator.Current.opcode == OpCodes.Stfld)
                            {
                                // Found our target - get next instruction, set flag and resume flow.
                                instructionsEnumerator.MoveNext();
                                instruction = instructionsEnumerator.Current;
                                completed = true;
                                break;
                            }
                        }
                    }
                }

                // Output instruction.
                yield return instruction;
            }

            // If we got here without finding our target, something went wrong.
            if (!completed)
            {
                Logging.Error("transpiler patching failed for PrivateBuildingAI.BuildingLoaded");
            }
        }
    }
}
