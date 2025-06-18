using HarmonyLib;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(KillAnimation._CoPerformKill_d__2), nameof(KillAnimation._CoPerformKill_d__2.MoveNext))]
public static class KillAnimationCoPerformKillPatch
{
    public static bool HideNextAnimation = false;
    public static void Prefix(
        KillAnimation._CoPerformKill_d__2 __instance)
    {
        if (HideNextAnimation)
        {
            __instance.source = __instance.target;
        }
        HideNextAnimation = false;
    }
}

[HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
public static class KillAnimationSetMovementPatch
{
    public static void Prefix(
        [HarmonyArgument(0)] PlayerControl source,
        [HarmonyArgument(1)] bool canMove)
    {
        if (!RoleAssignState.Instance.IsRoleSetUpEnd || canMove ||
            source.PlayerId != PlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}

		ExtremeRoleManager.InvokeInterfaceRoleMethod<IRolePerformKillHook>(
			x => x.OnStartKill());
    }

    public static void Postfix(
        [HarmonyArgument(0)] PlayerControl source,
        [HarmonyArgument(1)] bool canMove)
    {
        if (!RoleAssignState.Instance.IsRoleSetUpEnd || !canMove ||
            source.PlayerId != PlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}

        ExtremeRoleManager.InvokeInterfaceRoleMethod<IRolePerformKillHook>(
			x => x.OnEndKill());
    }
}