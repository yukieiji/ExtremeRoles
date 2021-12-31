using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using UnityEngine;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Patches.Manager
{

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public class GameStartManageBeginGamePatch
    {
        public static bool Prefix(GameStartManager __instance)
        {
            bool continueStart = true;

            if (OptionsHolder.AllOption[(int)OptionsHolder.CommonOptionKey.RandomMap].GetValue())
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
            RPCOperator.Initialize();
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        private static float timer = 600f;

        private static bool update = false;
        private static string currentText = "";

        public static void Prefix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance) { return; } // Not host or no instance
            update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }
        public static void Postfix(GameStartManager __instance)
        {
            // ロビータイマー設定
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance){ return; } // Not host or no instance

            if (update) { currentText = __instance.PlayerCounter.text; };

            timer = Mathf.Max(0f, timer -= Time.deltaTime);
            int minutes = (int)timer / 60;
            int seconds = (int)timer % 60;
            string suffix = $" ({minutes:00}:{seconds:00})";

            __instance.PlayerCounter.text = currentText + suffix;
            __instance.PlayerCounter.autoSizeTextContainer = true;

        }

        public static void RestTimer()
        {
            timer = 600f;
        }
    }

}
