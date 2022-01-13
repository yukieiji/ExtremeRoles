using System;
using System.Collections.Generic;

using HarmonyLib;


namespace ExtremeRoles.Patches.Button
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    class KillButtonDoClickPatch
    {
        public enum MurderKillResult
        {
            NormalKill,
            NoAnimatedKill
        }
        public static bool Prefix(KillButton __instance)
        {
            var localPlayer = PlayerControl.LocalPlayer;
            var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();

            if (__instance.isActiveAndEnabled &&
                __instance.currentTarget &&
                !__instance.isCoolingDown &&
                !localPlayer.Data.IsDead &&
                localPlayer.CanMove && 
                role.CanKill)
            {

                PlayerControl killer = PlayerControl.LocalPlayer;
                PlayerControl target = __instance.currentTarget;

                var targetPlayerRole = Roles.ExtremeRoleManager.GameRole[target.PlayerId];

                bool canKill = targetPlayerRole.TryRolePlayerKilledFrom(
                    target, killer);
                if (!canKill) { return false; }
                
                canKill = role.TryRolePlayerKillTo(
                    killer, target);
                if (!canKill) { return false; }

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderKillResult res = checkMuderKill(
                    killer, target);

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
                            Byte.MaxValue);
                        break;
                    default:
                        break;
                }

                __instance.SetTarget(null);
            }
            return false;
        }

        private static MurderKillResult checkMuderKill(
            PlayerControl killer,
            PlayerControl target,
            bool blockRewind = false)
        {
            if (AmongUsClient.Instance.IsGameOver) { return MurderKillResult.NoAnimatedKill; }
            if (killer == null ||
                killer.Data == null ||
                killer.Data.IsDead ||
                killer.Data.Disconnected)
            {
                return MurderKillResult.NoAnimatedKill; // Allow non Impostor kills compared to vanilla code
            }
            if (target == null || 
                target.Data == null || 
                target.Data.IsDead || 
                target.Data.Disconnected)
            {
                return MurderKillResult.NoAnimatedKill; // Allow killing players in vents compared to vanilla code
            }

            return MurderKillResult.NormalKill;

        }
    }
}
