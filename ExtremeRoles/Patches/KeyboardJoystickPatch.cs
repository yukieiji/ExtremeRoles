using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using HarmonyLib;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Patches
{

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

            // Spawn dummys
            if ((Input.GetKeyDown(KeyCode.F)) && GameSystem.IsLobby)
            {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl);
                AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                int hat = RandomGenerator.Instance.Next(HatManager.Instance.allHats.Count);
                int pet = RandomGenerator.Instance.Next(HatManager.Instance.allPets.Count);
                int skin = RandomGenerator.Instance.Next(HatManager.Instance.allSkins.Count);
                int visor = RandomGenerator.Instance.Next(HatManager.Instance.allVisors.Count);
                int color = RandomGenerator.Instance.Next(Palette.PlayerColors.Length);

                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor(color);
                playerControl.SetHat(HatManager.Instance.allHats[hat].ProdId, color);
                playerControl.SetPet(HatManager.Instance.allPets[pet].ProdId, color);
                playerControl.SetVisor(HatManager.Instance.allVisors[visor].ProdId);
                playerControl.SetSkin(HatManager.Instance.allSkins[skin].ProdId, color);
                GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            }

            // Terminate round
            if (Input.GetKeyDown(KeyCode.L) && !GameSystem.IsLobby)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    (byte)RPCOperator.Command.ForceEnd,
                    Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCOperator.ForceEnd();
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
                for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    var role = dict[playerInfo.PlayerId];
                    if (!role.HasTask)
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
                for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    var role = dict[playerInfo.PlayerId];
                    if (!role.HasTask)
                    {
                        continue;
                    }
                    var (_, totalTask) = GameSystem.GetTaskInfo(playerInfo);
                    if (totalTask == 0)
                    {
                        var taskId = GameSystem.GetRandomCommonTaskId();
                        Logging.Debug($"PlayerName:{playerInfo.PlayerName}  AddTask:{taskId}");
                        GameSystem.SetTask(
                            playerInfo, taskId);
                    }

                }
            }
        }
        private static string RandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RandomGenerator.Instance.Next(s.Length)]).ToArray());
        }
    }
#endif

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class KeyboardJoystickPatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null) { return; }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                Logging.Dump();
            }

            if (GameSystem.IsLobby)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    OptionHolder.OptionsPage = OptionHolder.OptionsPage + 1;
                }
                if (Input.GetKeyDown(KeyCode.PageDown) &&
                    ExtremeRolesPlugin.Info.OverlayShown)
                {
                    ExtremeRolesPlugin.Info.ChangePage(1);
                }
                if (Input.GetKeyDown(KeyCode.PageUp) &&
                    ExtremeRolesPlugin.Info.OverlayShown)
                {
                    ExtremeRolesPlugin.Info.ChangePage(-1);
                }
            }

            if (Input.GetKeyDown(KeyCode.H) && !HudManager.Instance.Chat.IsOpen)
            {
                var showType = Module.InfoOverlay.InfoOverlay.ShowType.Normal;
                ExtremeRolesPlugin.Info.ToggleInfoOverlay(showType);

            }
            if (Input.GetKeyDown(KeyCode.G) && !HudManager.Instance.Chat.IsOpen)
            {
                var showType = Module.InfoOverlay.InfoOverlay.ShowType.Ghost;
                ExtremeRolesPlugin.Info.ToggleInfoOverlay(showType);
            }

            // キルとベントボタン
            if (PlayerControl.LocalPlayer.Data != null && 
                PlayerControl.LocalPlayer.Data.Role != null &&
                ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd())
            {

                var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();

                if (role.CanKill && KeyboardJoystick.player.GetButtonDown(8))
                {
                    DestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
                }
                if (role.UseVent && KeyboardJoystick.player.GetButtonDown(50))
                {
                    if(role.IsVanillaRole())
                    {
                        if (!(((Roles.Solo.VanillaRoleWrapper)role).VanilaRoleId == RoleTypes.Engineer) ||
                            OptionHolder.AllOption[
                                (int)OptionHolder.CommonOptionKey.EngineerUseImpostorVent].GetValue())
                        {
                            DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
                        }
                    }
                    else
                    {
                        DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
                    }
                }
            }

        }
    }
}
