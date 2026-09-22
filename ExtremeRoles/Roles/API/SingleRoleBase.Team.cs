using ExtremeRoles.Roles.API.Interface.Team;

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
	public ITeam Team { get; }

    public int GameControlId => this.Team.GameControlId;

    public bool IsVanillaRole() => this.Team.IsVanillaRole;

    public bool IsCrewmate() => this.Team.IsCrewmate;

    public bool IsImpostor() => this.Team.IsImpostor;

    public bool IsNeutral() => this.Team.IsNeutral;

    public bool IsLiberal() => this.Team.IsLiberal;

    public bool IsSameTeam(SingleRoleBase targetRole)
    {
		bool? result = this.Team.IsSame(targetRole);
		if (result.HasValue)
		{
			return result.Value;
		}

        if (targetRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is not null)

		{
			return this.IsSameTeam(multiAssignRole.AnotherRole);
		}

        return false;
    }

	public void SetControlId(int id)
		=> this.Team.SetControlId(id);

    protected bool IsSameControlId(SingleRoleBase tarrgetRole)
    {
        return this.GameControlId == tarrgetRole.GameControlId;
    }

}
