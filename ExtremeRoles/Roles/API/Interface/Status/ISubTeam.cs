namespace ExtremeRoles.Roles.API.Interface.Status;

public interface ISubTeam
{
	public NeutralSeparateTeam Main { get; }
	public NeutralSeparateTeam Sub { get; }
	public bool IsSub { get; }
}
