using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Button
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public static class KillButtonDoClickPatch
    {
        
        public static bool Prefix(KillButton __instance)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            PlayerControl killer = CachedPlayerControl.LocalPlayer;
            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (__instance.isActiveAndEnabled &&
                __instance.currentTarget &&
                !__instance.isCoolingDown &&
                !killer.Data.IsDead &&
                killer.CanMove && 
                role.CanKill())
            {
                PlayerControl target = __instance.currentTarget;

                if (target.Data.IsDead) { return false; }

                var targetPlayerRole = ExtremeRoleManager.GameRole[target.PlayerId];
                if (role.Id == ExtremeRoleId.Villain)
                {
                    villainSpecialKill(__instance, killer, target, targetPlayerRole);
                    return false;
                }

                bool canKill = role.TryRolePlayerKillTo(
                    killer, target);
                if (!canKill) { return false; }

                canKill = targetPlayerRole.TryRolePlayerKilledFrom(
                    target, killer);
                if (!canKill) { return false; }

                var multiAssignRole = role as MultiAssignRoleBase;
                if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        canKill = multiAssignRole.AnotherRole.TryRolePlayerKillTo(
                            killer, target);
                        if (!canKill) { return false; }
                    }
                }

                multiAssignRole = targetPlayerRole as MultiAssignRoleBase;
                if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        canKill = multiAssignRole.AnotherRole.TryRolePlayerKilledFrom(
                            target, killer);
                        if (!canKill) { return false; }
                    }
                }

                if (BodyGuard.TryRpcKillGuardedBodyGuard(killer.PlayerId, target.PlayerId))
                {
                    return false;
                }

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                if (IsMissMuderKill(killer, target)) {  return false; }

                var lastWolf = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<LastWolf>();
                
                excuteKill(
                    __instance, killer, target,
                    lastWolf == null || !lastWolf.IsAwake);
            }
            return false;
        }

        public static bool IsMissMuderKill(
            PlayerControl killer,
            PlayerControl target)
        {
            return
                AmongUsClient.Instance.IsGameOver &&
                killer is null &&
                killer.Data is null &&
                killer.Data.IsDead &&
                killer.Data.Disconnected &&
                target is null &&
                target.Data is null &&
                target.Data.IsDead &&
                target.Data.Disconnected &&
                target.inMovingPlat;
        }

        private static void villainSpecialKill(
            KillButton instance,
            PlayerControl killer,
            PlayerControl target,
            SingleRoleBase targetRole)
        {
            if (targetRole.Id == ExtremeRoleId.Vigilante)
            {
                var vigilante = (Vigilante)targetRole;
                if (vigilante.Condition != Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip)
                {
                    return;
                }
            }
            else if (targetRole.Id == ExtremeRoleId.Hero)
            {
                HeroAcademia.RpcDrawHeroAndVillan(
                    target, killer);
                return;
            }
            else if (IsMissMuderKill(killer, target))
            {
                return;
            }
            excuteKill(instance, killer, target);
        }

        private static void excuteKill(
            KillButton instance,
            PlayerControl killer,
            PlayerControl target,
            bool isAnime = true)
        {

            Helper.Player.RpcUncheckMurderPlayer(
                killer.PlayerId, target.PlayerId,
                isAnime ? byte.MaxValue : byte.MinValue);
            instance.SetTarget(null);
        }
    }
}
