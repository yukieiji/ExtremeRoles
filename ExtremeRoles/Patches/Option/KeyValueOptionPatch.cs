using UnityEngine;
using AmongUs.GameOptions;

using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.OnEnable))]
    public static class KeyValueOptionOnEnablePatch
    {
        public static void Postfix(KeyValueOption __instance)
        {
            if (__instance.Title == StringNames.GameMapName)
            {
                __instance.Selected = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                    ByteOptionNames.MapId);
            }
            try
            {
                __instance.ValueText.text = __instance.Values[
                    Mathf.Clamp(
                        __instance.Selected, 0,
                        __instance.Values.Count - 1)].Key;
            }
            catch { }
        }
    }
}
