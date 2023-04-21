using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;
using AmongUs.GameOptions;


using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches;


#if DEBUG
[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class DebugTool
{
    private static List<PlayerControl> bots = new List<PlayerControl>();
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public static void Postfix(KeyboardJoystick __instance)
    {
        // ExtremeRolesPlugin.Logger.LogInfo($"DebugMode: {ExtremeRolesPlugin.DebugMode.Value}");

        if (!ExtremeRolesPlugin.DebugMode.Value || 
            AmongUsClient.Instance == null || 
            PlayerControl.LocalPlayer == null) { return; }
        if (!AmongUsClient.Instance.AmHost) { return; }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            foreach (var (key, value) in TranslationController.Instance.currentLanguage.AllStrings)
            {
                Helper.Logging.Debug(
                    $"TransKey:{key}    Value:{value}");
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F2))
        {
            Logging.Debug("Set Wide Resolution");
            ResolutionManager.SetResolution(1680, 720, false);
        }

        // Spawn dummys
        if ((Input.GetKeyDown(KeyCode.F)) && GameSystem.IsLobby)
        {
            GameSystem.SpawnDummyPlayer();
        }

        // Terminate round
        if (Input.GetKeyDown(KeyCode.F1) && !GameSystem.IsLobby)
        {
            GameSystem.ForceEndGame();
        }

        // See All roles
        if (Input.GetKeyDown(KeyCode.K))
        {
            var dict = Roles.ExtremeRoleManager.GameRole;
            if (dict.Count == 0) { return; }

            foreach (KeyValuePair<byte, SingleRoleBase> value in dict)
            {
                Logging.Debug(
                    $"PlayerId:{value.Key}    AssignedTo:{value.Value.RoleName}   Team:{value.Value.Team}");
            }
        }

        // See All task
        if (Input.GetKeyDown(KeyCode.P))
        {
            var dict = Roles.ExtremeRoleManager.GameRole;
            if (dict.Count == 0) { return; }
            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                var role = dict[playerInfo.PlayerId];
                if (!role.HasTask())
                {
                    continue;
                }
                var (playerCompleted, playerTotal) = GameSystem.GetTaskInfo(playerInfo);
                Logging.Debug($"PlayerName:{playerInfo.PlayerName}  TotalTask:{playerTotal}   ComplatedTask:{playerCompleted}");
            }
        }

        // See Player TaskInfo
        if (Input.GetKeyDown(KeyCode.I))
        {
            var dict = Roles.ExtremeRoleManager.GameRole;
            if (dict.Count == 0) { return; }
            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                var role = dict[playerInfo.PlayerId];
                if (!role.HasTask())
                {
                    continue;
                }
                var (_, totalTask) = GameSystem.GetTaskInfo(playerInfo);
                if (totalTask == 0)
                {
                    int taskId = GameSystem.GetRandomCommonTaskId();
                    Logging.Debug($"PlayerName:{playerInfo.PlayerName}  AddTask:{taskId}");
                    GameSystem.SetTask(
                        playerInfo, taskId);
                }

            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            var player = CachedPlayerControl.LocalPlayer;
            GameSystem.CreateNoneReportableDeadbody(
                player, player.transform.position + new Vector3(0.75f, 0.75f));
        }
    }
}
#endif

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class KeyboardJoystickPatch
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance == null || CachedPlayerControl.LocalPlayer == null) 
        { return; }

        if (ExtremeGameModeManager.Instance.RoleSelector.CanUseXion &&
            AllOptionHolder.Instance.GetValue<bool>((int)RoleGlobalOption.UseXion) &&
            !ExtremeRolesPlugin.DebugMode.Value)
        {
            Roles.Solo.Host.Xion.SpecialKeyShortCut();
        }

        if (GameSystem.IsLobby)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                OptionHolder.OptionsPage = OptionHolder.OptionsPage + 1;
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
