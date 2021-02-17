using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ColossalFramework;
using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


namespace ABLC
{
	/// <summary>
	/// Harmony Postfix patch to keep a building upgrading until it reaches the minimum controlled level.
	/// </summary>
	[HarmonyPatch(typeof(PrivateBuildingAI))]
	[HarmonyPatch("BuildingUpgraded")]
	internal static class BuildingUpgradedPatch
    {
		/// <summary>
		/// Harmony Postfix patch to keep a building upgrading until it reaches the minimum controlled level.
		/// </summary>
		/// <param name="__instance">Instance reference</param>
		/// <param name="buildingID">Building instance ID</param>
		/// <param name="data">Building data</param>
		public static void Postfix(PrivateBuildingAI __instance, ushort buildingID, ref Building data)
		{
			byte minLevel;

			// Check for individual building restrictions, as they have highest priority.
			if (BuildingsABLC.levelRanges.ContainsKey(buildingID))
			{
				// Get individual building minimum level.
				minLevel = BuildingsABLC.levelRanges[buildingID].minLevel;
			}
			else
			{
				// No building restrictions; check district for restrictions.
				ushort districtID = Singleton<DistrictManager>.instance.GetDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_position);

				// Get our maximum level for this district for this type of building.
				minLevel = data.Info.GetService() == ItemClass.Service.Residential ? DistrictsABLC.minResLevel[districtID] : DistrictsABLC.minWorkLevel[districtID];
			}

			// If the maminimum permissible level is greater than the current building level, force another upgrade.
			if (minLevel > data.m_level)
			{
				Singleton<SimulationManager>.instance.AddAction(() =>
				{
					((Action<ushort>)LevelUtils.TriggerLevelUp).Invoke(buildingID);
				});
			}
		}


        /// <summary>
        /// Transpiler to remove minimum level check from PrivateBuildingAI.BuildingUpgraded.
        /// </summary>
        /// <param name="original">Original method to patch</param>
        /// <param name="instructions">Original ILCode</param>
        /// <param name="generator">ILCode generator</param>
        /// <returns>Replacement ILCode</returns>
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Removing the following code from the start of the method (locks minimum level to info base level, which we don't want):
            /* // data.m_level = (byte)Mathf.Max(data.m_level, (int)m_info.m_class.m_level);
             * 
             * IL_0000: ldarg.2
             * IL_0001: ldarg.2
             * IL_0002: ldfld uint8 Building::m_level
             * IL_0007: ldarg.0
             * IL_0008: ldfld class BuildingInfo BuildingAI::m_info
             * IL_000d: ldfld class ItemClass BuildingInfo::m_class
             * IL_0012: ldfld valuetype ItemClass/Level ItemClass::m_level
             * IL_0017: call int32 [UnityEngine]UnityEngine.Mathf::Max(int32, int32)
             * IL_001c: conv.u1
             * IL_001d: stfld uint8 Building::m_level
             */
            // Code resumes with the second ldarg.0 in the method:
            /* IL_0022: ldarg.0
             */

            // Transpiler meta.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            bool removed = false;

            Logging.Message("transpiling PrivateBuildingAI.BuildingUpgraded");

            // Iterate through ILCode instructions.
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction and add it to output.
                CodeInstruction instruction = instructionsEnumerator.Current;

                // If we haven't finished removing the target code yet, we need to check to see if we're at the right spot to do so.
                if (!removed)
                {
                    // Haven't yet finished removal.  Check to see if we have a pattern match - looking for stfld uint8 Building::m_level followed by ldarg.0.
                    if (instruction.opcode == OpCodes.Stfld && instruction.operand.ToString().Equals("System.Byte m_level"))
                    {
                        // Got a stfld - skip it than check for a following ldarg.0.
                        instructionsEnumerator.MoveNext();
                        instruction = instructionsEnumerator.Current;
                        
                        // Check for following ldarg.0 to confirm.
                        if (instruction.opcode == OpCodes.Ldarg_0)
                        {
                            // Found our spot! Insert our custom instructions in the output.
                            Logging.Message("skipped code to stfld");

                            // Set flag to indicate that removal is complete.
                            removed = true;

                            // Add this opcode (Ldarg_0) to return.
                            yield return instruction;
                        }
                    }
                }
                else
                {
                    // We've finished removing the targeted code - just add this instruction to output and continue.
                    yield return instruction;
                }
            }
        }
	}
}

#pragma warning restore IDE0060 // Remove unused parameter
