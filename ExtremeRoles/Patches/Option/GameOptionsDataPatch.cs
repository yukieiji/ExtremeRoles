using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;


namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch]
    class GameOptionsDataPatch
    {

        private static void Postfix(ref string __result)
        {

            void AddChildren(CustomOption option, ref StringBuilder entry, bool indent = true)
            {
                if (!option.Enabled) return;

                foreach (var child in option.Children)
                {
                    if (!child.IsHidden)
                        entry.AppendLine((indent ? "    " : "") + OptionToString(child));
                    AddChildren(child, ref entry, indent);
                }
            }

            List<string> pages = new List<string>();
            pages.Add(__result);

            StringBuilder entry = new StringBuilder();
            List<string> entries = new List<string>();

            var allSetting = OptionsHolder.AllOptions;

            entries.Add(
                OptionToString(allSetting[(int)OptionsHolder.CommonOptionKey.PresetSelection]));

            var optionName = Design.Cs(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                Tl("crewmateRoles"));
            var min = allSetting[(int)OptionsHolder.CommonOptionKey.MinCremateRoles].GetSelection();
            var max = allSetting[(int)OptionsHolder.CommonOptionKey.MaxCremateRoles].GetSelection();
            if (min > max) min = max;
            var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.Cs(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                Tl("neutralRoles"));
            min = allSetting[(int)OptionsHolder.CommonOptionKey.MinNeutralRoles].GetSelection();
            max = allSetting[(int)OptionsHolder.CommonOptionKey.MaxNeutralRoles].GetSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.Cs(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                Tl("impostorRoles"));
            min = allSetting[(int)OptionsHolder.CommonOptionKey.MinImpostorRoles].GetSelection();
            max = allSetting[(int)OptionsHolder.CommonOptionKey.MaxImpostorRoles].GetSelection();

            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            entries.Add(entry.ToString().Trim('\r', '\n'));

            foreach (CustomOption option in OptionsHolder.AllOptions.Values)
            {
                if ((option == allSetting[(int)OptionsHolder.CommonOptionKey.PresetSelection]) ||
                    (option == allSetting[(int)OptionsHolder.CommonOptionKey.MinCremateRoles]) ||
                    (option == allSetting[(int)OptionsHolder.CommonOptionKey.MaxCremateRoles]) ||
                    (option == allSetting[(int)OptionsHolder.CommonOptionKey.MinNeutralRoles]) ||
                    (option == allSetting[(int)OptionsHolder.CommonOptionKey.MaxNeutralRoles]) ||
                    (option == allSetting[(int)OptionsHolder.CommonOptionKey.MinImpostorRoles]) ||
                    (option == allSetting[(int)OptionsHolder.CommonOptionKey.MaxImpostorRoles]))
                {
                    continue;
                }

                if (option.Parent == null)
                {
                    if (!option.Enabled)
                    {
                        continue;
                    }

                    entry = new StringBuilder();
                    if (!option.IsHidden)
                        entry.AppendLine(OptionToString(option));

                    AddChildren(option, ref entry, !option.IsHidden);
                    entries.Add(entry.ToString().Trim('\r', '\n'));
                }
            }

            int maxLines = 28;
            int lineCount = 0;
            string page = "";
            foreach (var e in entries)
            {
                int lines = e.Count(c => c == '\n') + 1;

                if (lineCount + lines > maxLines)
                {
                    pages.Add(page);
                    page = "";
                    lineCount = 0;
                }

                page = page + e + "\n\n";
                lineCount += lines + 1;
            }

            page = page.Trim('\r', '\n');
            if (page != "")
            {
                pages.Add(page);
            }

            int numPages = pages.Count;
            int counter = ExtremeRolesPlugin.OptionsPage = ExtremeRolesPlugin.OptionsPage % numPages;

            __result = pages[counter].Trim('\r', '\n') + "\n\n" + Tl("pressTabForMore") + $" ({counter + 1}/{numPages})";

        }

        private static string OptionToString(CustomOption option)
        {
            if (option == null) return "";
            return $"{option.GetName()}: {option.GetString()}";
        }
        private static string OptionsToString(CustomOption option, bool skipFirst = false)
        {
            if (option == null) return "";

            List<string> options = new List<string>();
            if (!option.IsHidden && !skipFirst) options.Add(OptionToString(option));
            if (option.Enabled)
            {
                foreach (CustomOption op in option.Children)
                {
                    string str = OptionToString(op);
                    if (str != "") options.Add(str);
                }
            }
            return string.Join("\n", options);
        }
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(GameOptionsData).GetMethods().Where(
                x => x.ReturnType == typeof(string) &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == typeof(int));
        }
        private static string Tl(string key)
        {
            return Translation.GetString(key);
        }

    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.GetAdjustedNumImpostors))]
    public static class GameOptionsGetAdjustedNumImpostorsPatch
    {
        public static bool Prefix(GameOptionsData __instance, ref int __result)
        {
            __result = PlayerControl.GameOptions.NumImpostors;
            return false;
        }
    }
}
