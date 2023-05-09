using HarmonyLib;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
public static class KillAnimationCoPerformKillPatch
{
    public static bool HideNextAnimation = false;
    public static void Prefix(
        KillAnimation __instance,
        [HarmonyArgument(0)] ref PlayerControl source,
        [HarmonyArgument(1)] ref PlayerControl target)
    {
        if (HideNextAnimation)
        {
            source = target;
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
            source.PlayerId != CachedPlayerControl.LocalPlayer.PlayerId) { return; }

        var (role, anothorRole) = ExtremeRoleManager.GetInterfaceCastedLocalRole<
            IRolePerformKillHook>();
        role?.OnStartKill();
        anothorRole?.OnStartKill();
    }

    public static void Postfix(
        [HarmonyArgument(0)] PlayerControl source,
        [HarmonyArgument(1)] bool canMove)
    {
        if (!RoleAssignState.Instance.IsRoleSetUpEnd || !canMove ||
            source.PlayerId != CachedPlayerControl.LocalPlayer.PlayerId) { return; }

        var (role, anothorRole) = ExtremeRoleManager.GetInterfaceCastedLocalRole<
            IRolePerformKillHook>();
        role?.OnEndKill();
        anothorRole?.OnEndKill();
    }
}