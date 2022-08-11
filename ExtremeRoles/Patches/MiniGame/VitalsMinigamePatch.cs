using System.Collections.Generic;
using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    public static class VitalsMinigameUpdatePatch
    {
        private static float vitalTimer = 0.0f;
        private static bool enableVitalLimit = false;
        private static bool isRemoveVital = false;
        private static TMPro.TextMeshPro timerText;

        private static readonly HashSet<ExtremeRoleId> vitalUseRole = new HashSet<ExtremeRoleId>()
        {
            ExtremeRoleId.Traitor
        };

        public static bool Prefix(VitalsMinigame __instance)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (ExtremeRoleManager.GetLocalPlayerRole().CanUseVital()) { return true; }

            __instance.SabText.text = Translation.GetString("youDonotUse");

            __instance.SabText.gameObject.SetActive(true);
            for (int j = 0; j < __instance.vitals.Length; j++)
            {
                __instance.vitals[j].gameObject.SetActive(false);
            }

            return false;
        }

        public static void Postfix(VitalsMinigame __instance)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            if (isRemoveVital || // バイタル無効化してる
                !enableVitalLimit || //バイタル制限あるか
                __instance.BatteryText.gameObject.active) //科学者の能力使用か
            { 
                return; 
            }

            foreach (ExtremeRoleId roleId in vitalUseRole)
            {

                SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
                MultiAssignRoleBase multiAssignRole = role as MultiAssignRoleBase;

                if (role.Id == roleId)
                {
                    if (((IRoleAbility)role).Button.IsAbilityActive())
                    {
                        return;
                    }
                }
                if (multiAssignRole?.AnotherRole?.Id == roleId)
                {
                    if (((IRoleAbility)multiAssignRole.AnotherRole).Button.IsAbilityActive())
                    {
                        return;
                    }
                }
            }

            if (timerText == null)
            {
                timerText = Object.Instantiate(
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                    __instance.transform);
                timerText.transform.localPosition = new Vector3(3.4f, 2.7f, -9.0f);
                timerText.name = "vitalTimer";
            }

            if (vitalTimer > 0.0f)
            {
                vitalTimer -= Time.deltaTime;
            }

            timerText.text = $"{Mathf.CeilToInt(vitalTimer)}";
            timerText.gameObject.SetActive(true);

            if (vitalTimer <= 0.0f)
            {
                disableVital();
                __instance.ForceClose();
            }
        }

        public static void Initialize()
        {
            Object.Destroy(timerText);
            vitalTimer = OptionHolder.Ship.VitalLimitTime;
            isRemoveVital = OptionHolder.Ship.IsRemoveVital;
            enableVitalLimit = OptionHolder.Ship.EnableVitalLimit;

            Logging.Debug("---- VitalCondition ----");
            Logging.Debug($"IsRemoveVital:{isRemoveVital}");
            Logging.Debug($"EnableVitalLimit:{enableVitalLimit}");
            Logging.Debug($"VitalTime:{vitalTimer}");
        }

        private static void disableVital()
        {
            HashSet<string> vitalObj = new HashSet<string>();
            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                vitalObj = ExtremeRolesPlugin.Compat.ModMap.GetSystemObjectName(
                    Compat.Interface.SystemConsoleType.Vital);
            }
            else
            {
                switch (PlayerControl.GameOptions.MapId)
                {
                    case 0:
                    case 1:
                        break;
                    case 2:
                        vitalObj.Add(GameSystem.PolusVital);
                        break;
                    case 4:
                        vitalObj.Add(GameSystem.AirShipVital);
                        break;
                    default:
                        break;
                }
            }

            foreach (string objectName in vitalObj)
            {
                GameSystem.DisableMapModule(objectName);
            }
        }
    }
}
