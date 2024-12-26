using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Module.SystemType.Roles;

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
            [HarmonyArgument(0)] NetworkedPlayerInfo target)
        {
            if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

			byte instancePlayerId = __instance.Player.PlayerId;

            if (!(
					ExtremeRoleManager.TryGetRole(instancePlayerId, out var role) &&
					role.CanKill()
				))
			{
				return true;
			}

			byte targetPlayerId = target.PlayerId;


            __result =
                target != null &&
                !target.Disconnected &&
                !target.IsDead &&
				targetPlayerId != instancePlayerId &&
                target.Role != null &&
                target.Object != null &&
                (
					!target.Object.inVent ||
					ExtremeGameModeManager.Instance.ShipOption.Vent.CanKillVentInPlayer
				) &&
				!target.Object.inMovingPlat &&
				ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole) &&
				!role.IsSameTeam(targetRole) &&
				!MonikaTrashSystem.InvalidTarget(targetRole, instancePlayerId);

            return false;
        }
    }
}
