using System.Reflection;

using HarmonyLib;

using UnityEngine;

namespace ExtremeSkins.Patches.AmongUs.Manager
{
    [HarmonyPatch]
    public class GameStartManagerPatch
    {
        private const float kickTime = 30f;
        private const float timerMaxValue = 600f;
        private const string errorColorPlaceHolder = "<color=#FF0000FF>{0}\n</color>";

        private static float kickingTimer;
        private static bool isVersionSent;


        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public static bool BeginGamePrefix(GameStartManager __instance)
        {

            bool continueStart = true;

            if (AmongUsClient.Instance.AmHost)
            {
                var allPlayerVersion = VersionManager.PlayerVersion;

                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients)
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
            return continueStart;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public static void StartPrefix(GameStartManager __instance)
        {
            isVersionSent = false;
            kickingTimer = 0f;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static void UpdatePostfix(GameStartManager __instance)
        {
            if (PlayerControl.LocalPlayer != null && !isVersionSent)
            {
                isVersionSent = true;
                VersionManager.ShareVersion();
            }

            // Instanceミス
            if (!GameData.Instance) { return; }

            var localGameVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var allPlayerVersion = VersionManager.PlayerVersion;

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
                        Helper.Translation.GetString("errorDiffHostVersion"),
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
                Helper.Translation.GetString("errorCannotGameStart"));
            foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
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
                        $"{client.Character.Data.PlayerName}:  {Helper.Translation.GetString("errorNotInstalled")}");
                }
                else
                {
                    System.Version playerVersion = allPlayerVersion[client.Id];
                    int diff = localGameVersion.CompareTo(playerVersion);
                    if (diff > 0)
                    {
                        message += string.Format(
                            errorColorPlaceHolder,
                            $"{client.Character.Data.PlayerName}:  {Helper.Translation.GetString("errorOldInstalled")}");
                        blockStart = true;
                    }
                    else if (diff < 0)
                    {
                        message += string.Format(
                            errorColorPlaceHolder,
                            $"{client.Character.Data.PlayerName}:  {Helper.Translation.GetString("errorNewInstalled")}");
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

        }

    }
}
