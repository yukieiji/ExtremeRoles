using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.MiniGame
{
    public static class SecurityHelper
    {
        private static float cameraTimer = 0.0f;
        private static bool enableCameraLimit = false;
        private static bool isRemoveSecurity = false;
        private static TMPro.TextMeshPro timerText;

        private static readonly HashSet<ExtremeRoleId> securityUseRole = new HashSet<ExtremeRoleId>()
        {
            ExtremeRoleId.Traitor,
            ExtremeRoleId.Watchdog,
        };


        public static void Initialize()
        {
            cameraTimer = OptionHolder.Ship.SecurityLimitTime;
            isRemoveSecurity = OptionHolder.Ship.IsRemoveSecurity;
            enableCameraLimit = OptionHolder.Ship.EnableSecurityLimit;

            Logging.Debug("---- SecurityCondition ----");
            Logging.Debug($"IsRemoveSecurity:{enableCameraLimit}");
            Logging.Debug($"EnableSecurityLimit:{isRemoveSecurity}");
            Logging.Debug($"SecurityTime:{cameraTimer}");

        }

        public static void PostUpdate(Minigame instance)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            if (isRemoveSecurity || // セキュリティ無効化してる
                !enableCameraLimit) // セキュリティ制限あるか
            {
                return;
            }

            foreach (ExtremeRoleId roleId in securityUseRole)
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
                    instance.transform);
                timerText.transform.localPosition = new Vector3(3.4f, 2.7f, -9.0f);
                timerText.name = "securityTimer";
            }

            if (cameraTimer > 0.0f)
            {
                cameraTimer -= Time.deltaTime;
            }

            timerText.text = $"{Mathf.CeilToInt(cameraTimer)}";
            timerText.gameObject.SetActive(true);

            if (cameraTimer <= 0.0f)
            {
                disableSecurity();
                instance.ForceClose();
            }
        }

        public static TMPro.TextMeshPro GetTimerText() => timerText;

        private static void disableSecurity()
        {
            HashSet<string> vitalObj = new HashSet<string>();
            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                vitalObj = ExtremeRolesPlugin.Compat.ModMap.GetSystemObjectName(
                    Compat.Interface.SystemConsoleType.SecurityCamera);
            }
            else
            {
                switch (PlayerControl.GameOptions.MapId)
                {
                    case 0:
                        vitalObj.Add(GameSystem.SkeldSecurity);
                        break;
                    case 1:
                        vitalObj.Add(GameSystem.MiraHqSecurity);
                        break;
                    case 2:
                        vitalObj.Add(GameSystem.PolusSecurity);
                        break;
                    case 4:
                        vitalObj.Add(GameSystem.AirShipSecurity);
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

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    public static class SurveillanceMinigameBeginPatch
    {
        public static void Postfix(SurveillanceMinigame __instance)
        {
            SurveillanceMinigameUpdatePatch.Timer = SurveillanceMinigameUpdatePatch.ChangeTime;
            SurveillanceMinigameUpdatePatch.Page = 0;

            if (CachedShipStatus.Instance.AllCameras.Length > 4 && __instance.FilteredRooms.Length > 0)
            {
                __instance.textures = __instance.textures.ToList().Concat(
                    new RenderTexture[CachedShipStatus.Instance.AllCameras.Length - 4]).ToArray();
                for (int i = 4; i < CachedShipStatus.Instance.AllCameras.Length; i++)
                {
                    SurvCamera surv = CachedShipStatus.Instance.AllCameras[i];
                    Camera camera = UnityEngine.Object.Instantiate<Camera>(__instance.CameraPrefab);
                    camera.transform.SetParent(__instance.transform);
                    camera.transform.position = new Vector3(
                        surv.transform.position.x,
                        surv.transform.position.y, 8f);
                    camera.orthographicSize = 2.35f;
                    RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
                    __instance.textures[i] = temporary;
                    camera.targetTexture = temporary;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
    public static class SurveillanceMinigameUpdatePatch
    {
        public const float ChangeTime = 4.0f;
        public static float Timer;
        public static int Page;

        public static bool Prefix(SurveillanceMinigame __instance)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity())
            {
                updateCamera(__instance);
                return false;
            }

            __instance.isStatic = true;
            for (int i = 0; i < __instance.ViewPorts.Length; ++i)
            {
                __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                __instance.SabText[i].text = Translation.GetString("youDonotUse");
                __instance.SabText[i].gameObject.SetActive(true);
            }

            return false;
        }

        private static void updateCamera(SurveillanceMinigame instance)
        {
            Timer -= Time.deltaTime;
            int numberOfPages = Mathf.CeilToInt(CachedShipStatus.Instance.AllCameras.Length / 4f);

            bool update = false;

            if (Timer < 0.0f || Input.GetKeyDown(KeyCode.RightArrow))
            {
                update = true;
                Timer = ChangeTime;
                Page = (Page + 1) % numberOfPages;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Page = (Page + numberOfPages - 1) % numberOfPages;
                update = true;
                Timer = ChangeTime;
            }

            if ((instance.isStatic || update) && 
                !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
                    CachedPlayerControl.LocalPlayer))
            {
                instance.isStatic = false;
                for (int i = 0; i < instance.ViewPorts.Length; i++)
                {
                    instance.ViewPorts[i].sharedMaterial = instance.DefaultMaterial;
                    instance.SabText[i].gameObject.SetActive(false);
                    if (Page * 4 + i < instance.textures.Length)
                    {
                        instance.ViewPorts[i].material.SetTexture(
                            "_MainTex", instance.textures[Page * 4 + i]);
                    }
                    else
                    {
                        instance.ViewPorts[i].sharedMaterial = instance.StaticMaterial;
                    }
                }
            }
            else if (!instance.isStatic && 
                PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(
                    CachedPlayerControl.LocalPlayer))
            {
                instance.isStatic = true;
                for (int j = 0; j < instance.ViewPorts.Length; j++)
                {
                    instance.ViewPorts[j].sharedMaterial = instance.StaticMaterial;
                    instance.SabText[j].gameObject.SetActive(true);
                }
            }
        }

        public static void Postfix(SurveillanceMinigame __instance)
        {
            SecurityHelper.PostUpdate(__instance);
        }
    }
}
