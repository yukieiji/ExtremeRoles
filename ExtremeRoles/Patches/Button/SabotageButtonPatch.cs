using HarmonyLib;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Button;


[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
public static class SabotageButtonDoClickPatch
{
    public static bool Prefix(SabotageButton __instance)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

        var role = ExtremeRoleManager.GetLocalPlayerRole();
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
        if (ExtremeRoleManager.GameRole.Count == 0 || 
            !RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

        var role = ExtremeRoleManager.GetLocalPlayerRole();

        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

        bool roleCanUseSabotage = role.CanUseSabotage();

        if (MeetingHud.Instance ||
            ExileController.Instance ||
            !GameManager.Instance || 
            !GameManager.Instance.SabotagesEnabled() || 
            !roleCanUseSabotage ||
            (
                roleCanUseSabotage &&
                !role.IsImpostor() &&
                role.Id is not ExtremeRoleId.Vigilante or ExtremeRoleId.Xion &&
                localPlayer.Data.IsDead
            ))
        {
            __instance.ToggleVisible(false);
            __instance.SetDisabled();
            return false;
        }

        __instance.ToggleVisible(true);

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
