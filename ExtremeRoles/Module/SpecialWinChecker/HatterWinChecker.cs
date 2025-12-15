using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.SpecialWinChecker;

internal sealed class HatterWinChecker : IWinChecker
{
	public RoleGameOverReason Reason => RoleGameOverReason.HatterTeaPartyTime;

	private int hatterAliveNum = 0;

	public void AddAliveRole(
		byte playerId, SingleRoleBase role)
	{
		++this.hatterAliveNum;
	}

	public bool IsWin(
		PlayerStatistics statistics)
	{
		int killerPlayer = statistics.TeamImpostorAlive;

		foreach (int num in statistics.SeparatedNeutralAlive.Values)
		{
			killerPlayer += num;
		}
		killerPlayer += statistics.LiberalMilitantAlive;

		int otherPlayerNum = statistics.TotalAlive - killerPlayer - this.hatterAliveNum;

		return otherPlayerNum == killerPlayer && otherPlayerNum == this.hatterAliveNum;
	}
}
