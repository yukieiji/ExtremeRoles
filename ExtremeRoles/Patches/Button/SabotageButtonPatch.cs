using HarmonyLib;

using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Button
{

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    public static class SabotageButtonDoClickPatch
    {
        public static bool Prefix(SabotageButton __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();
            // The sabotage button behaves just fine if it's a regular impostor
            if ((CachedPlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor) ||
                role.IsImpostor()) { return true; }
            if (!role.CanUseSabotage()) { return true; }

            FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(
                new MapOptions
                {
                    Mode = MapOptions.Modes.Sabotage,
                    AllowMovementWhileMapOpen = true,
                });
            return false;
        }
    }
}
