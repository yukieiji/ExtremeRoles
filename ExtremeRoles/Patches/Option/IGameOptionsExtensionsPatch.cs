using AmongUs.GameOptions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch(
        typeof(IGameOptionsExtensions),
        nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    public static class IGameOptionsExtensionsNumImpostorsPatch
    {
        public static bool Prefix(GameOptionsData __instance, ref int __result)
        {
            __result = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                Int32OptionNames.NumImpostors);
            return false;
        }
    }
}
