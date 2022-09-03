using System.Collections.Generic;

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
        public enum MurderKillResult
        {
            Failure,
            NormalKill,
            NoAnimatedKill
        }
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

                if (BodyGuard.TryGetShiledPlayerId(target.PlayerId, out byte bodyGuard) &&
                    BodyGuard.RpcTryKillBodyGuard(killer.PlayerId, bodyGuard))
                {
                    return false;
                }

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderKillResult res = checkMuderKill(
                    __instance, killer, target);

                var lastWolf = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<LastWolf>();
                if (lastWolf != null)
                {
                    if (lastWolf.IsAwake)
                    {
                        res = MurderKillResult.NoAnimatedKill;
                    }
                }

                switch (res)
                {
                    case MurderKillResult.NormalKill:

                        RPCOperator.Call(
                            CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                            RPCOperator.Command.UncheckedMurderPlayer,
                            new List<byte> { killer.PlayerId, target.PlayerId, byte.MaxValue });
                        RPCOperator.UncheckedMurderPlayer(
                            killer.PlayerId,
                            target.PlayerId,
                            byte.MaxValue);
                        break;
                    case MurderKillResult.NoAnimatedKill:
                        RPCOperator.Call(
                            CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                            RPCOperator.Command.UncheckedMurderPlayer,
                            new List<byte> { killer.PlayerId, target.PlayerId, 0 });
                        RPCOperator.UncheckedMurderPlayer(
                            killer.PlayerId,
                            target.PlayerId, 0);
                        break;
                    default:
                        break;
                }

                __instance.SetTarget(null);
            }
            return false;
        }

        private static MurderKillResult checkMuderKill(
            KillButton instance,
            PlayerControl killer,
            PlayerControl target)
        {
            if (AmongUsClient.Instance.IsGameOver) { return MurderKillResult.Failure; }
            if (killer == null ||
                killer.Data == null ||
                killer.Data.IsDead ||
                killer.Data.Disconnected)
            {
                return MurderKillResult.Failure; // Allow non Impostor kills compared to vanilla code
            }
            if (target == null || 
                target.Data == null || 
                target.Data.IsDead || 
                target.Data.Disconnected)
            {
                return MurderKillResult.Failure; // Allow killing players in vents compared to vanilla code
            }
            if (target.PlayerId != instance.currentTarget.PlayerId)
            {
                return MurderKillResult.NoAnimatedKill;
            }

            return MurderKillResult.NormalKill;

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
            
            MurderKillResult res = checkMuderKill(
                instance, killer, target);

            switch (res)
            {
                case MurderKillResult.NormalKill:

                    RPCOperator.Call(
                        CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                        RPCOperator.Command.UncheckedMurderPlayer,
                        new List<byte> { killer.PlayerId, target.PlayerId, byte.MaxValue });
                    RPCOperator.UncheckedMurderPlayer(
                        killer.PlayerId,
                        target.PlayerId,
                        byte.MaxValue);
                    break;
                case MurderKillResult.NoAnimatedKill:
                    RPCOperator.Call(
                        CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                        RPCOperator.Command.UncheckedMurderPlayer,
                        new List<byte> { killer.PlayerId, target.PlayerId, 0 });
                    RPCOperator.UncheckedMurderPlayer(
                        killer.PlayerId,
                        target.PlayerId, 0);
                    break;
                default:
                    break;
            }
            instance.SetTarget(null);
        }
    }
}
