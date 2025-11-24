using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Patches.Role;

[HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.GetAbilityDistance))]
public static class RoleBehaviourGetAbilityDistancePatch
{
    public static bool Prefix(
        RoleBehaviour __instance,
        ref float __result)
    {
        if (!(
				GameProgressSystem.IsGameNow &&
				ExtremeRoleManager.TryGetRole(__instance.Player.PlayerId, out var role) &&
				role.CanKill() &&
				role.TryGetKillRange(out int range) &&
				GameSystem.TryGetKillDistance(out var killRange)
			))
		{
			return true;
		}

        __result = killRange[range];

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
        if (!GameProgressSystem.IsGameNow)
		{
			return true;
		}

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
			(role.AbilityClass is not IInvincible invincible || invincible.IsValidTarget(targetPlayerId));

        return false;
    }
}
