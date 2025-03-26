using System;
using System.Collections.Generic;

using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.GameResult.StatusOverrider;

using DeadInfo = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.DeadInfo;
using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;
using TaskInfo = ExtremeRoles.Module.GameResult.ExtremeGameResultManager.TaskInfo;
using PlayerSummary = ExtremeRoles.Module.CustomMonoBehaviour.FinalSummary.PlayerSummary;

namespace ExtremeRoles.Module.GameResult;


public class FinalSummaryBuilder(
	GameOverReason reason,
	IReadOnlyDictionary<byte, DeadInfo> deadInfo,
	IReadOnlyDictionary<byte, TaskInfo> taskInfo) : IDisposable
{
	private readonly GameOverReason reason = reason;
	private readonly IReadOnlyDictionary<byte, DeadInfo> deadInfo = deadInfo;
	private readonly IReadOnlyDictionary<byte, TaskInfo> taskInfo = taskInfo;
	private readonly IReadOnlyDictionary<GameOverReason, IStatusOverrider> statusOverride = staticStatusOverride;
	private static Dictionary<GameOverReason, IStatusOverrider> staticStatusOverride = new Dictionary<GameOverReason, IStatusOverrider>();

	public static void AddStatusOverride(GameOverReason reason, IStatusOverrider @override)
	{
		staticStatusOverride.Add(reason, @override);
	}

	public static void AddStatusOverride<T>(GameOverReason reason) where T : IStatusOverrider, new()
	{
		staticStatusOverride.Add(reason, new T());
	}

	public PlayerSummary? Create(
		NetworkedPlayerInfo playerInfo,
		SingleRoleBase role,
		GhostRoleBase ghostRole)
	{
		byte playerId = playerInfo.PlayerId;
		if (!this.taskInfo.TryGetValue(playerId, out var taskInfo))
		{
			return null;
		}
		var finalStatus = PlayerStatus.Alive;

		if (playerInfo.IsDead &&
			this.deadInfo.TryGetValue(playerId, out DeadInfo deadInfo))
		{
			finalStatus = deadInfo.Reason;
		}
		else if (playerInfo.Disconnected)
		{
			finalStatus = PlayerStatus.Disconnected;
		}

		if (this.statusOverride.TryGetValue(this.reason, out var overrider) &&
			overrider.TryGetOverride(role, ghostRole, playerInfo, out var status))
		{
			finalStatus = status;
		}
		else if (
			this.reason is GameOverReason.ImpostorsBySabotage &&
			!role.IsImpostor() &&
			finalStatus is PlayerStatus.Alive)
		{
			finalStatus = PlayerStatus.Dead;
		}

		int totalTask = taskInfo.TotalTask;
		int completedTaskNum = taskInfo.CompletedTask;
		if (this.reason is GameOverReason.CrewmatesByTask && role.IsCrewmate())
		{
			ExtremeRolesPlugin.Logger.LogWarning($"WARNING:Force replace taskNum: {completedTaskNum} => {totalTask}");
			completedTaskNum = totalTask;
		}

		return
			new PlayerSummary(
				playerId,
				playerInfo.PlayerName,
				role,
				ghostRole,
				completedTaskNum,
				totalTask,
				finalStatus);
	}

	public void Dispose()
	{
		staticStatusOverride.Clear();
	}
}
