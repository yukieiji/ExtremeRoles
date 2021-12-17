using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using HarmonyLib;

using Hazel;

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

            // Spawn dummys
            if ((Input.GetKeyDown(KeyCode.F)) && Modules.Helpers.IsGameLobby)
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
            if (Input.GetKeyDown(KeyCode.L) && !Modules.Helpers.IsGameLobby)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ForceEnd, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                ExtremeRoleRPC.ForceEnd();
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                Dictionary<byte, Roles.SingleRoleAbs> dict = Roles.ExtremeRoleManager.GameRole;
                if (dict.Count == 0) { return; }

                foreach (KeyValuePair<byte, Roles.SingleRoleAbs> value in dict)
                {
                    Modules.Helpers.DebugLog(
                        $"PlayerId:{value.Key}    AssignedTo:{value.Value.RoleName}");
                }
            }

            // See All roles
            if (Input.GetKeyDown(KeyCode.P))
            {
                Dictionary<byte, Roles.SingleRoleAbs> dict = Roles.ExtremeRoleManager.GameRole;
                if (dict.Count == 0) { return; }
                for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    var role = dict[playerInfo.PlayerId];
                    if (!role.HasTask)
                    {
                        continue;
                    }
                    var (playerCompleted, playerTotal) = Modules.Helpers.GetTaskInfo(playerInfo);
                    Modules.Helpers.DebugLog($"PlayerName:{playerInfo.PlayerName}  TotalTask:{playerTotal}   ComplatedTask:{playerCompleted}");
                }
            }

            // See Player TaskInfo
            if (Input.GetKeyDown(KeyCode.I))
            {
                Dictionary<byte, Roles.SingleRoleAbs> dict = Roles.ExtremeRoleManager.GameRole;
                if (dict.Count == 0) { return; }
                for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    var role = dict[playerInfo.PlayerId];
                    if (!role.HasTask)
                    {
                        continue;
                    }
                    var (_, totalTask) = Modules.Helpers.GetTaskInfo(playerInfo);
                    if (totalTask == 0)
                    {
                        var taskId = Modules.Helpers.GetRandomCommonTaskId();
                        Modules.Helpers.DebugLog($"PlayerName:{playerInfo.PlayerName}  AddTask:{taskId}");
                        Modules.Helpers.SetTask(
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
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            if (Input.GetKeyDown(KeyCode.Tab) &&
                AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            {
                ExtremeRolesPlugin.OptionsPage = ExtremeRolesPlugin.OptionsPage + 1;
            }
        }
    }
}
