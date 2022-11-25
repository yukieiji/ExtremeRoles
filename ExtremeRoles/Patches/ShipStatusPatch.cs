using System.Linq;
using System.Runtime.CompilerServices;

using HarmonyLib;
using UnityEngine;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.ExtremeShipStatus;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusAwakePatch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(ShipStatus __instance)
        {
            CachedShipStatus.SetUp(__instance);
            ExtremeRolesPlugin.Compat.SetUpMap(__instance);   
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public static class ShipStatusCalculateLightRadiusPatch
    {
        public static bool Prefix(
            ref float __result,
            ShipStatus __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo playerInfo)
        {
            switch (ExtremeRolesPlugin.ShipState.CurVison)
            {
                case ExtremeShipStatus.ForceVisonType.LastWolfLightOff:
                    if (ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
                        Roles.Solo.Impostor.LastWolf>() == null)
                    {
                        __result = 0.3f;
                        return false;
                    }
                    break;
                case ExtremeShipStatus.ForceVisonType.WispLightOff:
                    if (!Wisp.HasTorch(playerInfo.PlayerId))
                    {
                        __result = __instance.MinLightRadius * 
                            PlayerControl.GameOptions.CrewLightMod;
                        return false;
                    }
                    break;
                default:
                    break;
            }

            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd)
            {
                return checkNormalOrCustomCalculateLightRadius(playerInfo, ref __result);
            }

            ISystemType systemType = __instance.Systems.ContainsKey(
                SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null)
            {
                return checkNormalOrCustomCalculateLightRadius(playerInfo, ref __result);
            }

            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null)
            {
                return checkNormalOrCustomCalculateLightRadius(playerInfo, ref __result);
            }

            var allRole = ExtremeRoleManager.GameRole;
            if (allRole.Count == 0)
            {
                if (requireCustomCustomCalculateLightRadius())
                {
                    __result = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                        playerInfo, false, playerInfo.Role.IsImpostor);
                    return false;
                }
                return true;
            }

            SingleRoleBase role = allRole[playerInfo.PlayerId];

            if (requireCustomCustomCalculateLightRadius())
            {
                float visonMulti;
                bool applayVisonEffects = !role.IsImpostor();

                if (role.TryGetVisonMod(out float vison, out bool isApplyEnvironmentVision))
                {
                    visonMulti = vison;
                    applayVisonEffects = isApplyEnvironmentVision;
                }
                else if (playerInfo.Role.IsImpostor)
                {
                    visonMulti = PlayerControl.GameOptions.ImpostorLightMod;
                }
                else
                {
                    visonMulti = PlayerControl.GameOptions.CrewLightMod;
                }

                __result = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                    playerInfo, visonMulti, applayVisonEffects);

                return false;
            }

            float num = (float)switchSystem.Value / 255f;
            float switchVisonMulti = Mathf.Lerp(
                __instance.MinLightRadius,
                __instance.MaxLightRadius, num);

            float baseVison = __instance.MaxLightRadius;

            if (playerInfo == null || playerInfo.IsDead) // IsDead
            {
                __result = baseVison;
            }
            else if (role.TryGetVisonMod(out float vison, out bool isApplyEnvironmentVision))
            {
                if (isApplyEnvironmentVision)
                {
                    baseVison = switchVisonMulti;
                }
                __result = baseVison * vison;
            }
            else if (playerInfo.Role.IsImpostor)
            {
                __result = baseVison * PlayerControl.GameOptions.ImpostorLightMod;
            }
            else
            {
                __result = switchVisonMulti * PlayerControl.GameOptions.CrewLightMod;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool requireCustomCustomCalculateLightRadius() =>
            ExtremeRolesPlugin.Compat.IsModMap &&
            ExtremeRolesPlugin.Compat.ModMap.IsCustomCalculateLightRadius;

        private static bool checkNormalOrCustomCalculateLightRadius(
            GameData.PlayerInfo player, ref float result)
        {
            if (requireCustomCustomCalculateLightRadius())
            {
                result = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                    player, false, player.Role.IsImpostor);
                return false;
            }
            return true;
        }

    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
    public static class ShipStatusIsGameOverDueToDeathPatch
    {
        public static void Postfix(ShipStatus __instance, ref bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    public static class ShipStatusCheckEndCriteriaPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) { return false; };
            if (DestroyableSingleton<TutorialManager>.InstanceExists) { return true; } // InstanceExists | Don't check Custom Criteria when in Tutorial
            if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed){ return false; }

            if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger ||
                ExtremeRolesPlugin.ShipState.IsDisableWinCheck) { return false; }

            var statistics = ExtremeRolesPlugin.ShipState.CreateStatistics();


            if (isImpostorSpecialWin(__instance)) { return false; }
            if (isSabotageWin(__instance)) { return false; }
            
            if (isTaskWin(__instance)) { return false; };

            if (isSpecialRoleWin(__instance, statistics)) { return false; }

            if (isNeutralSpecialWin(__instance)) { return false; };
            if (isNeutralAliveWin(__instance, statistics)) { return false; };

            if (statistics.SeparatedNeutralAlive.Count != 0) { return false; }

            if (isImpostorWin(__instance, statistics)) { return false; };
            if (isCrewmateWin(__instance, statistics)) { return false; };
            
            return false;
        }

        private static void gameIsEnd(
            ref ShipStatus　instance,
            GameOverReason reason,
            bool trigger = false)
        {
            instance.enabled = false;
            ShipStatus.RpcEndGame(
                reason, trigger);
        }

        private static bool isCrewmateWin(
            ShipStatus __instance,
            ExtremeShipStatus.PlayerStatistics statistics)
        {
            if (statistics.TeamCrewmateAlive > 0 && 
                statistics.TeamImpostorAlive == 0 && 
                statistics.SeparatedNeutralAlive.Count == 0)
            {
                gameIsEnd(ref __instance, GameOverReason.HumansByVote);
                return true;
            }
            return false;
        }

        private static bool isImpostorWin(
            ShipStatus __instance,
            ExtremeShipStatus.PlayerStatistics statistics)
        {
            bool isGameEnd = false;
            GameOverReason endReason = GameOverReason.HumansDisconnect;

            if (statistics.TeamImpostorAlive >= (statistics.TotalAlive - statistics.TeamImpostorAlive) &&
                statistics.SeparatedNeutralAlive.Count == 0)
            {
                isGameEnd = true;
                switch (TempData.LastDeathReason)
                {
                    case DeathReason.Exile:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                    case DeathReason.Kill:
                        endReason = GameOverReason.ImpostorByKill;
                        break;
                    default:
                        break;
                }
            }

            if (isGameEnd)
            {

                gameIsEnd(ref __instance, endReason);
                return true;
            }

            return false;

        }
        private static bool isImpostorSpecialWin(
            ShipStatus __instance)
        {
            if (ExtremeRolesPlugin.ShipState.IsAssassinateMarin)
            {
                gameIsEnd(
                    ref __instance,
                    (GameOverReason)RoleGameOverReason.AssassinationMarin);
                return true;
            }

            return false;
        
        }

        private static bool isNeutralAliveWin(
            ShipStatus __instance,
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
                    default:
                        break;
                }
                if (endReason != (GameOverReason)RoleGameOverReason.UnKnown)
                {
                    gameIsEnd(ref __instance, endReason);
                    return true;
                }
            }
            return false;
        }

        private static bool isNeutralSpecialWin(ShipStatus __instance)
        {

            if (OptionHolder.Ship.DisableNeutralSpecialForceEnd) { return false; }

            foreach(var role in ExtremeRoleManager.GameRole.Values)
            {
                
                if (!role.IsNeutral()) { continue; }
                if (role.IsWin)
                {
                    setWinGameContorlId(role.GameControlId);

                    GameOverReason endReason = (GameOverReason)RoleGameOverReason.UnKnown;

                    switch (role.Id)
                    {
                        case ExtremeRoleId.Alice:
                            endReason = (GameOverReason)RoleGameOverReason.AliceKilledByImposter;
                            break;
                        case ExtremeRoleId.TaskMaster:
                            endReason = (GameOverReason)RoleGameOverReason.TaskMasterGoHome;
                            break;
                        case ExtremeRoleId.Jester:
                            endReason = (GameOverReason)RoleGameOverReason.JesterMeetingFavorite;
                            break;
                        case ExtremeRoleId.Eater:
                            endReason = (GameOverReason)RoleGameOverReason.EaterAllEatInTheShip;
                            break;
                        case ExtremeRoleId.Umbrer:
                            endReason = (GameOverReason)RoleGameOverReason.UmbrerBiohazard;
                            break;
                        default :
                            break;
                    }
                    gameIsEnd(ref __instance, endReason);
                    return true;

                }
            }

            return false;
        }

        private static bool isSpecialRoleWin(
            ShipStatus __instance,
            ExtremeShipStatus.PlayerStatistics statistics)
        {
            if (statistics.SpecialWinCheckRoleAlive.Count == 0) { return false; }
            foreach (var(id, checker) in statistics.SpecialWinCheckRoleAlive)
            {
                if (checker.IsWin(statistics))
                {
                    setWinGameContorlId(id);
                    gameIsEnd(
                        ref __instance,
                        (GameOverReason)checker.Reason);
                    return true;
                }
            }
            return false;
        }

        private static bool isSabotageWin(
            ShipStatus __instance)
        {
            if (__instance.Systems == null) { return false; };
            ISystemType systemType = __instance.Systems.ContainsKey(
                SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    gameIsEnd(
                        ref __instance,
                        GameOverReason.ImpostorBySabotage);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(
                SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null)
            {
                systemType2 = __instance.Systems.ContainsKey(
                    SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null)
            {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f)
                {
                    gameIsEnd(
                        ref __instance,
                        GameOverReason.ImpostorBySabotage);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }
        private static bool isTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks > 0 && 
                GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                gameIsEnd(
                    ref __instance,
                    GameOverReason.HumansByTask);
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

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
    public static class ShipStatusOnDestroyPatch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            CachedShipStatus.Destroy();
            ExtremeRolesPlugin.Compat.RemoveMap();
        }
    }
}
