using HarmonyLib;

using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;

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

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.Refresh))]
    public static class SabotageButtonRefreshPatch
    {
        public static bool Prefix(SabotageButton __instance)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            if (MeetingHud.Instance ||
                ExileController.Instance ||
                !GameManager.Instance || 
                !GameManager.Instance.SabotagesEnabled() || 
                !role.CanUseSabotage() ||
                (
                    !role.IsImpostor() &&
                    role.Id is not ExtremeRoleId.Vigilante or ExtremeRoleId.Xion &&
                    localPlayer.Data.IsDead
                ))
            {
                __instance.ToggleVisible(false);
                __instance.SetDisabled();
                return false;
            }

            if (localPlayer.inVent || localPlayer.petting)
            {
                __instance.SetDisabled();
            }
            else
            {
                __instance.SetEnabled();
            }
            
            return false;
        }
    }
}
