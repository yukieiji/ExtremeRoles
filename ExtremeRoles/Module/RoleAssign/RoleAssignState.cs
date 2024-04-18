namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleAssignState : NullableSingleton<RoleAssignState>
{
	public bool IsRoleSetUpEnd { get; private set; } = false;

	public bool IsReady { get; private set; } = false;

	public void SwitchRoleAssignToEnd()
	{
		this.IsRoleSetUpEnd = true;
	}

	public void SwitchToReady()
	{
		this.IsReady = true;
	}
}
