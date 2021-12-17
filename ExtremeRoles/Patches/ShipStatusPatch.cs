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
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) return true; // InstanceExists | Don't check Custom Criteria when in Tutorial
            if (HudManager.Instance.isIntroDisplayed) return false;


            var statistics = new PlayerDataContainer.PlayerStatistics();
            if (IsSabotageWin(__instance)) { return false; }
            if (IsTaskWin(__instance)) { return false; };
            if (IsImpostorWin(__instance, statistics)) { return false; };
            if (IsForCrewmateWin(__instance, statistics)) { return false; };
            return false;
        }

        private static void EndGameForSabotage(
            ShipStatus __instance)
        {
            __instance.enabled = false;
            ShipStatus.RpcEndGame(
                GameOverReason.ImpostorBySabotage, false);
            return;
        }

        private static bool IsForCrewmateWin(
            ShipStatus __instance,
            PlayerDataContainer.PlayerStatistics statistics)
        {
            if (statistics.TeamCrewmateAlive > 0 && statistics.TeamImpostorAlive == 0)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(
                    GameOverReason.HumansByVote, false);
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
                __instance.enabled = false;
                ShipStatus.RpcEndGame(
                    endReason, false);
                return true;
            }

            return false;

        }
        private static bool IsSabotageWin(
            ShipStatus __instance)
        {
            if (__instance.Systems == null) return false;
            ISystemType systemType = __instance.Systems.ContainsKey(
                SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
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
                    EndGameForSabotage(__instance);
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
                __instance.enabled = false;
                ShipStatus.RpcEndGame(
                    GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }
    }
}
