using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using UnityEngine;
using ExtremeRoles.Performance;

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
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(
                FastDestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f));
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
            var submergedMod = ExtremeRolesPlugin.Compat.ModMap as Mods.SubmergedMap;
            if (submergedMod == null) { return; }

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
        private static FieldInfo floorButtonInfo;
        
        public static void Postfix(object __instance)
        {
            var submergedMod = ExtremeRolesPlugin.Compat.ModMap as Mods.SubmergedMap;
            if (submergedMod == null) { return; }

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
            FieldInfo[] hubManagerUpdateField = type.GetFields(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            floorButtonInfo = hubManagerUpdateField.First(f => f.Name == "FloorButton");
            floorButtonInfo = type.GetField("FloorButton");
        }

        public static void ButtonTriggerReset()
        {
            changed = false;
        }
    }

    public static class SubmarineOxygenSystemDetorioratePatch
    {
        private static PropertyInfo submarineOxygenSystemInstance;
        private static FieldInfo submarineOxygenSystemPlayersWithMask;

        public static void Postfix(object __instance)
        {
            var submergedMod = ExtremeRolesPlugin.Compat.ModMap as Mods.SubmergedMap;
            if (submergedMod == null) { return; }

            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return; }
            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().Id != Roles.ExtremeRoleId.Assassin) { return; }

            object instance = submarineOxygenSystemInstance.GetValue(null);
            if (instance == null) { return; }
            
            HashSet<byte> playersWithMask = submarineOxygenSystemPlayersWithMask.GetValue(__instance) as HashSet<byte>;
            
            if (playersWithMask != null && 
                !playersWithMask.Contains(PlayerControl.LocalPlayer.PlayerId))
            {
                submergedMod.RepairCustomSabotage(
                        submergedMod.RetrieveOxygenMask);
            }
        }
        public static void SetType(System.Type type)
        {
            submarineOxygenSystemInstance = AccessTools.Property(type, "Instance");

            FieldInfo[] submarineOxygenSystemField = type.GetFields(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            submarineOxygenSystemPlayersWithMask = submarineOxygenSystemField.First(f => f.Name == "PlayersWithMask");
            submarineOxygenSystemPlayersWithMask = type.GetField("PlayersWithMask");
        }

    }
}
