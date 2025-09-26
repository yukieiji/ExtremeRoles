using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches.Button;

[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
public static class PassiveButtonReceiveClickDownPatch
{
    public static bool Prefix(PassiveButton __instance)
    {
        var obj = __instance.gameObject;
		var localPlayer = PlayerControl.LocalPlayer;

        if (obj == null ||
			localPlayer == null ||
			obj.transform.parent == null ||
            obj.transform.parent.name == GameSystem.BottomRightButtonGroupObjectName ||
            ExtremeRoleManager.GameRole.Count == 0 ||
            !RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		if (localPlayer.gameObject.TryGetComponent<BoxerButtobiBehaviour>(out _))
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
