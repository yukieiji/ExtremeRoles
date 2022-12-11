using HarmonyLib;

using ExtremeRoles.Roles;


namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
    public static class HauntMenuMinigameFilterTextPatch
    {
        public static bool Prefix(HauntMenuMinigame __instance)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();
            var isBlocked = role.IsBlockShowPlayingRoleInfo();

            var targetRoleTeam = ExtremeRoleManager.GameRole[__instance.HauntTarget.PlayerId].Team;

            __instance.FilterText.text =
                isBlocked ||
                (
                    role.IsImpostor() &&
                    ExtremeRolesPlugin.ShipState.IsAssassinAssign
                ) ? 
                    Helper.Translation.GetString("???") : 
                    Helper.Translation.GetString(targetRoleTeam.ToString());

            return false;
        }
    }
}
