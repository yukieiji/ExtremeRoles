using System;

using HarmonyLib;

namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Start))]
    public class CreateOptionsPickerPatch
    {
        public static void Postfix(CreateOptionsPicker __instance)
        {
            int numImpostors = Math.Clamp(__instance.GetTargetOptions().NumImpostors, 1, 3);
            __instance.SetImpostorButtons(numImpostors);
        }
    }
}
