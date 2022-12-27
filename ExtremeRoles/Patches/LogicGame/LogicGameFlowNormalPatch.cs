using System.Linq;

using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.ExtremeShipStatus;

namespace ExtremeRoles.Patches.LogicGame
{
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    public static class LogicGameFlowNormalIsGameOverDueToDeathPatch
    {
        public static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    public static class LogicGameFlowNormalCheckEndCriteriaPatch
    {
        public static bool Prefix()
        {
            if (!GameData.Instance) { return false; };
            if (DestroyableSingleton<TutorialManager>.InstanceExists) { return true; } // InstanceExists | Don't check Custom Criteria when in Tutorial
            if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed) { return false; }

            if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger ||
                ExtremeRolesPlugin.ShipState.IsDisableWinCheck) { return false; }

            var statistics = ExtremeRolesPlugin.ShipState.CreateStatistics();


            if (isImpostorSpecialWin()) { return false; }
            if (isSabotageWin()) { return false; }

            if (isTaskWin()) { return false; };

            if (isSpecialRoleWin(statistics)) { return false; }

            if (isNeutralSpecialWin()) { return false; };
            if (isNeutralAliveWin(statistics)) { return false; };

            if (statistics.SeparatedNeutralAlive.Count != 0) { return false; }

            if (isImpostorWin(statistics)) { return false; };
            if (isCrewmateWin(statistics)) { return false; };

            return false;
        }

        private static void gameIsEnd(
            GameOverReason reason,
            bool trigger = false)
        {
            CachedShipStatus.Instance.enabled = false;
            GameManager.Instance.RpcEndGame(reason, trigger);
        }

        private static bool isCrewmateWin(
            ExtremeShipStatus.PlayerStatistics statistics)
        {
            if (statistics.TeamCrewmateAlive > 0 &&
                statistics.TeamImpostorAlive == 0 &&
                statistics.SeparatedNeutralAlive.Count == 0)
            {
                gameIsEnd(GameOverReason.HumansByVote);
                return true;
            }
            return false;
        }

