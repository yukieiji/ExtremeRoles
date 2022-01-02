using System;

using HarmonyLib;

namespace ExtremeRoles.Patches
{
	[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
	public static class GameDataRecomputeTaskCountsPatch
	{
		public static bool Prefix(GameData __instance)
		{
			__instance.TotalTasks = 0;
			__instance.CompletedTasks = 0;

			var roles = Roles.ExtremeRoleManager.GameRole;
			if (roles.Count == 0) { return false; }

			for (int i = 0; i < __instance.AllPlayers.Count; i++)
			{
				GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
				if (!playerInfo.Disconnected &&
					playerInfo.Tasks != null &&
					playerInfo.Object &&
					(PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
					playerInfo.Role && playerInfo.Role.TasksCountTowardProgress)
				{

					if (!roles[playerInfo.PlayerId].HasTask)
					{
						continue;
					}

					for (int j = 0; j < playerInfo.Tasks.Count; ++j)
					{
						++__instance.TotalTasks;
						if (playerInfo.Tasks[j].Complete)
						{
							++__instance.CompletedTasks;
						}
					}
				}
			}

			return false;
		}
	}


	[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect))]
	public static class GameDataHandleDisconnectPatch
	{
		public static void Postfix(
			GameData __instance,
			[HarmonyArgument(0)] PlayerControl player,
			[HarmonyArgument(1)] DisconnectReasons reason)
        {
			ExtremeRolesPlugin.GameDataStore.AddDeadInfo(
				player, DeathReason.Disconnect, null);
        }
	}
}
