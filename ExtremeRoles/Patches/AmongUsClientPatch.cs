using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public class AmongUsClientCoStartGamePatch
    {
        public static void Prefix(AmongUsClient __instance)
        {
            ExtremeRolesPlugin.Info.HideInfoOverlay();
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix()
        {
            if (PlayerControl.LocalPlayer != null)
            {
                Helper.GameSystem.ShareVersion();
            }
        }
    }

    // ゲームが終了した瞬間の処理
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class AmongUsClientOnGameEndPatch
    {
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            ExtremeRolesPlugin.Info.HideInfoOverlay();
            ExtremeRolesPlugin.GameDataStore.EndReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10)
            {
                endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
            }
        }
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            List<PlayerControl> noWinner = new List<PlayerControl>();

            var roleData = ExtremeRoleManager.GameRole;
            var gameData = ExtremeRolesPlugin.GameDataStore;

            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {

                var role = roleData[playerInfo.PlayerId];

                gameData.AddPlayerSummary(playerInfo);

                if (role.IsNeutral())
                {
                    if (ExtremeRoleManager.IsAliveWinNeutral(role, playerInfo))
                    {
                        gameData.PlusWinner.Add(playerInfo.Object);
                    }
                    else
                    {
                        noWinner.Add(playerInfo.Object);
                    }
                }
            }

            List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
            foreach (WinningPlayerData winner in TempData.winners)
            {
                if (noWinner.Any(x => x.Data.PlayerName == winner.PlayerName) ||
                    gameData.PlusWinner.Any(
                        x => x.Data.PlayerName == winner.PlayerName))
                {
                    winnersToRemove.Add(winner);
                }
            }

            foreach (var winner in winnersToRemove)
            {
                TempData.winners.Remove(winner);
            }

            if (OptionHolder.Ship.DisableNeutralSpecialForceEnd)
            {
                addNeutralWinner();
            }

            switch ((RoleGameOverReason)gameData.EndReason)
            {
                case RoleGameOverReason.AssassinationMarin:
                    resetWinner();
                    foreach (var player in GameData.Instance.AllPlayers)
                    {
                        if (ExtremeRoleManager.GameRole[player.PlayerId].IsImpostor())
                        { 
                            addWinner(player);
                        }
                    }
                    break;
                case RoleGameOverReason.AliceKilledByImposter:
                case RoleGameOverReason.AliceKillAllOther:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Alice });
                    break;
                case RoleGameOverReason.JackalKillAllOther:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Jackal, ExtremeRoleId.Sidekick });
                    break;
                case RoleGameOverReason.LoverKillAllOther:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Lover });
                    break;
                case RoleGameOverReason.TaskMasterGoHome:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.TaskMaster });
                    break;
                case RoleGameOverReason.MissionaryAllAgainstGod:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Missionary });
                    break;
                case RoleGameOverReason.JesterMeetingFavorite:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Jester });
                    break;
                case RoleGameOverReason.ShipFallInLove:
                    replaceWinnerToSpecificRolePlayer(
                        ExtremeRoleId.Lover);
                    break;
                default:
                    break;
            }

            foreach(var player in gameData.PlusWinner)
            {
                addWinner(player);
            }
        }
        private static void replaceWinnerToSpecificRolePlayer(
            ExtremeRoleId roleId)
        {
            resetWinner();

            var gameData = ExtremeRolesPlugin.GameDataStore;

            foreach (var player in GameData.Instance.AllPlayers)
            {

                var role = ExtremeRoleManager.GameRole[player.PlayerId];

                var multiAssignRole = role as Roles.API.MultiAssignRoleBase;

                if (role.Id == roleId)
                {
                    if ((gameData.WinGameControlId != int.MaxValue) &&
                        (gameData.WinGameControlId == role.GameControlId))
                    {
                        addWinner(player);
                    }
                }
                else if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        if (role.Id == roleId)
                        {
                            if ((gameData.WinGameControlId != int.MaxValue) &&
                                (gameData.WinGameControlId == multiAssignRole.AnotherRole.GameControlId))
                            {
                                addWinner(player);
                            }
                        }
                    }
                }
            }
        }

        private static void replaceWinnerToSpecificNeutralRolePlayer(
            List<PlayerControl> noWinner, ExtremeRoleId[] roles)
        {
            resetWinner();

            var gameData = ExtremeRolesPlugin.GameDataStore;

            foreach (var player in noWinner)
            {

                var role = ExtremeRoleManager.GameRole[player.PlayerId];

                var multiAssignRole = role as Roles.API.MultiAssignRoleBase;

                if (roles.Contains(role.Id))
                {
                    if (OptionHolder.Ship.IsSameNeutralSameWin)
                    {
                        addWinner(player);
                    }
                    else if (
                        (gameData.WinGameControlId != int.MaxValue) &&
                        (gameData.WinGameControlId == role.GameControlId))
                    {
                        addWinner(player);
                    }
                }
                else if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        if (roles.Contains(multiAssignRole.AnotherRole.Id))
                        {
                            if (OptionHolder.Ship.IsSameNeutralSameWin)
                            {
                                addWinner(player);
                            }
                            else if (
                                (gameData.WinGameControlId != int.MaxValue) &&
                                (gameData.WinGameControlId == multiAssignRole.AnotherRole.GameControlId))
                            {
                                addWinner(player);
                            }
                        }
                    }
                }
            }
        }

        private static void addNeutralWinner()
        {
            List<(ExtremeRoleId, int)> winRole = new List<(ExtremeRoleId, int)>();

            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                var role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
                var multiAssignRole = role as Roles.API.MultiAssignRoleBase;

                if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        if (checkAndAddWinRole(
                            multiAssignRole.AnotherRole,
                            playerInfo, ref winRole))
                        {
                            continue;
                        }
                    }
                }
                checkAndAddWinRole(role, playerInfo, ref winRole);
            }
        }

        private static bool checkAndAddWinRole(
            Roles.API.SingleRoleBase role,
            GameData.PlayerInfo playerInfo,
            ref List<(ExtremeRoleId, int)> winRole)
        {
            int gameControlId = role.GameControlId;

            if (OptionHolder.Ship.IsSameNeutralSameWin)
            {
                gameControlId = int.MaxValue;
            }


            if (winRole.Contains((role.Id, gameControlId)))
            {
                addWinner(playerInfo);
                return true;
            }
            else if (role.IsNeutral() && role.IsWin)
            {
                winRole.Add((role.Id, gameControlId));
                addWinner(playerInfo);
                return true;
            }
            return false;
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

        private static void resetWinner()
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
        }

    }
}