        private static bool isImpostorWin(
            ExtremeShipStatus.PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorAlive >= (statistics.TotalAlive - statistics.TeamImpostorAlive) &&
                statistics.SeparatedNeutralAlive.Count == 0)
            {
                GameOverReason endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill  => GameOverReason.ImpostorByKill,
                    _                 => GameOverReason.HumansDisconnect,
                };
                gameIsEnd(endReason);
                return true;
            }

            return false;

        }
        private static bool isImpostorSpecialWin()
        {
            if (ExtremeRolesPlugin.ShipState.IsAssassinateMarin)
            {
                gameIsEnd((GameOverReason)RoleGameOverReason.AssassinationMarin);
                return true;
            }

            return false;

        }

        private static bool isNeutralAliveWin(
            ExtremeShipStatus.PlayerStatistics statistics)
        {
            if (statistics.SeparatedNeutralAlive.Count != 1) { return false; }

            var ((team, id), num) = statistics.SeparatedNeutralAlive.ElementAt(0);

            if (num >= (statistics.TotalAlive - num))
            {

                GameOverReason endReason = (GameOverReason)RoleGameOverReason.UnKnown;
                switch (team)
                {
                    // アリス vs インポスターは絶対にインポスターが勝てないので
                    // 別の殺人鬼が存在しないかつ、生存者数がアリスの生存者以下になれば勝利
                    case NeutralSeparateTeam.Alice:
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.AliceKillAllOther;
                        break;
                    // 以下は全てインポスターと勝負しても問題ないのでインポスターが生きていると勝利できない
                    // アサシンがキルできないオプションのとき、ニュートラルの勝ち目が少なくなるので、勝利とする
                    case NeutralSeparateTeam.Jackal:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.JackalKillAllOther;
                        break;
                    case NeutralSeparateTeam.Lover:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.LoverKillAllOther;
                        break;
                    case NeutralSeparateTeam.Missionary:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.MissionaryAllAgainstGod;
                        break;
                    case NeutralSeparateTeam.Yandere:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.YandereKillAllOther;
                        break;
                    case NeutralSeparateTeam.Vigilante:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.VigilanteKillAllOther;
                        break;
                    case NeutralSeparateTeam.Miner:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.MinerExplodeEverything;
                        break;
                    case NeutralSeparateTeam.Eater:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.EaterAliveAlone;
                        break;
                    case NeutralSeparateTeam.Traitor:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.TraitorKillAllOther;
                        break;
                    case NeutralSeparateTeam.Queen:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.QueenKillAllOther;
                        break;
                    case NeutralSeparateTeam.Kids:
                        if (statistics.TeamImpostorAlive > 0 &&
                            statistics.TeamImpostorAlive != statistics.AssassinAlive) { return false; }
                        setWinGameContorlId(id);
                        endReason = (GameOverReason)RoleGameOverReason.KidsAliveAlone;
                        break;
                    default:
                        break;
                }
                if (endReason != (GameOverReason)RoleGameOverReason.UnKnown)
                {
                    gameIsEnd(endReason);
                    return true;
                }
            }
            return false;
        }

        private static bool isNeutralSpecialWin()
        {

            if (OptionHolder.Ship.DisableNeutralSpecialForceEnd) { return false; }

            foreach (var role in ExtremeRoleManager.GameRole.Values)
            {

                if (!role.IsNeutral()) { continue; }
                if (role.IsWin)
                {
                    setWinGameContorlId(role.GameControlId);

                    GameOverReason endReason = (GameOverReason)(role.Id switch
                    {
                        ExtremeRoleId.Alice      => RoleGameOverReason.AliceKilledByImposter,
                        ExtremeRoleId.TaskMaster => RoleGameOverReason.TaskMasterGoHome,
                        ExtremeRoleId.Jester     => RoleGameOverReason.JesterMeetingFavorite,
                        ExtremeRoleId.Eater      => RoleGameOverReason.EaterAllEatInTheShip,
                        ExtremeRoleId.Umbrer     => RoleGameOverReason.UmbrerBiohazard,
                        _ => RoleGameOverReason.UnKnown,
                    });
                    gameIsEnd(endReason);
                    return true;

                }
            }

            return false;
        }

        private static bool isSpecialRoleWin(
            ExtremeShipStatus.PlayerStatistics statistics)
        {
            if (statistics.SpecialWinCheckRoleAlive.Count == 0) { return false; }
            foreach (var (id, checker) in statistics.SpecialWinCheckRoleAlive)
            {
                if (checker.IsWin(statistics))
                {
                    setWinGameContorlId(id);
                    gameIsEnd((GameOverReason)checker.Reason);
                    return true;
                }
            }
            return false;
        }

        private static bool isSabotageWin()
        {

            var systems = CachedShipStatus.Systems;

            if (systems == null) { return false; };

            var gameOverReason = GameOverReason.ImpostorBySabotage;

            ISystemType systemType;
            if (systems.TryGetValue(SystemTypes.LifeSupp, out systemType))
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    gameIsEnd(gameOverReason);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }

            if (systems.TryGetValue(SystemTypes.Reactor, out systemType) || 
                systems.TryGetValue(SystemTypes.Laboratory, out systemType))
            {
                ICriticalSabotage criticalSystem = systemType.TryCast<ICriticalSabotage>();

                if (criticalSystem != null && 
                    criticalSystem.Countdown < 0f)
                {
                    gameIsEnd(gameOverReason);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }
        private static bool isTaskWin()
        {
            if (GameData.Instance.TotalTasks > 0 &&
                GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                gameIsEnd(GameOverReason.HumansByTask);
                return true;
            }
            return false;
        }

        private static void setWinGameContorlId(int id)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.SetWinGameControlId))
            {
                caller.WriteInt(id);
            }
            RPCOperator.SetWinGameControlId(id);
        }
    }
}
