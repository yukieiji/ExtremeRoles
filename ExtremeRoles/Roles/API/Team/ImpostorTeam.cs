using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.API.Team;

public sealed class ImpostorTeam : ITeam
{
    public int GameControlId { get; private set; }
    public ExtremeRoleType Type => ExtremeRoleType.Impostor;

    public bool Is(ExtremeRoleType teamType)
    {
        return Type == teamType;
    }

    public bool IsSameTeam(SingleRoleBase self, SingleRoleBase targetRole)
    {
        return targetRole.Team.Is(ExtremeRoleType.Impostor);
    }

    public void SetControlId(int id)
    {
        this.GameControlId = id;
    }
}
