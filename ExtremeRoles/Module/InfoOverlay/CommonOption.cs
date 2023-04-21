using System;
using System.Text;

using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.InfoOverlay;

public static class CommonOption
{
    public static string GetGameOptionString()
    {
        StringBuilder printOption = new StringBuilder();

        foreach (OptionHolder.CommonOptionKey key in Enum.GetValues(
            typeof(OptionHolder.CommonOptionKey)))
        {
            if (key == OptionHolder.CommonOptionKey.PresetSelection) { continue; }

            addOptionString(ref printOption, key);
        }

        foreach (GlobalOption key in Enum.GetValues(typeof(GlobalOption)))
        {
            addOptionString(ref printOption, key);
        }

        return printOption.ToString();
    }

    private static void addOptionString<T>(
        ref StringBuilder builder, T optionKey) where T : struct, IConvertible
    {
        if (!AllOptionHolder.Instance.TryGetIOption(
                Convert.ToInt32(optionKey), out IOptionInfo option) ||
            option.IsHidden)
        {
            return;
        }

        string optStr = option.ToHudString();
        if (optStr != string.Empty)
        {
            builder.AppendLine(optStr);
        }
    }
}
