﻿using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


namespace ABLC
{
	/// <summary>
	/// Harmony Prefix patch for PrivateBuildingAI.BuildingLoaded.
	/// </summary>
	[HarmonyPatch(typeof(PrivateBuildingAI))]
	[HarmonyPatch("BuildingLoaded")]
	public static class BuildingLoadedPatch
	{
		/// <summary>
		/// Harmony Prefix patch to catch any buildings being loaded with levels above their maximum permitted level.
		/// Note that this won't in itself solve any broken prefabs with illegal levels, as the game's BuildingUpgraded method will set their level to the maximum of the actual or prefab level.
		/// </summary>
		/// <param name="__instance">Instance reference</param>
		/// <param name="buildingID">Building instance ID</param>
		/// <param name="data">Building data</param>
		/// <param name="version">Data version</param>
		public static void Prefix(PrivateBuildingAI __instance, ushort buildingID, ref Building data, uint version)
		{
			// Only do this if settings permit.
			if (ModSettings.loadLevelCheck)
			{
				byte maxLevel = LevelUtils.GetMaxLevel(buildingID);

				// Check against maxLevel (m_level is zero-based, maxLevel is 1-based, so >= to catch overflows).
				if (data.m_level >= maxLevel)
				{
					Logging.Error("building ", buildingID.ToString(), " (", __instance.m_info.name, ") had illegal level ", (data.m_level + 1).ToString(), "; setting to ", maxLevel.ToString());
					data.m_level = (byte)(maxLevel - 1);
				}
			}
		}
	}
}

#pragma warning restore IDE0060 // Remove unused parameter
