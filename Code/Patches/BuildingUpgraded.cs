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
			// If the minimum permissible level is greater than the current building level, force another upgrade.
			if (BuildingsABLC.GetMinLevel(buildingID, data.Info.GetService() == ItemClass.Service.Residential) > data.m_level)
			{
				Singleton<SimulationManager>.instance.AddAction(() =>
				{
					((Action<ushort>)LevelUtils.TriggerLevelUp).Invoke(buildingID);
				});
			}
		}
	}
}
