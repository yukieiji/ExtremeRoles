using System;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch]
    public static class StringOptionSelectionUpdatePatch
    {
        private const KeyCode maxSelectionKey = KeyCode.LeftControl;
        private const KeyCode skipSelectionKey = KeyCode.LeftShift;
        
        private const int defaultStep = 1;
        private const int skipStep = 10;

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
        public static class StringOptionDecreasePatch
        {
            public static bool Prefix(StringOption __instance)
            {

                string idStr = __instance.gameObject.name.Replace(
                    OptionMenuTab.StringOptionName, string.Empty);

                if (!int.TryParse(idStr, out int id)) { return true; };

                IOption option = OptionHolder.AllOption[id];

                int step = Input.GetKey(skipSelectionKey) ? skipStep : defaultStep;
                int newSelection = Input.GetKey(maxSelectionKey) ? 0 : option.CurSelection - step;
                option.UpdateSelection(newSelection);
                
                return false;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
        public static class StringOptionIncreasePatch
        {
            public static bool Prefix(StringOption __instance)
            {
                string idStr = __instance.gameObject.name.Replace(
                    OptionMenuTab.StringOptionName, string.Empty);

                if (!int.TryParse(idStr, out int id)) { return true; };

                IOption option = OptionHolder.AllOption[id];

                int step = Input.GetKey(skipSelectionKey) ? skipStep : defaultStep;
                int newSelection = Input.GetKey(maxSelectionKey) ? 
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
            string idStr = __instance.gameObject.name.Replace(
                OptionMenuTab.StringOptionName, string.Empty);

            if (!int.TryParse(idStr, out int id)) { return true; };

            IOption option = OptionHolder.AllOption[id];

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.GetTranslatedName();
            __instance.Value = __instance.oldValue = option.CurSelection;
            __instance.ValueText.text = option.GetTranslatedValue();

            return false;
        }
    }
}
