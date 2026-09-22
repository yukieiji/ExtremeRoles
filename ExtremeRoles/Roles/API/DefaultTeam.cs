using ExtremeRoles.Roles.API.Interface.Team;

namespace ExtremeRoles.Roles.API;

public class DefaultTeam(RoleCore core) : ITeam
{
	public RoleCore Core { get; } = core;

	public int GameControlId { get; private set; }

	public bool IsVanillaRole => Core.Id == ExtremeRoleId.VanillaRole;

	public bool IsCrewmate => Core.Team == ExtremeRoleType.Crewmate;

	public bool IsImpostor => Core.Team == ExtremeRoleType.Impostor;

	public bool IsNeutral => Core.Team == ExtremeRoleType.Neutral;

	public bool IsLiberal => Core.Team == ExtremeRoleType.Liberal;

	public bool? IsSame(SingleRoleBase targetRole)
	{
		if (this.IsLiberal)
		{
			return targetRole.IsLiberal();
		}

		if (this.IsImpostor)
		{
			return targetRole.IsImpostor();
		}
		return null;
	}

	public void SetControlId(int id)
	{
		this.GameControlId = id;
	}
}
