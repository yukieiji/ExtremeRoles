using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Combination.Avalon;

public class AssassinStatusModel(bool canKilled, bool canKilledFromCrew, bool canKilledFromNeutral) : IStatusModel
{
	public bool CanKilled { get; } = canKilled;
	public bool CanKilledFromCrew { get; } = canKilledFromCrew;
	public bool CanKilledFromNeutral { get; } = canKilledFromNeutral;
}
