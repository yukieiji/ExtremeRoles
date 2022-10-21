using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using UnityEngine;

namespace ExtremeSkins.Patches.AmongUs.Tab
{
    [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
    public static class PlayerTabEnablePatch
    {
        public static void Postfix(PlayerTab __instance)
        {   
            // Replace instead
            Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

            int cols = 7;

            for (int i = 0; i < chips.Count; i++)
            {
                ColorChip chip = chips[i];
                int row = i / cols, col = i % cols; // Dynamically do the positioning
                chip.transform.localPosition = new Vector3(
                    -0.975f + (col * 0.485f),
                    1.475f - (row * 0.49f),
                    chip.transform.localPosition.z);
                chip.transform.localScale *= 0.78f;
            }
        }
    }
}
