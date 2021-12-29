using System;
using System.Linq;

using HarmonyLib;

using ExtremeRoles.Module;


namespace ExtremeRoles.Patches.Option
{

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOptionBase option = OptionsHolder.AllOption.Values.FirstOrDefault(
                option => option.Behaviour == __instance);
            if (option == null) { return true; };
            option.UpdateSelection(option.CurSelection - 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOptionBase option = OptionsHolder.AllOption.Values.FirstOrDefault(
                option => option.Behaviour == __instance);
            if (option == null) { return true; };
            option.UpdateSelection(option.CurSelection + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public class StringOptionOnEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOptionBase option = OptionsHolder.AllOption.Values.FirstOrDefault(
                option => option.Behaviour == __instance);
            if (option == null) { return true; };

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.GetName();
            __instance.Value = __instance.oldValue = option.CurSelection;
            __instance.ValueText.text = option.GetString();

            return false;
        }
    }

}
