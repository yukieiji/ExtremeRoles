using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Combination.Avalon;

public class AssassinStatusModel(
	bool canKilled,
	bool canKilledFromCrew,
	bool canKilledFromNeutral,
	bool canKilledFromLiberal,
	bool canSeeRoleBeforeFirstMeeting) : IStatusModel
{
	public bool IsBlockKill { get; } = !canKilled;
	public bool IsBlockKillFromCrew { get; } = !canKilledFromCrew;
	public bool IsBlockKillFromNeutral { get; } = !canKilledFromNeutral;
	public bool IsBlockKillFromLiberal { get; } = !canKilledFromLiberal;

	public bool IsSeeRoleBeforeFirstMeeting { get; } = canSeeRoleBeforeFirstMeeting;

	public bool IsFirstMeeting { get; private set; } = true;

	public void UseMeeting()
	{
		IsFirstMeeting = false;
	}
}
