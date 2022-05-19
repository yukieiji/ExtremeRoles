using HarmonyLib;

namespace ExtremeRoles.Patches
{
	[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
	public static class GameDataRecomputeTaskCountsPatch
	{
		public static bool Prefix(GameData __instance)
		{

			var roles = Roles.ExtremeRoleManager.GameRole;
			if (roles.Count == 0) { return false; }

			int totalTask = 0;
			int completedTask = 0;

			foreach (GameData.PlayerInfo playerInfo in __instance.AllPlayers)
			{
				if (!playerInfo.Disconnected &&
					playerInfo.Tasks != null &&
					playerInfo.Object &&
					(PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
					playerInfo.Role && 
					playerInfo.Role.TasksCountTowardProgress &&
					roles.ContainsKey(playerInfo.PlayerId))
				{

					if (!roles[playerInfo.PlayerId].HasTask)
					{
						continue;
					}

					if (!roles[playerInfo.PlayerId].IsCrewmate())
                    {
						continue;
                    }

					foreach (GameData.TaskInfo taskInfo in playerInfo.Tasks)
					{
						++totalTask;
						if (taskInfo.Complete)
						{
							++completedTask;
						}
					}
				}
			}

			__instance.TotalTasks = totalTask;
			__instance.CompletedTasks = completedTask;

			return false;
		}
	}
}
