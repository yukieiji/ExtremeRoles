namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{

    public int GameControlId { get; private set; } = 0;

    public bool IsVanillaRole() => this.Core.Id == ExtremeRoleId.VanillaRole;

    public bool IsCrewmate() => this.Core.Team == ExtremeRoleType.Crewmate;

    public bool IsImpostor() => this.Core.Team == ExtremeRoleType.Impostor;

    public bool IsNeutral() => this.Core.Team == ExtremeRoleType.Neutral;

    public virtual bool IsSameTeam(SingleRoleBase targetRole)
    {

        if (this.IsImpostor())
        {
            return targetRole.Core.Team == ExtremeRoleType.Impostor;
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
