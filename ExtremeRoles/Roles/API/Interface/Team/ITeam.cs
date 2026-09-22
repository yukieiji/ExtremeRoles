namespace ExtremeRoles.Roles.API.Interface.Team;

public interface ITeam
{
	public int GameControlId { get; }

	public bool? IsSame(SingleRoleBase targetRole);

	public void SetControlId(int id);
}
