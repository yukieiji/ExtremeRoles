using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.API.Team;

public sealed class CrewmateTeam : ITeam
{
    public int GameControlId { get; private set; }
    public ExtremeRoleType Type => ExtremeRoleType.Crewmate;

    public bool Is(ExtremeRoleType teamType)
    {
        return Type == teamType;
    }

    public bool IsSameTeam(SingleRoleBase self, SingleRoleBase targetRole)
    {
        MultiAssignRoleBase multiAssignRole = targetRole as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            if (multiAssignRole.AnotherRole != null)
            {
                return self.IsSameTeam(
                    multiAssignRole.AnotherRole);
            }
        }

        return false;
    }

    public void SetControlId(int id)
    {
        this.GameControlId = id;
    }
}
