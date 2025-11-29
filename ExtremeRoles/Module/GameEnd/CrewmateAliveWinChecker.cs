using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.GameEnd;

public sealed class CrewmateAliveWinChecker(PlayerStatistics statistics) : IGameEndChecker
{
	private readonly PlayerStatistics statistics = statistics;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = GameOverReason.CrewmatesByVote;
		return
			(
				this.statistics.TeamCrewmateAlive > 0 &&
				this.statistics.LiberalKillerAlive <= 0 &&
				this.statistics.TeamImpostorAlive <= 0 &&
				this.statistics.SeparatedNeutralAlive.Count <= 0
			)
			||
			(
				this.statistics.TeamCrewmateAlive <= 0 &&
				this.statistics.LiberalKillerAlive <= 0 &&
				this.statistics.TeamImpostorAlive <= 0 &&
				this.statistics.SeparatedNeutralAlive.Count <= 0 &&
				this.statistics.TotalAlive == statistics.TeamNeutralAlive
			);
	}
}
