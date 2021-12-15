using UnityEngine;

using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.OnEnable))]
    public class KeyValueOptionOnEnablePatch
    {
        public static void Postfix(KeyValueOption __instance)
        {
            GameOptionsData gameOptions = PlayerControl.GameOptions;
            if (__instance.Title == StringNames.GameMapName)
            {
                __instance.Selected = gameOptions.MapId;
            }
            try
            {
                __instance.ValueText.text = __instance.Values[Mathf.Clamp(__instance.Selected, 0, __instance.Values.Count - 1)].Key;
            }
            catch { }
        }
    }
}
