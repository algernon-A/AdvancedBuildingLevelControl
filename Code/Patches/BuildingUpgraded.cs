using System;
using ColossalFramework;
using HarmonyLib;


namespace ABLC
{
	/// <summary>
	/// Harmony Postfix patch to keep a building upgrading until it reaches the minimum controlled level.
	/// </summary>
	[HarmonyPatch(typeof(PrivateBuildingAI))]
	[HarmonyPatch("BuildingUpgraded")]
	class BuildingUpgradedPatch
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
					((Action<ushort>)LevelUtils.ForceLevelUp).Invoke(buildingID);
				});
			}
		}
	}
}

