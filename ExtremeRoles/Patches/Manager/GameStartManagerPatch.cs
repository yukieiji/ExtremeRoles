using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;

using UnityEngine;
using AmongUs.Data;

using ExtremeRoles.GameMode;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.ApiHandler;
using ExtremeRoles.Module.CustomOption.OLDS;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch]
public static class GameStartManagerPatch
{
    private const float timerMaxValue = 600f;

    private static bool isCustomServer;

    private static float timer;

    private static bool update = false;

    private static string currentText = "";
    private static bool prevOptionValue;
    private static TMPro.TextMeshPro customShowText;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public static bool BeginGamePrefix(GameStartManager __instance)
    {
        if (!(
                AmongUsClient.Instance.AmHost &&
                __instance.TryGetComponent<VersionChecker>(out var version)
            ))
        {
            return true;
        }

        bool isError = version.IsError;
        if (isError)
        {
            return false;
        }

        InfoOverlay.Instance.Hide();
        // ホストはここでオプションを読み込み
        OldOptionManager.Load();

        if (ExtremeGameModeManager.Instance.ShipOption.IsRandomMap)
        {
            // 0 = Skeld
            // 1 = Mira HQ
            // 2 = Polus
            // 3 = Dleks - deactivated
            // 4 = Airship
			// 5 = Fungle

            var rng = RandomGenerator.GetTempGenerator();

            List<byte> possibleMaps = new List<byte>() { 0, 1, 2, 4, 5 };

			foreach (var mod in CompatModManager.Instance.LoadedMod.Values)
			{
				if (mod is IMapMod mapMod)
				{
					possibleMaps.Add(mapMod.MapId);
				}
			}

            byte mapId = possibleMaps[
                rng.Next(possibleMaps.Count)];

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.ShareMapId))
            {
                caller.WriteByte(mapId);
            }
            RPCOperator.ShareMapId(mapId);
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static void StartPrefix(GameStartManager __instance)
    {
        // ロビーコードコピー
        GUIUtility.systemCopyBuffer = ConectGame.CreateDirectConectUrl(AmongUsClient.Instance.GameId);

		timer = timerMaxValue;
        isCustomServer = ServerManager.Instance.IsCustomServer();

        prevOptionValue = DataManager.Settings.Gameplay.StreamerMode;

        // 値リセット
        RPCOperator.Initialize();
        __instance.gameObject.TryAddComponent<VersionChecker>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static void StartPostfix(GameStartManager __instance)
    {
        updateText(__instance, DataManager.Settings.Gameplay.StreamerMode);
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
        // ルームコード設定

        bool isStreamerMode = DataManager.Settings.Gameplay.StreamerMode;

        if (isStreamerMode != prevOptionValue)
        {
            prevOptionValue = isStreamerMode;
            updateText(__instance, isStreamerMode);
        }

		if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && !isCustomServer)
        {
            // プレイヤーカウントアップデート
            if (update)
            {
                currentText = __instance.PlayerCounter.text;
            }

            timer = Mathf.Max(0f, timer -= Time.deltaTime);
            int minutes = (int)timer / 60;
            int seconds = (int)timer % 60;

            __instance.PlayerCounter.text = $"<size=-1>{currentText}\n({minutes:00}:{seconds:00})</size>";
		}
    }

    private static void updateText(
        GameStartManager instance,
        bool isStreamerMode)
    {
        var button = GameObject.Find("Main Camera/Hud/GameStartManager/GameRoomButton");
        if (button == null) { return; }

        var info = button.transform.FindChild("GameRoomInfo_TMP");
        if (info == null) { return; }

        if (customShowText == null)
        {
            customShowText = UnityEngine.Object.Instantiate(
                instance.GameStartText, button.transform);
            customShowText.name = "StreamerModeCustomMessage";
            customShowText.transform.localPosition = new Vector3(0.0f, -0.32f, 0.0f);
            customShowText.text = $"<size=60%>{ClientOption.Instance.StreamerModeReplacementText.Value}</size>";
            customShowText.gameObject.SetActive(false);
        }

        if (isStreamerMode)
        {
            button.transform.localPosition = new Vector3(0.0f, -0.85f, 0.0f);
            info.localPosition = new Vector3(0.0f, -0.08f, 0.0f);
            customShowText.gameObject.SetActive(true);
        }
        else
        {
            button.transform.localPosition = new Vector3(0.0f, -0.958f, 0.0f);
            info.localPosition = new Vector3(0.0f, -0.229f, 0.0f);
            customShowText.gameObject.SetActive(false);
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
