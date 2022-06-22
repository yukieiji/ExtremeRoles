using HarmonyLib;
using UnityEngine;
using ExtremeRoles.Roles;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Patches.Role
{
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.FindClosestTarget))]
    public class RoleRoleBehaviourFindClosestTargetPatch
    {
        static bool Prefix(
            RoleBehaviour __instance,
            ref PlayerControl __result)
        {
            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return true; }

            var gameRoles = ExtremeRoleManager.GameRole;

            if (gameRoles.Count == 0) { return true; }

            var role = gameRoles[__instance.Player.PlayerId];

            __result = null;

            int killRange = PlayerControl.GameOptions.KillDistance;
            if (role.HasOtherKillRange)
            {
                killRange = role.KillRange;
            }

            float num = GameOptionsData.KillDistances[Mathf.Clamp(killRange, 0, 2)];

            if (!ShipStatus.Instance)
            {
                return false;
            }
            Vector2 truePosition = __instance.Player.GetTruePosition();

            foreach (GameData.PlayerInfo playerInfo in
                GameData.Instance.AllPlayers.GetFastEnumerator())
            {

                if (playerInfo == null) { continue; }

                if (!playerInfo.Disconnected &&
                    (playerInfo.PlayerId != __instance.Player.PlayerId) &&
                    !playerInfo.IsDead &&
                    !role.IsSameTeam(gameRoles[playerInfo.PlayerId]) &&
                    playerInfo.Object != null &&
                    (!playerInfo.Object.inVent || OptionHolder.Ship.CanKillVentInPlayer))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object && @object.Collider.enabled)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num &&
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                truePosition, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            __result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return false;
        }
    }
}
