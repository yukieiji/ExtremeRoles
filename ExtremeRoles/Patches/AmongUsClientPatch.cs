using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch]
    public static class SyncSettingPatch
    {
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
        public static void Postfix()
        {
            GameManager.Instance?.LogicOptions.SyncOptions();
        }
    }

    // from Reactor : https://github.com/NuclearPowered/Reactor/commit/0a03a9d90d41b3bb158fa95bb23186f6769e0f9f
    [HarmonyPatch(typeof(AmongUsClient._CoJoinOnlinePublicGame_d__1),
        nameof(AmongUsClient._CoJoinOnlinePublicGame_d__1.MoveNext))]
    public static class EnableUdpMatchmakingPatch
    {
        public static void Prefix(
            AmongUsClient._CoJoinOnlinePublicGame_d__1 __instance)
        {
            // Skip to state 1 which just calls CoJoinOnlineGameDirect
            if (__instance.__1__state == 0 && !ServerManager.Instance.IsHttp)
            {
                __instance.__1__state = 1;
                __instance.__8__1 = new AmongUsClient.__c__DisplayClass1_0
                {
                    matchmakerToken = string.Empty,
                };
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class AmongUsClientCoStartGamePatch
    {
        public static void Prefix(AmongUsClient __instance)
        {
            ExtremeRolesPlugin.Info.HideInfoOverlay();
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public static class AmongUsClientOnPlayerJoinedPatch
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
    public static class AmongUsClientOnGameEndPatch
    {
        public static void Prefix([HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            ExtremeRolesPlugin.Info.HideInfoOverlay();
            ExtremeRolesPlugin.ShipState.SetGameOverReason(endGameResult.GameOverReason);
            if ((int)endGameResult.GameOverReason >= 20)
            {
                endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
            }
        }
        public static void Postfix()
        {
            List<GameData.PlayerInfo> noWinner = new List<GameData.PlayerInfo>();
            List<(GameData.PlayerInfo, IRoleWinPlayerModifier)> modRole = new List<
                (GameData.PlayerInfo, IRoleWinPlayerModifier)> ();

            List<(GameData.PlayerInfo, IGhostRoleWinable)> ghostWinCheckRole = new List<
               (GameData.PlayerInfo, IGhostRoleWinable)>();

            var roleData = ExtremeRoleManager.GameRole;
            var gameData = ExtremeRolesPlugin.ShipState;

            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {

                var role = roleData[playerInfo.PlayerId];
                bool hasGhostRole = ExtremeGhostRoleManager.GameRole.TryGetValue(
                    playerInfo.PlayerId, out GhostRoleBase ghostRole);

                Module.CustomMonoBehaviour.FinalSummary.Add(
                    playerInfo, role, ghostRole);

                if (role.IsNeutral())
                {
                    if (ExtremeRoleManager.IsAliveWinNeutral(role, playerInfo))
                    {
                        gameData.AddWinner(playerInfo);
                    }
                    else
                    {
                        noWinner.Add(playerInfo);
                    }
                }
                else if (role.Id == ExtremeRoleId.Xion)
                {
                    noWinner.Add(playerInfo);
                }

                var multiAssignRole = role as MultiAssignRoleBase;

                var winModRole = role as IRoleWinPlayerModifier;
                if (winModRole != null)
                {
                    modRole.Add((playerInfo, winModRole));
                }
                if (multiAssignRole != null)
                {
                    if (multiAssignRole.AnotherRole != null)
                    {
                        winModRole = multiAssignRole.AnotherRole as IRoleWinPlayerModifier;
                        if (winModRole != null)
                        {
                            modRole.Add((playerInfo, winModRole));
                        }
                    }
                }

                if (hasGhostRole && 
                    ghostRole.IsNeutral() &&
                    ghostRole is IGhostRoleWinable winCheckGhostRole)
                {
                    ghostWinCheckRole.Add((playerInfo, winCheckGhostRole));
                }
            }

            List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
            List<GameData.PlayerInfo> plusWinner = gameData.GetPlusWinner();
            foreach (WinningPlayerData winner in TempData.winners.GetFastEnumerator())
            {
                if (noWinner.Any(x => x.PlayerName == winner.PlayerName) ||
                    plusWinner.Any(x => x.PlayerName == winner.PlayerName))
                {
                    winnersToRemove.Add(winner);
                }
            }

            foreach (WinningPlayerData winner in winnersToRemove)
            {
                TempData.winners.Remove(winner);
            }

            if (ExtremeGameManager.Instance.ShipOption.DisableNeutralSpecialForceEnd)
            {
                addNeutralWinner();
            }

            GameOverReason reason = gameData.EndReason;

            switch ((RoleGameOverReason)reason)
            {
                case RoleGameOverReason.AssassinationMarin:
                    resetWinner();
                    foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers.GetFastEnumerator())
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
                case RoleGameOverReason.ShipFallInLove:
                    replaceWinnerToSpecificRolePlayer(
                        ExtremeRoleId.Lover);
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
                case RoleGameOverReason.YandereKillAllOther:
                case RoleGameOverReason.YandereShipJustForTwo:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Yandere });
                    break;
                case RoleGameOverReason.VigilanteKillAllOther:
                case RoleGameOverReason.VigilanteNewIdealWorld:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Vigilante });
                    break;
                case RoleGameOverReason.MinerExplodeEverything:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Miner });
                    break;
                case RoleGameOverReason.EaterAllEatInTheShip:
                case RoleGameOverReason.EaterAliveAlone:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Eater });
                    break;
                case RoleGameOverReason.TraitorKillAllOther:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Traitor });
                    break;
                case RoleGameOverReason.QueenKillAllOther:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Queen, ExtremeRoleId.Servant });
                    break;
                case RoleGameOverReason.UmbrerBiohazard:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Umbrer });
                    break;
                case RoleGameOverReason.KidsTooBigHomeAlone:
                case RoleGameOverReason.KidsAliveAlone:
                    replaceWinnerToSpecificNeutralRolePlayer(
                        noWinner,
                        new ExtremeRoleId[] { ExtremeRoleId.Delinquent });
                    break;
                default:
                    break;
            }

            foreach (var player in gameData.GetPlusWinner())
            {
                addWinner(player);
            }

            foreach (var (playerInfo, winCheckRole) in ghostWinCheckRole)
            {
                if (winCheckRole.IsWin(reason, playerInfo))
                {
                    addWinner(playerInfo);
                    plusWinner.Add(playerInfo);
                }
            }

            Il2CppSystem.Collections.Generic.List<WinningPlayerData> winnerList = TempData.winners;

            foreach (var (playerInfo, winModRole) in modRole)
            {
                winModRole.ModifiedWinPlayer(
                    playerInfo,
                    gameData.EndReason,
                    ref winnerList,
                    ref plusWinner);
            }

            gameData.SetPlusWinner(plusWinner);
            TempData.winners = winnerList;

        }
        private static void replaceWinnerToSpecificRolePlayer(
            ExtremeRoleId roleId)
        {
            resetWinner();

            int winGameControlId = ExtremeRolesPlugin.ShipState.WinGameControlId;
            foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
            {

                var role = ExtremeRoleManager.GameRole[player.PlayerId];

                var multiAssignRole = role as MultiAssignRoleBase;

                if (role.Id == roleId)
                {
                    if ((winGameControlId != int.MaxValue) &&
                        (winGameControlId == role.GameControlId))
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
                            if ((winGameControlId != int.MaxValue) &&
                                (winGameControlId == multiAssignRole.AnotherRole.GameControlId))
                            {
                                addWinner(player);
                            }
                        }
                    }
                }
            }
        }

        private static void replaceWinnerToSpecificNeutralRolePlayer(
            List<GameData.PlayerInfo> noWinner, ExtremeRoleId[] roles)
        {
            resetWinner();

            int winGameControlId = ExtremeRolesPlugin.ShipState.WinGameControlId;

            foreach (var player in noWinner)
            {

                var role = ExtremeRoleManager.GameRole[player.PlayerId];

                var multiAssignRole = role as MultiAssignRoleBase;

                if (roles.Contains(role.Id))
                {
                    if (ExtremeGameManager.Instance.ShipOption.IsSameNeutralSameWin)
                    {
                        addWinner(player);
                    }
                    else if (
                        (winGameControlId != int.MaxValue) &&
                        (winGameControlId == role.GameControlId))
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
                            if (ExtremeGameManager.Instance.ShipOption.IsSameNeutralSameWin)
                            {
                                addWinner(player);
                            }
                            else if (
                                (winGameControlId != int.MaxValue) &&
                                (winGameControlId == multiAssignRole.AnotherRole.GameControlId))
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

            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                var role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
                var multiAssignRole = role as MultiAssignRoleBase;

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
            SingleRoleBase role,
            GameData.PlayerInfo playerInfo,
            ref List<(ExtremeRoleId, int)> winRole)
        {
            int gameControlId = role.GameControlId;

            if (ExtremeGameManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                gameControlId = ExtremeGameManager.SameNeutralGameControlId;
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

        private static void resetWinner()
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
        }

    }
}
