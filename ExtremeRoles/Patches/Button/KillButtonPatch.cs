using System.Collections.Generic;

using HarmonyLib;


namespace ExtremeRoles.Patches.Button
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    class KillButtonDoClickPatch
    {
        public enum MurderKillResult
        {
            Failure,
            NormalKill,
            NoAnimatedKill
        }
        public static bool Prefix(KillButton __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            PlayerControl killer = PlayerControl.LocalPlayer;
            var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();

            if (__instance.isActiveAndEnabled &&
                __instance.currentTarget &&
                !__instance.isCoolingDown &&
                !killer.Data.IsDead &&
                killer.CanMove && 
                role.CanKill)
            {
                PlayerControl target = __instance.currentTarget;

                if (target.Data.IsDead) { return false; }

                var targetPlayerRole = Roles.ExtremeRoleManager.GameRole[target.PlayerId];

                bool canKill = role.TryRolePlayerKillTo(
                    killer, target);
                if (!canKill) { return false; }

                canKill = targetPlayerRole.TryRolePlayerKilledFrom(
                    target, killer);
                if (!canKill) { return false; }

                var multiAssignRole = role as Roles.API.MultiAssignRoleBase;
                if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        canKill = multiAssignRole.AnotherRole.TryRolePlayerKillTo(
                            killer, target);
                        if (!canKill) { return false; }
                    }
                }

                multiAssignRole = targetPlayerRole as Roles.API.MultiAssignRoleBase;
                if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        canKill = multiAssignRole.AnotherRole.TryRolePlayerKilledFrom(
                            target, killer);
                        if (!canKill) { return false; }
                    }
                }


                var bodyGuard = ExtremeRolesPlugin.GameDataStore.ShildPlayer.GetBodyGuardPlayerId(
                    target.PlayerId);

                if (bodyGuard != byte.MaxValue)
                {
                    target = Helper.Player.GetPlayerControlById(bodyGuard);
                    if (target == null)
                    {
                        target = __instance.currentTarget;
                    }
                    else if (target.Data.IsDead || target.Data.Disconnected)
                    {
                        target = __instance.currentTarget;
                    }
                }

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderKillResult res = checkMuderKill(
                    __instance, killer, target);

                switch (res)
                {
                    case MurderKillResult.NormalKill:

                        RPCOperator.Call(
                            PlayerControl.LocalPlayer.NetId,
                            RPCOperator.Command.UncheckedMurderPlayer,
                            new List<byte> { killer.PlayerId, target.PlayerId, byte.MaxValue });
                        RPCOperator.UncheckedMurderPlayer(
                            killer.PlayerId,
                            target.PlayerId,
                            byte.MaxValue);
                        break;
                    case MurderKillResult.NoAnimatedKill:
                        RPCOperator.Call(
                            PlayerControl.LocalPlayer.NetId,
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
    }
}
