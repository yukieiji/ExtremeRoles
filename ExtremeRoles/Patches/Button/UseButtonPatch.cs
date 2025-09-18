using HarmonyLib;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;

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

		if (PlayerControl.LocalPlayer == null ||
			PlayerControl.LocalPlayer.gameObject.TryGetComponent<BoxerButtobiBehaviour>(out _))
		{
			return false;
		}

        var (useRole, anotherUseRole) = 
            ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleUsableOverride>();

        return 
            (useRole is null && anotherUseRole is null) ||
            (useRole.EnableUseButton && anotherUseRole is null) ||
            (useRole is null && anotherUseRole.EnableUseButton) ||
            (useRole.EnableUseButton && anotherUseRole.EnableUseButton);
    }
}
