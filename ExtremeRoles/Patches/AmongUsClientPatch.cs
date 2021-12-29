using System;
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
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            GameDataContainer.EndReason = endGameResult.GameOverReason;
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

                var finalStatus = GameDataContainer.PlayerStatus.Alive;
                if (playerInfo.Disconnected)
                { 
                    finalStatus = GameDataContainer.PlayerStatus.Disconnected; 
                }
                else if (playerInfo.IsDead)
                { 
                    finalStatus = GameDataContainer.PlayerStatus.Dead; }
                else if (
                    (GameDataContainer.EndReason == GameOverReason.ImpostorBySabotage) &&
                    (!playerInfo.Role.IsImpostor))
                { 
                    finalStatus = GameDataContainer.PlayerStatus.Dead; 
                }

                GameDataContainer.EndGameAddStatus(
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

            if (OptionsHolder.Ship.DisableNeutralSpecialForceEnd)
            {
                setNeutralWinner();
            }

            switch (GameDataContainer.EndReason)
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

        private static void setNeutralWinner()
        {
            List<(ExtremeRoleId, int)> winRole = new List<(ExtremeRoleId, int)>();

            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                var role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];

                if (role.IsNeutral() && role.IsWin)
                {
                    int gameControlId = role.GameControlId;

                    if (OptionsHolder.Ship.IsSameNeutralSameWin)
                    {
                        gameControlId = int.MaxValue;
                    }

                    if (!winRole.Contains((role.Id, gameControlId)))
                    {
                        winRole.Add((role.Id, gameControlId));
                    }

                    addWinner(playerInfo);
                }
            }
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

                var role = ExtremeRoleManager.GameRole[player.PlayerId];

                if (roles.Contains(role.Id))
                {
                    if (OptionsHolder.Ship.IsSameNeutralSameWin)
                    {
                        addWinner(player);
                    }
                    else if (
                        (GameDataContainer.WinGameControlId != int.MaxValue) &&
                        (GameDataContainer.WinGameControlId == role.GameControlId))
                    {
                        addWinner(player);
                    }
                }
            }
        }

        private static void addWinner(GameData.PlayerInfo playerInfo)
        {
            WinningPlayerData wpd = new WinningPlayerData(playerInfo);
            TempData.winners.Add(wpd);
        }

        private static void addWinner(PlayerControl player)
        {
            addWinner(player.Data);
        }

    }
}
