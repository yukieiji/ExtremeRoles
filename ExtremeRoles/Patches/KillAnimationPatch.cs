using HarmonyLib;

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
public static class KillAnimationSetMovementKillPatch
{
    public static void Prefix(
        KillAnimation __instance,
        [HarmonyArgument(0)] PlayerControl source,
        [HarmonyArgument(1)] bool canMove)
    {
        if (source.PlayerId != CachedPlayerControl.LocalPlayer.PlayerId) { return; }
        var (killCheckerRole, anotherKillChekerRole) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleKillAnimationChecker>();

        IRoleKillAnimationChecker.SetKillAnimating(killCheckerRole, !canMove);
        IRoleKillAnimationChecker.SetKillAnimating(anotherKillChekerRole, !canMove);
    }
}
