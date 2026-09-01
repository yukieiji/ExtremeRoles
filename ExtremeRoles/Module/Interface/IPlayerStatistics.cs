using System.Collections.Generic;
using ExtremeRoles.Module.GameEnd;

namespace ExtremeRoles.Module.Interface;

public interface IPlayerStatistics
{
	int AllTeamCrewmate { get; }
	int TeamImpostorAlive { get; }
	int TeamCrewmateAlive { get; }
	int TeamNeutralAlive { get; }
	int TeamLiberalAlive { get; }
	int LiberalMilitantAlive { get; }
	bool LeaderIsBlockKill { get; }
	int TotalAlive { get; }
	int AssassinAlive { get; }

	IReadOnlyDictionary<int, IWinChecker> SpecialWinCheckRoleAlive { get; }
	IReadOnlyDictionary<NeutralSeparateTeamContainer.NeutralTeam, int> SeparatedNeutralAlive { get; }

	void Update();
}
