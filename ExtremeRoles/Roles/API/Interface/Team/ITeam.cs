namespace ExtremeRoles.Roles.API.Interface.Team;

public interface ITeam
{
	public int GameControlId { get; }

	public bool IsVanillaRole { get; }

	public bool IsCrewmate { get; }

	public bool IsImpostor { get; }

	public bool IsNeutral { get; }

	public bool IsLiberal { get; }

	public bool? IsSame(SingleRoleBase targetRole);

	public void SetControlId(int id);
}
