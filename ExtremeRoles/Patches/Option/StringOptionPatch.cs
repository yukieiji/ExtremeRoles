using System;
using System.Linq;

using HarmonyLib;

using ExtremeRoles.Module;
using UnityEngine;

namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch]
    public static class StringOptionSelectionUpdatePatch
    {
        private const KeyCode MaxSelectionKey = KeyCode.LeftControl;
        private const KeyCode SkipSelectionKey = KeyCode.LeftShift;
        
        private const int defaultStep = 1;
        private const int skipStep = 10;

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
        public static class StringOptionDecreasePatch
        {
            public static bool Prefix(StringOption __instance)
            {
                IOption option = OptionHolder.AllOption.Values.FirstOrDefault(
                    option => option.Body == __instance);
                if (option == null) { return true; };

                int step = Input.GetKey(SkipSelectionKey) ? skipStep : defaultStep;
                int newSelection = Input.GetKey(MaxSelectionKey) ? 0 : option.CurSelection - step;
                option.UpdateSelection(newSelection);
                
                return false;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
        public static class StringOptionIncreasePatch
        {
            public static bool Prefix(StringOption __instance)
            {
                IOption option = OptionHolder.AllOption.Values.FirstOrDefault(
                    option => option.Body == __instance);
                if (option == null) { return true; };

                int step = Input.GetKey(SkipSelectionKey) ? skipStep : defaultStep;
                int newSelection = Input.GetKey(MaxSelectionKey) ? 
                    option.ValueCount - 1 : option.CurSelection + step;
                option.UpdateSelection(newSelection);

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public static class StringOptionOnEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            IOption option = OptionHolder.AllOption.Values.FirstOrDefault(
                option => option.Body == __instance);
            if (option == null) { return true; };

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.GetTranedName();
            __instance.Value = __instance.oldValue = option.CurSelection;
            __instance.ValueText.text = option.GetString();

            return false;
        }
    }
}
