using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using UnityEngine;
using ExtremeRoles.Roles.API.Extension.State;
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

    public static class SubmarineSelectOnDestroyPatch
    {
        public static void Prefix()
        {
            ExtremeRoles.Patches.Controller.ExileControllerReEnableGameplayPatch.ReEnablePostfix();
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
            ExtremeRoles.Patches.Controller.ExileControllerWrapUpPatch.WrapUpPostfix(
                __instance.exiled);
            
            if (!ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger)
            {
                HudManagerUpdatePatchPostfixPatch.ButtonTriggerReset();
            }
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

            FieldInfo completeStringInfo = exileControllerBeginPatchType.GetField("CompleteString");
            FieldInfo impostorTextInfo = exileControllerBeginPatchType.GetField("ImpostorText");

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

            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd) { return; }
            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().Id != Roles.ExtremeRoleId.Assassin) { return; }

            object instance = submarineOxygenSystemInstance.GetValue(null);
            if (instance == null) { return; }
            
            HashSet<byte> playersWithMask = submarineOxygenSystemPlayersWithMask.GetValue(__instance) as HashSet<byte>;
            
            if (playersWithMask != null && 
                !playersWithMask.Contains(CachedPlayerControl.LocalPlayer.PlayerId))
            {
                submergedMod.RepairCustomSabotage(
                    submergedMod.RetrieveOxygenMask);
            }
        }
        public static void SetType(System.Type type)
        {
            submarineOxygenSystemInstance = AccessTools.Property(type, "Instance");
            submarineOxygenSystemPlayersWithMask = type.GetField("PlayersWithMask");
        }

    }

    public static class SubmarineSurvillanceMinigamePatch
    {
        private static FieldInfo screenStaticInfo;
        private static FieldInfo screenTextInfo;

        public static bool Prefix(Minigame __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity()) { return true; }


            GameObject screenStatic = screenStaticInfo.GetValue(__instance) as GameObject;
            GameObject screenText = screenTextInfo.GetValue(__instance) as GameObject;

            if (screenStatic != null)
            {
                TMPro.TextMeshPro comText = screenStatic.GetComponentInChildren<TMPro.TextMeshPro>();
                if (comText != null)
                {
                    comText.text = Helper.Translation.GetString("youDonotUse");
                }

                screenStatic.SetActive(true);
            }
            if (screenText != null)
            {
                screenText.SetActive(true);
            }

            return false;
        }

        public static void Postfix(Minigame __instance)
        {
            ExtremeRoles.Patches.MiniGame.SecurityHelper.PostUpdate(__instance);

            var timer = ExtremeRoles.Patches.MiniGame.SecurityHelper.GetTimerText();
            if (timer != null)
            {
                timer.gameObject.layer = 5;
                timer.transform.localPosition = new Vector3(15.3f, 9.3f, -900.0f);
                timer.transform.localScale = new Vector3(3.0f, 3.0f, 3.0f);
            }
        }

        public static void SetType(System.Type type)
        {
            screenStaticInfo = type.GetField("ScreenStatic");
            screenTextInfo = type.GetField("ScreenText");
        }
    }

}
