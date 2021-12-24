using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    class ShipStatusCalculateLightRadiusPatch
    {
        public static bool Prefix(
            ref float __result,
            ShipStatus __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo playerInfo)
        {
            ISystemType systemType = __instance.Systems.ContainsKey(
                SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) { return true; }

            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) { return true; }

            var allRole = ExtremeRoleManager.GameRole;
            if (allRole.Count == 0) { return true; }

            float num = (float)switchSystem.Value / 255f;
            float switchVisonMulti = Mathf.Lerp(
                    __instance.MinLightRadius,
                    __instance.MaxLightRadius, num);

            float baseVison = __instance.MaxLightRadius;

            if (playerInfo == null || playerInfo.IsDead) // IsDead
            {
                __result = baseVison;
            }
            else if (allRole[playerInfo.PlayerId].HasOtherVison)
            {   
                if (allRole[playerInfo.PlayerId].IsApplyEnvironmentVision)
                {
                    baseVison = switchVisonMulti;
                }
                __result = baseVison * allRole[playerInfo.PlayerId].Vison;
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
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    class ShipStatusCheckEndCriteriaPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) { return false; };
            if (DestroyableSingleton<TutorialManager>.InstanceExists) { return true; } // InstanceExists | Don't check Custom Criteria when in Tutorial
            if (HudManager.Instance.isIntroDisplayed){ return false; }

            if (AssassinMeeting.AssassinMeetingTrigger) { return false; }

            var statistics = new PlayerDataContainer.PlayerStatistics();

            // Helper.Logging.Debug($"Neutral Alive:{statistics.NeutralKillerPlayer.Count}");

            if (IsNeutralSpecialWin(__instance)) { return false; }
            if (IsNeutralAliveWin(__instance, statistics)) { return false; }
            if (IsSabotageWin(__instance)) { return false; }
            if (IsTaskWin(__instance)) { return false; }

            if (statistics.NeutralKillerPlayer.Count > 0) { return false; }
            
            if (IsImpostorWin(__instance, statistics)) { return false; }
            if (IsCrewmateWin(__instance, statistics)) { return false; }
            
            return false;
        }

        private static void GameIsEnd(
            ref ShipStatus curShip,
            GameOverReason reason,
            bool trigger = false)
        {
            curShip.enabled = false;
            ShipStatus.RpcEndGame(reason, trigger);
        }

        private static bool IsCrewmateWin(
            ShipStatus __instance,
            PlayerDataContainer.PlayerStatistics statistics)
        {
            if (statistics.TeamCrewmateAlive > 0 && 
                statistics.TeamImpostorAlive == 0 && 
                statistics.NeutralKillerPlayer.Count == 0)
            {
                GameIsEnd(ref __instance, GameOverReason.HumansByVote);
                return true;
            }
            return false;
        }

        private static bool IsImpostorWin(
            ShipStatus __instance,
            PlayerDataContainer.PlayerStatistics statistics)
        {
            bool isGameEnd = false;
            GameOverReason endReason = GameOverReason.HumansDisconnect;

            if (statistics.IsAssassinationMarin)
            {
                isGameEnd = true;
                endReason = (GameOverReason)RoleGameOverReason.AssassinationMarin;
            }

            if (statistics.TeamImpostorAlive >= (statistics.TotalAlive - statistics.TeamImpostorAlive))
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

                GameIsEnd(ref __instance, endReason);
                return true;
            }

            return false;

        }

        private static bool IsNeutralAliveWin(
            ShipStatus __instance,
            PlayerDataContainer.PlayerStatistics statistics)
        {

            if (statistics.TeamImpostorAlive != 0) { return false; }

            var allRole = ExtremeRoleManager.GameRole;
            int killerAlive = statistics.NeutralKillerPlayer.Count;

            ExtremeRoleId roleId = allRole[statistics.NeutralAlivePlayer[0]].Id;
            for (int i = 1; i < killerAlive; ++i)
            {
                var checkRoleId = allRole[statistics.NeutralAlivePlayer[i]].Id;
                if (roleId != checkRoleId)
                {
                    return false;
                }
                roleId = checkRoleId;
            }

            if (statistics.TeamCrewmateAlive <= killerAlive)
            {

                GameOverReason endReason = (GameOverReason)RoleGameOverReason.UnKnown;

                switch (roleId)
                {
                    case ExtremeRoleId.Alice:
                        endReason = (GameOverReason)RoleGameOverReason.AliceKillAllOthers;
                        break;
                    default:
                        break;
                }

                GameIsEnd(ref __instance, endReason);
                return true;

            }

            return false;
        }

        private static bool IsNeutralSpecialWin(
            ShipStatus __instance)
        {
            
            foreach(var role in ExtremeRoleManager.GameRole.Values)
            {
                
                if (!role.IsNeutral()) { continue; }
                if (role.IsWin)
                {

                    GameOverReason endReason = (GameOverReason)RoleGameOverReason.UnKnown;

                    switch (role.Id)
                    {
                        case ExtremeRoleId.Alice:
                            endReason = (GameOverReason)RoleGameOverReason.AliceKilledByImposter;
                            break;
                        default :
                            break;
                    }
                    GameIsEnd(ref __instance, endReason);
                    return true;

                }
            }

            return false;
        }


        private static bool IsSabotageWin(
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
                    GameIsEnd(ref __instance, GameOverReason.ImpostorBySabotage);
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
                    GameIsEnd(ref __instance, GameOverReason.ImpostorBySabotage);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }
        private static bool IsTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks > 0 && 
                GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                GameIsEnd(ref __instance, GameOverReason.ImpostorBySabotage);
                return true;
            }
            return false;
        }
    }
}
