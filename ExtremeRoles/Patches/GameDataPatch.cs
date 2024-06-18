using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Patches;

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
	private const int forceDisableTaskNum = 88659;

	public static bool IsDisableTaskWin =>
		GameData.Instance == null ||
		(GameData.Instance.TotalTasks == forceDisableTaskNum && GameData.Instance.CompletedTasks == 0);

	public static bool Prefix(GameData __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

		var roles = Roles.ExtremeRoleManager.GameRole;
		var shipOpt = ExtremeGameModeManager.Instance.ShipOption;

        if (roles.Count == 0 ||
			(shipOpt.DisableTaskWin && shipOpt.DisableTaskWinWhenNoneTaskCrew))
		{
			__instance.TotalTasks = forceDisableTaskNum;
			__instance.CompletedTasks = 0;
			return false;
		}

		int totalTask = 0;
		int completedTask = 0;
		int doTaskCrew = 0;

		foreach (NetworkedPlayerInfo playerInfo in __instance.AllPlayers.GetFastEnumerator())
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
				roles.TryGetValue(playerInfo.PlayerId, out var role) &&
				role != null)
			{

				if (!role.HasTask())
				{
					continue;
				}

				if (!role.IsCrewmate())
                {
					continue;
                }

				++doTaskCrew;

				foreach (var taskInfo in playerInfo.Tasks.GetFastEnumerator())
				{
					++totalTask;
					if (taskInfo.Complete)
					{
						++completedTask;
					}
				}
			}
		}

		if (doTaskCrew == 0 && shipOpt.DisableTaskWinWhenNoneTaskCrew)
        {
			totalTask = forceDisableTaskNum;
			completedTask = 0;
		}

		__instance.TotalTasks = totalTask;
		__instance.CompletedTasks = completedTask;

		return false;
	}
}
