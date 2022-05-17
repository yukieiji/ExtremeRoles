using System.Linq;
using UnityEngine;

using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    public static class SurveillanceMinigameBeginPatch
    {
        public static void Postfix(SurveillanceMinigame __instance)
        {
            if (ShipStatus.Instance.AllCameras.Length > 4 && __instance.FilteredRooms.Length > 0)
            {
                __instance.textures = __instance.textures.ToList().Concat(
                    new RenderTexture[ShipStatus.Instance.AllCameras.Length - 4]).ToArray();
                for (int i = 4; i < ShipStatus.Instance.AllCameras.Length; i++)
                {
                    SurvCamera surv = ShipStatus.Instance.AllCameras[i];
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
        public static bool Prefix(SurveillanceMinigame __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (Roles.ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].CanUseSecurity) { return true; }

            __instance.isStatic = true;
            for (int i = 0; i < __instance.ViewPorts.Length; ++i)
            {
                __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                __instance.SabText[i].text = Helper.Translation.GetString("youDonotUse");
                __instance.SabText[i].gameObject.SetActive(true);
            }

            return false;
        }
    }
}
