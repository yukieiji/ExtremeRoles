using HarmonyLib;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
public static class DeadBodyOnClickPatch
{
	public static bool Prefix()
	{
		if (!GameProgressSystem.IsTaskPhase)
		{
			return true;
		}

		if (ButtonLockSystem.IsReportButtonLock())
		{
			return false;
		}

		if (PlayerControl.LocalPlayer == null ||
			PlayerControl.LocalPlayer.gameObject.TryGetComponent<BoxerButtobiBehaviour>(out _))
		{
			return false;
		}

		var (role, another) = ExtremeRoleManager.GetLocalRole();
		if (role is null) return true;

		var status = role.Status as IDeadBodyReportOverrideStatus;
		var anotherStatus = another?.Status as IDeadBodyReportOverrideStatus;

		return
			!OnemanMeetingSystemManager.IsActive &&
			(status != null ? status.CanReport : true) &&
			(anotherStatus != null ? anotherStatus.CanReport : true);
	}
}
