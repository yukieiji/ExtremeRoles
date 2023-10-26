using HarmonyLib;
using ExtremeRoles.Roles.API.Extension.State;

namespace ExtremeRoles.Patches.MiniGame;

[HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Update))]
public static class FungleSurveillanceMinigameePatch
{
    public static void Prefix(FungleSurveillanceMinigame __instance)
    {
        if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return; }

		bool enable =
			Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity() ||
			SecurityHelper.IsAbilityUse();

		__instance.viewport.enabled = enable;
    }
    public static void Postfix(FungleSurveillanceMinigame __instance)
    {
        SecurityHelper.PostUpdate(__instance);
    }
}
