using ExtremeRoles.Roles.API.Interface.Team;

namespace ExtremeRoles.Roles.API;

public class DefaultTeam(RoleCore core) : ITeam
{
	public RoleCore Core { get; } = core;

	public int GameControlId { get; private set; }

	public bool? IsSame(SingleRoleBase targetRole)
	{
		if (this.Core.IsLiberal)
		{
			return targetRole.IsLiberal();
		}

		if (this.Core.IsImpostor)
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
