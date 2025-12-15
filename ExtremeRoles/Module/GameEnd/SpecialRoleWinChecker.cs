using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.GameEnd;

public sealed class SpecialRoleWinChecker(PlayerStatistics statistics) : IGameEndChecker
{
	private readonly PlayerStatistics statistics = statistics;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = (GameOverReason)RoleGameOverReason.UnKnown;

		foreach (var (id, checker) in this.statistics.SpecialWinCheckRoleAlive)
		{
			if (checker.IsWin(this.statistics))
			{
				IGameEndChecker.SetWinGameContorlId(id);
				reason = (GameOverReason)checker.Reason;
				return true;
			}
		}
		return false;
	}
}
