using System;
using System.Collections.Generic;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;

using DeadInfo = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.DeadInfo;
using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;
using TaskInfo = ExtremeRoles.Module.GameResult.ExtremeGameResultManager.TaskInfo;
using PlayerSummary = ExtremeRoles.Module.CustomMonoBehaviour.FinalSummary.PlayerSummary;

namespace ExtremeRoles.Module.GameResult;

public interface IStatusOverride
{
	public bool TryGetOverride(
		SingleRoleBase role,
		GhostRoleBase ghostRole,
		NetworkedPlayerInfo player,
		out PlayerStatus status);
}

public class FinalSummaryBuilder(
	GameOverReason reason,
	IReadOnlyDictionary<byte, DeadInfo> deadInfo,
	IReadOnlyDictionary<byte, TaskInfo> taskInfo) : IDisposable
{
	private readonly GameOverReason reason = reason;
	private readonly IReadOnlyDictionary<byte, DeadInfo> deadInfo = deadInfo;
	private readonly IReadOnlyDictionary<byte, TaskInfo> taskInfo = taskInfo;
	private readonly IReadOnlyDictionary<GameOverReason, IStatusOverride> statusOverride = staticStatusOverride;
	private static Dictionary<GameOverReason, IStatusOverride> staticStatusOverride = new Dictionary<GameOverReason, IStatusOverride>();

	public static void AddStatusOverride(GameOverReason reason, IStatusOverride @override)
	{
		staticStatusOverride.Add(reason, @override);
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
		// IsImpostor

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
			this.reason is GameOverReason.ImpostorBySabotage &&
			!role.IsImpostor() &&
			finalStatus is PlayerStatus.Alive)
		{
			finalStatus = PlayerStatus.Dead;
		}

		return
			new PlayerSummary(
				playerId,
				playerInfo.PlayerName,
				role,
				ghostRole,
				this.reason is GameOverReason.HumansByTask && role.IsCrewmate() ?
					taskInfo.TotalTask : taskInfo.CompletedTask,
				taskInfo.TotalTask,
				finalStatus);
	}

	public void Dispose()
	{
		staticStatusOverride.Clear();
	}
}
