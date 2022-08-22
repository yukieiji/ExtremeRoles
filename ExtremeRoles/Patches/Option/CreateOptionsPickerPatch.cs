using System;

using HarmonyLib;

namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Awake))]
    public static class CreateOptionsPickerPatch
    {
        public static void Postfix(CreateOptionsPicker __instance)
        {
            int numImpostors = Math.Clamp(
                __instance.GetTargetOptions().NumImpostors,
                1, OptionHolder.MaxImposterNum);
            __instance.SetImpostorButtons(numImpostors);
        }
    }
}
