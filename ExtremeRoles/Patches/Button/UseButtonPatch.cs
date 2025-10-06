using HarmonyLib;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Patches.Button;

[HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
public static class UseButtonReceiveClickDownPatch
{
    public static bool Prefix()
    {
        if (ExtremeRoleManager.GameRole.Count == 0 ||
            !RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		var pc = PlayerControl.LocalPlayer;
		if (pc == null || pc.gameObject.TryGetComponent<BoxerButtobiBehaviour>(out _))
		{
			return false;
		}

		return ExtremeRoleManager.GetLocalRoleCastedStatusFlag<IUsableOverrideStatus>(x => x.EnableUseButton);
    }
}
