using System.Collections;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace ExtremeRoles.Compat.Patches
{
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

    public static class HudManager_Update_PatchPostfixPatch
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

            if (floorButton != null && !changed)
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
    }
}
