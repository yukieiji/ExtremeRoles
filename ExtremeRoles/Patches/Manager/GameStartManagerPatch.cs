using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch]
    public class GameStartManagerPatch
    {
        private const float kickTime = 30f;
        private const float timerMaxValue = 600f;
        private const string errorColorPlaceHolder = "<color=#FF0000FF>{0}\n</color>";
        
        private static bool isCustomServer;

        private static float timer;
        private static float kickingTimer;

        private static bool isVersionSent;
        private static bool update = false;
        private static string currentText = "";
        private static string roomCodeText = string.Empty;


        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public static bool BeginGamePrefix(GameStartManager __instance)
        {

            bool continueStart = true;

            if (AmongUsClient.Instance.AmHost)
            {
                var allPlayerVersion = ExtremeRolesPlugin.GameDataStore.PlayerVersion;

                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.GetFastEnumerator())
                {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled)
                    {
                        continue;
                    }
                    if (!allPlayerVersion.ContainsKey(client.Id))
                    {
                        continueStart = false;
                        break;
                    }
                    int diff = Assembly.GetExecutingAssembly().GetName().Version.CompareTo(
                        allPlayerVersion[client.Id]);
                    if (diff != 0)
                    {
                        continueStart = false;
                        break;
                    }
                }
            }

            ExtremeRolesPlugin.Info.HideInfoOverlay();

            if (OptionHolder.AllOption[(int)OptionHolder.CommonOptionKey.RandomMap].GetValue())
            {
                // 0 = Skeld
                // 1 = Mira HQ
                // 2 = Polus
                // 3 = Dleks - deactivated
                // 4 = Airship

                var rng = RandomGenerator.GetTempGenerator();

                List<byte> possibleMaps = new List<byte>() { 0, 1, 2, 4 };
                byte mapId = possibleMaps[
                    rng.Next(possibleMaps.Count)];
                RPCOperator.Call(
                    PlayerControl.LocalPlayer.NetId,
                    RPCOperator.Command.ShareMapId,
                    new List<byte> { mapId });
                RPCOperator.ShareMapId(mapId);
            }
            return continueStart;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public static void StartPrefix(GameStartManager __instance)
        {
            // ロビーコードコピー
            string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            GUIUtility.systemCopyBuffer = code;

            isVersionSent = false;
            roomCodeText = code;
            timer = timerMaxValue;
            kickingTimer = 0f;
            isCustomServer = DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == "custom";

            // 値リセット
            RPCOperator.Initialize();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public static void StartPostfix(GameStartManager __instance)
        {
            if (Module.Prefab.Arrow == null)
            {
                Module.Prefab.Arrow = __instance.StartButton.sprite;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static void UpdatePrefix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance) { return; } // Not host or no instance
            update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static void UpdatePostfix(GameStartManager __instance)
        {
            if (PlayerControl.LocalPlayer != null && !isVersionSent)
            {
                isVersionSent = true;
                GameSystem.ShareVersion();
            }

            // ルームコード設定
            if (OptionHolder.Client.StreamerMode)
            {
                __instance.GameRoomName.text = OptionHolder.ConfigParser.StreamerModeReplacementText.Value;
            }
            else
            {
                __instance.GameRoomName.text = roomCodeText;
            }

            // Instanceミス
            if (!GameData.Instance) { return; }

            var localGameVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var allPlayerVersion = ExtremeRolesPlugin.GameDataStore.PlayerVersion;

            // ホスト以外
            if (!AmongUsClient.Instance.AmHost)
            {
                if (!allPlayerVersion.ContainsKey(AmongUsClient.Instance.HostId) ||
                    localGameVersion.CompareTo(allPlayerVersion[AmongUsClient.Instance.HostId]) != 0)
                {
                    kickingTimer += Time.deltaTime;
                    if (kickingTimer > kickTime)
                    {
                        kickingTimer = 0;
                        AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                        SceneChanger.ChangeScene("MainMenu");
                    }

                    __instance.GameStartText.text = string.Format(
                        Translation.GetString("errorDiffHostVersion"),
                        Mathf.Round(kickTime - kickingTimer));
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                }
                else
                {
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    if (__instance.startState != GameStartManager.StartingStates.Countdown)
                    {
                        __instance.GameStartText.text = string.Empty;
                    }
                }
                return;
            }

            bool blockStart = false;
            string message = string.Format(
                errorColorPlaceHolder,
                Translation.GetString("errorCannotGameStart"));
            foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.GetFastEnumerator())
            {
                if (client.Character == null) { continue; }

                var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                if (dummyComponent != null && dummyComponent.enabled)
                {
                    continue;
                }
                else if (!allPlayerVersion.ContainsKey(client.Id))
                {
                    blockStart = true;
                    message += string.Format(
                        errorColorPlaceHolder,
                        $"{client.Character.Data.PlayerName}:  {Translation.GetString("errorNotInstalled")}");
                }
                else
                {
                    System.Version playerVersion = allPlayerVersion[client.Id];
                    int diff = localGameVersion.CompareTo(playerVersion);
                    if (diff > 0)
                    {
                        message += string.Format(
                            errorColorPlaceHolder,
                            $"{client.Character.Data.PlayerName}:  {Translation.GetString("errorOldInstalled")}");
                        blockStart = true;
                    }
                    else if (diff < 0)
                    {
                        message += string.Format(
                            errorColorPlaceHolder,
                            $"{client.Character.Data.PlayerName}:  {Translation.GetString("errorNewInstalled")}");
                        blockStart = true;
                    }
                }
            }

            if (blockStart)
            {
                __instance.StartButton.color = __instance.startLabelText.color = Palette.DisabledClear;
                __instance.GameStartText.text = message;
                __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
            }
            else
            {
                __instance.StartButton.color = __instance.startLabelText.color = (
                    (__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
            }

            if (AmongUsClient.Instance.GameMode == GameModes.OnlineGame && !isCustomServer)
            {
                // プレイヤーカウントアップデート
                if (update)
                {
                    currentText = __instance.PlayerCounter.text;
                }

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;

                __instance.PlayerCounter.text = $"{currentText}   ({minutes:00}:{seconds:00})";
                __instance.PlayerCounter.autoSizeTextContainer = true;
            }
        }

    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.SetStartCounter))]
    public static class GameStartManagerSetStartCounterPatch
    {
        public static void Postfix(GameStartManager __instance, sbyte sec)
        {
            if (sec > 0)
            {
                __instance.startState = GameStartManager.StartingStates.Countdown;
            }

            if (sec <= 0)
            {
                __instance.startState = GameStartManager.StartingStates.NotStarting;
            }
        }
    }

}
