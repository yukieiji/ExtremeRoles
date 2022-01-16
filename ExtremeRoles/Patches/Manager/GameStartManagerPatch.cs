using System.Collections.Generic;
using HarmonyLib;

using UnityEngine;

namespace ExtremeRoles.Patches.Manager
{

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public class GameStartManageBeginGamePatch
    {
        public static bool Prefix(GameStartManager __instance)
        {

            bool continueStart = true;

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
                PlayerControl.GameOptions.MapId = possibleMaps[
                    rng.Next(possibleMaps.Count)];
            }
            return continueStart;

        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public class GameStartManagerStartPatch
    {
        public static void Prefix(GameStartManager __instance)
        {
            // ロビーコードコピー
            string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            GUIUtility.systemCopyBuffer = code;
            GameStartManagerUpdatePatch.SetRoomCode(code);

            // 値リセット
            RPCOperator.Initialize();
        }
        public static void Postfix(GameStartManager __instance)
        {   
            if (Module.Prefab.Arrow == null)
            {
                Module.Prefab.Arrow = __instance.StartButton.sprite;
            }
            
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        private static float timer = 600f;

        private static bool update = false;
        private static string currentText = "";
        private static string roomCodeText = string.Empty;

        public static void Prefix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance) { return; } // Not host or no instance
            update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }
        public static void Postfix(GameStartManager __instance)
        {

            // ルームコード設定
            if (OptionHolder.Client.StreamerMode)
            {
                __instance.GameRoomName.text = $"\n\n{OptionHolder.ConfigParser.StreamerModeReplacementText.Value}";
            }
            else
            {
                __instance.GameRoomName.text = roomCodeText;
            }
            
            // ロビータイマー設定
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance){ return; } // Not host or no instance

            bool blockStart = false;

            if (update)
            {
                /*
                string message = "";

                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                {
                    if (client.Character == null) { continue; }

                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    
                    if (dummyComponent != null && dummyComponent.enabled)
                    {
                        continue;
                    }
                    else if (!playerVersions.ContainsKey(client.Id))
                    {
                        blockStart = true;
                        message += $"<color=#FF0000FF>{client.Character.Data.PlayerName}:  {Helper.Translation.GetString("errorNotInstalled")}\n</color>";
                    }
                    else
                    {
                        PlayerVersion PV = playerVersions[client.Id];
                        int diff = TheOtherRolesPlugin.Version.CompareTo(PV.version);
                        if (diff > 0)
                        {
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName}:  {Helper.Translation.GetString("errorOlderVersion")} (v{playerVersions[client.Id].version.ToString()})\n</color>";
                            blockStart = true;
                        }
                        else if (diff < 0)
                        {
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName}:  {Helper.Translation.GetString("errorNewerVersion")} (v{playerVersions[client.Id].version.ToString()})\n</color>";
                            blockStart = true;
                        }
                        else if (!PV.GuidMatches())
                        { // version presumably matches, check if Guid matches
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName}:  {Helper.Translation.GetString("errorWrongVersion")} v{playerVersions[client.Id].version.ToString()} <size=30%>({PV.guid.ToString()})</size>\n</color>";
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
                    __instance.StartButton.color = __instance.startLabelText.color = ((__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                }
                */
                // プレイヤーカウントアップデート
                currentText = __instance.PlayerCounter.text; 
            }

            timer = Mathf.Max(0f, timer -= Time.deltaTime);
            int minutes = (int)timer / 60;
            int seconds = (int)timer % 60;
            string suffix = $" ({minutes:00}:{seconds:00})";

            __instance.PlayerCounter.text = currentText + suffix;
            __instance.PlayerCounter.autoSizeTextContainer = true;

        }

        public static void SetRoomCode(string roomCode)
        {
            roomCodeText = roomCode;
        }

        public static void RestTimer()
        {
            timer = 600f;
        }
    }

}
