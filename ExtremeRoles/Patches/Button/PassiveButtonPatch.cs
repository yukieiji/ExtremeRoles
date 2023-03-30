using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches.Button;

[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
public static class PassiveButtonReceiveClickDownPatch
{
    public static bool Prefix(PassiveButton __instance)
    {
        GameObject obj = __instance.gameObject;

        if (obj is null ||
            obj.transform.parent is null ||
            obj.transform.parent.name == GameSystem.BottomRightButtonGroupObjectName ||
            ExtremeRoleManager.GameRole.Count == 0 ||
            !RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

        var (useRole, anotherUseRole) =
            ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleUsableOverride>();

        return
            (useRole is null && anotherUseRole is null) ||
            (useRole.EnableUseButton && anotherUseRole is null) ||
            (useRole is null && anotherUseRole.EnableUseButton) ||
            (useRole.EnableUseButton && anotherUseRole.EnableUseButton);
    }
}
