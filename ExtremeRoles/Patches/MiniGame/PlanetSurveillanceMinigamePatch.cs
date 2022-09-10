using System;
using HarmonyLib;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.NextCamera))]
    public static class PlanetSurveillanceMinigameNextCameraPatch
    {
        public static bool Prefix(
            PlanetSurveillanceMinigame __instance,
            [HarmonyArgument(0)] int direction)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }
            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity()) { return true; }

            if (direction != 0 && Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(__instance.ChangeSound, false, 1f);
            }
            __instance.Dots[__instance.currentCamera].sprite = __instance.DotDisabled;
            __instance.currentCamera = (__instance.currentCamera + direction).Wrap(
                __instance.survCameras.Length);
            __instance.Dots[__instance.currentCamera].sprite = __instance.DotEnabled;
            SurvCamera survCamera = __instance.survCameras[__instance.currentCamera];
            __instance.Camera.transform.position = survCamera.transform.position + __instance.survCameras[
                __instance.currentCamera].Offset;
            __instance.LocationName.text = (
                (survCamera.NewName > StringNames.ExitButton) ? 
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        survCamera.NewName, Array.Empty<Il2CppSystem.Object>()) : survCamera.CamName);

            if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
            {
                __instance.StartCoroutine(__instance.PulseStatic());
            }

            return false;
        }
    }


    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
    public static class PlanetSurveillanceMinigameUpdatePatch
    {
        public static bool Prefix(PlanetSurveillanceMinigame __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity()) { return true; }

            __instance.isStatic = true;
            __instance.ViewPort.sharedMaterial = __instance.StaticMaterial;
            __instance.SabText.text = Helper.Translation.GetString("youDonotUse");
            __instance.SabText.gameObject.SetActive(true);

            return false;
        }

        public static void Postfix(SurveillanceMinigame __instance)
        {
            SecurityHelper.PostUpdate(__instance);
        }
    }
}
