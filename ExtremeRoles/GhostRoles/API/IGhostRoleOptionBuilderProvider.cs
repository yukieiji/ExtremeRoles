namespace ExtremeRoles.GhostRoles.API;

public interface IGhostRoleOptionBuilderProvider
{
	public IGhostRoleOptionBuilder Get(ExtremeGhostRoleId id);
}
