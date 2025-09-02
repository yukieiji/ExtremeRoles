using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
public static class GameDataRecomputeTaskCountsPatch
{
	private const int forceDisableTaskNum = 88659;

	public static bool IsDisableTaskWin =>
		GameData.Instance == null ||
		(GameData.Instance.TotalTasks == forceDisableTaskNum && GameData.Instance.CompletedTasks == 0);

	public static bool Prefix(GameData __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

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

		foreach (var playerInfo in __instance.AllPlayers.GetFastEnumerator())
		{
			if (!(
					GameSystem.TryGetTaskDoRole(playerInfo, out var role) &&
					role.HasTask() &&
					role.IsCrewmate()
				))
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

[HarmonyPatch(typeof(GameData), nameof(GameData.OnGameEnd))]
public static class GameDataOnGameEndPatch
{
	public static void Prefix()
	{
		ExtremeGameResultManager.Instance.CreateTaskInfo();
	}
}
