using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API.Extension.State;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Patches;

public class GameDataRecomputeTaskCountsPatchBody(IGameProgress progress, IGameRuntime runtime)
{
	private const int forceDisableTaskNum = 88659;

	public static bool IsDisableTaskWin =>
		GameData.Instance == null ||
		(GameData.Instance.TotalTasks == forceDisableTaskNum && GameData.Instance.CompletedTasks == 0);

	public bool Prefix(GameData __instance)
	{
		if (!progress.IsGameNow || !runtime.TryGetGameContext(out var ctx))
		{
			return true;
		}
		OverridePatch(__instance, ctx);
		return false;
	}
	private void OverridePatch(GameData __instance, IGameContext ctx)
	{
		var roles = ctx.Roles.All;
		var shipOpt = ctx.GlobalOption;

		if (roles.Count == 0 ||
			(shipOpt.DisableTaskWin && shipOpt.DisableTaskWinWhenNoneTaskCrew))
		{
			__instance.TotalTasks = forceDisableTaskNum;
			__instance.CompletedTasks = 0;
			return;
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
	}
}


[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
public static class GameDataRecomputeTaskCountsPatch
{
	private static GameDataRecomputeTaskCountsPatchBody? _body;

	public static bool Prefix(GameData __instance)
	{
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<GameDataRecomputeTaskCountsPatchBody>();
		}
		return _body.Prefix(__instance);
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
