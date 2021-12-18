using HarmonyLib;

namespace ExtremeRoles.Patches.Button
{

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    public static class SabotageButtonDoClickPatch
    {
        public static bool Prefix(SabotageButton __instance)
        {
            var localPlayer = PlayerControl.LocalPlayer;
            var role = Roles.ExtremeRoleManager.GameRole[localPlayer.PlayerId];
            // The sabotage button behaves just fine if it's a regular impostor
            if ((localPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor) ||
                role.IsImposter()) { return true; }
            if (!role.UseSabotage) { return true; }

            DestroyableSingleton<HudManager>.Instance.ShowMap(
                (Il2CppSystem.Action<MapBehaviour>)((m) => { m.ShowSabotageMap(); }));
            return false;
        }
    }
}
