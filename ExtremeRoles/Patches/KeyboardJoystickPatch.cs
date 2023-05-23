using UnityEngine;

using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class KeyboardJoystickPatch
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance == null || CachedPlayerControl.LocalPlayer == null) 
        { return; }

        if (ExtremeGameModeManager.Instance.RoleSelector.CanUseXion &&
            OptionManager.Instance.GetValue<bool>((int)RoleGlobalOption.UseXion) &&
            !ExtremeRolesPlugin.DebugMode.Value)
        {
            Roles.Solo.Host.Xion.SpecialKeyShortCut();
        }

        if (GameSystem.IsLobby)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Option.IGameOptionsExtensionsToHudStringPatch.ChangePage(1);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ExtremeRolesPlugin.Info.ToggleInfoOverlay(
                    Module.InfoOverlay.InfoOverlay.ShowType.AllRole);
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                ExtremeRolesPlugin.Info.ToggleInfoOverlay(
                    Module.InfoOverlay.InfoOverlay.ShowType.AllGhostRole);
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            ExtremeRolesPlugin.Info.ToggleInfoOverlay(
                Module.InfoOverlay.InfoOverlay.ShowType.LocalPlayerRole);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            ExtremeRolesPlugin.Info.ToggleInfoOverlay(
                Module.InfoOverlay.InfoOverlay.ShowType.LocalPlayerGhostRole);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ExtremeRolesPlugin.Info.ToggleInfoOverlay(
                Module.InfoOverlay.InfoOverlay.ShowType.VanilaOption);
        }

        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            ExtremeRolesPlugin.Info.ChangePage(1);
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            ExtremeRolesPlugin.Info.ChangePage(-1);
        }

       
        // キルとベントボタン
        if (CachedPlayerControl.LocalPlayer.Data == null ||
            CachedPlayerControl.LocalPlayer.Data.Role == null ||
            !RoleAssignState.Instance.IsRoleSetUpEnd) { return; }

        var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();

        if (role.IsImpostor()) { return; }

        var player = KeyboardJoystick.player;
        var hudManager = FastDestroyableSingleton<HudManager>.Instance;

        if (player.GetButtonDown(8) && role.CanKill())
        {
            hudManager.KillButton.DoClick();
        }

        if (player.GetButtonDown(50) && role.CanUseVent())
        {
            if (role.TryGetVanillaRoleId(out RoleTypes roleId))
            {
                if (roleId != RoleTypes.Engineer || 
                    ExtremeGameModeManager.Instance.ShipOption.EngineerUseImpostorVent)
                {
                    hudManager.ImpostorVentButton.DoClick();
                }
            }
            else
            {
                hudManager.ImpostorVentButton.DoClick();
            }
        }
    }
}
