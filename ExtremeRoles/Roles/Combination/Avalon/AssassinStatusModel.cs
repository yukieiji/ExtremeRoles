using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Combination.Avalon;

public class AssassinStatusModel(bool canKilled, bool canKilledFromCrew, bool canKilledFromNeutral) : IStatusModel
{
	public bool IsBlockKill { get; } = !canKilled;
	public bool IsBlockKillFromCrew { get; } = !canKilledFromCrew;
	public bool IsBlockKillFromNeutral { get; } = !canKilledFromNeutral;
}
