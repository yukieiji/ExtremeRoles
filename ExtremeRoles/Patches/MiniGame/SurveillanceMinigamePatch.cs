using System.Linq;
using UnityEngine;

using HarmonyLib;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.MiniGame
{
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
                    camera.transform.position = new Vector3(surv.transform.position.x, surv.transform.position.y, 8f);
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
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity)
            {
                updateCamera(__instance);
                return false;
            }

            __instance.isStatic = true;
            for (int i = 0; i < __instance.ViewPorts.Length; ++i)
            {
                __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                __instance.SabText[i].text = Helper.Translation.GetString("youDonotUse");
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

            if ((instance.isStatic || update) && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(CachedPlayerControl.LocalPlayer))
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
            else if (!instance.isStatic && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(CachedPlayerControl.LocalPlayer))
            {
                instance.isStatic = true;
                for (int j = 0; j < instance.ViewPorts.Length; j++)
                {
                    instance.ViewPorts[j].sharedMaterial = instance.StaticMaterial;
                    instance.SabText[j].gameObject.SetActive(true);
                }
            }
        }

    }
}
