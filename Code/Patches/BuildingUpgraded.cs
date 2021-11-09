using System;
using ColossalFramework;
using HarmonyLib;



namespace ABLC
{
	/// <summary>
	/// Harmony patches (Prefix and Postfix) for PrivateBuildingAI.BuildingUpgraded.
	/// </summary>
	[HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.BuildingUpgraded))]
	public static class BuildingUpgradedPatch
    {
		/// <summary>
		/// Harmony Prefix patch to catch any buildings trying to upgrade beyond their maximum permitted level.
		/// Note that this won't in itself solve any broken prefabs with illegal levels, as the game's BuildingUpgraded method will set their level to the maximum of the actual or prefab level.
		/// </summary>
		/// <param name="__instance">Instance reference</param>
		/// <param name="buildingID">Building instance ID</param>
		/// <param name="data">Building data</param>
		public static void Prefix(PrivateBuildingAI __instance, ushort buildingID, ref Building data)
        {
			byte maxLevel = LevelUtils.GetMaxLevel(buildingID);

			// Check against maxLevel (m_level is zero-based, maxLevel is 1-based, so >= to catch overflows).
			if (data.m_level >= maxLevel)
            {
				Logging.Error("prevented building ", buildingID, " (", __instance.m_info.name, ") from upgrading to illegal level ", data.m_level + 1, "; setting to ", maxLevel);
				data.m_level = (byte)(maxLevel - 1);
            }
        }


		/// <summary>
		/// Harmony Postfix patch to ensure valid building levels and to keep a building upgrading until it reaches the minimum controlled level.
		/// </summary>
		/// <param name="buildingID">Building instance ID</param>
		/// <param name="data">Building data</param>
		public static void Postfix(ushort buildingID, ref Building data)
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

			// If the minimum permissible level is greater than the current building level, force another upgrade.
			if (minLevel > data.m_level)
			{
				Singleton<SimulationManager>.instance.AddAction(() =>
				{
					((Action<ushort>)LevelUtils.TriggerLevelUp).Invoke(buildingID);
				});
			}
		}
	}
}
