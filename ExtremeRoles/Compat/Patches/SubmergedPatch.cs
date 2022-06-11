﻿using System.Collections;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using UnityEngine;

namespace ExtremeRoles.Compat.Patches
{
    public static class SubmarineSelectSpawnPrespawnStepPatch
    {
        public static bool Prefix(ref IEnumerator __result)
        {
            if (!ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger) { return true; }
            __result = assassinMeetingEnumerator();
            return false;
        }
        public static IEnumerator assassinMeetingEnumerator()
        {
            // 真っ暗になるのでそれを解除する
            HudManager.Instance.StartCoroutine(
                HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f));
            yield break;
        }
    }

    public static class SubmergedExileControllerWrapUpAndSpawnPatch
    {
        public static void Prefix(ExileController __instance)
        {
            ExtremeRoles.Patches.Controller.ExileControllerWrapUpPatch.WrapUpPrefix(
                __instance);
        }

        public static void Postfix(ExileController __instance)
        {
            ExtremeRoles.Patches.Controller.ExileControllerReEnableGameplayPatch.ReEnablePostfix();
            ExtremeRoles.Patches.Controller.ExileControllerWrapUpPatch.WrapUpPostfix(
                __instance.exiled);
        }
    }

    public static class ExileControllerBeginPrefixPatch
    {
        private static System.Type exileControllerBeginPatchType;

        public static void Postfix(object __instance)
        {
            var gameData = ExtremeRolesPlugin.GameDataStore;

            if (!gameData.AssassinMeetingTrigger) { return; }

            GameData.PlayerInfo player = GameData.Instance.GetPlayerById(
                gameData.IsMarinPlayerId);

            string printStr;

            if (gameData.AssassinateMarin)
            {
                printStr = player?.PlayerName + Helper.Translation.GetString(
                    "assassinateMarinSucsess");
            }
            else
            {
                printStr = player?.PlayerName + Helper.Translation.GetString(
                    "assassinateMarinFail");
            }

            FieldInfo[] exileControllerBeginPatchField = exileControllerBeginPatchType.GetFields(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            FieldInfo completeStringInfo = exileControllerBeginPatchField.First(f => f.Name == "CompleteString");
            completeStringInfo = exileControllerBeginPatchType.GetField("CompleteString");

            FieldInfo impostorTextInfo = exileControllerBeginPatchField.First(f => f.Name == "ImpostorText");
            impostorTextInfo = exileControllerBeginPatchType.GetField("ImpostorText");

            completeStringInfo.SetValue(__instance, printStr);
            impostorTextInfo.SetValue(__instance, string.Empty);
        }

        public static void SetType(System.Type type)
        {
            exileControllerBeginPatchType = type;
        }
    }

    public static class HudManagerUpdatePatchPostfixPatch
    {
        private static bool changed = false;
        private static System.Type hubManagerUpdateType;
        
        public static void Postfix(object __instance)
        {

            FieldInfo[] hubManagerUpdateField = hubManagerUpdateType.GetFields(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            FieldInfo floorButtonInfo = hubManagerUpdateField.First(f => f.Name == "FloorButton");
            floorButtonInfo = hubManagerUpdateType.GetField("FloorButton");
            GameObject floorButton = floorButtonInfo.GetValue(__instance) as GameObject;

            if (!Helper.GameSystem.IsFreePlay && floorButton != null && !changed)
            {
                changed = true;
                floorButton.transform.localPosition -= new Vector3(0.0f, 0.75f, 0.0f);
            }

        }
        public static void SetType(System.Type type)
        {
            changed = false;
            hubManagerUpdateType = type;
        }

        public static void ButtonTriggerReset()
        {
            changed = false;
        }
    }

    public static class SubmarineOxygenSystemDetorioratePatch
    {
        private static System.Type submarineOxygenSystemType;

        public static void Postfix()
        {
            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return; }
            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().Id == Roles.ExtremeRoleId.Assassin)
            {
                ShipStatus.Instance.RpcRepairSystem((SystemTypes)130, 64);

                MethodInfo submarineOxygenSystemInstanceField = AccessTools.PropertyGetter(
                    submarineOxygenSystemType, "Instance");
                MethodInfo RepairDamageMethod = AccessTools.Method(
                    submarineOxygenSystemType, "RepairDamage");
                RepairDamageMethod.Invoke(submarineOxygenSystemInstanceField.Invoke(
                    null, System.Array.Empty<object>()),
                    new object[] { PlayerControl.LocalPlayer.PlayerId, 64 });
            }
        }
        public static void SetType(System.Type type)
        {
            submarineOxygenSystemType = type;
        }
    }
}
