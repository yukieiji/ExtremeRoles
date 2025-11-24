using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Combination.Avalon;

public class AssassinStatusModel(
	bool canKilled,
	bool canKilledFromCrew,
	bool canKilledFromNeutral,
	bool canKilledFromLiberal) : IStatusModel
{
	public bool IsBlockKill { get; } = !canKilled;
	public bool IsBlockKillFromCrew { get; } = !canKilledFromCrew;
	public bool IsBlockKillFromNeutral { get; } = !canKilledFromNeutral;
	public bool IsBlockKillFromLiberal { get; } = !canKilledFromLiberal;
}
