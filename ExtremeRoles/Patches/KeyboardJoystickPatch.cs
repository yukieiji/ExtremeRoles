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
        private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
        private static List<PlayerControl> bots = new List<PlayerControl>();

        public static void Postfix(KeyboardJoystick __instance)
        {
            // ExtremeRolesPlugin.Logger.LogInfo($"DebugMode: {ExtremeRolesPlugin.DebugMode.Value}");

            if (!ExtremeRolesPlugin.DebugMode.Value) { return; }
            if (!AmongUsClient.Instance.AmHost) { return; }

            // Spawn dummys
            if ((Input.GetKeyDown(KeyCode.F)) && Map.IsGameLobby)
            {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl);
                AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                int hat = random.Next(HatManager.Instance.AllHats.Count);
                int pet = random.Next(HatManager.Instance.AllPets.Count);
                int skin = random.Next(HatManager.Instance.AllSkins.Count);
                int visor = random.Next(HatManager.Instance.AllVisors.Count);
                int color = random.Next(Palette.PlayerColors.Length);

                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor(color);
                playerControl.SetHat(HatManager.Instance.AllHats[hat].ProductId, color);
                playerControl.SetPet(HatManager.Instance.AllPets[pet].ProductId, color);
                playerControl.SetVisor(HatManager.Instance.AllVisors[visor].ProductId);
                playerControl.SetSkin(HatManager.Instance.AllSkins[skin].ProductId);
                GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            }

            // Terminate round
            if (Input.GetKeyDown(KeyCode.L) && !Map.IsGameLobby)
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
                Dictionary<byte, SingleRoleBase> dict = Roles.ExtremeRoleManager.GameRole;
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
                Dictionary<byte, SingleRoleBase> dict = Roles.ExtremeRoleManager.GameRole;
                if (dict.Count == 0) { return; }
                for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    var role = dict[playerInfo.PlayerId];
                    if (!role.HasTask)
                    {
                        continue;
                    }
                    var (playerCompleted, playerTotal) = Task.GetTaskInfo(playerInfo);
                    Logging.Debug($"PlayerName:{playerInfo.PlayerName}  TotalTask:{playerTotal}   ComplatedTask:{playerCompleted}");
                }
            }

            // See Player TaskInfo
            if (Input.GetKeyDown(KeyCode.I))
            {
                Dictionary<byte, SingleRoleBase> dict = Roles.ExtremeRoleManager.GameRole;
                if (dict.Count == 0) { return; }
                for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    var role = dict[playerInfo.PlayerId];
                    if (!role.HasTask)
                    {
                        continue;
                    }
                    var (_, totalTask) = Task.GetTaskInfo(playerInfo);
                    if (totalTask == 0)
                    {
                        var taskId = Task.GetRandomCommonTaskId();
                        Logging.Debug($"PlayerName:{playerInfo.PlayerName}  AddTask:{taskId}");
                        Task.SetTask(
                            playerInfo, taskId);
                    }

                }
            }
        }
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
#endif

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class KeyboardJoystickPatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {

            InnerNet.InnerNetClient.GameStates state = AmongUsClient.Instance.GameState;


            if (state != InnerNet.InnerNetClient.GameStates.Started)
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

            if (Input.GetKeyDown(KeyCode.H))
            {
                ExtremeRolesPlugin.Info.ToggleInfoOverlay();
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
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
                }
            }

        }
    }
}
