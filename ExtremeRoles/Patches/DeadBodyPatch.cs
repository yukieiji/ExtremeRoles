using HarmonyLib;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface.Status;

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

		var localPlayer = PlayerControl.LocalPlayer;

		if (ButtonLockSystem.IsReportButtonLock() ||
			localPlayer == null ||
			localPlayer.gameObject.TryGetComponent<BoxerButtobiBehaviour>(out _))
		{
			return false;
		}

		return 
			!OnemanMeetingSystemManager.IsActive && 
			ExtremeRoleManager.GetLocalRoleCastedStatusFlag<IDeadBodyReportOverrideStatus>(x => x.CanReport);
	}
}
