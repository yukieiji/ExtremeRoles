using HarmonyLib;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(GameData), nameof(GameData.AddPlayer))]
	public static class GameDataAddPlayerPatch
	{
		public static void Postfix()
		{
			foreach (CachedPlayerControl cachedPlayer in CachedPlayerControl.AllPlayerControls)
			{
				cachedPlayer.Data = cachedPlayer.PlayerControl.Data;
				cachedPlayer.PlayerId = cachedPlayer.PlayerControl.PlayerId;
			}
		}
	}

	[HarmonyPatch(typeof(GameData), nameof(GameData.Deserialize))]
	public static class GameDataDeserializePatch
	{
		public static void Postfix()
		{
			foreach (CachedPlayerControl cachedPlayer in CachedPlayerControl.AllPlayerControls)
			{
				cachedPlayer.Data = cachedPlayer.PlayerControl.Data;
				cachedPlayer.PlayerId = cachedPlayer.PlayerControl.PlayerId;
			}
		}
	}


	[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
	public static class GameDataRecomputeTaskCountsPatch
	{
		public static bool Prefix(GameData __instance)
		{
			if (ExtremeGameModeManager.Instance == null ||
				ExtremeGameModeManager.Instance.ShipOption == null) { return true; }

			var roles = Roles.ExtremeRoleManager.GameRole;
			var shipOpt = ExtremeGameModeManager.Instance.ShipOption;

            if (roles.Count == 0 || 
				(shipOpt.DisableTaskWin && shipOpt.DisableTaskWinWhenNoneTaskCrew))
			{
				__instance.TotalTasks = 88659;
				__instance.CompletedTasks = 0;
				return false; 
			}

			int totalTask = 0;
			int completedTask = 0;
			int doTaskCrew = 0;

			foreach (GameData.PlayerInfo playerInfo in __instance.AllPlayers.GetFastEnumerator())
			{
				if (!playerInfo.Disconnected &&
					playerInfo.Tasks != null &&
					playerInfo.Object &&
					(
                        GameManager.Instance.LogicOptions.GetGhostsDoTasks() || 
						!playerInfo.IsDead
					) &&
					playerInfo.Role && 
					playerInfo.Role.TasksCountTowardProgress &&
					roles.ContainsKey(playerInfo.PlayerId))
				{

					if (!roles[playerInfo.PlayerId].HasTask())
					{
						continue;
					}

					if (!roles[playerInfo.PlayerId].IsCrewmate())
                    {
						continue;
                    }
					
					++doTaskCrew;

					foreach (GameData.TaskInfo taskInfo in playerInfo.Tasks.GetFastEnumerator())
					{
						++totalTask;
						if (taskInfo.Complete)
						{
							++completedTask;
						}
					}
				}
			}

			if (doTaskCrew == 0 && ExtremeGameModeManager.Instance.ShipOption.DisableTaskWinWhenNoneTaskCrew)
            {
				totalTask = 88659;
				completedTask = 0;	
			}

			__instance.TotalTasks = totalTask;
			__instance.CompletedTasks = completedTask;

			return false;
		}
	}
}
