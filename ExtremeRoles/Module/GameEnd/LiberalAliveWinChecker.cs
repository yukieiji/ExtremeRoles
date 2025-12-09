using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.GameEnd;

public sealed class LiberalAliveWinChecker(PlayerStatistics statistics) : IGameEndChecker
{
	private readonly PlayerStatistics statistics = statistics;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = (GameOverReason)RoleGameOverReason.LiberalRevolution;
		return
			(
				this.statistics.TeamCrewmateAlive <= 0 &&
				this.statistics.TeamImpostorAlive <= 0 &&
				this.statistics.SeparatedNeutralAlive.Count <= 0 &&
				this.statistics.TotalAlive == this.statistics.TeamLiberalAlive
			);
	}
}
