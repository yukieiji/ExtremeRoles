using AmongUs.GameOptions;
using HarmonyLib;

namespace ExtremeRoles.Patches.Role
{
    [HarmonyPatch(
        typeof(ShapeshifterRole),
        nameof(ShapeshifterRole.FixedUpdate))]
    public static class ShapeshifterRoleFixedUpdatePatch
    {
        public static void Postfix(ShapeshifterRole __instance)
        {
            __instance.DefaultGhostRole = RoleTypes.ImpostorGhost;
        }
    }
}
