using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;

namespace ExtremeRoles.Patches.Role
{
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.GetAbilityDistance))]
    public static class RoleBehaviourGetAbilityDistancePatch
    {
        public static bool Prefix(
            RoleBehaviour __instance,
            ref float __result)
        {
            if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

            var role = ExtremeRoleManager.GameRole[__instance.Player.PlayerId];

            if (!role.CanKill() || !role.TryGetKillRange(out int range)) { return true; }

            __result = GameOptionsData.KillDistances[range];

            return false;
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.IsValidTarget))]
    public static class RoleBehaviourIsValidTargetPatch
    {
        public static bool Prefix(
            RoleBehaviour __instance,
            ref bool __result,
            [HarmonyArgument(0)] GameData.PlayerInfo target)
        {
            if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

            var gameRoles = ExtremeRoleManager.GameRole;
            var role = ExtremeRoleManager.GameRole[__instance.Player.PlayerId];

            if (!role.CanKill()) { return true; }

            __result = 
                target != null && 
                !target.Disconnected && 
                !target.IsDead && 
                target.PlayerId !=  __instance.Player.PlayerId && 
                !(target.Role == null) && 
                !(target.Object == null) && 
                (!target.Object.inVent || ExtremeGameModeManager.Instance.ShipOption.CanKillVentInPlayer) &&
                !role.IsSameTeam(gameRoles[target.PlayerId]);

            return false;
        }
    }
}
