using HarmonyLib;

namespace ExtremeRoles.Patches.Role
{
    
    [HarmonyPatch(typeof(EngineerRole), nameof(EngineerRole.UseAbility))]
    public static class EngineerRoleUseAbilityPatch
    {
        public static void Prefix(EngineerRole __instance)
        {

            bool engineerImpostorVent = OptionHolder.AllOption[
                (int)OptionHolder.CommonOptionKey.EngineerUseImpostorVent].GetValue();

            if (!engineerImpostorVent) { return; }

            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            if (__instance.isActiveAndEnabled &&
                 __instance.currentTarget && 
                 !__instance.IsCoolingDown && 
                 !localPlayer.Data.IsDead && 
                 !localPlayer.MustCleanVent(__instance.currentTarget.Id) && 
                 (!__instance.IsAffectedByComms || localPlayer.inVent))
            {
                __instance.inVentTimeRemaining = PlayerControl.GameOptions.RoleOptions.EngineerInVentMaxTime;
                __instance.currentTarget.Use();
            }
        }
    }
}
