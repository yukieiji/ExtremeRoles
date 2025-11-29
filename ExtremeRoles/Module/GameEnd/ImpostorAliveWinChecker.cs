using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.GameEnd;

public sealed class ImpostorAliveWinChecker(PlayerStatistics statistics) : IGameEndChecker
{
	private readonly PlayerStatistics statistics = statistics;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = GameData.LastDeathReason switch
		{
			DeathReason.Exile => GameOverReason.ImpostorsByVote,
			DeathReason.Kill => GameOverReason.ImpostorsByKill,
			_ => GameOverReason.CrewmateDisconnect,
		};
		return
			this.statistics.SeparatedNeutralAlive.Count <= 0 &&
			this.statistics.LiberalKillerAlive <= 0 &&
			this.statistics.TeamImpostorAlive >= (this.statistics.TotalAlive - this.statistics.TeamImpostorAlive);
	}
}
