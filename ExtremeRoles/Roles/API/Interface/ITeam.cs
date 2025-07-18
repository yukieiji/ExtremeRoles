namespace ExtremeRoles.Roles.API.Interface;

public interface ITeam
{
    public int GameControlId { get; }
    public ExtremeRoleType Type { get; }

    public bool Is(ExtremeRoleType teamType);

    public bool IsSameTeam(SingleRoleBase self, SingleRoleBase targetRole);

    public void SetControlId(int id);
}
