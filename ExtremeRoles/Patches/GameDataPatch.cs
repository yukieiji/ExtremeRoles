using HarmonyLib;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

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

			var roles = Roles.ExtremeRoleManager.GameRole;
			if (roles.Count == 0 || 
				(OptionHolder.Ship.DisableTaskWin && OptionHolder.Ship.DisableTaskWinWhenNoneTaskCrew))
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
					(PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
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

			if (doTaskCrew == 0 && OptionHolder.Ship.DisableTaskWinWhenNoneTaskCrew)
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
