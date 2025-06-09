using ExtremeRoles.Module.Interface;

using ExtremeRoles.Helper;


namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class NotAssignedPlayerAssignDataBuilder : IRoleAssignDataBuildBehaviour
{
	public int Priority => (int)ExtremeRoleAssignDataBuilder.Priority.Single;

	public void Build(in PreparationData data)
	{
		Logging.Debug($"------------------- NotAssignedPlayer to VanillaRole Assign - START -------------------");
		var assingn = data.Assign;
		foreach (var player in assingn.NeedRoleAssignPlayer)
		{
			var roleId = player.Role;
			Logging.Debug($"------------------- AssignToPlayer:{player.PlayerName} -------------------");
			Logging.Debug($"---AssignRole:{roleId}---");
			assingn.AddAssignData(new PlayerToSingleRoleAssignData(
				player.PlayerId, (byte)roleId, assingn.ControlId));
		}
		Logging.Debug($"------------------- NotAssignedPlayer to VanillaRole Assign - END -------------------");
	}
}
