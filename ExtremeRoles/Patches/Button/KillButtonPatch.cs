using System;

using HarmonyLib;

using Hazel;

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
            var role = Roles.ExtremeRoleManager.GameRole[localPlayer.PlayerId];
            if (
                __instance.isActiveAndEnabled &&
                __instance.currentTarget &&
                !__instance.isCoolingDown &&
                !localPlayer.Data.IsDead &&
                localPlayer.CanMove && role.CanKill)
            {

                PlayerControl killer = PlayerControl.LocalPlayer;
                PlayerControl target = __instance.currentTarget;

                bool canKilledTargetRole = Roles.ExtremeRoleManager.GameRole[target.PlayerId].TryRolePlayerKill(
                    target, killer);
                if (!canKilledTargetRole) { return false; }

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderKillResult res = checkMuderKill(
                    killer, target);

                switch (res)
                {
                    case MurderKillResult.NormalKill:
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                            PlayerControl.LocalPlayer.NetId,
                            (byte)CustomRPC.UncheckedMurderPlayer,
                            Hazel.SendOption.Reliable, -1);
                        writer.Write(killer.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(Byte.MaxValue);

                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        ExtremeRoleRPC.UncheckedMurderPlayer(
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
