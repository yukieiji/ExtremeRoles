namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{

    public int GameControlId { get; private set; } = 0;

    public bool IsVanillaRole() => this.Core.Id == ExtremeRoleId.VanillaRole;

    public bool IsCrewmate() => this.Core.Team == ExtremeRoleType.Crewmate;

    public bool IsImpostor() => this.Core.Team == ExtremeRoleType.Impostor;

    public bool IsNeutral() => this.Core.Team == ExtremeRoleType.Neutral;

    public bool IsLiberal() => this.Core.Team == ExtremeRoleType.Liberal;

    public virtual bool IsSameTeam(SingleRoleBase targetRole)
    {
        if (this.IsLiberal())
        {
            return targetRole.IsLiberal();
        }

        if (this.IsImpostor())
        {
            return targetRole.IsImpostor();
        }

        if (targetRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is not null)

		{
			return this.IsSameTeam(multiAssignRole.AnotherRole);
		}

        return false;
    }

    public void SetControlId(int id)
    {
        this.GameControlId = id;
    }

    protected bool IsSameControlId(SingleRoleBase tarrgetRole)
    {
        return this.GameControlId == tarrgetRole.GameControlId;
    }

}
