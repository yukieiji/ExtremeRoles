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
        private static GameOverReason EndReason;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            EndReason = endGameResult.GameOverReason;
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
                    (EndReason == GameOverReason.ImpostorBySabotage) &&
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

            switch (EndReason)
            {
                case (GameOverReason)RoleGameOverReason.AliceKilledByImposter:
                case (GameOverReason)RoleGameOverReason.AliceKillAllOthers:
                    ResetWinner();
                    foreach(var player in noWinner)
                    {
                        var playerRole = roleData[player.PlayerId];
                        if (playerRole.Id == ExtremeRoleId.Alice)
                        {
                            AddWinner(player);
                        }
                    }
                    break;
                default:
                    break;
            }

            ExtremeRoleRPC.GameInit();

        }

        private static void ResetWinner()
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
        }
        private static void AddWinner(PlayerControl player)
        {
            WinningPlayerData wpd = new WinningPlayerData(player.Data);
            TempData.winners.Add(wpd);
        }

    }
}
