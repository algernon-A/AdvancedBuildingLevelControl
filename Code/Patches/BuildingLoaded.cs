using HarmonyLib;


namespace ABLC
{
	/// <summary>
	/// Harmony Prefix patch for PrivateBuildingAI.BuildingLoaded.
	/// </summary>
	[HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.BuildingLoaded))]
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
		public static void Prefix(PrivateBuildingAI __instance, ushort buildingID, ref Building data)
		{
			// Only do this if settings permit.
			if (ModSettings.loadLevelCheck)
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
	}
}
