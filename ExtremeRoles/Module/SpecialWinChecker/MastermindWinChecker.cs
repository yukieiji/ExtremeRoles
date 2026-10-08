using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.SpecialWinChecker;

public sealed class MastermindWinChecker : IWinChecker
{
	public RoleGameOverReason Reason => RoleGameOverReason.MastermindDeathGame;

	private int aliveNum = 0;

	public void AddAliveRole(byte playerId, SingleRoleBase role)
	{
		this.aliveNum++;
	}

	public bool IsWin(IPlayerStatistics statistics)
	{
		return this.aliveNum > 0 && statistics.TotalAlive <= 3;
	}
}
