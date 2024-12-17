using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
public static class DeadBodyOnClickPatch
{
	public static bool Prefix()
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

		var (role, another) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IDeadBodyReportOverride>();

		return
			!OnemanMeetingSystemManager.IsActive &&
			(role != null ? role.CanReport : true) &&
			(another != null ? another.CanReport : true);
	}
}
