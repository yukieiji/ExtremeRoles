using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using ExtremeRoles.Module;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{

    // ゲームが終了した瞬間の処理
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class AmongUsClientOnGameEndPatch
    {
        private static GameOverReason endReason;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            endReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10)
            {
                endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
            }
        }
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            List<PlayerControl> noWinner = new List<PlayerControl>();

            var roleData = ExtremeRoleManager.GameRole;

            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {

                var role = roleData[playerInfo.PlayerId];
                var (completedTask, totalTask) = Helper.Task.GetTaskInfo(playerInfo);

                var finalStatus = PlayerDataContainer.PlayerStatus.Alive;
                if (playerInfo.Disconnected) { finalStatus = PlayerDataContainer.PlayerStatus.Disconnected; }
                else if (playerInfo.IsDead) { finalStatus = PlayerDataContainer.PlayerStatus.Dead; }
                else if (
                    (endReason == GameOverReason.ImpostorBySabotage) &&
                    (!playerInfo.Role.IsImpostor)) { finalStatus = PlayerDataContainer.PlayerStatus.Dead; }

                PlayerDataContainer.EndGameAddStatus(
                    playerInfo, finalStatus, role, totalTask, completedTask);

                if (role.IsNeutral())
                {
                    noWinner.Add(playerInfo.Object);
                }
            }

            List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
            foreach (WinningPlayerData winner in TempData.winners)
            {
                if (noWinner.Any(x => x.Data.PlayerName == winner.PlayerName))
                {
                    winnersToRemove.Add(winner);
                }
            }

            foreach (var winner in winnersToRemove)
            {
                TempData.winners.Remove(winner);
            }

            switch (endReason)
            {
                case (GameOverReason)RoleGameOverReason.AliceKilledByImposter:
                case (GameOverReason)RoleGameOverReason.AliceKillAllOthers:
                    addSpecificRolePlayerToWinner(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Alice });
                    break;
                case (GameOverReason)RoleGameOverReason.JackalKillAllOthers:
                    addSpecificRolePlayerToWinner(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Jackal, ExtremeRoleId.Sidekick });
                    break;
                default:
                    break;
            }

            RPCOperator.GameInit();

        }

        private static void resetWinner()
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
        }
        
        private static void addSpecificRolePlayerToWinner(
            List<PlayerControl> noWinner, ExtremeRoleId[] roles)
        {
            resetWinner();

            foreach (var player in noWinner)
            {
                if (roles.Contains(ExtremeRoleManager.GameRole[player.PlayerId].Id))
                {
                    addWinner(player);
                }
            }
        }

        private static void addWinner(PlayerControl player)
        {
            WinningPlayerData wpd = new WinningPlayerData(player.Data);
            TempData.winners.Add(wpd);
        }

    }
}
